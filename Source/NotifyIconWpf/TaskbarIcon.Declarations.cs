using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Hardcodet.Wpf.TaskbarNotification.Interop;

namespace Hardcodet.Wpf.TaskbarNotification
{
  /// <summary>
  /// Contains declarations of WPF dependency properties
  /// and events.
  /// </summary>
  partial class TaskbarIcon
  {
    //DEPENDENCY PROPERTIES

    #region ToolTipText dependency property

    /// <summary>
    /// A tooltip text that is being displayed if no custom <see cref="ToolTip"/>
    /// was set or if custom tooltips are not supported.
    /// </summary>
    public static readonly DependencyProperty ToolTipTextProperty =
        DependencyProperty.Register("ToolTipText",
                                    typeof (string),
                                    typeof (TaskbarIcon),
                                    new FrameworkPropertyMetadata(String.Empty, ToolTipTextPropertyChanged));


    /// <summary>
    /// A property wrapper for the <see cref="ToolTipTextProperty"/>
    /// dependency property:<br/>
    /// A tooltip text that is being displayed if no custom <see cref="ToolTip"/>
    /// was set or if custom tooltips are not supported.
    /// </summary>
    public string ToolTipText
    {
      get { return (string) GetValue(ToolTipTextProperty); }
      set { SetValue(ToolTipTextProperty, value); }
    }


    /// <summary>
    /// A static callback listener which is being invoked if the
    /// <see cref="ToolTipTextProperty"/> dependency property has
    /// been changed. Invokes the <see cref="OnToolTipTextPropertyChanged"/>
    /// instance method of the changed instance.
    /// </summary>
    /// <param name="d">The currently processed owner of the property.</param>
    /// <param name="e">Provides information about the updated property.</param>
    private static void ToolTipTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      TaskbarIcon owner = (TaskbarIcon) d;
      owner.OnToolTipTextPropertyChanged(e);
    }


    /// <summary>
    /// Handles changes of the <see cref="ToolTipTextProperty"/> dependency property. As
    /// WPF internally uses the dependency property system and bypasses the
    /// <see cref="ToolTipText"/> property wrapper, updates of the property's value
    /// should be handled here.
    /// </summary
    /// <param name="e">Provides information about the updated property.</param>
    private void OnToolTipTextPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      string newValue = (string) e.NewValue;

