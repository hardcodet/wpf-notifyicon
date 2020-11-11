using System;
using System.Windows;
using System.Windows.Forms;
using Hardcodet.Wpf.TaskbarNotification;
using NotifyIconWpf.Sample.WindowsForms.Properties;

namespace NotifyIconWpf.Sample.WindowsForms
{
    public partial class Form1 : Form
    {
        private TaskbarIcon notifyIcon;

        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            notifyIcon = new TaskbarIcon
            {
                Icon = Resources.Led,
                ToolTipText = "Left-click to open popup",
                Visibility = Visibility.Visible,
                TrayPopup = new FancyPopup()
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