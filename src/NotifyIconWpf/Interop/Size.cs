// hardcodet.net NotifyIcon for WPF
// Copyright (c) 2009 - 2022 Philipp Sumi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Contact and Information: http://www.hardcodet.net

using System.Runtime.InteropServices;

namespace Hardcodet.Wpf.TaskbarNotification.Interop
{
    /// <summary>
    /// Win API struct representing a size with width and height.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Size
    {
        /// <summary>
        /// Width.
        /// </summary>
        public int Width;
        /// <summary>
        /// Height.
        /// </summary>
        public int Height;
    }
}