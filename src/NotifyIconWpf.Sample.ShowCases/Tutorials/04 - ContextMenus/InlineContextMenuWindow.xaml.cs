using System.Windows;

namespace NotifyIconWpf.Sample.ShowCases.Tutorials
{
    /// <summary>
    /// Interaction logic for InlineContextMenuWindow.xaml
    /// </summary>
    public partial class InlineContextMenuWindow : Window
    {
        public InlineContextMenuWindow()
        {
            InitializeComponent();
        }


        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //clean up notifyicon (would otherwise stay open until application finishes)
            MyNotifyIcon.Dispose();

            base.OnClosing(e);
        }

        private void MyNotifyIcon_TrayContextMenuOpen(object sender, RoutedEventArgs e)
        {
            OpenEventCounter.Text = (int.Parse(OpenEventCounter.Text) + 1).ToString();
        }

        private void MyNotifyIcon_PreviewTrayContextMenuOpen(object sender, RoutedEventArgs e)
        {
            //marking the event as handled suppresses the context menu
            e.Handled = (bool) SuppressContextMenu.IsChecked;

            PreviewOpenEventCounter.Text = (int.Parse(PreviewOpenEventCounter.Text) + 1).ToString();
        }
    }
}