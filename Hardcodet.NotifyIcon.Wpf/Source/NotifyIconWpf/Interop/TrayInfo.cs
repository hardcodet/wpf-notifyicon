// Some interop code taken from Mike Marshall's AnyForm

using System;
using System.Drawing;
using System.Runtime.InteropServices;


namespace Hardcodet.Wpf.TaskbarNotification.Interop
{
    /// <summary>
    /// Resolves the current tray position.
    /// </summary>
    public static class TrayInfo
    {
        /// <summary>
        /// Gets the position of the system tray.
        /// </summary>
        /// <returns>Tray coordinates.</returns>
        public static Point GetTrayLocation()
        {
            var info = new AppBarInfo();
            info.GetSystemTaskBarPosition();

            Rectangle rcWorkArea = info.WorkArea;

            int x = 0, y = 0;
            if (info.Edge == AppBarInfo.ScreenEdge.Left)
            {
                x = rcWorkArea.Left + 2;
                y = rcWorkArea.Bottom;
            }
            else if (info.Edge == AppBarInfo.ScreenEdge.Bottom)
            {
                x = rcWorkArea.Right;
                y = rcWorkArea.Bottom;
            }
            else if (info.Edge == AppBarInfo.ScreenEdge.Top)
            {
                x = rcWorkArea.Right;
                y = rcWorkArea.Top;
            }
            else if (info.Edge == AppBarInfo.ScreenEdge.Right)
            {
                x = rcWorkArea.Right;
                y = rcWorkArea.Bottom;
            }

            return new Point {X = x, Y = y};
        }
    }


    internal class AppBarInfo
    {
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(String lpClassName, String lpWindowName);

        [DllImport("shell32.dll")]
        private static extern UInt32 SHAppBarMessage(UInt32 dwMessage, ref APPBARDATA data);

        [DllImport("user32.dll")]
        private static extern Int32 SystemParametersInfo(UInt32 uiAction, UInt32 uiParam,
            IntPtr pvParam, UInt32 fWinIni);


        private const int ABE_BOTTOM = 3;
        private const int ABE_LEFT = 0;
        private const int ABE_RIGHT = 2;
        private const int ABE_TOP = 1;

        private const int ABM_GETTASKBARPOS = 0x00000005;

        // SystemParametersInfo constants
        private const UInt32 SPI_GETWORKAREA = 0x0030;

        private APPBARDATA m_data;

        public ScreenEdge Edge
        {
            get { return (ScreenEdge) m_data.uEdge; }
        }


        public Rectangle WorkArea
        {
            get
            {
                Int32 bResult = 0;
                var rc = new RECT();
                IntPtr rawRect = Marshal.AllocHGlobal(Marshal.SizeOf(rc));
                bResult = SystemParametersInfo(SPI_GETWORKAREA, 0, rawRect, 0);
                rc = (RECT) Marshal.PtrToStructure(rawRect, rc.GetType());

                if (bResult == 1)
                {
                    Marshal.FreeHGlobal(rawRect);
                    return new Rectangle(rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top);
                }

                return new Rectangle(0, 0, 0, 0);
            }
        }


        public void GetPosition(string strClassName, string strWindowName)
        {
            m_data = new APPBARDATA();
            m_data.cbSize = (UInt32) Marshal.SizeOf(m_data.GetType());

            IntPtr hWnd = FindWindow(strClassName, strWindowName);

            if (hWnd != IntPtr.Zero)
            {
                UInt32 uResult = SHAppBarMessage(ABM_GETTASKBARPOS, ref m_data);

                if (uResult != 1)
                {
                    throw new Exception("Failed to communicate with the given AppBar");
                }
            }
            else
            {
                throw new Exception("Failed to find an AppBar that matched the given criteria");
            }
        }


        public void GetSystemTaskBarPosition()
        {
            GetPosition("Shell_TrayWnd", null);
        }


        public enum ScreenEdge
        {
            Undefined = -1,
            Left = ABE_LEFT,
            Top = ABE_TOP,
            Right = ABE_RIGHT,
            Bottom = ABE_BOTTOM
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct APPBARDATA
        {
            public UInt32 cbSize;
            public IntPtr hWnd;
            public UInt32 uCallbackMessage;
            public UInt32 uEdge;
            public RECT rc;
            public Int32 lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public Int32 left;
            public Int32 top;
            public Int32 right;
            public Int32 bottom;
        }
    }
}