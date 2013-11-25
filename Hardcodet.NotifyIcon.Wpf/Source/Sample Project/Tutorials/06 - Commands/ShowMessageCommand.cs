using System;
using System.Windows;
using System.Windows.Input;

namespace Samples.Tutorials.Commands
{
    /// <summary>
    /// A simple command that displays the command parameter as
    /// a dialog message.
    /// </summary>
    public class ShowMessageCommand : ICommand
    {
        public void Execute(object parameter)
        {
            MessageBox.Show(parameter.ToString());
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}