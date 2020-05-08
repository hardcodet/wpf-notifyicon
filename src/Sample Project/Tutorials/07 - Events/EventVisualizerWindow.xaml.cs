using System.Windows;

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