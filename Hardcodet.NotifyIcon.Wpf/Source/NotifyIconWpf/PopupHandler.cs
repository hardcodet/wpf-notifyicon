using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification.Interop;

namespace Hardcodet.Wpf.TaskbarNotification
{

    public class PopupSetting : DependencyObject
    {
        #region Trigger

        /// <summary>
        /// Trigger Dependency Property
        /// </summary>
        public static readonly DependencyProperty TriggerProperty =
            DependencyProperty.Register("Trigger", typeof(PopupActivationMode), typeof(PopupSetting),
                new FrameworkPropertyMetadata((PopupActivationMode)PopupActivationMode.AnyClick));

        /// <summary>
        /// Gets or sets the Trigger property. This dependency property 
        /// indicates ....
        /// </summary>
        public PopupActivationMode Trigger
        {
            get { return (PopupActivationMode)GetValue(TriggerProperty); }
            set { SetValue(TriggerProperty, value); }
        }

        #endregion


        
        #region PreviewOpen

        /// <summary>
        /// PreviewOpen Routed Event
        /// </summary>
        public static readonly RoutedEvent PreviewOpenEvent = EventManager.RegisterRoutedEvent("PreviewOpen",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PopupSetting));

        /// <summary>
        /// Occurs when ...
        /// </summary>
        public event RoutedEventHandler PreviewOpen
        {
            add { RoutedEventHelper.AddHandler(this, PreviewOpenEvent, value); }
            remove { RoutedEventHelper.RemoveHandler(this, PreviewOpenEvent, value); }
        }

        /// <summary>
        /// A helper method to raise the PreviewOpen event.
        /// </summary>
        protected RoutedEventArgs RaisePreviewOpenEvent()
        {
            return RaisePreviewOpenEvent(this);
        }

        /// <summary>
        /// A static helper method to raise the PreviewOpen event on a target element.
        /// </summary>
        /// <param name="target">UIElement or ContentElement on which to raise the event</param>
        internal static RoutedEventArgs RaisePreviewOpenEvent(DependencyObject target)
        {
            if (target == null) return null;

            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = PreviewOpenEvent;
            RoutedEventHelper.RaiseEvent(target, args);
            return args;
        }

