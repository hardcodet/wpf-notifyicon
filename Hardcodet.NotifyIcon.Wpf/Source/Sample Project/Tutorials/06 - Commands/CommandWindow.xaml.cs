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

namespace Samples.Tutorials.Commands
{
    /// <summary>
    /// Interaction logic for CommandWindow.xaml
    /// </summary>
    public partial class CommandWindow : Window
    {
        public CommandWindow()
        {
            InitializeComponent();
        }


        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //clean up notifyicon (would otherwise stay open until application finishes)
            CustomCommandNotifyIcon.Dispose();
            RoutedCommandNotifyIcon.Dispose();

            base.OnClosing(e);
        }
    }
}