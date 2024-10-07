// hardcodet.net NotifyIcon for WPF
// Copyright (c) 2009 - 2022 Philipp Sumi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Contact and Information: http://www.hardcodet.net

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace NotifyIconWpf.Sample.ShowCases.Tutorials
{
    public class MvvmSampleViewModel : INotifyPropertyChanged
    {
        private DispatcherTimer timer;

        public string Timestamp
        {
            get { return DateTime.Now.ToLongTimeString(); }
        }


        public MvvmSampleViewModel()
        {
            timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, OnTimerTick, Application.Current.Dispatcher);
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            //fire a property change event for the timestamp
            Application.Current.Dispatcher.BeginInvoke(new Action(() => OnPropertyChanged("Timestamp")));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
