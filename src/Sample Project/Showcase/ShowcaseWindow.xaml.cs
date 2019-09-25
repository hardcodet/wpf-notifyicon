using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;
using Hardcodet.Wpf.TaskbarNotification;

namespace Samples
{
    /// <summary>
    /// Interaction logic for ShowcaseWindow.xaml
    /// </summary>
    public partial class ShowcaseWindow : Window
    {
        public ShowcaseWindow()
        {
            InitializeComponent();


            Loaded += delegate
            {
                //show balloon at startup
                var balloon = new WelcomeBalloon();
                tb.ShowCustomBalloon(balloon, PopupAnimation.Slide, 12000);
            };
        }


        /// <summary>
        /// Displays a balloon tip.
        /// </summary>
        private void showBalloonTip_Click(object sender, RoutedEventArgs e)
        {
            string title = txtBalloonTitle.Text;
            string message = txtBalloonText.Text;

            if (rbCustomIcon.IsChecked == true)
            {
                //just display the icon on the tray
                var icon = tb.Icon;
                tb.ShowBalloonTip(title, message, icon);
            }
            else
            {
                BalloonIcon bi = rbInfo.IsChecked == true ? BalloonIcon.Info : BalloonIcon.Error;
                tb.ShowBalloonTip(title, message, bi);
            }
        }

        private void hideBalloonTip_Click(object sender, RoutedEventArgs e)
        {
            tb.HideBalloonTip();
        }


        /// <summary>
        /// Resets the tooltip.
        /// </summary>
        private void removeToolTip_Click(object sender, RoutedEventArgs e)
        {
            tb.TrayToolTip = null;
        }


        private void showCustomBalloon_Click(object sender, RoutedEventArgs e)
        {
            FancyBalloon balloon = new FancyBalloon();
            balloon.BalloonText = customBalloonTitle.Text;
            //show and close after 2.5 seconds
            tb.ShowCustomBalloon(balloon, PopupAnimation.Slide, 5000);
        }

        private void hideCustomBalloon_Click(object sender, RoutedEventArgs e)
        {
            tb.CloseBalloon();
        }


        private void OnNavigationRequest(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
            e.Handled = true;
        }


        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //clean up notifyicon (would otherwise stay open until application finishes)
            tb.Dispose();
            base.OnClosing(e);
        }
    }
}