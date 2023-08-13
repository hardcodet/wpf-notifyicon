// hardcodet.net NotifyIcon for WPF
// Copyright (c) 2009 - 2022 Philipp Sumi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Contact and Information: http://www.hardcodet.net

namespace Hardcodet.Wpf.TaskbarNotification.Interop
{
    /// <summary>
    /// Event flags for clicked events.
    /// </summary>
    public enum MouseEvent
    {
        /// <summary>
        /// The mouse was moved withing the
        /// taskbar icon's area.
        /// </summary>
        MouseMove,

        /// <summary>
        /// The right mouse button was clicked.
        /// </summary>
        IconRightMouseDown,

        /// <summary>
        /// The left mouse button was clicked.
        /// </summary>
        IconLeftMouseDown,

        /// <summary>
        /// The right mouse button was released.
        /// </summary>
        IconRightMouseUp,

        /// <summary>
        /// The left mouse button was released.
        /// </summary>
        IconLeftMouseUp,

        /// <summary>
        /// The middle mouse button was clicked.
        /// </summary>
        IconMiddleMouseDown,

        /// <summary>
        /// The middle mouse button was released.
        /// </summary>
        IconMiddleMouseUp,

        /// <summary>
        /// The taskbar icon was double clicked.
        /// </summary>
        IconDoubleClick,

        /// <summary>
        /// The balloon tip was clicked.
        /// </summary>
        BalloonToolTipClicked
    }
}