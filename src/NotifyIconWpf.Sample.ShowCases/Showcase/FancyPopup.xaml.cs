// hardcodet.net NotifyIcon for WPF
// Copyright (c) 2009 - 2022 Philipp Sumi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Contact and Information: http://www.hardcodet.net

using System.Windows;
using System.Windows.Controls;

namespace NotifyIconWpf.Sample.ShowCases.Showcase
{
    /// <summary>
    /// Interaction logic for FancyPopup.xaml
    /// </summary>
    public partial class FancyPopup : UserControl
    {
        #region ClickCount dependency property

        /// <summary>
        /// The number of clicks on the popup button.
        /// </summary>
        public static readonly DependencyProperty ClickCountProperty =
            DependencyProperty.Register(nameof(ClickCount),
                typeof (int),
                typeof (FancyPopup),
                new FrameworkPropertyMetadata(0));

        /// <summary>
        /// A property wrapper for the <see cref="ClickCountProperty"/>
        /// dependency property:<br/>
        /// The number of clicks on the popup button.
        /// </summary>
        public int ClickCount
        {
            get { return (int) GetValue(ClickCountProperty); }
            set { SetValue(ClickCountProperty, value); }
        }

        #endregion

        public FancyPopup()
        {
            InitializeComponent();
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            //just increment a counter - will be displayed on screen
            ClickCount++;
        }
    }
}