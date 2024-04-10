using System;
using System.Windows.Input;

namespace GalaxyBudsClient.Model;

public class MiniCommand(Action<object?> executeMethod) : ICommand
{
#pragma warning disable CS0067
    public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter)
    {
        executeMethod.Invoke(parameter);
    }
}