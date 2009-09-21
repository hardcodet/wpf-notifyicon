using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using Hardcodet.Wpf.TaskbarNotification;
using Samples;
using WindowsFormsSample.Properties;

namespace WindowsFormsSample
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
      TaskbarIcon tb = new TaskbarIcon();
      tb.Icon = Resources.Led;
      tb.Visibility = Visibility.Visible;

      tb.TrayPopup = new FancyPopup();
    }
  }
}
