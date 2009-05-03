using System;
using System.Windows.Input;
using System.Windows.Markup;

namespace Sample_Project.Commands
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
      return true;
    }
  }
}
