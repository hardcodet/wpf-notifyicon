// hardcodet.net NotifyIcon for WPF
// Copyright (c) 2009 - 2022 Philipp Sumi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Contact and Information: http://www.hardcodet.net

using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;

namespace NotifyIconWpf.Sample.WindowsForms
{
    public partial class Form1 : Form
    {
        private TaskbarIcon notifyIcon;

        private static BitmapSource GetSourceForOnRender(string name)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            var bitmap = new BitmapImage();
            using var stream = assembly.GetManifestResourceStream(name);
            bitmap.BeginInit();
            bitmap.StreamSource = stream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            return bitmap;
        }

        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var contextMenu = new System.Windows.Controls.ContextMenu()
            {
                Items =
                {
                    new System.Windows.Controls.MenuItem()
                    {
                        Header = "Menu 1",
                        Icon = new System.Windows.Controls.Image
                        {
                            Source = GetSourceForOnRender("NotifyIconWpf.Sample.WindowsForms.Icon.Bulb.ico")
                        }
                    },
                    new System.Windows.Controls.MenuItem()
                    {
                        Header = "Menu 2",
                        Icon = new System.Windows.Controls.Image
                        {
                            Source = GetSourceForOnRender("NotifyIconWpf.Sample.WindowsForms.Icon.Computers.ico")
                        }
                    },
                }
            };

            notifyIcon = new TaskbarIcon
            {
                IconSource = GetSourceForOnRender("NotifyIconWpf.Sample.WindowsForms.Images.Preferences.png"),
                ToolTipText = "Left-click to open popup",
                Visibility = Visibility.Visible,
                TrayPopup = new FancyPopup(),
                ContextMenu = contextMenu
            };
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            //the notify icon only closes automatically on WPF applications
            //-> dispose the notify icon manually
            notifyIcon.Dispose();
        }
    }
}