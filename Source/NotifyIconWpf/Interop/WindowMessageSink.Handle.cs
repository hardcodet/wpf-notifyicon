using System;
using System.ComponentModel;

namespace Hardcodet.Wpf.TaskbarNotification.Interop
{
  /// <summary>
  /// Provides low level code that is used to receive
  /// window messages without having a window that
  /// prevents a WPF application from shutting down
  /// properly.
  /// </summary>
  public partial class WindowMessageSink
  {
    /// <summary>
    /// Window class ID.
    /// </summary>
    private string WindowId;
    
    /// <summary>
    /// Handle for the message window.
    /// </summary
    internal IntPtr MessageWindowHandle { get; private set; }
    
    /// <summary>
    /// The ID of the message that is being received if the
    /// taskbar is (re)started.
    /// </summary>
    private uint taskbarRestartMessageId;
    
    /// <summary>
    /// A delegate that processes messages of the hidden
    /// native window that receives window messages. Storing
    /// this reference makes sure we don't loose our reference
    /// to the message window.
    /// </summary>
    private WindowProcedureHandler messageHandler;

    /// <summary>
    /// Creates the helper message window that is used
    /// to receive messages from the taskbar icon.
    /// </summary>
    private void CreateMessageWindow()
    {
      WindowId = "WPFTaskbarIcon_" + Guid.NewGuid().ToString();

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



    /// <summary>
    /// Callback method that receives messages from the taskbar area.
    /// </summary>
    private long OnWindowMessageReceived(IntPtr hwnd, uint messageId, uint wparam, uint lparam)
    {      
      if (messageId == taskbarRestartMessageId)
      {
        //recreate the icon if the taskbar was restarted
        //TODO refresh icon
      }

      ProcessWindowMessage(messageId, wparam, lparam);

      //handle mouse clicks...

      // Pass the message to the default window procedure
      return WinApi.DefWindowProc(hwnd, messageId, wparam, lparam);
    }

  }
}