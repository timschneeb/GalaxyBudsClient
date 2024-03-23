using System;
using System.Windows.Input;

namespace GalaxyBudsClient.Utils.Interface;

public class MiniCommand(Action<object?> executeMethod) : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter)
    {
        executeMethod.Invoke(parameter);
    }
}