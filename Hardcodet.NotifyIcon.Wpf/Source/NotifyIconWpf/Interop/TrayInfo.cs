﻿// Some interop code taken from Mike Marshall's AnyForm

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
            int space = 2;
            var info = new AppBarInfo();
            info.GetSystemTaskBarPosition();

            Rectangle rcWorkArea = info.WorkArea;

            int x = 0, y = 0;
            switch (info.Edge)
            {
                case AppBarInfo.ScreenEdge.Left:
                    x = rcWorkArea.Right + space;
                    y = rcWorkArea.Bottom;
                    break;
                case AppBarInfo.ScreenEdge.Bottom:
                    x = rcWorkArea.Right;
                    y = rcWorkArea.Bottom - rcWorkArea.Height - space;
                    break;
                case AppBarInfo.ScreenEdge.Top:
                    x = rcWorkArea.Right;
                    y = rcWorkArea.Top + rcWorkArea.Height + space;
                    break;
                case AppBarInfo.ScreenEdge.Right:
                    x = rcWorkArea.Right - rcWorkArea.Width - space;
                    y = rcWorkArea.Bottom;
                    break;
            }

            return GetDeviceCoordinates(new Point {X = x, Y = y});
        }

        /// <summary>
        /// Recalculates OS coordinates in order to support WPFs coordinate
        /// system if OS scaling (DPIs) is not 100%.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Point GetDeviceCoordinates(Point point)
        {
          return new Point() { X = (int)(point.X / SystemInfo.DpiXFactor), Y = (int)(point.Y / SystemInfo.DpiYFactor) };
        }
    }


    public class AppBarInfo
    {
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("shell32.dll")]
        private static extern uint SHAppBarMessage(uint dwMessage, ref APPBARDATA data);

        [DllImport("user32.dll")]
        private static extern int SystemParametersInfo(uint uiAction, uint uiParam,
            IntPtr pvParam, uint fWinIni);


        private const int ABE_BOTTOM = 3;
        private const int ABE_LEFT = 0;
        private const int ABE_RIGHT = 2;
        private const int ABE_TOP = 1;

        private const int ABM_GETTASKBARPOS = 0x00000005;

        // SystemParametersInfo constants
        private const uint SPI_GETWORKAREA = 0x0030;

        private APPBARDATA m_data;

        public ScreenEdge Edge
        {
            get { return (ScreenEdge) m_data.uEdge; }
        }

        public Rectangle WorkArea
        {
          get { return GetRectangle(m_data.rc); }
        }

        private Rectangle GetRectangle(RECT rc)
        {
            return new Rectangle(rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top);
        }     

        public void GetPosition(string strClassName, string strWindowName)
        {
            m_data = new APPBARDATA();
            m_data.cbSize = (uint) Marshal.SizeOf(m_data.GetType());

            IntPtr hWnd = FindWindow(strClassName, strWindowName);

            if (hWnd != IntPtr.Zero)
            {
                uint uResult = SHAppBarMessage(ABM_GETTASKBARPOS, ref m_data);

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
            public uint cbSize;
            public IntPtr hWnd;
            public uint uCallbackMessage;
            public uint uEdge;
            public RECT rc;
            public int lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
    }
}