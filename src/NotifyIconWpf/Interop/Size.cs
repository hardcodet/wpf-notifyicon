﻿using System.Runtime.InteropServices;

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