      iconData.ToolTipText = newValue ?? String.Empty;
      WriteToolTipSettings();
    }

    #endregion


    #region ToolTip dependency property override

    /// <summary>
    /// A static callback listener which is being invoked if the
    /// <see cref="FrameworkElement.ToolTipProperty"/> dependency property has
    /// been changed. Invokes the <see cref="OnToolTipPropertyChanged"/>
    /// instance method of the changed instance.
    /// </summary>
    /// <param name="d">The currently processed owner of the property.</param>
    /// <param name="e">Provides information about the updated property.</param>
    private static void ToolTipPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      TaskbarIcon owner = (TaskbarIcon) d;
      owner.OnToolTipPropertyChanged(e);
    }


    /// <summary>
    /// Handles changes of the <see cref="FrameworkElement.ToolTipProperty"/> dependency property. As
    /// WPF internally uses the dependency property system and bypasses the
    /// <see cref="ToolTip"/> property wrapper, updates of the property's value
    /// should be handled here.
    /// </summary
    /// <param name="e">Provides information about the updated property.</param>
    private void OnToolTipPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      if (e.NewValue != null)
      {
        ToolTip tt = e.NewValue as ToolTip;
        if (tt == null)
        {
          tt = new ToolTip();
          tt.Content = e.NewValue;

          ToolTip = tt;
          return;
        }
      }

      WriteToolTipSettings();
    }

    #endregion


    #region Icon property / IconSource dependency property

    private Icon icon;

    /// <summary>
    /// Gets or sets the icon to be displayed. This is not a
    /// dependency property - if you want to assign the property
    /// through XAML, please use the <see cref="IconSource"/>
    /// dependency property.
    /// </summary>
    public Icon Icon
    {
      get { return icon; }
      set
      {
        icon = value;
        iconData.IconHandle = value == null ? IntPtr.Zero : icon.Handle;

        Util.WriteIconData(ref iconData, NotifyCommand.Modify, IconDataMembers.Icon);
      }
    }


    /// <summary>
    /// Resolves an image source and updates the <see cref="Icon" /> property accordingly.
    /// </summary>
    public static readonly DependencyProperty IconSourceProperty =
        DependencyProperty.Register("IconSource",
                                    typeof (ImageSource),
                                    typeof (TaskbarIcon),
                                    new FrameworkPropertyMetadata(null, IconSourcePropertyChanged));

    /// <summary>
    /// A property wrapper for the <see cref="IconSourceProperty"/>
    /// dependency property:<br/>
    /// Resolves an image source and updates the <see cref="Icon" /> property accordingly.
    /// </summary>
    public ImageSource IconSource
    {
      get { return (ImageSource) GetValue(IconSourceProperty); }
      set { SetValue(IconSourceProperty, value); }
    }


    /// <summary>
    /// A static callback listener which is being invoked if the
    /// <see cref="IconSourceProperty"/> dependency property has
    /// been changed. Invokes the <see cref="OnIconSourcePropertyChanged"/>
    /// instance method of the changed instance.
    /// </summary>
    /// <param name="d">The currently processed owner of the property.</param>
    /// <param name="e">Provides information about the updated property.</param>
    private static void IconSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      TaskbarIcon owner = (TaskbarIcon) d;
      owner.OnIconSourcePropertyChanged(e);
    }


    /// <summary>
    /// Handles changes of the <see cref="IconSourceProperty"/> dependency property. As
    /// WPF internally uses the dependency property system and bypasses the
    /// <see cref="IconSource"/> property wrapper, updates of the property's value
    /// should be handled here.
    /// </summary
    /// <param name="e">Provides information about the updated property.</param>
    private void OnIconSourcePropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      ImageSource newValue = (ImageSource) e.NewValue;
      Icon = newValue.ToIcon();
    }

    #endregion


    #region TaskbarIconPopup dependency property

    /// <summary>
    /// A custom popup that is displayed when the taskbar icon is clicked.
    /// </summary>
    public static readonly DependencyProperty TaskbarIconPopupProperty =
        DependencyProperty.Register("TaskbarIconPopup",
                                    typeof (Popup),
                                    typeof (TaskbarIcon),
                                    new FrameworkPropertyMetadata(null, TaskbarIconPopupPropertyChanged));

    /// <summary>
    /// A property wrapper for the <see cref="TaskbarIconPopupProperty"/>
    /// dependency property:<br/>
    /// A custom popup that is displayed when the taskbar icon is clicked.
    /// </summary>
    public Popup TaskbarIconPopup
    {
      get { return (Popup) GetValue(TaskbarIconPopupProperty); }
      set { SetValue(TaskbarIconPopupProperty, value); }
    }


    /// <summary>
    /// A static callback listener which is being invoked if the
    /// <see cref="TaskbarIconPopupProperty"/> dependency property has
    /// been changed. Invokes the <see cref="OnTaskbarIconPopupPropertyChanged"/>
    /// instance method of the changed instance.
    /// </summary>
    /// <param name="d">The currently processed owner of the property.</param>
    /// <param name="e">Provides information about the updated property.</param>
    private static void TaskbarIconPopupPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      TaskbarIcon owner = (TaskbarIcon) d;
      owner.OnTaskbarIconPopupPropertyChanged(e);
    }


    /// <summary>
    /// Handles changes of the <see cref="TaskbarIconPopupProperty"/> dependency property. As
    /// WPF internally uses the dependency property system and bypasses the
    /// <see cref="TaskbarIconPopup"/> property wrapper, updates of the property's value
    /// should be handled here.
    /// </summary
    /// <param name="e">Provides information about the updated property.</param>
    private void OnTaskbarIconPopupPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      Popup newValue = (Popup) e.NewValue;
    }

    #endregion


    #region MenuActivation dependency property

    /// <summary>
    /// Defines what mouse events display the context menu.
    /// Defaults to <see cref="PopupActivationMode.RightClick"/>.
    /// </summary>
    public static readonly DependencyProperty MenuActivationProperty =
        DependencyProperty.Register("MenuActivation",
                                    typeof (PopupActivationMode),
                                    typeof (TaskbarIcon),
                                    new FrameworkPropertyMetadata(PopupActivationMode.RightClick, MenuActivationPropertyChanged));

    /// <summary>
    /// A property wrapper for the <see cref="MenuActivationProperty"/>
    /// dependency property:<br/>
    /// Defines what mouse events display the context menu.
    /// Defaults to <see cref="PopupActivationMode.RightClick"/>.
    /// </summary>
    public PopupActivationMode MenuActivation
    {
      get { return (PopupActivationMode) GetValue(MenuActivationProperty); }
      set { SetValue(MenuActivationProperty, value); }
    }


    /// <summary>
    /// A static callback listener which is being invoked if the
    /// <see cref="MenuActivationProperty"/> dependency property has
    /// been changed. Invokes the <see cref="OnMenuActivationPropertyChanged"/>
    /// instance method of the changed instance.
    /// </summary>
    /// <param name="d">The currently processed owner of the property.</param>
    /// <param name="e">Provides information about the updated property.</param>
    private static void MenuActivationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      TaskbarIcon owner = (TaskbarIcon) d;
      owner.OnMenuActivationPropertyChanged(e);
    }


    /// <summary>
    /// Handles changes of the <see cref="MenuActivationProperty"/> dependency property. As
    /// WPF internally uses the dependency property system and bypasses the
    /// <see cref="MenuActivation"/> property wrapper, updates of the property's value
    /// should be handled here.
    /// </summary
    /// <param name="e">Provides information about the updated property.</param>
    private void OnMenuActivationPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      PopupActivationMode newValue = (PopupActivationMode) e.NewValue;

      //TODO provide implementation
      throw new NotImplementedException("Change event handler for dependency property MenuActivation not implemented.");
    }

    #endregion
    
 
    #region PopupActivation dependency property

    /// <summary>
    /// Defines what mouse events trigger the <see cref="IconPopup" />.
    /// Default is <see cref="PopupActivationMode.LeftClick" />.
    /// </summary>
    public static readonly DependencyProperty PopupActivationProperty =
        DependencyProperty.Register("PopupActivation",
                                    typeof (PopupActivationMode),
                                    typeof (TaskbarIcon),
                                    new FrameworkPropertyMetadata(PopupActivationMode.LeftClick, PopupActivationPropertyChanged));

    /// <summary>
    /// A property wrapper for the <see cref="PopupActivationProperty"/>
    /// dependency property:<br/>
    /// Defines what mouse events trigger the <see cref="IconPopup" />.
    /// Default is <see cref="PopupActivationMode.LeftClick" />.
    /// </summary>
    public PopupActivationMode PopupActivation
    {
      get { return (PopupActivationMode) GetValue(PopupActivationProperty); }
      set { SetValue(PopupActivationProperty, value); }
    }


    /// <summary>
    /// A static callback listener which is being invoked if the
    /// <see cref="PopupActivationProperty"/> dependency property has
    /// been changed. Invokes the <see cref="OnPopupActivationPropertyChanged"/>
    /// instance method of the changed instance.
    /// </summary>
    /// <param name="d">The currently processed owner of the property.</param>
    /// <param name="e">Provides information about the updated property.</param>
    private static void PopupActivationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      TaskbarIcon owner = (TaskbarIcon) d;
      owner.OnPopupActivationPropertyChanged(e);
    }


    /// <summary>
    /// Handles changes of the <see cref="PopupActivationProperty"/> dependency property. As
    /// WPF internally uses the dependency property system and bypasses the
    /// <see cref="PopupActivation"/> property wrapper, updates of the property's value
    /// should be handled here.
    /// </summary
    /// <param name="e">Provides information about the updated property.</param>
    private void OnPopupActivationPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      PopupActivationMode newValue = (PopupActivationMode) e.NewValue;

      //TODO provide implementation
      throw new NotImplementedException("Change event handler for dependency property PopupActivation not implemented.");
    }

    #endregion


    #region Visibility dependency property override

    /// <summary>
    /// A static callback listener which is being invoked if the
    /// <see cref="UIElement.VisibilityProperty"/> dependency property has
    /// been changed. Invokes the <see cref="OnVisibilityPropertyChanged"/>
    /// instance method of the changed instance.
    /// </summary>
    /// <param name="d">The currently processed owner of the property.</param>
    /// <param name="e">Provides information about the updated property.</param>
    private static void VisibilityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      TaskbarIcon owner = (TaskbarIcon) d;
      owner.OnVisibilityPropertyChanged(e);
    }


    /// <summary>
    /// Handles changes of the <see cref="UIElement.VisibilityProperty"/> dependency property. As
    /// WPF internally uses the dependency property system and bypasses the
    /// <see cref="Visibility"/> property wrapper, updates of the property's value
    /// should be handled here.
    /// </summary
    /// <param name="e">Provides information about the updated property.</param>
    private void OnVisibilityPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      Visibility newValue = (Visibility) e.NewValue;

      //update
      if (newValue == Visibility.Visible)
      {
        CreateTaskbarIcon();
      }
      else
      {
        RemoveTaskbarIcon();
      }
    }

    #endregion


    #region ContextMenu dependency property override

    /// <summary>
    /// A static callback listener which is being invoked if the
    /// <see cref="FrameworkElement.ContextMenuProperty"/> dependency property has
    /// been changed. Invokes the <see cref="OnContextMenuPropertyChanged"/>
    /// instance method of the changed instance.
    /// </summary>
    /// <param name="d">The currently processed owner of the property.</param>
    /// <param name="e">Provides information about the updated property.</param>
    private static void ContextMenuPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      TaskbarIcon owner = (TaskbarIcon) d;
      owner.OnContextMenuPropertyChanged(e);
    }


    /// <summary>
    /// Handles changes of the <see cref="FrameworkElement.ContextMenuProperty"/> dependency property. As
    /// WPF internally uses the dependency property system and bypasses the
    /// <see cref="ContextMenu"/> property wrapper, updates of the property's value
    /// should be handled here.
    /// </summary
    /// <param name="e">Provides information about the updated property.</param>
    private void OnContextMenuPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      ContextMenu newValue = (ContextMenu) e.NewValue;
      //TODO provide implementation
    }

    #endregion



    //EVENTS

    #region TaskbarIconLeftMouseDown

    /// <summary>
    /// TaskbarIconLeftMouseDown Routed Event
    /// </summary>
    public static readonly RoutedEvent TaskbarIconLeftMouseDownEvent = EventManager.RegisterRoutedEvent("TaskbarIconLeftMouseDown",
        RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TaskbarIcon));

    /// <summary>
    /// Occurs when the user presses the left mouse button.
    /// </summary>
    public event RoutedEventHandler TaskbarIconLeftMouseDown
    {
      add { AddHandler(TaskbarIconLeftMouseDownEvent, value); }
      remove { RemoveHandler(TaskbarIconLeftMouseDownEvent, value); }
    }

    /// <summary>
    /// A helper method to raise the TaskbarIconLeftMouseDown event.
    /// </summary>
    protected RoutedEventArgs RaiseTaskbarIconLeftMouseDownEvent()
    {
      return RaiseTaskbarIconLeftMouseDownEvent(this);
    }

    /// <summary>
    /// A static helper method to raise the TaskbarIconLeftMouseDown event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaiseTaskbarIconLeftMouseDownEvent(DependencyObject target)
    {
      if (target == null) return null;

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = TaskbarIconLeftMouseDownEvent;
      RoutedEventHelper.RaiseEvent(target, args);
      return args;
    }

    #endregion

    #region TaskbarIconRightMouseDown

    /// <summary>
    /// TaskbarIconRightMouseDown Routed Event
    /// </summary>
    public static readonly RoutedEvent TaskbarIconRightMouseDownEvent = EventManager.RegisterRoutedEvent("TaskbarIconRightMouseDown",
        RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TaskbarIcon));

    /// <summary>
    /// Occurs when the presses the right mouse button.
    /// </summary>
    public event RoutedEventHandler TaskbarIconRightMouseDown
    {
      add { AddHandler(TaskbarIconRightMouseDownEvent, value); }
      remove { RemoveHandler(TaskbarIconRightMouseDownEvent, value); }
    }

    /// <summary>
    /// A helper method to raise the TaskbarIconRightMouseDown event.
    /// </summary>
    protected RoutedEventArgs RaiseTaskbarIconRightMouseDownEvent()
    {
      return RaiseTaskbarIconRightMouseDownEvent(this);
    }

    /// <summary>
    /// A static helper method to raise the TaskbarIconRightMouseDown event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaiseTaskbarIconRightMouseDownEvent(DependencyObject target)
    {
      if (target == null) return null;

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = TaskbarIconRightMouseDownEvent;
      RoutedEventHelper.RaiseEvent(target, args);
      return args;
    }

    #endregion

    #region TaskbarIconLeftMouseUp

    /// <summary>
    /// TaskbarIconLeftMouseUp Routed Event
    /// </summary>
    public static readonly RoutedEvent TaskbarIconLeftMouseUpEvent = EventManager.RegisterRoutedEvent("TaskbarIconLeftMouseUp",
        RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TaskbarIcon));

    /// <summary>
    /// Occurs when the user releases the left mouse button.
    /// </summary>
    public event RoutedEventHandler TaskbarIconLeftMouseUp
    {
      add { AddHandler(TaskbarIconLeftMouseUpEvent, value); }
      remove { RemoveHandler(TaskbarIconLeftMouseUpEvent, value); }
    }

    /// <summary>
    /// A helper method to raise the TaskbarIconLeftMouseUp event.
    /// </summary>
    protected RoutedEventArgs RaiseTaskbarIconLeftMouseUpEvent()
    {
      return RaiseTaskbarIconLeftMouseUpEvent(this);
    }

    /// <summary>
    /// A static helper method to raise the TaskbarIconLeftMouseUp event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaiseTaskbarIconLeftMouseUpEvent(DependencyObject target)
    {
      if (target == null) return null;

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = TaskbarIconLeftMouseUpEvent;
      RoutedEventHelper.RaiseEvent(target, args);
      return args;
    }

    #endregion

    #region TaskbarIconRightMouseUp

    /// <summary>
    /// TaskbarIconRightMouseUp Routed Event
    /// </summary>
    public static readonly RoutedEvent TaskbarIconRightMouseUpEvent = EventManager.RegisterRoutedEvent("TaskbarIconRightMouseUp",
        RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TaskbarIcon));

    /// <summary>
    /// Occurs when the user releases the right mouse button.
    /// </summary>
    public event RoutedEventHandler TaskbarIconRightMouseUp
    {
      add { AddHandler(TaskbarIconRightMouseUpEvent, value); }
      remove { RemoveHandler(TaskbarIconRightMouseUpEvent, value); }
    }

    /// <summary>
    /// A helper method to raise the TaskbarIconRightMouseUp event.
    /// </summary>
    protected RoutedEventArgs RaiseTaskbarIconRightMouseUpEvent()
    {
      return RaiseTaskbarIconRightMouseUpEvent(this);
    }

    /// <summary>
    /// A static helper method to raise the TaskbarIconRightMouseUp event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaiseTaskbarIconRightMouseUpEvent(DependencyObject target)
    {
      if (target == null) return null;

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = TaskbarIconRightMouseUpEvent;
      RoutedEventHelper.RaiseEvent(target, args);
      return args;
    }

    #endregion


    #region TaskbarIconPopupOpen (and PreviewTaskbarIconPopupOpen)

    /// <summary>
    /// TaskbarIconPopupOpen Routed Event
    /// </summary>
    public static readonly RoutedEvent TaskbarIconPopupOpenEvent = EventManager.RegisterRoutedEvent("TaskbarIconPopupOpen",
        RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TaskbarIcon));

    /// <summary>
    /// Bubbled event that occurs when the custom popup is being opened.
    /// </summary>
    public event RoutedEventHandler TaskbarIconPopupOpen
    {
      add { AddHandler(TaskbarIconPopupOpenEvent, value); }
      remove { RemoveHandler(TaskbarIconPopupOpenEvent, value); }
    }

    /// <summary>
    /// A helper method to raise the TaskbarIconPopupOpen event.
    /// </summary>
    protected RoutedEventArgs RaiseTaskbarIconPopupOpenEvent()
    {
      return RaiseTaskbarIconPopupOpenEvent(this);
    }

    /// <summary>
    /// A static helper method to raise the TaskbarIconPopupOpen event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaiseTaskbarIconPopupOpenEvent(DependencyObject target)
    {
      if (target == null) return null;

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = TaskbarIconPopupOpenEvent;
      RoutedEventHelper.RaiseEvent(target, args);
      return args;
    }

    /// <summary>
    /// PreviewTaskbarIconPopupOpen Routed Event
    /// </summary>
    public static readonly RoutedEvent PreviewTaskbarIconPopupOpenEvent = EventManager.RegisterRoutedEvent("PreviewTaskbarIconPopupOpen",
        RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(TaskbarIcon));

    /// <summary>
    /// Tunneled event that occurs when the custom popup is being opened.
    /// </summary>
    public event RoutedEventHandler PreviewTaskbarIconPopupOpen
    {
      add { AddHandler(PreviewTaskbarIconPopupOpenEvent, value); }
      remove { RemoveHandler(PreviewTaskbarIconPopupOpenEvent, value); }
    }

    /// <summary>
    /// A helper method to raise the PreviewTaskbarIconPopupOpen event.
    /// </summary>
    protected RoutedEventArgs RaisePreviewTaskbarIconPopupOpenEvent()
    {
      return RaisePreviewTaskbarIconPopupOpenEvent(this);
    }

    /// <summary>
    /// A static helper method to raise the PreviewTaskbarIconPopupOpen event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaisePreviewTaskbarIconPopupOpenEvent(DependencyObject target)
    {
      if (target == null) return null;

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = PreviewTaskbarIconPopupOpenEvent;
      RoutedEventHelper.RaiseEvent(target, args);
      return args;
    }

    #endregion

    #region TaskbarIconToolTipOpen (and PreviewTaskbarIconToolTipOpen)

    /// <summary>
    /// TaskbarIconToolTipOpen Routed Event
    /// </summary>
    public static readonly RoutedEvent TaskbarIconToolTipOpenEvent = EventManager.RegisterRoutedEvent("TaskbarIconToolTipOpen",
        RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TaskbarIcon));

    /// <summary>
    /// Bubbled event that occurs when the custom ToolTip is being displayed.
    /// </summary>
    public event RoutedEventHandler TaskbarIconToolTipOpen
    {
      add { AddHandler(TaskbarIconToolTipOpenEvent, value); }
      remove { RemoveHandler(TaskbarIconToolTipOpenEvent, value); }
    }

    /// <summary>
    /// A helper method to raise the TaskbarIconToolTipOpen event.
    /// </summary>
    protected RoutedEventArgs RaiseTaskbarIconToolTipOpenEvent()
    {
      return RaiseTaskbarIconToolTipOpenEvent(this);
    }

    /// <summary>
    /// A static helper method to raise the TaskbarIconToolTipOpen event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaiseTaskbarIconToolTipOpenEvent(DependencyObject target)
    {
      if (target == null) return null;

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = TaskbarIconToolTipOpenEvent;
      RoutedEventHelper.RaiseEvent(target, args);
      return args;
    }

    /// <summary>
    /// PreviewTaskbarIconToolTipOpen Routed Event
    /// </summary>
    public static readonly RoutedEvent PreviewTaskbarIconToolTipOpenEvent = EventManager.RegisterRoutedEvent("PreviewTaskbarIconToolTipOpen",
        RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(TaskbarIcon));

    /// <summary>
    /// Tunneled event that occurs when the custom ToolTip is being displayed.
    /// </summary>
    public event RoutedEventHandler PreviewTaskbarIconToolTipOpen
    {
      add { AddHandler(PreviewTaskbarIconToolTipOpenEvent, value); }
      remove { RemoveHandler(PreviewTaskbarIconToolTipOpenEvent, value); }
    }

    /// <summary>
    /// A helper method to raise the PreviewTaskbarIconToolTipOpen event.
    /// </summary>
    protected RoutedEventArgs RaisePreviewTaskbarIconToolTipOpenEvent()
    {
      return RaisePreviewTaskbarIconToolTipOpenEvent(this);
    }

    /// <summary>
    /// A static helper method to raise the PreviewTaskbarIconToolTipOpen event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaisePreviewTaskbarIconToolTipOpenEvent(DependencyObject target)
    {
      if (target == null) return null;

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = PreviewTaskbarIconToolTipOpenEvent;
      RoutedEventHelper.RaiseEvent(target, args);
      return args;
    }

    #endregion

    #region TaskbarIconContextMenuOpen (and PreviewTaskbarIconContextMenuOpen)

    /// <summary>
    /// TaskbarIconContextMenuOpen Routed Event
    /// </summary>
    public static readonly RoutedEvent TaskbarIconContextMenuOpenEvent = EventManager.RegisterRoutedEvent("TaskbarIconContextMenuOpen",
        RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TaskbarIcon));

    /// <summary>
    /// Bubbled event that occurs when the context menu of the taskbar icon is being displayed.
    /// </summary>
    public event RoutedEventHandler TaskbarIconContextMenuOpen
    {
      add { AddHandler(TaskbarIconContextMenuOpenEvent, value); }
      remove { RemoveHandler(TaskbarIconContextMenuOpenEvent, value); }
    }

    /// <summary>
    /// A helper method to raise the TaskbarIconContextMenuOpen event.
    /// </summary>
    protected RoutedEventArgs RaiseTaskbarIconContextMenuOpenEvent()
    {
      return RaiseTaskbarIconContextMenuOpenEvent(this);
    }

    /// <summary>
    /// A static helper method to raise the TaskbarIconContextMenuOpen event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaiseTaskbarIconContextMenuOpenEvent(DependencyObject target)
    {
      if (target == null) return null;

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = TaskbarIconContextMenuOpenEvent;
      RoutedEventHelper.RaiseEvent(target, args);
      return args;
    }

    /// <summary>
    /// PreviewTaskbarIconContextMenuOpen Routed Event
    /// </summary>
    public static readonly RoutedEvent PreviewTaskbarIconContextMenuOpenEvent = EventManager.RegisterRoutedEvent("PreviewTaskbarIconContextMenuOpen",
        RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(TaskbarIcon));

    /// <summary>
    /// Tunneled event that occurs when the context menu of the taskbar icon is being displayed.
    /// </summary>
    public event RoutedEventHandler PreviewTaskbarIconContextMenuOpen
    {
      add { AddHandler(PreviewTaskbarIconContextMenuOpenEvent, value); }
      remove { RemoveHandler(PreviewTaskbarIconContextMenuOpenEvent, value); }
    }

    /// <summary>
    /// A helper method to raise the PreviewTaskbarIconContextMenuOpen event.
    /// </summary>
    protected RoutedEventArgs RaisePreviewTaskbarIconContextMenuOpenEvent()
    {
      return RaisePreviewTaskbarIconContextMenuOpenEvent(this);
    }

    /// <summary>
    /// A static helper method to raise the PreviewTaskbarIconContextMenuOpen event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaisePreviewTaskbarIconContextMenuOpenEvent(DependencyObject target)
    {
      if (target == null) return null;

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = PreviewTaskbarIconContextMenuOpenEvent;
      RoutedEventHelper.RaiseEvent(target, args);
      return args;
    }

    #endregion
        
    


    //CONSTRUCTOR DECLARATIONS

    /// <summary>
    /// Registers properties.
    /// </summary>
    static TaskbarIcon()
    {
      //register change listener for the Visibility property
      PropertyMetadata md = new PropertyMetadata(Visibility.Visible, VisibilityPropertyChanged);
      VisibilityProperty.OverrideMetadata(typeof(TaskbarIcon), md);

      //register change listener for the ContextMenu property
      md = new FrameworkPropertyMetadata(new PropertyChangedCallback(ContextMenuPropertyChanged));
      ContextMenuProperty.OverrideMetadata(typeof (TaskbarIcon), md);

      //register change listener for the ToolTip property
      md = new FrameworkPropertyMetadata(new PropertyChangedCallback(ToolTipPropertyChanged));
      ToolTipProperty.OverrideMetadata(typeof(TaskbarIcon), md);

    }
  }
}