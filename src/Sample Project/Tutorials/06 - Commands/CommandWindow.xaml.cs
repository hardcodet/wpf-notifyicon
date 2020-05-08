using System.Windows;

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