using System.Windows;
using System.Windows.Controls.Primitives;

namespace Samples.Tutorials.Balloons
{
    /// <summary>
    /// Interaction logic for BalloonSampleWindow.xaml
    /// </summary>
    public partial class BalloonSampleWindow : Window
    {
        public BalloonSampleWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //clean up notifyicon (would otherwise stay open until application finishes)
            MyNotifyIcon.Dispose();

            base.OnClosing(e);
        }


        private void btnShowCustomBalloon_Click(object sender, RoutedEventArgs e)
        {
            FancyBalloon balloon = new FancyBalloon();
            balloon.BalloonText = "Custom Balloon";

            //show balloon and close it after 4 seconds
            MyNotifyIcon.ShowCustomBalloon(balloon, PopupAnimation.Slide, 4000);
        }

        private void btnHideStandardBalloon_Click(object sender, RoutedEventArgs e)
        {
            MyNotifyIcon.HideBalloonTip();
        }


        private void btnShowStandardBalloon_Click(object sender, RoutedEventArgs e)
        {
            string title = "WPF NotifyIcon";
            string text = "This is a standard balloon";

            MyNotifyIcon.ShowBalloonTip(title, text, MyNotifyIcon.Icon);
        }

        private void btnCloseCustomBalloon_Click(object sender, RoutedEventArgs e)
        {
            MyNotifyIcon.CloseBalloon();
        }
    }
}