using System;
using System.Diagnostics;

namespace Hardcodet.Wpf.TaskbarNotification.Interop
{
  /// <summary>
  /// Receives messages from the taskbar icon through
  /// window messages of an underlying helper window.
  /// </summary>
  public partial class WindowMessageSink : IDisposable
  {

    #region members

    /// <summary>
    /// The ID of messages that are received from the the
    /// taskbar icon.
    /// </summary>
    public const int CallbackMessageId = 0x400;

    /// <summary>
    /// The version of the underlying icon. Defines how
    /// incoming messages are interpreted.
    /// </summary>
    public NotifyIconVersion Version { get; set; }

    #endregion


    #region events

    /// <summary>
    /// The custom tooltip should be closed or hidden.
    /// </summary>
    public event Action<bool> ChangeToolTipStateRequest;

    /// <summary>
    /// Fired in case the user clicked or moved within
    /// the taskbar icon area.
    /// </summary>
    public event Action<MouseEvent> MouseEventReceived;

    /// <summary>
    /// Fired if a balloon ToolTip was either displayed
    /// or closed (indicated by the boolean flag).
    /// </summary>
    public event Action<bool> BallonToolTipChanged;

    /// <summary>
    /// Fired if the taskbar was created. Requires the taskbar
    /// icon to be reset.
    /// </summary>
    public event Action TaskbarCreated;

    #endregion


    #region construction

    /// <summary>
    /// Creates a new message sink that receives message from
    /// a given taskbar icon.
    /// </summary>
    /// <param name="version"></param>
    public WindowMessageSink(NotifyIconVersion version)
    {
      Version = version;
      CreateMessageWindow();
    }


    private WindowMessageSink()
    {
    }


    /// <summary>
    /// Creates a dummy instance that provides an empty
    /// pointer rather than a real window handler.<br/>
    /// Used at design time.
    /// </summary>
    /// <returns></returns>
    internal static WindowMessageSink CreateEmpty()
    {
      return new WindowMessageSink
                   {
                       MessageWindowHandle = IntPtr.Zero,
                       Version = NotifyIconVersion.Vista
                   };
    }

    #endregion


    #region Process Window Messages

    /// <summary>
    /// Processes incoming system messages.
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="wParam"></param>
    /// <param name="lParam"></param>
    /// <returns></returns>
    private void ProcessWindowMessage(uint msg, uint wParam, uint lParam)
    {
      if (msg != CallbackMessageId) return;

      switch (lParam)
      {
        case 0x200:
//          Debug.WriteLine("MOVE");
          MouseEventReceived(MouseEvent.MouseMove);
          break;

        case 0x201:
          Debug.WriteLine("left down 1");
          MouseEventReceived(MouseEvent.IconLeftMouseDown);
          break;

        case 0x202:
          Debug.WriteLine("left up");
          MouseEventReceived(MouseEvent.IconLeftMouseUp);
          break;

        case 0x203:
          Debug.WriteLine("left click 2");
          MouseEventReceived(MouseEvent.IconDoubleClick);
          break;

        case 0x204:
          Debug.WriteLine("right click 1");
          MouseEventReceived(MouseEvent.IconRightMouseDown);
          break;

        case 0x205:
          Console.Out.WriteLine("right mouse up");
          MouseEventReceived(MouseEvent.IconRightMouseUp);
          break;

        case 0x206:
          //double click with right mouse button - do not trigger event
          Debug.WriteLine("right click 2");  
          break;

        case 0x207:
          Debug.WriteLine("middle click 1");
          MouseEventReceived(MouseEvent.IconMiddleMouseDown);
          break;

        case 520:
          Debug.WriteLine("mouse up middle");
          MouseEventReceived(MouseEvent.IconMiddleMouseUp);
          break;

        case 0x209:
          //double click with middle mouse button - do not trigger event
          Debug.WriteLine("middle click 2");
          break;

        case 0x402:
          Debug.WriteLine("balloon shown");
          BallonToolTipChanged(true);
          break;

        case 0x403:
        case 0x404:
          Debug.WriteLine("balloon close");
          BallonToolTipChanged(false);
          break;

        case 0x405:
          Debug.WriteLine("balloon clicked");
          MouseEventReceived(MouseEvent.BalloonToolTipClicked);
          break;

        case 0x406:
          Debug.WriteLine("show custom tooltip");
          ChangeToolTipStateRequest(true);
          break;

        case 0x407:
          Debug.WriteLine("close custom tooltip");
          ChangeToolTipStateRequest(false);
          break;

        default:
          Debug.WriteLine("Unhandled message ID: " + lParam);
          break;
      }

    }

    #endregion



    #region Dispose

    /// <summary>
    /// Set to true as soon as <see cref="Dispose"/>
    /// has been invoked.
    /// </summary>
    public bool IsDisposed { get; private set; }


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
    /// This destructor will run only if the <see cref="Dispose()"/>
    /// method does not get called. This gives this base class the
    /// opportunity to finalize.
    /// <para>
    /// Important: Do not provide destructors in types derived from
    /// this class.
    /// </para>
    /// </summary>
    ~WindowMessageSink()
    {
      Dispose(false);
    }


    /// <summary>
    /// Removes the windows hook that receives window
    /// messages and closes the underlying helper window.
    /// </summary>
    private void Dispose(bool disposing)
    {
      //don't do anything if the component is already disposed
      if (IsDisposed || !disposing) return;
      IsDisposed = true;

      WinApi.DestroyWindow(MessageWindowHandle);
      messageHandler = null;
    }

    #endregion
  }
}