using System.Windows;
using System.Windows.Input;

namespace Sample_Project.Commands
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
      return Application.Current.MainWindow.IsVisible;
    }


  }
}
