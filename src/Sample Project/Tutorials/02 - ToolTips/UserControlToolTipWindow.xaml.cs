using System.Windows;

namespace Samples.Tutorials.ToolTips
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