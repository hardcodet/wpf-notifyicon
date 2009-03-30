using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Resources;
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
          (bool)DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty, typeof(FrameworkElement))
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
    /// <param name="data"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    public static bool WriteIconData(ref NotifyIconData data, NotifyCommand command)
    {
      return WriteIconData(ref data, command, data.ValidMembers);
    }


    /// <summary>
    /// Updates the taskbar icons with data provided by a given
    /// <see cref="NotifyIconData"/> instance.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="command"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
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
        msg = String.Format(msg, imageSource);
        throw new ArgumentException(msg);
      }

      return new Icon(streamInfo.Stream);
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
          return true;
        default:
          throw new ArgumentOutOfRangeException("activationMode");
      }
    }

    #endregion
  }
}
