// hardcodet.net NotifyIcon for WPF
// Copyright (c) 2009 - 2022 Philipp Sumi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Contact and Information: http://www.hardcodet.net

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification.Interop;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace Hardcodet.Wpf.TaskbarNotification
{
    /// <summary>
    /// Extension to support using different types to set the tray Icon
    /// With added FrameworkElement support, one now can use the PackIcon from
    /// <a href="https://github.com/ControlzEx/ControlzEx">ControlzEx</a>
    /// and
    /// <a href="https://github.com/MahApps/MahApps.Metro.IconPacks">MahApps.Metro.IconPacks</a>
    /// </summary>
    public static class IconExtensions
    {
        /// <summary>
        /// Reads a given image resource into a WinForms icon.
        /// </summary>
        /// <param name="imageSource">Image source pointing to an icon file (*.ico).</param>
        /// <returns>An icon object that can be used with the taskbar area.</returns>
        public static Icon ToIcon(this ImageSource imageSource)
        {
            if (imageSource == null) return null;

            if (imageSource is not DrawingImage)
            {
                if (!Uri.TryCreate(imageSource.ToString(), UriKind.RelativeOrAbsolute, out var resourceUri))
                {
                    // Check if the supplied ImageSource is a BitmapImage
                    if (imageSource is BitmapImage bimapImage)
                    {
                        // Get the UriSource
                        resourceUri = bimapImage.UriSource;
                    }
                }

                // Check if there is a Uri which can be used to create an icon from
                if (resourceUri != null)
                {
                    // Try to create an icon from the stream we have gotten via the Uri
                    try
                    {
                        using var stream = resourceUri.IsAbsoluteUri && File.Exists(resourceUri.AbsolutePath)
                            ? new MemoryStream(File.ReadAllBytes(resourceUri.AbsolutePath))
                            : Application.GetResourceStream(resourceUri)?.Stream;

                        if (stream != null)
                        {
                            Interop.Size iconSize = SystemInfo.SmallIconSize;
                            var bestIcon = GetBestFitIcon(stream,
                                new System.Drawing.Size(iconSize.Width, iconSize.Height));
                            return bestIcon;
                        }
                    }
                    catch
                    {
                        // Ignoring for now, we can make an Icon differently
                    }
                }
            }

            // Create an icon from the representation of the imageSource by creating an Image (wpf) and render it to a RenderTargetBitmap
            var image = new Image
            {
                Source = imageSource
            };
            return image.ToIcon();
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
            var idType = reader.ReadUInt16(); // Resource Type (1 for icons)
            var idCount = reader.ReadUInt16(); // Number of images

            if (idReserved != 0 || idType != 1)
                throw new InvalidDataException("Invalid ICO file header.");

            if (idCount == 0)
                throw new InvalidDataException("The ICO file contains no images.");

            // Read ICONDIRENTRYs
            var iconEntries = new List<IconEntry>();
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

        /// <summary>
        ///     Render a frameworkElement to a "GDI" Icon with a specified size or the system's default size.
        /// </summary>
        /// <param name="frameworkElement">FrameworkElement</param>
        /// <param name="size">Optional, specifies the size, if not given the system default is used</param>
        /// <returns>Icon</returns>
        public static Icon ToIcon(this FrameworkElement frameworkElement, int? size = null)
        {
            if (frameworkElement == null)
            {
                throw new ArgumentNullException(nameof(frameworkElement));
            }

            using var memoryStream = frameworkElement.ToIconMemoryStream(size.HasValue ? new[] { size.Value } : null);
            return new Icon(memoryStream);
        }

        /// <summary>
        /// Create a "GDI" icon from the supplied FrameworkElement, it is possible to specify multiple icon sizes.
        /// Note: this doesn't work on Windows versions BEFORE Windows Vista!
        /// </summary>
        /// <param name="frameworkElement">FrameworkElement to convert to an icon</param>
        /// <param name="optionalIconSizes">Optional, IEnumerable with icon sizes, default Icon sizes (as specified by windows): 16x16, 32x32, 48x48, 256x256</param>
        /// <returns>MemoryStream with the icon data</returns>
        public static MemoryStream ToIconMemoryStream(this FrameworkElement frameworkElement,
            IEnumerable<int> optionalIconSizes = null)
        {
            if (frameworkElement == null)
            {
                throw new ArgumentNullException(nameof(frameworkElement));
            }

            // Use the supplied or default values for the icon sizes
            var iconSizes = optionalIconSizes != null
                ? new List<int>(optionalIconSizes)
                : new List<int> { 16, 32, 48, 256 };

            var bitmapFrames = new List<BitmapFrame>();
            foreach (var iconSize in iconSizes)
            {
                var currentSize = new Size(iconSize, iconSize);
                var bitmapSource = frameworkElement.ToBitmapSource(currentSize);
                bitmapFrames.Add(BitmapFrame.Create(bitmapSource));
            }

            return bitmapFrames.ToIconMemoryStream();
        }

        /// <summary>
        /// Helper method to write one more BitmapFrames to a MemoryStream with GDI Icon data
        /// This can be written to a .ico or used with new Icon(stream)
        /// </summary>
        /// <param name="bitmapFrames">IList of BitmapFrames</param>
        /// <returns>MemoryStream with the icon data</returns>
        public static MemoryStream ToIconMemoryStream(this IList<BitmapFrame> bitmapFrames)
        {
            var stream = new MemoryStream();
            var binaryWriter = new BinaryWriter(stream);

            //
            // ICONDIR structure
            //
            binaryWriter.Write((short)0); // reserved
            binaryWriter.Write((short)1); // image type (icon)
            binaryWriter.Write((short)bitmapFrames.Count); // number of images

            //
            // ICONDIRENTRY structure
            //
            const int iconDirSize = 6;
            const int iconDirEntrySize = 16;

            var imageSizes = new List<Size>();
            var encodedImages = new List<MemoryStream>();
            foreach (var bitmapFrame in bitmapFrames)
            {
                imageSizes.Add(new Size(bitmapFrame.Width, bitmapFrame.Height));
                var imageStream = new MemoryStream();
                // Use PngBitmapEncoder for icons, with this we also respect transparency.
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(bitmapFrame);
                encoder.Save(imageStream);
                // Make sure the stream is read from the beginning
                imageStream.Seek(0, SeekOrigin.Begin);
                // Store the stream for later
                encodedImages.Add(imageStream);
            }

            var offset = iconDirSize + (imageSizes.Count * iconDirEntrySize);
            for (var i = 0; i < imageSizes.Count; i++)
            {
                var imageSize = imageSizes[i];
                // Write the width / height, 0 means 256
                binaryWriter.Write((int)imageSize.Width == 256 ? (byte)0 : (byte)imageSize.Width);
                binaryWriter.Write((int)imageSize.Height == 256 ? (byte)0 : (byte)imageSize.Height);
                binaryWriter.Write((byte)0); // no pallet
                binaryWriter.Write((byte)0); // reserved
                binaryWriter.Write((short)0); // no color planes
                binaryWriter.Write((short)32); // 32 bpp
                binaryWriter.Write((int)encodedImages[i].Length); // image data length
                binaryWriter.Write(offset);
                offset += (int)encodedImages[i].Length;
            }

            binaryWriter.Flush();

            //
            // Write image data
            //
            foreach (var encodedImage in encodedImages)
            {
                encodedImage.WriteTo(stream);
                encodedImage.Dispose();
            }

            // Rewind to make the MemoryStream usable
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        /// <summary>
        ///     Render the frameworkElement to a BitmapSource
        /// </summary>
        /// <param name="frameworkElement">FrameworkElement</param>
        /// <param name="size">Size, using the bound as size by default</param>
        /// <param name="dpiX">Horizontal DPI settings</param>
        /// <param name="dpiY">Vertical DPI settings</param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource ToBitmapSource(this FrameworkElement frameworkElement, Size? size = null,
            double dpiX = 96.0, double dpiY = 96.0)
        {
            if (frameworkElement == null)
            {
                throw new ArgumentNullException(nameof(frameworkElement));
            }

            // Make sure we have a size
            if (!size.HasValue)
            {
                var bounds = VisualTreeHelper.GetDescendantBounds(frameworkElement);
                size = bounds != Rect.Empty ? bounds.Size : new Size(16, 16);
            }

            // Create a viewbox to render the frameworkElement in the correct size
            var viewbox = new Viewbox
            {
                //frameworkElement to render
                Child = frameworkElement
            };
            viewbox.Measure(size.Value);
            viewbox.Arrange(new Rect(new Point(), size.Value));
            viewbox.UpdateLayout();

            var renderTargetBitmap = new RenderTargetBitmap((int)(size.Value.Width * dpiX / 96.0),
                (int)(size.Value.Height * dpiY / 96.0),
                dpiX,
                dpiY,
                PixelFormats.Pbgra32);
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                var visualBrush = new VisualBrush(viewbox);
                drawingContext.DrawRectangle(visualBrush, null, new Rect(new Point(), size.Value));
            }

            renderTargetBitmap.Render(drawingVisual);
            // Disassociate the frameworkElement from the viewbox, so the frameworkElement could be used elsewhere
            viewbox.RemoveChild(frameworkElement);
            return renderTargetBitmap;
        }

        /// <summary>
        /// Disassociate the child from the parent
        /// </summary>
        /// <param name="parent">DependencyObject</param>
        /// <param name="child">UIElement</param>
        private static void RemoveChild(this DependencyObject parent, UIElement child)
        {
            switch (parent)
            {
                case Panel panel:
                    panel.Children.Remove(child);

                    break;
                case Decorator decorator:
                {
                    if (ReferenceEquals(decorator.Child, child))
                    {
                        decorator.Child = null;
                    }

                    break;
                }
                case ContentPresenter contentPresenter:
                {
                    if (Equals(contentPresenter.Content, child))
                    {
                        contentPresenter.Content = null;
                    }

                    break;
                }
                case ContentControl contentControl:
                {
                    if (Equals(contentControl.Content, child))
                    {
                        contentControl.Content = null;
                    }

                    break;
                }
            }
        }
    }
}