using System;
using System.Collections.Generic;
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

namespace Samples.Tutorials.Events
{
  /// <summary>
  /// Interaction logic for EventVisualizerWindow.xaml
  /// </summary>
  public partial class EventVisualizerWindow : Window
  {
    public EventVisualizerWindow()
    {
      InitializeComponent();
    }


    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
      //clean up notifyicon (would otherwise stay open until application finishes)
      notifyIcon.Dispose();

      base.OnClosing(e);
    }
  }
}