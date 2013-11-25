using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Hardcodet.Wpf.TaskbarNotification;

namespace Samples.Commands
{
    /// <summary>
    /// Basic implementation of the <see cref="ICommand"/>
    /// interface, which is also accessible as a markup
    /// extension.
    /// </summary>
    public abstract class CommandBase<T> : MarkupExtension, ICommand
        where T : class, ICommand, new()
    {
        /// <summary>
        /// A singleton instance.
        /// </summary>
        private static T command;

        /// <summary>
        /// Gets a shared command instance.
        /// </summary>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (command == null) command = new T();
            return command;
        }

        /// <summary>
        /// Fires when changes occur that affect whether
        /// or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.
        /// If the command does not require data to be passed,
        /// this object can be set to null.
        /// </param>
        public abstract void Execute(object parameter);

        /// <summary>
        /// Defines the method that determines whether the command
        /// can execute in its current state.
        /// </summary>
        /// <returns>
        /// This default implementation always returns true.
        /// </returns>
        /// <param name="parameter">Data used by the command.  
        /// If the command does not require data to be passed,
        /// this object can be set to null.
        /// </param>
        public virtual bool CanExecute(object parameter)
        {
            return IsDesignMode ? false : true;
        }


        public static bool IsDesignMode
        {
            get
            {
                return (bool)
                    DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty,
                        typeof (FrameworkElement))
                        .Metadata.DefaultValue;
            }
        }


        /// <summary>
        /// Resolves the window that owns the TaskbarIcon class.
        /// </summary>
        /// <param name="commandParameter"></param>
        /// <returns></returns>
        protected Window GetTaskbarWindow(object commandParameter)
        {
            if (IsDesignMode) return null;

            //get the showcase window off the taskbaricon
            var tb = commandParameter as TaskbarIcon;
            return tb == null ? null : TryFindParent<Window>(tb);
        }

        #region TryFindParent helper

        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect child of the
        /// queried item.</param>
        /// <returns>The first parent item that matches the submitted
        /// type parameter. If not matching item can be found, a null
        /// reference is being returned.</returns>
        public static T TryFindParent<T>(DependencyObject child)
            where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = GetParentObject(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                //use recursion to proceed with next level
                return TryFindParent<T>(parentObject);
            }
        }

        /// <summary>
        /// This method is an alternative to WPF's
        /// <see cref="VisualTreeHelper.GetParent"/> method, which also
        /// supports content elements. Keep in mind that for content element,
        /// this method falls back to the logical tree of the element!
        /// </summary>
        /// <param name="child">The item to be processed.</param>
        /// <returns>The submitted item's parent, if available. Otherwise
        /// null.</returns>
        public static DependencyObject GetParentObject(DependencyObject child)
        {
            if (child == null) return null;
            ContentElement contentElement = child as ContentElement;

            if (contentElement != null)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;

                FrameworkContentElement fce = contentElement as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            //if it's not a ContentElement, rely on VisualTreeHelper
            return VisualTreeHelper.GetParent(child);
        }

        #endregion
    }
}