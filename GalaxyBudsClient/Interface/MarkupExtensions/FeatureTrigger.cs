using Avalonia;
using Avalonia.Metadata;
using Avalonia.Xaml.Interactivity;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Interface.MarkupExtensions;

public class FeatureTrigger : RequiresFeatureBehavior, ITrigger
{
    public static readonly DirectProperty<FeatureTrigger, ActionCollection> ActionsProperty =
        AvaloniaProperty.RegisterDirect<FeatureTrigger, ActionCollection>(nameof(Actions), t => t.Actions);

    private ActionCollection? _actions;
    
    [Content]
    public ActionCollection Actions => _actions ??= [];
    
    protected override void UpdateState()
    {
        var shouldExecute = BluetoothImpl.Instance.DeviceSpec.Supports(Feature) && !Not ||
                            !BluetoothImpl.Instance.DeviceSpec.Supports(Feature) && Not;
        
        if (shouldExecute)
        {
            Interaction.ExecuteActions(AssociatedObject, Actions, null);
        }
    }
}