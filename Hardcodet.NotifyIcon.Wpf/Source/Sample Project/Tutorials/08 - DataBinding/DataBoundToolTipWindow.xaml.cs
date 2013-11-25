using System.Windows;

namespace Samples.Tutorials.DataBinding
{
    /// <summary>
    /// Interaction logic for DataBoundToolTipWindow.xaml
    /// </summary>
    public partial class DataBoundToolTipWindow : Window
    {
        public DataBoundToolTipWindow()
        {
            InitializeComponent();
        }


        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //clean up notifyicon (would otherwise stay open until application finishes)
            MyNotifyIcon1.Dispose();
            MyNotifyIcon2.Dispose();

            base.OnClosing(e);
        }
    }
}