// hardcodet.net NotifyIcon for WPF
// Copyright (c) 2009 - 2022 Philipp Sumi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Contact and Information: http://www.hardcodet.net

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Resources;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification.Interop;

namespace Hardcodet.Wpf.TaskbarNotification
{
    /// <summary>
    /// Util and extension methods.
    /// </summary>
    internal static class Util
    {
        public static readonly object SyncRoot = new object();

        #region IsDesignMode

        private static readonly bool isDesignMode;

        /// <summary>
        /// Checks whether the application is currently in design mode.
        /// </summary>
        public static bool IsDesignMode
        {
            get { return isDesignMode; }
        }

        #endregion

        #region construction

        static Util()
        {
            isDesignMode =
                (bool)
                DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty,
                        typeof (FrameworkElement))
                    .Metadata.DefaultValue;
        }

        #endregion

        #region CreateHelperWindow

        /// <summary>
        /// Creates an transparent window without dimension that
        /// can be used to temporarily obtain focus and/or
        /// be used as a window message sink.
        /// </summary>
        /// <returns>Empty window.</returns>
        public static Window CreateHelperWindow()
        {
            return new Window
            {
                Width = 0,
                Height = 0,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Opacity = 0
            };
        }

        #endregion

        #region WriteIconData

        /// <summary>
        /// Updates the taskbar icons with data provided by a given
        /// <see cref="NotifyIconData"/> instance.
        /// </summary>
        /// <param name="data">Configuration settings for the NotifyIcon.</param>
        /// <param name="command">Operation on the icon (e.g. delete the icon).</param>
        /// <returns>True if the data was successfully written.</returns>
        /// <remarks>See Shell_NotifyIcon documentation on MSDN for details.</remarks>
        public static bool WriteIconData(ref NotifyIconData data, NotifyCommand command)
        {
            return WriteIconData(ref data, command, data.ValidMembers);
        }


        /// <summary>
        /// Updates the taskbar icons with data provided by a given
        /// <see cref="NotifyIconData"/> instance.
        /// </summary>
        /// <param name="data">Configuration settings for the NotifyIcon.</param>
        /// <param name="command">Operation on the icon (e.g. delete the icon).</param>
        /// <param name="flags">Defines which members of the <paramref name="data"/>
        /// structure are set.</param>
        /// <returns>True if the data was successfully written.</returns>
        /// <remarks>See Shell_NotifyIcon documentation on MSDN for details.</remarks>
        public static bool WriteIconData(ref NotifyIconData data, NotifyCommand command, IconDataMembers flags)
        {
            //do nothing if in design mode
            if (IsDesignMode) return true;

            data.ValidMembers = flags;
            lock (SyncRoot)
            {
                return WinApi.Shell_NotifyIcon(command, ref data);
            }
        }

        #endregion

        #region GetBalloonFlag

        /// <summary>
        /// Gets a <see cref="BalloonFlags"/> enum value that
        /// matches a given <see cref="BalloonIcon"/>.
        /// </summary>
        public static BalloonFlags GetBalloonFlag(this BalloonIcon icon)
        {
            switch (icon)
            {
                case BalloonIcon.None:
                    return BalloonFlags.None;
                case BalloonIcon.Info:
                    return BalloonFlags.Info;
                case BalloonIcon.Warning:
                    return BalloonFlags.Warning;
                case BalloonIcon.Error:
                    return BalloonFlags.Error;
                default:
                    throw new ArgumentOutOfRangeException("icon");
            }
        }

        #endregion

        #region ImageSource to Icon

        /// <summary>
        /// Reads a given image resource into a WinForms icon.
        /// </summary>
        /// <param name="imageSource">Image source pointing to
        /// an icon file (*.ico).</param>
        /// <returns>An icon object that can be used with the
        /// taskbar area.</returns>
        public static Icon ToIcon(this ImageSource imageSource)
        {
            if (imageSource == null) return null;

            Uri uri = new Uri(imageSource.ToString());
            StreamResourceInfo streamInfo = Application.GetResourceStream(uri);

            if (streamInfo == null)
            {
                string msg = "The supplied image source '{0}' could not be resolved.";
                msg = string.Format(msg, imageSource);
                throw new ArgumentException(msg);
            }

            Interop.Size iconSize = SystemInfo.SmallIconSize;

            using var stream = streamInfo.Stream;
            var bestIcon = GetBestFitIcon(stream, new System.Drawing.Size(iconSize.Width, iconSize.Height));
            return bestIcon;
        }

