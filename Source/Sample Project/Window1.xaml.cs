using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
    }

    private void OnClick(object sender, RoutedEventArgs e)
    {
      if (tb.Visibility == System.Windows.Visibility.Visible)
      {
        tb.Visibility = System.Windows.Visibility.Collapsed;
      }
      else
      {
        tb.Visibility = Visibility.Visible;
      }
    }
  }
}
