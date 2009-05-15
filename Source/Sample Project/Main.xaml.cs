using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Hardcodet.Wpf.TaskbarNotification;
using Samples.Tutorials;
using Samples.Tutorials.Balloons;
using Samples.Tutorials.Commands;
using Samples.Tutorials.ContextMenus;
using Samples.Tutorials.Popups;
using Samples.Tutorials.ToolTips;

namespace Samples
{
  /// <summary>
  /// Interaction logic for Main.xaml
  /// </summary>
  public partial class Main : Window
  {
    public Main()
    {
      InitializeComponent();
    }



    /// <summary>
    /// Sets <see cref="Window.WindowStartupLocation"/> and
    /// <see cref="Window.Owner"/> properties of a dialog that
    /// is about to be displayed.
    /// </summary>
    /// <param name="window">The processed window.</param>
    private void ShowDialog(Window window)
    {
      window.Owner = this;
      window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      window.ShowDialog();
    }

    private void btnDeclaration_Click(object sender, RoutedEventArgs e)
    {
      ShowDialog(new SimpleWindowWithNotifyIcon());
    }


    private void btnInlineToolTip_Click(object sender, RoutedEventArgs e)
    {
      ShowDialog(new InlineToolTipWindow());
    }

    private void btnToolTipControl_Click(object sender, RoutedEventArgs e)
    {
      ShowDialog(new UserControlToolTipWindow());
    }

    private void btnPopups_Click(object sender, RoutedEventArgs e)
    {
      ShowDialog(new InlinePopupWindow());
    }

    private void btnContextMenus_Click(object sender, RoutedEventArgs e)
    {
      ShowDialog(new InlineContextMenuWindow());
    }

    private void btnBalloons_Click(object sender, RoutedEventArgs e)
    {
      ShowDialog(new BalloonSampleWindow());
    }

    private void btnCommands_Click(object sender, RoutedEventArgs e)
    {
      ShowDialog(new CommandWindow());
    }

    private void btnMainSample_Click(object sender, RoutedEventArgs e)
    {
      var sampleWindow = new ShowcaseWindow();

      sampleWindow.Owner = this;
      sampleWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
      sampleWindow.ShowDialog();
    }


    private void OnNavigationRequest(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
      Process.Start(e.Uri.ToString());
      e.Handled = true;
    }




  }
}
