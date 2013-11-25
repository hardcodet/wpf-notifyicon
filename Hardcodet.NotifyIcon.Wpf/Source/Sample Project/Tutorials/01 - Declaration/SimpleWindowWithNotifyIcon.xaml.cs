using System.Windows;

namespace Samples.Tutorials
{
    /// <summary>
    /// Interaction logic for SimpleWindowWithNotifyIcon.xaml
    /// </summary>
    public partial class SimpleWindowWithNotifyIcon : Window
    {
        public SimpleWindowWithNotifyIcon()
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