        #endregion
        
    }

    /// <summary>
    /// Maintains a given popup, and optionally tracks activation / closing.
    /// </summary>
    public class PopupHandler
    {
        public FrameworkElement Parent { get; private set; }
        public DispatcherTimer Timer { get; private set; }

        private Action scheduledTimerAction;
        private Popup managedPopup;

        /// <summary>
        /// Indicates whether the popup is being activated if the
        /// user enters the mouse.
        /// </summary>
        public bool ActivateOnMouseEnterPopup { get; set; }

        /// <summary>
        /// Indicates whether the popup is being deactivated if the
        /// user leaves the popup. Can be set to false in order to
        /// keep a popup open indefinitely once the user hovered
        /// over it. Defaults to <c>true</c>.
        /// </summary>
        public bool CloseOnMouseLeavePopup { get; set; }

        public Popup ManagedPopup
        {
            get { return managedPopup; }
            set
            {
                ResetSchedule();

                var oldPopup = managedPopup;
                managedPopup = value;
                InitPopup(oldPopup);
            }
        }

        public Func<bool> PreviewOpenFunc { get; set; }
        public Action PostOpenAction { get; set; }

        public Func<bool> PreviewCloseFunc { get; set; }
        public Action PostCloseAction { get; set; }

        public int OpenPopupDelay { get; set; }
        public int ClosePopupDelay { get; set; }

        public PopupHandler(FrameworkElement parent)
        {
            Parent = parent;
            Timer = new DispatcherTimer(DispatcherPriority.Normal, parent.Dispatcher);
            Timer.Tick += OnTimerElapsed;
            CloseOnMouseLeavePopup = true;
        }


        private void InitPopup(Popup oldPopup)
        {
            if (oldPopup != null)
            {
                oldPopup.MouseEnter -= OnPopupMouseEnter;
                oldPopup.MouseLeave -= OnPopupMouseLeave;
            }

            if (ManagedPopup == null) return;

            ManagedPopup.MouseEnter += OnPopupMouseEnter;
            ManagedPopup.MouseLeave += OnPopupMouseLeave;

            //hook up the popup with the data context of it's associated object
            //in case we have content with bindings
            var binding = new Binding
            {
                Path = new PropertyPath(FrameworkElement.DataContextProperty),
                Mode = BindingMode.OneWay,
                Source = Parent
            };
            BindingOperations.SetBinding(ManagedPopup, FrameworkElement.DataContextProperty, binding);

            //force template application so we can switch visual states
            var fe = ManagedPopup.Child as FrameworkElement;
            if (fe != null)
            {
                fe.ApplyTemplate();
                GoToState("Closed", false);
            }
        }


        public void ShowPopup()
        {
            Debug.WriteLine("show popup request");

            if (ManagedPopup == null) return;

            //if the popup is still open, open it immediatly
            if (ManagedPopup.IsOpen)
            {
                //the popup is already open (maybe was scheduled to close)
                //-> abort schedule, set back to active state
                ResetSchedule();
                GoToState("Showing");
            }
            else
            {
                //validate whether we can open
                bool isHandled = PreviewOpenFunc();
                if (isHandled) return;

                Schedule(OpenPopupDelay, () =>
                {
                    Debug.WriteLine("showing popup");

                    //Open the popup, then start transition
                    ManagedPopup.IsOpen = true;
                    GoToState("Showing");

                    FocusPopup();

                    PostOpenAction();
                });
            }
        }

        /// <summary>
        /// Schedules closing the maintained popup, if it is currently open, using
        /// the configured <see cref="ClosePopupDelay"/>, if set.
        /// </summary>
        public void ScheduleClosePopup()
        {
            Debug.WriteLine("close request");

            if (ManagedPopup == null || !ManagedPopup.IsOpen) return;

            //validate whether we can close
            bool isHandled = PreviewCloseFunc();
            if (isHandled) return;

            //start hiding immediately
            GoToState("Hiding");

            Schedule(ClosePopupDelay, () =>
            {
                Debug.WriteLine("Close request scheduled");
                GoToState("Closed");
                ManagedPopup.IsOpen = false;
                PostCloseAction();
            });
        }

        /// <summary>
        /// Suppresses a scheduled hiding of the popup, and transitions the content
        /// back into active state.
        /// </summary>
        private void OnPopupMouseEnter(object sender, MouseEventArgs e)
        {
            if (!ActivateOnMouseEnterPopup) return;

            Debug.WriteLine("popup enter");

            //the popup is still open - just supress any scheduled action
            ResetSchedule();
            GoToState("Active");
        }


        /// <summary>
        /// Schedules hiding of the control if the user moves away from an interactive popup.
        /// </summary>
        private void OnPopupMouseLeave(object sender, MouseEventArgs e)
        {
            if (!CloseOnMouseLeavePopup) return;

            Debug.WriteLine("mouse leave");
            ScheduleClosePopup();
        }

        /// <summary>
        /// Performs a pending action.
        /// </summary>
        private void OnTimerElapsed(object sender, EventArgs eventArgs)
        {
            lock (Timer)
            {
                Timer.Stop();

                var action = scheduledTimerAction;
                scheduledTimerAction = null;

                //cache action and set timer action to null before invoking it, not the other way around
                //(that action could cause the timer action to be reassigned)
                if (action != null)
                {
                    action();
                }
            }
        }

        private void ResetSchedule()
        {
            lock (Timer)
            {
                Timer.Stop();
                scheduledTimerAction = null;
            }
        }

        /// <summary>
        /// Schedules an action to be executed on the next timer tick. Resets the timer
        /// and replaces any other pending action.
        /// </summary>
        /// <remarks>
        /// Customize this if you need custom delays to show / hide / fade tooltips by
        /// simply changing the timer interval depending on the state change.
        /// </remarks>
        private void Schedule(int delay, Action action)
        {
            lock (Timer)
            {
                Timer.Stop();

                if (delay == 0)
                {
                    //if there is no delay, execute action immediately
                    scheduledTimerAction = null;
                    action();
                }
                else
                {
                    Timer.Interval = TimeSpan.FromMilliseconds(delay);
                    scheduledTimerAction = action;
                    Timer.Start();
                }
            }
        }

        private void GoToState(string stateName, bool useTransitions = true)
        {
            var fe = ManagedPopup.Child as FrameworkElement;
            if (fe == null) return;

            VisualStateManager.GoToState(fe, stateName, useTransitions);
        }

        private void FocusPopup()
        {
            IntPtr handle = IntPtr.Zero;
            if (ManagedPopup.Child != null)
            {
                //try to get a handle on the popup itself (via its child)
                HwndSource source = (HwndSource)PresentationSource.FromVisual(ManagedPopup.Child);
                if (source != null) handle = source.Handle;
            }

            //TODO if we don't have a handle for the popup, fall back to the message sink
            //if (handle == IntPtr.Zero) handle = notifyIcon.messageSink.MessageWindowHandle;

            //activate either popup or message sink to track deactivation.
            //otherwise, the popup does not close if the user clicks somewhere else
            WinApi.SetForegroundWindow(handle);
        }
    }
}