using Avalonia.Controls;
using GalaxyBudsClient.Interface.ViewModels;

namespace GalaxyBudsClient.Interface.Pages;

public class BasePage<T> : ContentControl where T : ViewModelBase
{  
    protected T? ViewModel => DataContext as T;
}