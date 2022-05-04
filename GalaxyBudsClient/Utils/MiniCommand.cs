using System;
using System.Windows.Input;

namespace GalaxyBudsClient.Utils
{
    public class MiniCommand : ICommand
    {
        public MiniCommand(Action<object?> executeMethod)
        {
            _executeMethod = executeMethod;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            _executeMethod.Invoke(parameter);
        }

        private readonly Action<object?> _executeMethod;
    }
}