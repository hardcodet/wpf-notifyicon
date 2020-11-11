using System.Windows;

namespace NotifyIconWpf.Sample.ShowCases.Tutorials
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class InlineToolTipWindow : Window
    {
        public InlineToolTipWindow()
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