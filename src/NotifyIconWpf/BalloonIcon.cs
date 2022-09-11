// hardcodet.net NotifyIcon for WPF
// Copyright (c) 2009 - 2022 Philipp Sumi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Contact and Information: http://www.hardcodet.net

namespace Hardcodet.Wpf.TaskbarNotification
{
    ///<summary>
    /// Supported icons for the tray's balloon messages.
    ///</summary>
    public enum BalloonIcon
    {
        /// <summary>
        /// The balloon message is displayed without an icon.
        /// </summary>
        None,

        /// <summary>
        /// An information is displayed.
        /// </summary>
        Info,

        /// <summary>
        /// A warning is displayed.
        /// </summary>
        Warning,

        /// <summary>
        /// An error is displayed.
        /// </summary>
        Error
    }
}