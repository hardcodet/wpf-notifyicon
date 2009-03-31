using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Sample_Project.Commands
{
  /// <summary>
  /// Hides the main window.
  /// </summary>
  public class HideMainWindowCommand : ICommand
  {
    public event EventHandler CanExecuteChanged;

    public void Execute(object parameter)
    {
      Application.Current.MainWindow.Hide();
      TaskbarIconCommands.RefreshCommands();
    }


    public bool CanExecute(object parameter)
    {
      return Application.Current.MainWindow.IsVisible;
    }


    /// <summary>
    /// Raises the <see cref="CanExecuteChanged"/> event.
    /// </summary>
    internal void RaiseCanExcecuteChanged()
    {
      if (CanExecuteChanged != null) CanExecuteChanged(this, EventArgs.Empty);
    }

  }
}
