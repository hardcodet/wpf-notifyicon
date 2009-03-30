using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Hardcodet.Wpf.TaskbarNotification.Interop;

namespace Hardcodet.Wpf.TaskbarNotification
{
  partial class TaskbarIcon
  {
    /// <summary>
    /// An action that is being invoked if the
    /// <see cref="singleClickTimer"/> fires.
    /// </summary>
    private Action delayedTimerAction;

    /// <summary>
    /// A timer that is used to differentiate between single
    /// and double clicks.
    /// </summary>
    private readonly Timer singleClickTimer;


    #region SetVersion

    /// <summary>
    /// Sets the version flag for the <see cref="iconData"/>.
    /// </summary>
    private void SetVersion()
    {
      iconData.VersionOrTimeout = (uint) NotifyIconVersion.Vista;
      bool status = WinApi.Shell_NotifyIcon(NotifyCommand.SetVersion, ref iconData);
     
      if (!status)
      {
        iconData.VersionOrTimeout = (uint) NotifyIconVersion.Win2000;
        status = Util.WriteIconData(ref iconData, NotifyCommand.SetVersion);
      }

      if (!status)
      {
        iconData.VersionOrTimeout = (uint)NotifyIconVersion.Win95;
        status = Util.WriteIconData(ref iconData, NotifyCommand.SetVersion);
      }

      if (!status)
      {
        Debug.Fail("Could not set version");
      }
    }

    #endregion


    /// <summary>
    /// Sets tooltip settings for the class.
    /// </summary>
    private void WriteToolTipSettings()
    {
      IconDataMembers flags = IconDataMembers.Tip;
      iconData.ToolTipText = ToolTipText;

      if (messageSink.Version == NotifyIconVersion.Vista)
      {
        if (String.IsNullOrEmpty(ToolTipText) && ToolTip != null)
        {
          //if we have not tooltip text but a custom tooltip, we
          //need to set a dummy value
          iconData.ToolTipText = "ToolTip";
        }
        else if (!String.IsNullOrEmpty(ToolTipText) && ToolTip == null)
        {
          //if a tooltip text was set but there is no custom tooltip,
          //we need to fall back to legacy operations
          flags |= IconDataMembers.UseLegacyToolTips;
        }
      }

      //just write the the tooltip
      Util.WriteIconData(ref iconData, NotifyCommand.Modify, flags);
    }


    #region Show / Hide Balloon ToolTip

    /// <summary>
    /// Displays a balloon tip with the specified title,
    /// text, and icon in the taskbar for the specified time period.
    /// </summary>
    /// <param name="title">The title to display on the balloon tip.</param>
    /// <param name="message">The text to display on the balloon tip.</param>
    /// <param name="icon">Indicates the severity.</param>
    public void ShowBalloonTip(string title, string message, BalloonIcon icon)
    {
      lock(this)
      {
        ShowBalloonTip(title, message, icon.GetBalloonFlag(), IntPtr.Zero);
      }
    }


    /// <summary>
    /// Displays a balloon tip with the specified title,
    /// text, and a custom icon in the taskbar for the specified time period.
    /// </summary>
    /// <param name="title">The title to display on the balloon tip.</param>
    /// <param name="message">The text to display on the balloon tip.</param>
    /// <param name="customIcon">A custom icon.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="customIcon"/>
    /// is a null reference.</exception>
    public void ShowBalloonTip(string title, string message, Icon customIcon)
    {
      if (customIcon == null) throw new ArgumentNullException("customIcon");

      lock(this)
      {
        ShowBalloonTip(title, message, BalloonFlags.User, customIcon.Handle);
      }
    }



    /// <summary>
    /// Invokes <see cref="WinApi.Shell_NotifyIcon"/> in order to display
    /// a given balloon ToolTip.
    /// </summary>
    /// <param name="title">The title to display on the balloon tip.</param>
    /// <param name="message">The text to display on the balloon tip.</param>
    /// <param name="flags">Indicates what icon to use.</param>
    /// <param name="balloonIconHandle">A handle to a custom icon, if any, or
    /// <see cref="IntPtr.Zero"/>.</param>
    private void ShowBalloonTip(string title, string message, BalloonFlags flags, IntPtr balloonIconHandle)
    {
      EnsureNotDisposed();

      iconData.BalloonText = message;
      iconData.BalloonTitle = title;

      iconData.BalloonFlags = flags;
      iconData.CustomBalloonIconHandle = balloonIconHandle;
      Util.WriteIconData(ref iconData, NotifyCommand.Modify, IconDataMembers.Info);
    }



    /// <summary>
    /// Hides a balloon ToolTip, if any is displayed.
    /// </summary>
    public void HideBalloonTip()
    {
      EnsureNotDisposed();

      //reset balloon by just setting the info to an empty string
      iconData.BalloonText = iconData.BalloonTitle = String.Empty;
      Util.WriteIconData(ref iconData, NotifyCommand.Modify, IconDataMembers.Info);
    }

    #endregion


    #region Single Click Timer event

    /// <summary>
    /// Performs a delayed action if the user requested an action
    /// based on a single click of the left mouse.<br/>
    /// This method is invoked by the <see cref="singleClickTimer"/>.
    /// </summary>
    private void DoSingleClickAction(object state)
    {
      if (IsDisposed) return;

      //run action
      Action action = delayedTimerAction;
      if (action != null)
      {
        //cleanup action
        delayedTimerAction = null;

        //switch to UI thread
        Application.Current.Dispatcher.Invoke(action);
      }
    }

    #endregion


    #region Show Tray Popup / Context Menu

    /// <summary>
    /// Displays the <see cref="TaskbarIconPopup"/> control if
    /// it was set.
    /// </summary>
    private void ShowTrayPopup()
    {
      if (IsDisposed) return;

      if (TaskbarIconPopup != null)
      {
        //raise preview event
        var args = RaisePreviewTaskbarIconPopupOpenEvent();
        if (args.Handled) return;

        //open popup
        TaskbarIconPopup.IsOpen = true;

        //activate the message window to track deactivation - otherwise, the context menu
        //does not close if the user clicks somewhere else
        WinApi.SetForegroundWindow(messageSink.MessageWindowHandle);

        //bubble event
        RaiseTaskbarIconPopupOpenEvent();
      }
    }


    /// <summary>
    /// Displays the <see cref="ContextMenu"/> if
    /// it was set.
    /// </summary>
    private void ShowContextMenu()
    {
      if (IsDisposed) return;

      if (ContextMenu != null)
      {
        //raise preview event
        var args = RaisePreviewTaskbarIconContextMenuOpenEvent();
        if (args.Handled) return;
        
        //CreateActivationWindow();
        ContextMenu.IsOpen = true;
        
        //activate the message window to track deactivation - otherwise, the context menu
        //does not close if the user clicks somewhere else
        WinApi.SetForegroundWindow(messageSink.MessageWindowHandle);

        //bubble event
        RaiseTaskbarIconContextMenuOpenEvent();
      }
    }

    #endregion
  }
}
