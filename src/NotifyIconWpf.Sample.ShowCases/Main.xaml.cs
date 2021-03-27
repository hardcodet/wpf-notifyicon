using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using NotifyIconWpf.Sample.ShowCases.Showcase;
using NotifyIconWpf.Sample.ShowCases.Tutorials;

namespace NotifyIconWpf.Sample.ShowCases
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        public Main()
        {
            InitializeComponent();
            DataContext = this;
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

        private static string Version
        {
            get
            {
                var executingAssembly = Assembly.GetExecutingAssembly();

                // Use assembly version
                string version = executingAssembly.GetName().Version.ToString();

                // Use AssemblyFileVersion if available
                var assemblyFileVersionAttribute = executingAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
                if (!string.IsNullOrEmpty(assemblyFileVersionAttribute?.Version))
                {
                    var assemblyFileVersion = new Version(assemblyFileVersionAttribute.Version);
                    version = assemblyFileVersion.ToString(3);
                }

                return version.Replace("+", " - ");
            }
        }
        public string SampleTitle { get; } = $"WPF NotifyIcon {Version} - Samples";

        private void BtnDeclaration_Click(object sender, RoutedEventArgs e)
        {
            ShowDialog(new SimpleWindowWithNotifyIcon());
        }

        private void BtnInlineToolTip_Click(object sender, RoutedEventArgs e)
        {
            ShowDialog(new InlineToolTipWindow());
        }

        private void BtnToolTipControl_Click(object sender, RoutedEventArgs e)
        {
            ShowDialog(new UserControlToolTipWindow());
        }

        private void BtnPopups_Click(object sender, RoutedEventArgs e)
        {
            ShowDialog(new InlinePopupWindow());
        }

        private void BtnContextMenus_Click(object sender, RoutedEventArgs e)
        {
            ShowDialog(new InlineContextMenuWindow());
        }

        private void BtnBalloons_Click(object sender, RoutedEventArgs e)
        {
            ShowDialog(new BalloonSampleWindow());
        }

        private void BtnCommands_Click(object sender, RoutedEventArgs e)
        {
            ShowDialog(new CommandWindow());
        }

        private void BtnEvents_Click(object sender, RoutedEventArgs e)
        {
            ShowDialog(new EventVisualizerWindow());
        }

        private void BtnDataBinding_Click(object sender, RoutedEventArgs e)
        {
            ShowDialog(new DataBoundToolTipWindow());
        }
		
		private void BtnMvvm_Click(object sender, RoutedEventArgs e)
		{
			ShowDialog(new MvvmSampleWindow());
		}

        private void BtnMainSample_Click(object sender, RoutedEventArgs e)
        {
            var sampleWindow = new ShowcaseWindow
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            sampleWindow.ShowDialog();
        }


        private void OnNavigationRequest(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.ToString(),
                // UseShellExecute is default to false on .NET Core while true on .NET Framework.
                // Only this value is set to true, the url link can be opened.
                UseShellExecute = true
            });
            e.Handled = true;
        }
    }
}