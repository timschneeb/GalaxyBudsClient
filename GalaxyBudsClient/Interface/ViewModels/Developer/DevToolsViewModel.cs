using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Collections;

namespace GalaxyBudsClient.Interface.ViewModels.Developer;

public partial class DevToolsViewModel : ViewModelBase
{
    [Reactive] private bool _hasProperties = true;
    [Reactive] private bool _isAutoscrollEnabled = true;
    [Reactive] private bool _useAlternativeProtocol;
    [Reactive] private MessageViewHolder? _selectedMessage;
        
    public readonly DataGridCollectionView MsgTableDataView = new(new ObservableCollection<MessageViewHolder>());
    public readonly DataGridCollectionView PropTableDataView = new(new ObservableCollection<PropertyViewModel>());

    public ObservableCollection<MessageViewHolder> MsgTableDataSource =>
        (ObservableCollection<MessageViewHolder>)MsgTableDataView.SourceCollection;
    public ObservableCollection<PropertyViewModel> PropTableDataSource =>
        (ObservableCollection<PropertyViewModel>)PropTableDataView.SourceCollection;
    
    public DevToolsViewModel()
    {
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedMessage):
            {
                var item = SelectedMessage;
                if (item?.Message == null)
                {
                    HasProperties = true;
                    PropTableDataSource.Clear();
                }
                else
                {
                    var parser = item.Message.CreateDecoder();
                    if (parser != null)
                    {
                        PropTableDataSource.Clear();
                        foreach (var (key, value) in parser.ToStringMap())
                        {
                            PropTableDataSource?.Add(new PropertyViewModel(key, value));
                        }
                        HasProperties = true;
                    }
                    else
                    {
                        PropTableDataSource.Clear();
                        HasProperties = false;
                    }
                }
        
                PropTableDataView.Refresh();
                break;
            }
        }
    }
}
