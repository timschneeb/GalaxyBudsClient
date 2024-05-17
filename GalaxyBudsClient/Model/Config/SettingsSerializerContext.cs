using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace GalaxyBudsClient.Model.Config;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, 
    AllowTrailingCommas = true, 
    IgnoreReadOnlyProperties = true,
    PropertyNameCaseInsensitive = true,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(SettingsData))]
[JsonSerializable(typeof(CustomAction))]
[JsonSerializable(typeof(Hotkey))]
[JsonSerializable(typeof(Device))]
[JsonSerializable(typeof(ObservableCollection<Hotkey>))]
[JsonSerializable(typeof(ObservableCollection<Device>))]
public partial class SettingsSerializerContext : JsonSerializerContext;