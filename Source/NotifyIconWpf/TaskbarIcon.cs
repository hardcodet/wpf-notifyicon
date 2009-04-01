using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Hardcodet.Wpf.TaskbarNotification.Interop;
using Point=Hardcodet.Wpf.TaskbarNotification.Interop.Point;

namespace Hardcodet.Wpf.TaskbarNotification
{
  /// <summary>
  /// A WPF proxy to for a taskbar icon (NotifyIcon) that sits in the system's
  /// taskbar notification area ("system tray").
  /// </summary>
  public partial class TaskbarIcon : FrameworkElement, IDisposable
  {
    /// <summary>
    /// Represents the current icon data.
    /// </summary>
    private NotifyIconData iconData;

    /// <summary>
    /// Receives messages from the taskbar icon.
    /// </summary>
    private readonly WindowMessageSink messageSink;

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

    /// <summary>
    /// Indicates whether the taskbar icon has been created or not.
    /// </summary>
    public bool IsTaskbarIconCreated { get; private set; }

    /// <summary>
    /// Indicates whether custom tooltips are supported, which depends
    /// on the OS. Windows Vista or higher is required in order to
    /// support this feature.
    /// </summary>
    public bool SupportsCustomToolTips
    {
      get { return messageSink.Version == NotifyIconVersion.Vista; }
    }

    #region Construction

    /// <summary>
    /// Inits the taskbar icon and registers a message listener
    /// in order to receive events from the taskbar area.
    /// </summary>
    public TaskbarIcon()
    {
      //using dummy sink in design mode
      messageSink = Util.IsDesignMode
                        ? WindowMessageSink.CreateEmpty()
                        : new WindowMessageSink(NotifyIconVersion.Win95);

      //init icon data structure
      iconData = NotifyIconData.CreateDefault(messageSink.MessageWindowHandle);

      //create the taskbar icon
      CreateTaskbarIcon();

      //register event listeners
      messageSink.MouseEventReceived += OnMouseEvent;
      messageSink.TaskbarCreated += OnTaskbarCreated;
      messageSink.ChangeToolTipStateRequest += OnToolTipChange;
      messageSink.BallonToolTipChanged += OnBalloonToolTipChanged;

      //init single click timer
      singleClickTimer = new Timer(DoSingleClickAction);

      //register listener in order to get notified when the application closes
      if (Application.Current != null) Application.Current.Exit += OnExit;
    }

    #endregion

    #region Process Incoming Mouse Events

    /// <summary>
    /// Processes mouse events, which are bubbled
    /// through the class' routed events, trigger
    /// certain actions (e.g. show a popup), or
    /// both.
    /// </summary>
    /// <param name="me">Event flag.</param>
    private void OnMouseEvent(MouseEvent me)
    {
      if (IsDisposed) return;

      switch (me)
      {
        case MouseEvent.MouseMove:
          RaiseTaskbarIconMouseMoveEvent();
          //immediately return - there's nothing left to evaluate
          return;
        case MouseEvent.IconRightMouseDown:
          RaiseTaskbarIconRightMouseDownEvent();
          break;
        case MouseEvent.IconLeftMouseDown:
          RaiseTaskbarIconLeftMouseDownEvent();
          break;
        case MouseEvent.IconRightMouseUp:
          RaiseTaskbarIconRightMouseUpEvent();
          break;
        case MouseEvent.IconLeftMouseUp:
          RaiseTaskbarIconLeftMouseUpEvent();
          break;
        case MouseEvent.IconMiddleMouseDown:
          RaiseTaskbarIconMiddleMouseDownEvent();
          break;
        case MouseEvent.IconMiddleMouseUp:
          RaiseTaskbarIconMiddleMouseUpEvent();
          break;
        case MouseEvent.IconDoubleClick:
          //cancel single click timer
          singleClickTimer.Change(Timeout.Infinite, Timeout.Infinite);
          //bubble event
          RaiseTaskbarIconMouseDoubleClickEvent();
          break;
        case MouseEvent.BalloonToolTipClicked:
          RaiseTaskbarIconBalloonTipClickedEvent();
          break;
        default:
          throw new ArgumentOutOfRangeException("me", "Missing handler for mouse event flag: " + me);
      }


      //get mouse coordinates
      Point cursorPosition = new Point();
      WinApi.GetCursorPos(ref cursorPosition);

      //show popup, if requested
      if (me.IsMatch(PopupActivation))
      {
        if (me == MouseEvent.IconLeftMouseUp)
        {
          //show popup once we are sure it's not a double click
          delayedTimerAction = () => ShowTrayPopup(cursorPosition);
          singleClickTimer.Change(WinApi.GetDoubleClickTime(), Timeout.Infinite);
        }
        else
        {
          //show popup immediately
          ShowTrayPopup(cursorPosition);
        }
      }


      //show context menu, if requested
      if (me.IsMatch(MenuActivation))
      {
        if (me == MouseEvent.IconLeftMouseUp)
        {
          //show context menu once we are sure it's not a double click
          delayedTimerAction = () => ShowContextMenu(cursorPosition);
          singleClickTimer.Change(WinApi.GetDoubleClickTime(), Timeout.Infinite);
        }
        else
        {
          //show context menu immediately
          ShowContextMenu(cursorPosition);
        }
      }
    }

    #endregion

    #region ToolTips

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

        //raise attached event first
        if (TaskbarIconToolTip != null) RaiseToolTipOpenedEvent(TaskbarIconToolTip);
        
