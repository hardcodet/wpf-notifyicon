using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Hardcodet.Wpf.TaskbarNotification.Interop
{
  public struct Rect
  {
    public int left;
    public int top;
    public int right;
    public int bottom;
    public override string ToString()
    {
      return "(" + left + ", " + top + ") --> (" + right + ", " + bottom + ")";
    }
  }

  public struct TaskbarInfo
  {
    public int cbSize;
    public IntPtr WindowHandle;
    public int uCallbackMessage;
    public TaskbarPosition Position;
    public Rect Rectangle;
    public IntPtr lParam;
  }



  public enum TaskbarPosition
  {
    Left = 0,
    Top,
    Right,
    Bottom
  }




  /// <summary>
  /// Locates the position of the tray area.
  /// </summary>
  public class TrayLocator
  {
    public enum ABMsg
    {
      ABM_NEW = 0,
      ABM_REMOVE = 1,
      ABM_QUERYPOS = 2,
      ABM_SETPOS = 3,
      ABM_GETSTATE = 4,
      ABM_GETTASKBARPOS = 5,
      ABM_ACTIVATE = 6,
      ABM_GETAUTOHIDEBAR = 7,
      ABM_SETAUTOHIDEBAR = 8,
      ABM_WINDOWPOSCHANGED = 9,
      ABM_SETSTATE = 10
    }

    public enum ABNotify
    {
      ABN_STATECHANGE = 0,
      ABN_POSCHANGED,
      ABN_FULLSCREENAPP,
      ABN_WINDOWARRANGE
    }

    [DllImport("shell32.dll", EntryPoint = "SHAppBarMessage", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
    public static extern int SHAppBarMessage(int dwMessage, ref TaskbarInfo pData);


    /// <summary>
    /// Determines the current location of the taskbar.
    /// </summary>
    /// <returns></returns>
    public static TaskbarInfo GetTaskbarInformation()
    {
      TaskbarInfo tbInfo = new TaskbarInfo();
      tbInfo.cbSize = Marshal.SizeOf(tbInfo);
      
      //retrieve the bounding rectangle of the Windows taskbar.
      SHAppBarMessage((int)ABMsg.ABM_GETTASKBARPOS, ref tbInfo);

      return tbInfo;
    }

  }
}