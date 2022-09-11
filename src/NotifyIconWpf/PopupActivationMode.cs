// hardcodet.net NotifyIcon for WPF
// Copyright (c) 2009 - 2022 Philipp Sumi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Contact and Information: http://www.hardcodet.net

namespace Hardcodet.Wpf.TaskbarNotification
{
    /// <summary>
    /// Defines flags that define when a popup
    /// is being displyed.
    /// </summary>
    public enum PopupActivationMode
    {
        /// <summary>
        /// The item is displayed if the user clicks the
        /// tray icon with the left mouse button.
        /// </summary>
        LeftClick,

        /// <summary>
        /// The item is displayed if the user clicks the
        /// tray icon with the right mouse button.
        /// </summary>
        RightClick,

        /// <summary>
        /// The item is displayed if the user double-clicks the
        /// tray icon.
        /// </summary>
        DoubleClick,

        /// <summary>
        /// The item is displayed if the user clicks the
        /// tray icon with the left or the right mouse button.
        /// </summary>
        LeftOrRightClick,

        /// <summary>
        /// The item is displayed if the user clicks the
        /// tray icon with the left mouse button or if a
        /// double-click is being performed.
        /// </summary>
        LeftOrDoubleClick,

        /// <summary>
        /// The item is displayed if the user clicks the
        /// tray icon with the middle mouse button.
        /// </summary>
        MiddleClick,

        /// <summary>
        /// The item is displayed whenever a click occurs.
        /// </summary>
        All,

        /// <summary>
        /// The item is displayed manually from code.
        /// </summary>
        None
    }
}