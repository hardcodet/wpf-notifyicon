// hardcodet.net NotifyIcon for WPF
// Copyright (c) 2009 - 2020 Philipp Sumi
// Contact and Information: http://www.hardcodet.net
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the Code Project Open License (CPOL);
// either version 1.0 of the License, or (at your option) any later
// version.
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
// THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE

using System.Windows.Interop;

namespace Hardcodet.Wpf.TaskbarNotification.Interop
{
    /// <summary>
    /// This class is a helper for system information, currently to get the DPI factors
    /// </summary>
    public static class SystemInfo
    {
        static SystemInfo()
        {
            UpdateFactors();
        }

        internal static void UpdateFactors()
        {
            using (var source = new HwndSource(new HwndSourceParameters()))
            {
                if (source.CompositionTarget?.TransformToDevice != null)
                {
                    DpiFactorX = source.CompositionTarget.TransformToDevice.M11;
                    DpiFactorY = source.CompositionTarget.TransformToDevice.M22;
                    return;
                }
            }

            DpiFactorX = DpiFactorY = 1;
        }

        /// <summary>
        /// Returns the DPI X Factor
        /// </summary>
        public static double DpiFactorX { get; private set; } = 1;

        /// <summary>
        /// Returns the DPI Y Factor
        /// </summary>
        public static double DpiFactorY { get; private set; } = 1;
    }
}