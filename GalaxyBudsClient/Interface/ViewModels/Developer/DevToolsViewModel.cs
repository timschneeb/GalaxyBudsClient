using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Collections;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Developer;

public class DevToolsViewModel : ViewModelBase
{
    [Reactive] public bool HasProperties { set; get; } = true;
        
    public readonly DataGridCollectionView MsgTableDataView = new(new List<MessageViewHolder>());
    public readonly DataGridCollectionView PropTableDataView = new(new List<PropertyViewModel>());

    public List<MessageViewHolder>? MsgTableDataSource =>
        MsgTableDataView.SourceCollection as List<MessageViewHolder>;
    public List<PropertyViewModel>? PropTableDataSource =>
        PropTableDataView.SourceCollection as List<PropertyViewModel>;

    public MessageViewHolder? SelectedMessage { set; get; }
    
    public DevToolsViewModel()
    {
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedMessage))
        {
            var item = SelectedMessage;
            if (item?.Message == null)
            {
                HasProperties = true;
                PropTableDataSource?.Clear();
            }
            else
            {
                var parser = item.Message.BuildParser();
                if (parser != null)
                {
                    PropTableDataSource?.Clear();
                    foreach (var (key, value) in parser.ToStringMap())
                    {
                        PropTableDataSource?.Add(new PropertyViewModel(key, value));
                    }
                    HasProperties = true;
                }
                else
                {
                    PropTableDataSource?.Clear();
                    HasProperties = false;
                }
            }
        
            PropTableDataView.Refresh();
        }
    }
}
