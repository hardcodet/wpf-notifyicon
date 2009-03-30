using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Hardcodet.Wpf.TaskbarNotification.Interop;
using Rect=Hardcodet.Wpf.TaskbarNotification.Interop.Rect;

namespace Hardcodet.Wpf.TaskbarNotification
{
  internal class MyClass
  {
    public void Test()
    {
      TaskbarIcon icon = new TaskbarIcon();
      icon.Icon = Properties.Resources.DefaultTrayIcon;
      Console.Out.WriteLine("DISPLAY NOW...");
      Thread.CurrentThread.Join(1500);
      //icon.ShowBalloonTip("some title", "hello world", Properties.Resources.DefaultTrayIcon);
      //Console.Out.WriteLine("status = {0}", status);
      Thread.CurrentThread.Join(5000);
    }

    public void Test2()
    {
      var tbInfo = TrayLocator.GetTaskbarInformation();
      var w = new Window();
      w.Background = Brushes.Red;
      w.WindowStyle = WindowStyle.None;
      
      Rect rect = tbInfo.Rectangle;
      w.Width = Math.Max(20, rect.right - rect.left);
      w.Height = Math.Max(20, rect.bottom - rect.top);
      w.Left = rect.left;
      w.Top = rect.top - 100;
      w.ShowDialog();
    }
  }

  /// <summary>
  /// Represent a taskbar icon that sits in the system
  /// tray.
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
    /// Indicates whether the taskbar icon has been created or not.
    /// </summary>
    public bool IsTaskbarIconCreated { get; set; }


    /// <summary>
    /// Indicates whether custom tooltips are supported.
    /// </summary>
    public bool SupportsCustomToolTips
    {
      get { return messageSink.Version == NotifyIconVersion.Vista; }
    }


    /// <summary>
    /// Inits the taskbar icon and registers a message listener
    /// in order to receive events from the taskbar area.
    /// </summary>
    public TaskbarIcon()
    {
      //do nothing if in design mode
      if (Util.IsDesignMode)
      {
        messageSink = WindowMessageSink.CreateEmpty();
      }
      else
      {
        //create message sink that receives window messages
        messageSink = new WindowMessageSink(NotifyIconVersion.Win95); 
      }

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

      switch(me)
      {
        case MouseEvent.MouseMove:
          break;
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
          break;
        case MouseEvent.IconMiddleMouseUp:
          break;
        case MouseEvent.IconDoubleClick:
          
          //cancel single click timer
          singleClickTimer.Change(Timeout.Infinite, Timeout.Infinite);

          break;
        case MouseEvent.BalloonToolTipClicked:
          break;
        default:
          throw new ArgumentOutOfRangeException("me", "Missing handler for mouse event flag: " + me);
          
      }


      //show popup, if requested
      if (me.IsMatch(PopupActivation))
      {
        if (me == MouseEvent.IconLeftMouseUp)
        {
          //show popup once we are sure it's not a double click
          delayedTimerAction = ShowTrayPopup;
          singleClickTimer.Change(WinApi.GetDoubleClickTime(), Timeout.Infinite);
        }
        else
        {
          //show popup immediately
          ShowTrayPopup();
        }
      }


      //show context menu, if requested
      if (me.IsMatch(MenuActivation))
      {
        if (me == MouseEvent.IconLeftMouseUp)
        {
          //show context menu once we are sure it's not a double click
          delayedTimerAction = ShowContextMenu;
          singleClickTimer.Change(WinApi.GetDoubleClickTime(), Timeout.Infinite);
        }
        else
        {
          //show context menu immediately
          ShowContextMenu();
        }
      }
    }



    private void OnToolTipChange(bool visible)
    {
      //if we have a custom tooltip, show it now
      if (ToolTip == null) return;

      ToolTip tt = (ToolTip)ToolTip;
      tt.IsOpen = visible;
    }


    private void OnBalloonToolTipChanged(bool visible)
    {
      //TODO just raise event
    }



    /// <summary>
    /// Recreates the taskbar icon if the whole taskbar was
    /// recreated (e.g. because Explorer was shut down).
    /// </summary>
    private void OnTaskbarCreated()
    {
      IsTaskbarIconCreated = false;
      CreateTaskbarIcon();
    }






    #region create / remove taskbar icon

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

        RemoveTaskbarIcon();
      }
    }

    #endregion
  }
}
