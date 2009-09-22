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
    private TaskbarIcon notifyIcon;

    public Form1()
    {
      InitializeComponent();
    }

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
      notifyIcon = new TaskbarIcon();
      notifyIcon.Icon = Resources.Led;
      notifyIcon.ToolTipText = "Left-click to open popup";
      notifyIcon.Visibility = Visibility.Visible;

      notifyIcon.TrayPopup = new FancyPopup();
    }

    protected override void OnClosed(EventArgs e)
    {
      base.OnClosed(e);

      //close the notify icon
      notifyIcon.Dispose();
    }
  }
}
