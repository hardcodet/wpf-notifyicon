// hardcodet.net NotifyIcon for WPF
// Copyright (c) 2009 Philipp Sumi
// Contact and Information: http://www.hardcodet.net
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the Code Project Open License (CPOL);
// either version 1.0 of the License, or (at your option) any later
// version.
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
// THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE



using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Hardcodet.Wpf.TaskbarNotification.Interop
{
  /// <summary>
  /// Receives messages from the taskbar icon through
  /// window messages of an underlying helper window.
  /// </summary>
  public class WindowMessageSink : IDisposable
  {

    #region members

    /// <summary>
    /// The ID of messages that are received from the the
    /// taskbar icon.
    /// </summary>
    public const int CallbackMessageId = 0x400;

    /// <summary>
    /// The ID of the message that is being received if the
    /// taskbar is (re)started.
    /// </summary>
    private uint taskbarRestartMessageId;

    /// <summary>
    /// Used to track whether a mouse-up event is just
    /// the aftermath of a double-click and therefore needs
    /// to be suppressed.
    /// </summary>
    private bool isDoubleClick;

    /// <summary>
    /// A delegate that processes messages of the hidden
    /// native window that receives window messages. Storing
    /// this reference makes sure we don't loose our reference
    /// to the message window.
    /// </summary>
    private WindowProcedureHandler messageHandler;

    /// <summary>
    /// Window class ID.
    /// </summary>
    internal string WindowId { get; private set; }

    /// <summary>
    /// Handle for the message window.
    /// </summary
    internal IntPtr MessageWindowHandle { get; private set; }

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
    /// Fired if the taskbar was created or restarted. Requires the taskbar
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


    #region CreateMessageWindow

    /// <summary>
    /// Creates the helper message window that is used
    /// to receive messages from the taskbar icon.
    /// </summary>
    private void CreateMessageWindow()
    {
      //generate a unique ID for the window
      WindowId = "WPFTaskbarIcon_" + DateTime.Now.Ticks;

      //register window message handler
      messageHandler = OnWindowMessageReceived;

      // Create a simple window class which is reference through
      //the messageHandler delegate
      WindowClass wc;

      wc.style = 0;
      wc.lpfnWndProc = messageHandler;
      wc.cbClsExtra = 0;
      wc.cbWndExtra = 0;
      wc.hInstance = IntPtr.Zero;
      wc.hIcon = IntPtr.Zero;
      wc.hCursor = IntPtr.Zero;
      wc.hbrBackground = IntPtr.Zero;
      wc.lpszMenuName = "";
      wc.lpszClassName = WindowId;

      // Register the window class
      WinApi.RegisterClass(ref wc);

      // Get the message used to indicate the taskbar has been restarted
      // This is used to re-add icons when the taskbar restarts
      taskbarRestartMessageId = WinApi.RegisterWindowMessage("TaskbarCreated");

      // Create the message window
      MessageWindowHandle = WinApi.CreateWindowEx(0, WindowId, "", 0, 0, 0, 1, 1, 0, 0, 0, 0);

      if (MessageWindowHandle == IntPtr.Zero)
      {
        throw new Win32Exception();
      }
    }

    #endregion


    #region Handle Window Messages

    /// <summary>
    /// Callback method that receives messages from the taskbar area.
    /// </summary>
    private long OnWindowMessageReceived(IntPtr hwnd, uint messageId, uint wparam, uint lparam)
    {
      if (messageId == taskbarRestartMessageId)
      {
        //recreate the icon if the taskbar was restarted (e.g. due to Win Explorer shutdown)
        TaskbarCreated();
      }

      //forward message
      ProcessWindowMessage(messageId, wparam, lparam);

      // Pass the message to the default window procedure
      return WinApi.DefWindowProc(hwnd, messageId, wparam, lparam);
    }


    /// <summary>
    /// Processes incoming system messages.
    /// </summary>
    /// <param name="msg">Callback ID.</param>
    /// <param name="wParam">If the version is <see cref="NotifyIconVersion.Vista"/>
    /// or higher, this parameter can be used to resolve mouse coordinates.
    /// Currently not in use.</param>
    /// <param name="lParam">Provides information about the event.</param>
    private void ProcessWindowMessage(uint msg, uint wParam, uint lParam)
    {
      if (msg != CallbackMessageId) return;

      switch (lParam)
      {
        case 0x200:
          MouseEventReceived(MouseEvent.MouseMove);
          break;

        case 0x201:
          MouseEventReceived(MouseEvent.IconLeftMouseDown);
          break;

        case 0x202:
          if (!isDoubleClick)
          {
            MouseEventReceived(MouseEvent.IconLeftMouseUp);
          }
          isDoubleClick = false;
          break;

        case 0x203:
          isDoubleClick = true;
          MouseEventReceived(MouseEvent.IconDoubleClick);
          break;

        case 0x204:
          MouseEventReceived(MouseEvent.IconRightMouseDown);
          break;

        case 0x205:
          MouseEventReceived(MouseEvent.IconRightMouseUp);
          break;

        case 0x206:
          //double click with right mouse button - do not trigger event
          break;

        case 0x207:
          MouseEventReceived(MouseEvent.IconMiddleMouseDown);
          break;

        case 520:
          MouseEventReceived(MouseEvent.IconMiddleMouseUp);
          break;

        case 0x209:
          //double click with middle mouse button - do not trigger event
          break;

        case 0x402:
          BallonToolTipChanged(true);
          break;

        case 0x403:
        case 0x404:
          BallonToolTipChanged(false);
          break;

        case 0x405:
          MouseEventReceived(MouseEvent.BalloonToolTipClicked);
          break;

        case 0x406:
          ChangeToolTipStateRequest(true);
          break;

        case 0x407:
          ChangeToolTipStateRequest(false);
          break;

        default:
          Debug.WriteLine("Unhandled NotifyIcon message ID: " + lParam);
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