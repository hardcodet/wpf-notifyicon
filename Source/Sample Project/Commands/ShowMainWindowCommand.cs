using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Sample_Project.Commands
{
  /// <summary>
  /// Shows the main window.
  /// </summary>
  public class ShowMainWindowCommand : CommandBase<ShowMainWindowCommand>
  {
    public override void Execute(object parameter)
    {
      Application.Current.MainWindow.Show();
      CommandManager.InvalidateRequerySuggested();
    }


    public override bool CanExecute(object parameter)
    {
      return Application.Current.MainWindow.IsVisible == false;
    }

  }
}
