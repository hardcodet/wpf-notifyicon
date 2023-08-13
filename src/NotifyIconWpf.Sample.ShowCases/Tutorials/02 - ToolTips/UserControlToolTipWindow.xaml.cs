// hardcodet.net NotifyIcon for WPF
// Copyright (c) 2009 - 2022 Philipp Sumi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Contact and Information: http://www.hardcodet.net

using System.Windows;

namespace NotifyIconWpf.Sample.ShowCases.Tutorials
{
    /// <summary>
    /// Interaction logic for UserControlToolTipWindow.xaml
    /// </summary>
    public partial class UserControlToolTipWindow : Window
    {
        public UserControlToolTipWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //clean up notifyicon (would otherwise stay open until application finishes)
            MyNotifyIcon.Dispose();

            base.OnClosing(e);
        }
    }
}