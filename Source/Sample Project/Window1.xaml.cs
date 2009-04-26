using System.Windows;
using System.Windows.Controls.Primitives;
using Hardcodet.Wpf.TaskbarNotification;

namespace Sample_Project
{
  /// <summary>
  /// Interaction logic for Window1.xaml
  /// </summary>
  public partial class Window1 : Window
  {
    public Window1()
    {
      InitializeComponent();

      
      Loaded += delegate
                  {
                    //show balloon at startup, pointing to the icon
                    showBalloonTip_Click(null, null);
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
  }
}