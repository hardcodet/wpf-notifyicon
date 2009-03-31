using System;
using System.Runtime.InteropServices;

namespace Hardcodet.Wpf.TaskbarNotification.Interop
{
  /// <summary>
  /// Win32 API imports.
  /// </summary>
  internal static class WinApi
  {
    /// <summary>
    /// Creates, updates or deletes the taskbar icon.
    /// </summary>
    [DllImport("shell32.Dll")]
    public static extern bool Shell_NotifyIcon(NotifyCommand cmd, [In]ref NotifyIconData data);


    /// <summary>
    /// Creates the helper window that receives messages from the taskar icon.
    /// </summary>
    [DllImport("USER32.DLL", EntryPoint = "CreateWindowExW", SetLastError = true)]
    public static extern IntPtr CreateWindowEx(int dwExStyle, [MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
                           [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName, int dwStyle, int x, int y,
                           int nWidth, int nHeight, uint hWndParent, int hMenu, int hInstance,
                           int lpParam);


    /// <summary>
    /// Processes a default windows procedure.
    /// </summary>
    [DllImport("USER32.DLL")]
    public static extern long DefWindowProc(IntPtr hWnd, uint msg, uint wparam, uint lparam);
    
    /// <summary>
    /// Registers the helper window class.
    /// </summary>
    [DllImport("USER32.DLL", EntryPoint = "RegisterClassW", SetLastError = true)]
    public static extern short RegisterClass(ref WindowClass lpWndClass);

    /// <summary>
    /// Registers a listener for a window message.
    /// </summary>
    /// <param name="lpString"></param>
    /// <returns></returns>
    [DllImport("User32.Dll", EntryPoint = "RegisterWindowMessageW")]
    public static extern uint RegisterWindowMessage([MarshalAs(UnmanagedType.LPWStr)] string lpString);

    /// <summary>
    /// Used to destroy the hidden helper window that receives messages from the
    /// taskbar icon.
    /// </summary>
    /// <param name="hWnd"></param>
    /// <returns></returns>
    [DllImport("USER32.DLL", SetLastError = true)]
    public static extern bool DestroyWindow(IntPtr hWnd);


    /// <summary>
    /// Gives focus to a given window.
    /// </summary>
    /// <param name="hWnd"></param>
    /// <returns></returns>
    [DllImport("USER32.DLL")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);


    /// <summary>
    /// Gets the maximum number of milliseconds that can elapse between a
    /// first click and a second click for the OS to consider the
    /// mouse action a double-click.
    /// </summary>
    /// <returns>The maximum amount of time, in milliseconds, that can
    /// elapse between a first click and a second click for the OS to
    /// consider the mouse action a double-click.</returns>
    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern int GetDoubleClickTime();


    /// <summary>
    /// Gets the screen coordinates of the current mouse position.
    /// </summary>
    /// <param name="lpPoint"></param>
    /// <returns></returns>
    [DllImport("USER32.DLL", SetLastError = true)]
    public static extern bool GetCursorPos(ref Point lpPoint);
  }
}