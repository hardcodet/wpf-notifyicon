// hardcodet.net NotifyIcon for WPF
// Copyright (c) 2009 - 2022 Philipp Sumi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Contact and Information: http://www.hardcodet.net

using System.Windows;
using System.Windows.Input;

namespace NotifyIconWpf.Sample.ShowCases.Commands
{
    /// <summary>
    /// Hides the main window.
    /// </summary>
    public class HideSampleWindowCommand : CommandBase<HideSampleWindowCommand>
    {
        public override void Execute(object parameter)
        {
            GetTaskbarWindow(parameter).Hide();
            CommandManager.InvalidateRequerySuggested();
        }


        public override bool CanExecute(object parameter)
        {
            Window win = GetTaskbarWindow(parameter);
            return win != null && win.IsVisible;
        }
    }
}