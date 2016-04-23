using System;
using System.Windows;
using System.Windows.Input;

namespace LightImageViewer.Utilities.Helpers
{
    public class ClickCommand : ICommand
    {
        private readonly RoutedEventHandler _action;

        public ClickCommand(RoutedEventHandler action)
        {
            _action = action;
        }

        public void Execute(object parameter)
        {
            _action(parameter, null);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