        /// <summary>
        /// Finds the best fitting icon from a stream based on the desired size.
        /// </summary>
        /// <param name="iconStream">The stream containing the icon data.</param>
        /// <param name="desiredSize">The desired size of the icon.</param>
        /// <returns>The best fitting icon as an <see cref="Icon"/> object.</returns>
        /// <exception cref="InvalidDataException">Thrown if the ICO file header is invalid or contains no images.</exception>
        /// <exception cref="EndOfStreamException">Thrown if the complete icon image data could not be read.</exception>
        private static Icon GetBestFitIcon(Stream iconStream, System.Drawing.Size desiredSize)
        {
            // Read the icon entries
            iconStream.Seek(0, SeekOrigin.Begin);
            using var reader = new BinaryReader(iconStream);

            // Read and validate the ICONDIR header
            var idReserved = reader.ReadUInt16(); // Reserved (must be 0)
            var idType = reader.ReadUInt16();     // Resource Type (1 for icons)
            var idCount = reader.ReadUInt16();          // Number of images

            if (idReserved != 0 || idType != 1)
                throw new InvalidDataException("Invalid ICO file header.");

            if (idCount == 0)
                throw new InvalidDataException("The ICO file contains no images.");

            // Read ICONDIRENTRYs
            List<IconEntry> iconEntries = [];
            for (var i = 0; i < idCount; i++)
            {
                var entry = new IconEntry
                {
                    Width = reader.ReadByte(),
                    Height = reader.ReadByte(),
                    ColorCount = reader.ReadByte(),
                    Reserved = reader.ReadByte(),
                    Planes = reader.ReadUInt16(),
                    BitCount = reader.ReadUInt16(),
                    BytesInRes = reader.ReadUInt32(),
                    ImageOffset = reader.ReadUInt32()
                };

                // Adjust for 256x256 icons, which are stored with width and height as 0
                if (entry.Width == 0) entry.Width = 256;
                if (entry.Height == 0) entry.Height = 256;

                iconEntries.Add(entry);
            }

            // Find icons greater than or equal to the desired size
            IconEntry bestEntry;
            var largerOrEqualIcons = iconEntries
                .Where(entry => entry.Width >= desiredSize.Width && entry.Height >= desiredSize.Height)
                .OrderBy(entry => entry.Width * entry.Height)
                .ThenBy(entry => entry.Width)
                .ThenBy(entry => entry.Height)
                .ToList();

            if (largerOrEqualIcons.Any())
            {
                // Select the smallest icon among those larger or equal to the desired size
                bestEntry = largerOrEqualIcons.First();
            }
            else
            {
                // No larger icons; select the largest icon smaller than the desired size
                var smallerIcons = iconEntries
                    .Where(entry => entry.Width < desiredSize.Width && entry.Height < desiredSize.Height)
                    .OrderByDescending(entry => entry.Width * entry.Height)
                    .ThenByDescending(entry => entry.Width)
                    .ThenByDescending(entry => entry.Height)
                    .ToList();

                // If no icons are smaller or larger, select any available icon (unlikely case)
                bestEntry = smallerIcons.Any() ? smallerIcons.First() : iconEntries.FirstOrDefault();
            }

            if (bestEntry == null)
                return null;

            // Read the image data of the selected icon
            var iconImageData = new byte[bestEntry.BytesInRes];
            iconStream.Seek(bestEntry.ImageOffset, SeekOrigin.Begin);
            var bytesRead = iconStream.Read(iconImageData, 0, (int)bestEntry.BytesInRes);
            if (bytesRead != bestEntry.BytesInRes)
                throw new EndOfStreamException("Could not read the complete icon image data.");

            // Create a new .ico file with the single best-matching image
            using var destStream = new MemoryStream();
            using var writer = new BinaryWriter(destStream);

            writer.Write((ushort)0); // idReserved
            writer.Write((ushort)1); // idType
            writer.Write((ushort)1); // idCount

            writer.Write(bestEntry.Width == 256 ? (byte)0 : (byte)bestEntry.Width);
            writer.Write(bestEntry.Height == 256 ? (byte)0 : (byte)bestEntry.Height);
            writer.Write(bestEntry.ColorCount);
            writer.Write(bestEntry.Reserved);
            writer.Write(bestEntry.Planes);
            writer.Write(bestEntry.BitCount);
            writer.Write(bestEntry.BytesInRes);
            writer.Write((uint)(6 + 16)); // Image data offset

            // Write the image data
            writer.Write(iconImageData);

            destStream.Seek(0, SeekOrigin.Begin);
            return new Icon(destStream);
        }

