using System.Windows;
using System.Windows.Input;

namespace Samples.Commands
{
  /// <summary>
  /// Hides the main window.
  /// </summary>
  public class HideMainWindowCommand : CommandBase<HideMainWindowCommand>
  {

    public override void Execute(object parameter)
    {
      Application.Current.MainWindow.Hide();
      CommandManager.InvalidateRequerySuggested();
    }


    public override bool CanExecute(object parameter)
    {
      return !IsDesignMode && Application.Current.MainWindow.IsVisible;
    }


  }
}
