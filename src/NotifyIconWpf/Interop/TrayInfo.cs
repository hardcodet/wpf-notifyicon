// hardcodet.net NotifyIcon for WPF
// Copyright (c) 2009 - 2022 Philipp Sumi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Contact and Information: http://www.hardcodet.net
// Some interop code taken from Mike Marshall's AnyForm

using System.Drawing;

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
        public static Point GetTrayLocation(int space = 2)
        {
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
        /// <param name="point">Point</param>
        /// <returns>Point</returns>
        public static Point GetDeviceCoordinates(Point point) => point.ScaleWithDpi();
    }
}