        /// <summary>
        /// Represents an entry in the icon directory.
        /// </summary>
        private class IconEntry
        {
            public int Width;
            public int Height;
            public byte ColorCount;
            public byte Reserved;
            public ushort Planes;
            public ushort BitCount;
            public uint BytesInRes;
            public uint ImageOffset;
        }

        #endregion

        #region evaluate listings

        /// <summary>
        /// Checks a list of candidates for equality to a given
        /// reference value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The evaluated value.</param>
        /// <param name="candidates">A liste of possible values that are
        /// regarded valid.</param>
        /// <returns>True if one of the submitted <paramref name="candidates"/>
        /// matches the evaluated value. If the <paramref name="candidates"/>
        /// parameter itself is null, too, the method returns false as well,
        /// which allows to check with null values, too.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="candidates"/>
        /// is a null reference.</exception>
        public static bool Is<T>(this T value, params T[] candidates)
        {
            if (candidates == null) return false;

            foreach (var t in candidates)
            {
                if (value.Equals(t)) return true;
            }

            return false;
        }

        #endregion

        #region match MouseEvent to PopupActivation

        /// <summary>
        /// Checks if a given <see cref="PopupActivationMode"/> is a match for
        /// an effectively pressed mouse button.
        /// </summary>
        public static bool IsMatch(this MouseEvent me, PopupActivationMode activationMode)
        {
            switch (activationMode)
            {
                case PopupActivationMode.LeftClick:
                    return me == MouseEvent.IconLeftMouseUp;
                case PopupActivationMode.RightClick:
                    return me == MouseEvent.IconRightMouseUp;
                case PopupActivationMode.LeftOrRightClick:
                    return me.Is(MouseEvent.IconLeftMouseUp, MouseEvent.IconRightMouseUp);
                case PopupActivationMode.LeftOrDoubleClick:
                    return me.Is(MouseEvent.IconLeftMouseUp, MouseEvent.IconDoubleClick);
                case PopupActivationMode.DoubleClick:
                    return me.Is(MouseEvent.IconDoubleClick);
                case PopupActivationMode.MiddleClick:
                    return me == MouseEvent.IconMiddleMouseUp;
                case PopupActivationMode.All:
                    //return true for everything except mouse movements
                    return me != MouseEvent.MouseMove;
                case PopupActivationMode.None:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException("activationMode");
            }
        }

        #endregion

        #region execute command

        /// <summary>
        /// Executes a given command if its <see cref="ICommand.CanExecute"/> method
        /// indicates it can run.
        /// </summary>
        /// <param name="command">The command to be executed, or a null reference.</param>
        /// <param name="commandParameter">An optional parameter that is associated with
        /// the command.</param>
        /// <param name="target">The target element on which to raise the command.</param>
        public static void ExecuteIfEnabled(this ICommand command, object commandParameter, IInputElement target)
        {
            if (command == null) return;

            RoutedCommand rc = command as RoutedCommand;
            if (rc != null)
            {
                //routed commands work on a target
                if (rc.CanExecute(commandParameter, target)) rc.Execute(commandParameter, target);
            }
            else if (command.CanExecute(commandParameter))
            {
                command.Execute(commandParameter);
            }
        }

        #endregion

        /// <summary>
        /// Returns a dispatcher for multi-threaded scenarios
        /// </summary>
        /// <returns>Dispatcher</returns>
        internal static Dispatcher GetDispatcher(this DispatcherObject source)
        {
            //use the application's dispatcher by default
            if (Application.Current != null) return Application.Current.Dispatcher;

            //fallback for WinForms environments
            if (source.Dispatcher != null) return source.Dispatcher;

            // ultimately use the thread's dispatcher
            return Dispatcher.CurrentDispatcher;
        }


        /// <summary>
        /// Checks whether the <see cref="FrameworkElement.DataContextProperty"/>
        ///  is bound or not.
        /// </summary>
        /// <param name="element">The element to be checked.</param>
        /// <returns>True if the data context property is being managed by a
        /// binding expression.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="element"/>
        /// is a null reference.</exception>
        public static bool IsDataContextDataBound(this FrameworkElement element)
        {
            if (element == null) throw new ArgumentNullException("element");
            return element.GetBindingExpression(FrameworkElement.DataContextProperty) != null;
        }
    }
}