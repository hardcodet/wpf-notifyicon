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
    public enum CloseMode
    {
        /// <summary>
        /// Popup is not closed manually.
        /// </summary>
        None,

        OnLeavePopup,
    }


    /// <summary>
    /// Maintains a currently displayed, optionally interactive "ToolTip" (which is infact a popup).
    /// </summary>
    internal class ToolTipObserver
    {
        private readonly TaskbarIcon notifyIcon;
        private readonly DispatcherTimer timer;
        private Action timerAction;

        public Popup Popup
        {
            get { return popup; }
            set
            {
                popup = value;
                InitControls();
            }
        }

        /// <summary>
        /// Caches whether the mouse is currently over the popup. In that case, ignore
        /// the notify's request to close the popup (leads into an endless open/close cycle).
        /// </summary>
        private bool isMouseOverPopup;

        private Popup popup;

        public bool IsMouseOverToolTip
        {
            //TODO replace with better call
            get { return popup != null && popup.IsMouseOver; }
        }




        public ToolTipObserver(TaskbarIcon notifyIcon, int delay)
        {
            this.notifyIcon = notifyIcon;

            timer = new DispatcherTimer(TimeSpan.FromMilliseconds(delay), DispatcherPriority.Normal, OnTimerElapsed, notifyIcon.Dispatcher);
            timer.IsEnabled = false;

            //EventManager.RegisterClassHandler(type, Mouse.MouseEnterEvent, (Delegate)new MouseEventHandler(UIElement.OnMouseEnterThunk), false);
            //EventManager.RegisterClassHandler(type, Mouse.MouseLeaveEvent, (Delegate)new MouseEventHandler(UIElement.OnMouseLeaveThunk), false);
        }

        /// <summary>
        /// Shows the popup. Typically invoked if the user hovers over the NotifyIcon.
        /// </summary>
        public void ShowToolTip()
        {
            Debug.WriteLine("show request from NI");

            //don't do anything
            if (!notifyIcon.IsEnabled) return; //TODO should move into the control, also for other operations

            //if the popup is still open, open it immediatly
            if (Popup != null && Popup.IsOpen)
            {
                Debug.WriteLine("show request scheduled");
                Schedule(() => { }); //reset schedule
                GoToState("Showing");
            }
            else
            {
                Schedule(() =>
                {
                    Debug.WriteLine("showing popup");
                    Popup.IsOpen = true; //show, then transition
                    GoToState("Showing");

                    IntPtr handle = IntPtr.Zero;
                    Popup ttPopup = notifyIcon.TrayToolTipResolved;
                    if (ttPopup.Child != null)
                    {
                        //try to get a handle on the popup itself (via its child)
                        HwndSource source = (HwndSource)PresentationSource.FromVisual(ttPopup.Child);
                        if (source != null) handle = source.Handle;
                    }

                    //if we don't have a handle for the popup, fall back to the message sink
                    //if (handle == IntPtr.Zero) handle = notifyIcon.messageSink.MessageWindowHandle;

                    //activate either popup or message sink to track deactivation.
                    //otherwise, the popup does not close if the user clicks somewhere else
                    WinApi.SetForegroundWindow(handle);
                });
            }
        }

        /// <summary>
        /// Transitions the popup into a closed state.
        /// </summary>
        public void BeginCloseToolTip()
        {
            Debug.WriteLine("close request from notify icon");

            
            //Popup.InputHitTest(p == null)

            if (Popup.IsMouseOver)
            {
                Debug.WriteLine("ignoring close request since mouse is over tooltip");
                return;
            }

            if (Popup == null) return;

            //start fading immediately if animation is programmed that way
            GoToState("Hiding");

            Schedule(() =>
            {
                Debug.WriteLine("Close request scheduled");
                GoToState("Closed");
                Popup.IsOpen = false;
            });
        }



        /// <summary>
        /// Suppresses a scheduled hiding of the popup, and transitions the content
        /// back into active state.
        /// </summary>
        private void OnPopupMouseEnter(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("popup enter");

            this.isMouseOverPopup = true;

            timer.Stop(); //suppress any other actions
            GoToState("Active"); //the popup is still open
        }

        
        /// <summary>
        /// Schedules hiding of the control if the user moves away from an interactive popup.
        /// </summary>
        private void OnPopupMouseLeave(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("mouse leave");
            this.isMouseOverPopup = false;

            Schedule(() =>
            {
                Debug.WriteLine("mouse leave scheduler");

                //start hiding immediately
                GoToState("Hiding"); //switch with a delay when leaving the popup

                Schedule(() =>
                {
                    Popup.IsOpen = false;
                }); //close with yet another delay
            });
        }



        /// <summary>
        /// Inits the helper popup and tooltip controls.
        /// </summary>
        private void InitControls()
        {
            Popup.MouseEnter += OnPopupMouseEnter;
            Popup.MouseLeave += OnPopupMouseLeave;

            //hook up the popup with the data context of it's associated object
            //in case we have content with bindings
            var binding = new Binding
            {
                Path = new PropertyPath(FrameworkElement.DataContextProperty),
                Mode = BindingMode.OneWay,
                Source = notifyIcon
            };
            BindingOperations.SetBinding(Popup, FrameworkElement.DataContextProperty, binding);

            //force template application so we can switch visual states
            var fe = notifyIcon.TrayToolTip as FrameworkElement;
            if (fe != null)
            {
                fe.ApplyTemplate();
                GoToState("Closed", false);
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
        private void Schedule(Action action)
        {
            lock (timer)
            {
                //close whatever was scheduled
                timer.Stop();

                timerAction = action;
                timer.Start();
            }
        }

        /// <summary>
        /// Performs a pending action.
        /// </summary>
        private void OnTimerElapsed(object sender, EventArgs eventArgs)
        {
            lock (timer)
            {
                timer.Stop();

                var action = timerAction;
                timerAction = null;

                //cache action and set timer action to null before invoking
                //(that action could cause the timeraction to be reassigned)
                if (action != null)
                {
                    action();
                }
            }
        }


        private void GoToState(string stateName, bool useTransitions = true)
        {
            var fe = notifyIcon.TrayToolTip as FrameworkElement;
            if (fe == null) return;

            VisualStateManager.GoToState(fe, stateName, useTransitions);
        }

    }
}
