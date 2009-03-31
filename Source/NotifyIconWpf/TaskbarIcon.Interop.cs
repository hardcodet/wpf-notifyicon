using System;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Hardcodet.Wpf.TaskbarNotification.Interop;
using Point=Hardcodet.Wpf.TaskbarNotification.Interop.Point;


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


    #region ToolTip

    /// <summary>
    /// Displays a custom tooltip, if available. This method is only
    /// invoked for Windows Vista and above.
    /// </summary>
    /// <param name="visible">Whether to show or hide the tooltip.</param>
    private void OnToolTipChange(bool visible)
    {
      //if we don't have a tooltip, there's nothing to do here...
      if (CustomToolTip == null) return;

      if (visible)
      {
        if (ContextMenu != null && ContextMenu.IsOpen ||
            CustomPopup != null && CustomPopup.IsOpen)
        {
          //ignore if we have an open context menu or popup
          return;
        }
        
        var args = RaisePreviewTaskbarIconToolTipOpenEvent();
        if (args.Handled) return;

        CustomToolTip.IsOpen = true;
        RaiseTaskbarIconToolTipOpenEvent();
      }
      else
      {
        var args = RaisePreviewTaskbarIconToolTipCloseEvent();
        if (args.Handled) return;

        CustomToolTip.IsOpen = false;
        RaiseTaskbarIconToolTipCloseEvent();
      }
    }

    /// <summary>
    /// Creates a <see cref="ToolTip"/> control that either
    /// wraps the currently set <see cref="TaskbarIconToolTip"/>
    /// control or the <see cref="ToolTipText"/> string.<br/>
    /// If <see cref="TaskbarIconToolTip"/> itself is already
    /// a <see cref="ToolTip"/> instance, it will be used directly.
    /// </summary>
    /// <remarks>We use a <see cref="ToolTip"/> rather than
    /// <see cref="Popup"/> because there was no way to prevent a
    /// popup from causing cyclic open/close commands if it was
    /// placed under the mouse. ToolTip internally uses a Popup of
    /// its own, but takes advance of Popup's internal <see cref="Popup.HitTestable"/>
    /// property which prevents this issue.</remarks>
    private void CreateCustomToolTip()
    {
      //check if the item itself is a tooltip
      ToolTip tt = TaskbarIconToolTip as ToolTip;

      if (tt == null && TaskbarIconToolTip != null)
      {
        //create an invisible tooltip that hosts the UIElement
        tt = new ToolTip();
        tt.Placement = PlacementMode.Mouse;
        tt.PlacementTarget = this;

        //the tooltip (and implicitly its context) explicitly gets
        //the DataContext of this instance. If there is no DataContext,
        //the TaskbarIcon sets itself
        tt.DataContext = DataContext ?? this;

        //make sure the tooltip is invisible
        tt.HasDropShadow = false;
        tt.BorderThickness = new Thickness(0);
        tt.Background = System.Windows.Media.Brushes.Transparent;

        //setting the 
        tt.StaysOpen = true;

        tt.Content = TaskbarIconToolTip;
      }
      else if (tt == null && !String.IsNullOrEmpty(ToolTipText))
      {
        //create a simple tooltip for the string
        tt = new ToolTip();
        tt.Content = ToolTipText;
      }

      //store a reference to the used tooltip
      CustomToolTip = tt;
    }


    /// <summary>
    /// Sets tooltip settings for the class depending on defined
    /// dependency properties and OS support.
    /// </summary>
    private void WriteToolTipSettings()
    {
      const IconDataMembers flags = IconDataMembers.Tip;
      iconData.ToolTipText = ToolTipText;

      if (messageSink.Version == NotifyIconVersion.Vista)
      {
        //we need to set a tooltip text to get tooltip events from the
        //taskbar icon
        if (String.IsNullOrEmpty(iconData.ToolTipText) && CustomToolTip != null)
        {
          //if we have not tooltip text but a custom tooltip, we
          //need to set a dummy value (we're displaying the ToolTip control, not the string)
          iconData.ToolTipText = "ToolTip";
        }
      }

      //update the tooltip text
      Util.WriteIconData(ref iconData, NotifyCommand.Modify, flags);
    }

    #endregion

    #region Show / Hide Balloon Tip

    /// <summary>
    /// Displays a balloon tip with the specified title,
    /// text, and icon in the taskbar for the specified time period.
    /// </summary>
    /// <param name="title">The title to display on the balloon tip.</param>
    /// <param name="message">The text to display on the balloon tip.</param>
    /// <param name="symbol">A symbol that indicates the severity.</param>
    public void ShowBalloonTip(string title, string message, BalloonIcon symbol)
    {
      lock (this)
      {
        ShowBalloonTip(title, message, symbol.GetBalloonFlag(), IntPtr.Zero);
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

      lock (this)
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

      Console.Out.WriteLine("TIMER EVENT");

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

    #region Create Popup

    /// <summary>
    /// Creates a <see cref="ToolTip"/> control that either
    /// wraps the currently set <see cref="TaskbarIconToolTip"/>
    /// control or the <see cref="ToolTipText"/> string.<br/>
    /// If <see cref="TaskbarIconToolTip"/> itself is already
    /// a <see cref="ToolTip"/> instance, it will be used directly.
    /// </summary>
    /// <remarks>We use a <see cref="ToolTip"/> rather than
    /// <see cref="Popup"/> because there was no way to prevent a
    /// popup from causing cyclic open/close commands if it was
    /// placed under the mouse. ToolTip internally uses a Popup of
    /// its own, but takes advance of Popup's internal <see cref="Popup.HitTestable"/>
    /// property which prevents this issue.</remarks>
    private void CreatePopup()
    {
      //no popup is available
      if (TaskbarIconPopup == null) return;

      //check if the item itself is a popup
      Popup popup = TaskbarIconPopup as Popup;

      if (popup == null)
      {
        //create an invisible popup that hosts the UIElement
        popup = new Popup();
        popup.AllowsTransparency = true;
        popup.PopupAnimation = PopupAnimation.Fade;

        //the tooltip (and implicitly its context) explicitly gets
        //the DataContext of this instance. If there is no DataContext,
        //the TaskbarIcon assigns itself
        popup.DataContext = DataContext ?? this;

        Popup.CreateRootPopup(popup, TaskbarIconPopup);

        popup.PlacementTarget = this;
        popup.Placement = PlacementMode.AbsolutePoint;
        popup.StaysOpen = false;
      }

      //store a reference to the used tooltip
      CustomPopup = popup;
    }

    #endregion  

    #region Show Tray Popup / Context Menu

    /// <summary>
    /// Displays the <see cref="TaskbarIconPopup"/> control if
    /// it was set.
    /// </summary>
    private void ShowTrayPopup(Point cursorPosition)
    {
      if (IsDisposed) return;

      //raise preview event no matter whether popup is currently set
      //or not (enables client to set it on demand)
      var args = RaisePreviewTaskbarIconPopupOpenEvent();
      if (args.Handled) return;

      if (TaskbarIconPopup != null)
      {
        //use absolute position, but place the popup centered above the icon
        CustomPopup.Placement = PlacementMode.AbsolutePoint;
        CustomPopup.HorizontalOffset = cursorPosition.X; //+ TaskbarIconPopup.ActualWidth/2;
        CustomPopup.VerticalOffset = cursorPosition.Y;

        //open popup
        CustomPopup.IsOpen = true;

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
    private void ShowContextMenu(Point cursorPosition)
    {
      if (IsDisposed) return;

      //raise preview event no matter whether context menu is currently set
      //or not (enables client to set it on demand)
      var args = RaisePreviewTaskbarIconContextMenuOpenEvent();
      if (args.Handled) return;

      if (ContextMenu != null)
      {
        //use absolute position
        ContextMenu.Placement = PlacementMode.AbsolutePoint;
        ContextMenu.HorizontalOffset = cursorPosition.X;
        ContextMenu.VerticalOffset = cursorPosition.Y;
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