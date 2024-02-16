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

#pragma warning disable CS0067
        public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            _executeMethod.Invoke(parameter);
        }

        private readonly Action<object?> _executeMethod;
    }
}