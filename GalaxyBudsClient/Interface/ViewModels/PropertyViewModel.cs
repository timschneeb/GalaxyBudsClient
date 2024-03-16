namespace GalaxyBudsClient.Interface.ViewModels
{
    public class PropertyViewModel(string key, string value)
    {
        public string Key { get; set; } = key;
        public string Value { get; set; } = value;
    }
}
