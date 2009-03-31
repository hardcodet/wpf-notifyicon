using System;
using System.ComponentModel;
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
    /// <summary>
    /// Category name that is set on designer properties.
    /// </summary>
    public const string CategoryName = "NotifyIcon";

    /// <summary>
    /// A <see cref="ToolTip"/> control that was created
    /// in order to display either <see cref="TaskbarIconToolTip"/>
    /// or <see cref="ToolTipText"/>.
    /// </summary>
    internal ToolTip CustomToolTip { get; private set; }

    /// <summary>
    /// A <see cref="Popup"/> which is either the
    /// <see cref="TaskbarIconPopup"/> control itself or a
    /// <see cref="Popup"/> that wraps it.
    /// </summary>
    internal Popup CustomPopup { get; private set; }


    //DEPENDENCY PROPERTIES

    #region Icon property / IconSource dependency property

    private Icon icon;

    /// <summary>
    /// Gets or sets the icon to be displayed. This is not a
    /// dependency property - if you want to assign the property
    /// through XAML, please use the <see cref="IconSource"/>
    /// dependency property.
    /// </summary>
    [Browsable(false)]
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
                                    typeof(ImageSource),
                                    typeof(TaskbarIcon),
                                    new FrameworkPropertyMetadata(null, IconSourcePropertyChanged));

    /// <summary>
    /// A property wrapper for the <see cref="IconSourceProperty"/>
    /// dependency property:<br/>
    /// Resolves an image source and updates the <see cref="Icon" /> property accordingly.
    /// </summary>
    [Category(CategoryName)]
    [Description("Sets the displayed taskbar icon.")]
    public ImageSource IconSource
    {
      get { return (ImageSource)GetValue(IconSourceProperty); }
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
      TaskbarIcon owner = (TaskbarIcon)d;
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
      ImageSource newValue = (ImageSource)e.NewValue;

      //resolving the ImageSource at design time probably won't work
      if (!Util.IsDesignMode) Icon = newValue.ToIcon();
    }

    #endregion

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
    [Category(CategoryName)]
    [Description("Alternative to a fully blown ToolTip, which is only displayed on Vista and above.")]
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
      //only recreate tooltip if we're not using a custom control
      if (CustomToolTip == null || CustomToolTip.Content is string)
      {
        CreateCustomToolTip();
      }
      
      WriteToolTipSettings();
    }

    #endregion

    #region TaskbarIconToolTip dependency property

    /// <summary>
    /// A custom UI element that is displayed as a tooltip if the user hovers over the taskbar icon.
    /// Works only with Vista and above. Accordingly, you should make sure that
    /// the <see cref="ToolTipText"/> property is set as well.
    /// </summary>
    public static readonly DependencyProperty TaskbarIconToolTipProperty =
        DependencyProperty.Register("TaskbarIconToolTip",
                                    typeof (UIElement),
                                    typeof (TaskbarIcon),
                                    new FrameworkPropertyMetadata(null, TaskbarIconToolTipPropertyChanged));

    /// <summary>
    /// A property wrapper for the <see cref="TaskbarIconToolTipProperty"/>
    /// dependency property:<br/>
    /// A custom UI element that is displayed as a tooltip if the user hovers over the taskbar icon.
    /// Works only with Vista and above. Accordingly, you should make sure that
    /// the <see cref="ToolTipText"/> property is set as well.
    /// </summary>
    [Category(CategoryName)]
    [Description("Custom UI element that is displayed as a tooltip. Only on Vista and above")]
    public UIElement TaskbarIconToolTip
    {
      get { return (UIElement) GetValue(TaskbarIconToolTipProperty); }
      set { SetValue(TaskbarIconToolTipProperty, value); }
    }


    /// <summary>
    /// A static callback listener which is being invoked if the
    /// <see cref="TaskbarIconToolTipProperty"/> dependency property has
    /// been changed. Invokes the <see cref="OnTaskbarIconToolTipPropertyChanged"/>
    /// instance method of the changed instance.
    /// </summary>
    /// <param name="d">The currently processed owner of the property.</param>
    /// <param name="e">Provides information about the updated property.</param>
    private static void TaskbarIconToolTipPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      TaskbarIcon owner = (TaskbarIcon) d;
      owner.OnTaskbarIconToolTipPropertyChanged(e);
    }


    /// <summary>
    /// Handles changes of the <see cref="TaskbarIconToolTipProperty"/> dependency property. As
    /// WPF internally uses the dependency property system and bypasses the
    /// <see cref="TaskbarIconToolTip"/> property wrapper, updates of the property's value
    /// should be handled here.
    /// </summary
    /// <param name="e">Provides information about the updated property.</param>
    private void OnTaskbarIconToolTipPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      //recreate tooltip control
      CreateCustomToolTip();

      //udpate tooltip settings - needed to make sure a string is set, even
      //if the ToolTipText property is not set. Otherwise, the event that
      //triggers tooltip display is never fired.
      WriteToolTipSettings();
    }

    #endregion

    #region TaskbarIconPopup dependency property

    /// <summary>
    /// A control that is displayed as a popup when the taskbar icon is clicked.
    /// </summary>
    public static readonly DependencyProperty TaskbarIconPopupProperty =
        DependencyProperty.Register("TaskbarIconPopup",
                                    typeof(UIElement),
                                    typeof (TaskbarIcon),
                                    new FrameworkPropertyMetadata(null, TaskbarIconPopupPropertyChanged));

    /// <summary>
    /// A property wrapper for the <see cref="TaskbarIconPopupProperty"/>
    /// dependency property:<br/>
    /// A control that is displayed as a popup when the taskbar icon is clicked.
    /// </summary>
    [Category(CategoryName)]
    [Description("Displayed as a Popup if the user clicks on the taskbar icon.")]
    public UIElement TaskbarIconPopup
    {
      get { return (UIElement)GetValue(TaskbarIconPopupProperty); }
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
      //currently not needed
      CreatePopup();
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
    [Category(CategoryName)]
    [Description("Defines what mouse events display the context menu.")]
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
      //currently not needed
    }

    #endregion
    
    #region PopupActivation dependency property

    /// <summary>
    /// Defines what mouse events trigger the <see cref="TaskbarIconPopup" />.
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
    /// Defines what mouse events trigger the <see cref="TaskbarIconPopup" />.
    /// Default is <see cref="PopupActivationMode.LeftClick" />.
    /// </summary>
    [Category(CategoryName)]
    [Description("Defines what mouse events display the TaskbarIconPopup.")]
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
      //currently not needed
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
      //currently not needed
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
    [Category(CategoryName)]
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

    #region TaskbarIconMiddleMouseDown

    /// <summary>
    /// TaskbarIconMiddleMouseDown Routed Event
    /// </summary>
    public static readonly RoutedEvent TaskbarIconMiddleMouseDownEvent = EventManager.RegisterRoutedEvent("TaskbarIconMiddleMouseDown",
        RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TaskbarIcon));

    /// <summary>
    /// Occurs when the user presses the middle mouse button.
    /// </summary>
    public event RoutedEventHandler TaskbarIconMiddleMouseDown
    {
      add { AddHandler(TaskbarIconMiddleMouseDownEvent, value); }
      remove { RemoveHandler(TaskbarIconMiddleMouseDownEvent, value); }
    }

    /// <summary>
    /// A helper method to raise the TaskbarIconMiddleMouseDown event.
    /// </summary>
    protected RoutedEventArgs RaiseTaskbarIconMiddleMouseDownEvent()
    {
      return RaiseTaskbarIconMiddleMouseDownEvent(this);
    }

    /// <summary>
    /// A static helper method to raise the TaskbarIconMiddleMouseDown event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaiseTaskbarIconMiddleMouseDownEvent(DependencyObject target)
    {
      if (target == null) return null;

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = TaskbarIconMiddleMouseDownEvent;
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

    #region TaskbarIconMiddleMouseUp

    /// <summary>
    /// TaskbarIconMiddleMouseUp Routed Event
    /// </summary>
    public static readonly RoutedEvent TaskbarIconMiddleMouseUpEvent = EventManager.RegisterRoutedEvent("TaskbarIconMiddleMouseUp",
        RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TaskbarIcon));

    /// <summary>
    /// Occurs when the user releases the middle mouse button.
    /// </summary>
    public event RoutedEventHandler TaskbarIconMiddleMouseUp
    {
      add { AddHandler(TaskbarIconMiddleMouseUpEvent, value); }
      remove { RemoveHandler(TaskbarIconMiddleMouseUpEvent, value); }
    }

    /// <summary>
    /// A helper method to raise the TaskbarIconMiddleMouseUp event.
    /// </summary>
    protected RoutedEventArgs RaiseTaskbarIconMiddleMouseUpEvent()
    {
      return RaiseTaskbarIconMiddleMouseUpEvent(this);
    }

    /// <summary>
    /// A static helper method to raise the TaskbarIconMiddleMouseUp event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaiseTaskbarIconMiddleMouseUpEvent(DependencyObject target)
    {
      if (target == null) return null;

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = TaskbarIconMiddleMouseUpEvent;
      RoutedEventHelper.RaiseEvent(target, args);
      return args;
    }

    #endregion
        

    #region TaskbarIconMouseDoubleClick

    /// <summary>
    /// TaskbarIconMouseDoubleClick Routed Event
    /// </summary>
    public static readonly RoutedEvent TaskbarIconMouseDoubleClickEvent = EventManager.RegisterRoutedEvent("TaskbarIconMouseDoubleClick",
        RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TaskbarIcon));

    /// <summary>
    /// Occurs when the user double-clicks the taskbar icon.
    /// </summary>
    public event RoutedEventHandler TaskbarIconMouseDoubleClick
    {
      add { AddHandler(TaskbarIconMouseDoubleClickEvent, value); }
      remove { RemoveHandler(TaskbarIconMouseDoubleClickEvent, value); }
    }

    /// <summary>
    /// A helper method to raise the TaskbarIconMouseDoubleClick event.
    /// </summary>
    protected RoutedEventArgs RaiseTaskbarIconMouseDoubleClickEvent()
    {
      return RaiseTaskbarIconMouseDoubleClickEvent(this);
    }

    /// <summary>
    /// A static helper method to raise the TaskbarIconMouseDoubleClick event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaiseTaskbarIconMouseDoubleClickEvent(DependencyObject target)
    {
      if (target == null) return null;

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = TaskbarIconMouseDoubleClickEvent;
      RoutedEventHelper.RaiseEvent(target, args);
      return args;
    }

    #endregion
        
    #region TaskbarIconMouseMove

    /// <summary>
    /// TaskbarIconMouseMove Routed Event
    /// </summary>
    public static readonly RoutedEvent TaskbarIconMouseMoveEvent = EventManager.RegisterRoutedEvent("TaskbarIconMouseMove",
        RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TaskbarIcon));

    /// <summary>
    /// Occurs when the user moves the mouse over the taskbar icon.
    /// </summary>
    public event RoutedEventHandler TaskbarIconMouseMove
    {
      add { AddHandler(TaskbarIconMouseMoveEvent, value); }
      remove { RemoveHandler(TaskbarIconMouseMoveEvent, value); }
    }

    /// <summary>
    /// A helper method to raise the TaskbarIconMouseMove event.
    /// </summary>
    protected RoutedEventArgs RaiseTaskbarIconMouseMoveEvent()
    {
      return RaiseTaskbarIconMouseMoveEvent(this);
    }

    /// <summary>
    /// A static helper method to raise the TaskbarIconMouseMove event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaiseTaskbarIconMouseMoveEvent(DependencyObject target)
    {
      if (target == null) return null;

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = TaskbarIconMouseMoveEvent;
      RoutedEventHelper.RaiseEvent(target, args);
      return args;
    }

    #endregion


    #region TaskbarIconBalloonTipShown

    /// <summary>
    /// TaskbarIconBalloonTipShown Routed Event
    /// </summary>
    public static readonly RoutedEvent TaskbarIconBalloonTipShownEvent = EventManager.RegisterRoutedEvent("TaskbarIconBalloonTipShown",
        RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TaskbarIcon));

    /// <summary>
    /// Occurs when a balloon ToolTip is displayed.
    /// </summary>
    public event RoutedEventHandler TaskbarIconBalloonTipShown
    {
      add { AddHandler(TaskbarIconBalloonTipShownEvent, value); }
      remove { RemoveHandler(TaskbarIconBalloonTipShownEvent, value); }
    }

    /// <summary>
    /// A helper method to raise the TaskbarIconBalloonTipShown event.
    /// </summary>
    protected RoutedEventArgs RaiseTaskbarIconBalloonTipShownEvent()
    {
      return RaiseTaskbarIconBalloonTipShownEvent(this);
    }

    /// <summary>
    /// A static helper method to raise the TaskbarIconBalloonTipShown event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaiseTaskbarIconBalloonTipShownEvent(DependencyObject target)
    {
      if (target == null) return null;

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = TaskbarIconBalloonTipShownEvent;
      RoutedEventHelper.RaiseEvent(target, args);
      return args;
    }

    #endregion

    #region TaskbarIconBalloonTipClosed

    /// <summary>
    /// TaskbarIconBalloonTipClosed Routed Event
    /// </summary>
    public static readonly RoutedEvent TaskbarIconBalloonTipClosedEvent = EventManager.RegisterRoutedEvent("TaskbarIconBalloonTipClosed",
        RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TaskbarIcon));

    /// <summary>
    /// Occurs when a balloon ToolTip was closed.
    /// </summary>
    public event RoutedEventHandler TaskbarIconBalloonTipClosed
    {
      add { AddHandler(TaskbarIconBalloonTipClosedEvent, value); }
      remove { RemoveHandler(TaskbarIconBalloonTipClosedEvent, value); }
    }

    /// <summary>
    /// A helper method to raise the TaskbarIconBalloonTipClosed event.
    /// </summary>
    protected RoutedEventArgs RaiseTaskbarIconBalloonTipClosedEvent()
    {
      return RaiseTaskbarIconBalloonTipClosedEvent(this);
    }

    /// <summary>
    /// A static helper method to raise the TaskbarIconBalloonTipClosed event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaiseTaskbarIconBalloonTipClosedEvent(DependencyObject target)
    {
      if (target == null) return null;

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = TaskbarIconBalloonTipClosedEvent;
      RoutedEventHelper.RaiseEvent(target, args);
      return args;
    }

    #endregion

    #region TaskbarIconBalloonTipClicked

    /// <summary>
    /// TaskbarIconBalloonTipClicked Routed Event
    /// </summary>
    public static readonly RoutedEvent TaskbarIconBalloonTipClickedEvent = EventManager.RegisterRoutedEvent("TaskbarIconBalloonTipClicked",
        RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TaskbarIcon));

    /// <summary>
    /// Occurs when the user clicks on a balloon ToolTip.
    /// </summary>
    public event RoutedEventHandler TaskbarIconBalloonTipClicked
    {
      add { AddHandler(TaskbarIconBalloonTipClickedEvent, value); }
      remove { RemoveHandler(TaskbarIconBalloonTipClickedEvent, value); }
    }

    /// <summary>
    /// A helper method to raise the TaskbarIconBalloonTipClicked event.
    /// </summary>
    protected RoutedEventArgs RaiseTaskbarIconBalloonTipClickedEvent()
    {
      return RaiseTaskbarIconBalloonTipClickedEvent(this);
    }

    /// <summary>
    /// A static helper method to raise the TaskbarIconBalloonTipClicked event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaiseTaskbarIconBalloonTipClickedEvent(DependencyObject target)
    {
      if (target == null) return null;

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = TaskbarIconBalloonTipClickedEvent;
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

    #region TaskbarIconToolTipClose (and PreviewTaskbarIconToolTipClose)

    /// <summary>
    /// TaskbarIconToolTipClose Routed Event
    /// </summary>
    public static readonly RoutedEvent TaskbarIconToolTipCloseEvent = EventManager.RegisterRoutedEvent("TaskbarIconToolTipClose",
        RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TaskbarIcon));

    /// <summary>
    /// Bubbled event that occurs when a custom tooltip is being closed.
    /// </summary>
    public event RoutedEventHandler TaskbarIconToolTipClose
    {
      add { AddHandler(TaskbarIconToolTipCloseEvent, value); }
      remove { RemoveHandler(TaskbarIconToolTipCloseEvent, value); }
    }

    /// <summary>
    /// A helper method to raise the TaskbarIconToolTipClose event.
    /// </summary>
    protected RoutedEventArgs RaiseTaskbarIconToolTipCloseEvent()
    {
      return RaiseTaskbarIconToolTipCloseEvent(this);
    }

    /// <summary>
    /// A static helper method to raise the TaskbarIconToolTipClose event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaiseTaskbarIconToolTipCloseEvent(DependencyObject target)
    {
      if (target == null) return null;

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = TaskbarIconToolTipCloseEvent;
      RoutedEventHelper.RaiseEvent(target, args);
      return args;
    }

    /// <summary>
    /// PreviewTaskbarIconToolTipClose Routed Event
    /// </summary>
    public static readonly RoutedEvent PreviewTaskbarIconToolTipCloseEvent = EventManager.RegisterRoutedEvent("PreviewTaskbarIconToolTipClose",
        RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(TaskbarIcon));

    /// <summary>
    /// Tunneled event that occurs when a custom tooltip is being closed.
    /// </summary>
    public event RoutedEventHandler PreviewTaskbarIconToolTipClose
    {
      add { AddHandler(PreviewTaskbarIconToolTipCloseEvent, value); }
      remove { RemoveHandler(PreviewTaskbarIconToolTipCloseEvent, value); }
    }

    /// <summary>
    /// A helper method to raise the PreviewTaskbarIconToolTipClose event.
    /// </summary>
    protected RoutedEventArgs RaisePreviewTaskbarIconToolTipCloseEvent()
    {
      return RaisePreviewTaskbarIconToolTipCloseEvent(this);
    }

    /// <summary>
    /// A static helper method to raise the PreviewTaskbarIconToolTipClose event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaisePreviewTaskbarIconToolTipCloseEvent(DependencyObject target)
    {
      if (target == null) return null;

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = PreviewTaskbarIconToolTipCloseEvent;
      RoutedEventHelper.RaiseEvent(target, args);
      return args;
    }

    #endregion
        


    //BASE CLASS PROPERTY OVERRIDES

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
    }
  }
}