using System;
using System.Linq;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Message;

namespace GalaxyBudsClient.Interface.MarkupExtensions;

public class HiddenCmdBindingSource : MarkupExtension
{
    public override object ProvideValue(IServiceProvider? serviceProvider)
    {
        return HiddenCmds.Commands
            .Select(c => new HiddenCmdBindingItem
            {
                CmdId = Convert.ToHexString(BitConverter.GetBytes(c.Key).Reverse().ToArray()),
                Description = $"{Convert.ToHexString(BitConverter.GetBytes(c.Key).Reverse().ToArray())} {c.Value}"
            })
            .ToArray();
    }
}

public class HiddenCmdBindingItem
{
    public required string CmdId { init; get; }
    public required string Description { init; get; }
}