        //bubble routed event
        RaiseTaskbarIconToolTipOpenEvent();
      }
      else
      {
        var args = RaisePreviewTaskbarIconToolTipCloseEvent();
        if (args.Handled) return;

        //raise attached event first
        if (TaskbarIconToolTip != null) RaiseToolTipCloseEvent(TaskbarIconToolTip);

        //CustomToolTip.IsOpen = false;
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

    #region Custom Popup

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

        //raise attached event - item should never be null unless developers
        //changed the CustomPopup directly...
        if (TaskbarIconPopup != null) RaisePopupOpenedEvent(TaskbarIconPopup);

        //bubble routed event
        RaiseTaskbarIconPopupOpenEvent();
      }
    }

    #endregion

    #region Context Menu

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

    #region Balloon Tips

    /// <summary>
    /// Bubbles events if a balloon ToolTip was displayed
    /// or removed.
    /// </summary>
    /// <param name="visible">Whether the ToolTip was just displayed
    /// or removed.</param>
    private void OnBalloonToolTipChanged(bool visible)
    {
      if (visible)
      {
        RaiseTaskbarIconBalloonTipShownEvent();
      }
      else
      {
        RaiseTaskbarIconBalloonTipClosedEvent();
      }
    }

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

    #region Set Version (API)

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
        iconData.VersionOrTimeout = (uint) NotifyIconVersion.Win95;
        status = Util.WriteIconData(ref iconData, NotifyCommand.SetVersion);
      }

      if (!status)
      {
        Debug.Fail("Could not set version");
      }
    }

    #endregion

    #region Create / Remove Taskbar Icon

    /// <summary>
    /// Recreates the taskbar icon if the whole taskbar was
    /// recreated (e.g. because Explorer was shut down).
    /// </summary>
    private void OnTaskbarCreated()
    {
      IsTaskbarIconCreated = false;
      CreateTaskbarIcon();
    }


    /// <summary>
    /// Creates the taskbar icon. This message is invoked during initialization,
    /// if the taskbar is restarted, and whenever the icon is displayed.
    /// </summary>
    private void CreateTaskbarIcon()
    {
      lock (this)
      {
        if (!IsTaskbarIconCreated)
        {
          const IconDataMembers members = IconDataMembers.Message
                                          | IconDataMembers.Icon
                                          | IconDataMembers.Tip;

          //write initial configuration
          var status = Util.WriteIconData(ref iconData, NotifyCommand.Add, members);
          if (!status)
          {
            throw new Win32Exception("Could not create icon data");
          }

          //set to most recent version
          SetVersion();
          messageSink.Version = (NotifyIconVersion) iconData.VersionOrTimeout;

          IsTaskbarIconCreated = true;
        }
      }
    }


    /// <summary>
    /// Closes the taskbar icon if required.
    /// </summary>
    private void RemoveTaskbarIcon()
    {
      lock (this)
      {
        if (IsTaskbarIconCreated)
        {
          Util.WriteIconData(ref iconData, NotifyCommand.Delete, IconDataMembers.Message);
          IsTaskbarIconCreated = false;
        }
      }
    }

    #endregion

    #region Dispose / Exit

    /// <summary>
    /// Set to true as soon as <see cref="Dispose"/>
    /// has been invoked.
    /// </summary>
    public bool IsDisposed { get; private set; }


    /// <summary>
    /// Checks if the object has been disposed and
    /// raises a <see cref="ObjectDisposedException"/> in case
    /// the <see cref="IsDisposed"/> flag is true.
    /// </summary>
    private void EnsureNotDisposed()
    {
      if (IsDisposed) throw new ObjectDisposedException(Name ?? GetType().FullName);
    }


    /// <summary>
    /// Disposes the class if the application exits.
    /// </summary>
    private void OnExit(object sender, EventArgs e)
    {
      Dispose();
    }


    /// <summary>
    /// This destructor will run only if the <see cref="Dispose()"/>
    /// method does not get called. This gives this base class the
    /// opportunity to finalize.
    /// <para>
    /// Important: Do not provide destructors in types derived from
    /// this class.
    /// </para>
    /// </summary>
    ~TaskbarIcon()
    {
      Dispose(false);
    }


    /// <summary>
    /// Disposes the object.
    /// </summary>
    /// <remarks>This method is not virtual by design. Derived classes
    /// should override <see cref="Dispose(bool)"/>.
    /// </remarks>
    public void Dispose()
    {
      Dispose(true);

      // This object will be cleaned up by the Dispose method.
      // Therefore, you should call GC.SupressFinalize to
      // take this object off the finalization queue 
      // and prevent finalization code for this object
      // from executing a second time.
      GC.SuppressFinalize(this);
    }


    /// <summary>
    /// Closes the tray and releases all resources.
    /// </summary>
    /// <summary>
    /// <c>Dispose(bool disposing)</c> executes in two distinct scenarios.
    /// If disposing equals <c>true</c>, the method has been called directly
    /// or indirectly by a user's code. Managed and unmanaged resources
    /// can be disposed.
    /// </summary>
    /// <param name="disposing">If disposing equals <c>false</c>, the method
    /// has been called by the runtime from inside the finalizer and you
    /// should not reference other objects. Only unmanaged resources can
    /// be disposed.</param>
    /// <remarks>Check the <see cref="IsDisposed"/> property to determine whether
    /// the method has already been called.</remarks>
    private void Dispose(bool disposing)
    {
      //don't do anything if the component is already disposed
      if (IsDisposed || !disposing) return;

      lock (this)
      {
        IsDisposed = true;

        //deregister application event listener
        Application.Current.Exit -= OnExit;

        //stop timer
        singleClickTimer.Dispose();

        //dispose message sink
        messageSink.Dispose();

        //remove icon
        RemoveTaskbarIcon();
      }
    }

    #endregion
  }
}