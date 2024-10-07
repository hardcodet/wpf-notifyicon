// hardcodet.net NotifyIcon for WPF
// Copyright (c) 2009 - 2022 Philipp Sumi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Contact and Information: http://www.hardcodet.net

using System.Windows;
using System.Windows.Input;

namespace NotifyIconWpf.Sample.Windowless
{
    /// <summary>
    /// Provides bindable properties and commands for the NotifyIcon. In this sample, the
    /// view model is assigned to the NotifyIcon in XAML. Alternatively, the startup routing
    /// in App.xaml.cs could have created this view model, and assigned it to the NotifyIcon.
    /// </summary>
    public class NotifyIconViewModel
    {
        private MainWindow GetMainWindow()
        {
            return Application.Current.MainWindow as MainWindow;
        }

        /// <summary>
        /// Shows a window, if none is already open.
        /// </summary>
        public ICommand ShowWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => this.GetMainWindow() == null,
                    CommandAction = () =>
                    {
                        Application.Current.MainWindow = new MainWindow();
                        Application.Current.MainWindow.Show();
                    }
                };
            }
        }

        /// <summary>
        /// Hides the main window. This command is only enabled if a window is open.
        /// </summary>
        public ICommand HideWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () => this.GetMainWindow()?.Close(),
                    CanExecuteFunc = () => this.GetMainWindow() != null
                };
            }
        }


        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get { return new DelegateCommand { CommandAction = () => Application.Current.Shutdown() }; }
        }
    }
}