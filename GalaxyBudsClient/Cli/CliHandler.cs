using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using GalaxyBudsClient.Cli.Ipc;
using GalaxyBudsClient.Cli.Ipc.Objects;
using GalaxyBudsClient.Utils.Extensions;
using Serilog;
using Tmds.DBus;

namespace GalaxyBudsClient.Cli;

public static class CliHandler
{
    [Verb("action", HelpText = "List and execute app actions")]
    public class ActionOptions {
        [Option( 'l', "list", Required = false, HelpText = "Describe available actions")]
        public bool ListActions { get; set; }

        [Option('e', "execute", Required = false, MetaValue = "action", HelpText = "Execute action by identifier")]
        public string? ExecuteAction { get; set; }
    }
    
    [Verb("app", HelpText = "Application options")]
    public class AppOptions {
        [Option( 'b', "show-battery-popup", Required = false, HelpText = "Show the battery popup")]
        public bool ShowBatteryPopup { get; set; }    
        
        [Option( 'a', "activate-window", Required = false, HelpText = "Bring the app window to the front")]
        public bool ActivateWindow { get; set; }
    }
    
    public static void ProcessArguments(IEnumerable<string> args)
    {
        using var parser = new Parser(s =>
        {
            s.AutoHelp = true;
            s.AutoVersion = true;
            s.HelpWriter = Console.Out;
        });

        try
        {
            parser.ParseArguments<ActionOptions, AppOptions>(args)
                .MapResult(
                    (ActionOptions opts) => ProcessActionVerb(opts).WaitAndReturnResult(),
                    (AppOptions opts) => ProcessAppVerb(opts).WaitAndReturnResult(),
                    errs => false);
        }
        catch (AggregateException e)
        {
            if(e.InnerExceptions.Any(x => x is DBusException { ErrorName: "org.freedesktop.DBus.Error.ServiceUnknown" }))
                Console.Error.WriteLine("\nError: The Galaxy Buds Manager app is not running. The CLI can only be used while the app is running.");
            else
                Console.Error.WriteLine("\nError: " + e.Message);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("\nError: " + e.Message);
        }
        Console.Error.Flush();
    }
    
    private static async Task<Connection?> OpenConnection()
    {
        try
        {
            return await IpcService.OpenClientConnection();
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to open connection to IPC service");
            await Console.Error.WriteLineAsync("\nError: Failed to connect to the Galaxy Buds Manager app. The CLI can only be used while the app is running.");
            await Console.Error.FlushAsync();
            Environment.Exit(1);
            return null;
        }
    }
    
    private static async Task<bool> ProcessActionVerb(ActionOptions opts)
    {
        using var client = await OpenConnection();
        if (client is null)
            return false;
        
        var proxy = client.CreateProxy<IApplicationObject>(IpcService.ServiceName, ApplicationObject.Path);

        if (opts.ListActions)
        {
            var actions = await proxy.ListActionsAsync();
            foreach (var (id, description) in actions)
            {
                Console.WriteLine($"{id}: {description}");
            }
        }
        else if (opts.ExecuteAction != null)
        {
            await proxy.ExecuteActionAsync(opts.ExecuteAction);
        }
        else
        {
            return false;
        }

        return true;
    }

    private static async Task<bool> ProcessAppVerb(AppOptions opts)
    {
        using var client = await OpenConnection();
        if (client is null)
            return false;
        
        var proxy = client.CreateProxy<IApplicationObject>(IpcService.ServiceName, ApplicationObject.Path);

        if (opts.ActivateWindow)
            await proxy.ActivateAsync();
        else if (opts.ShowBatteryPopup)
            await proxy.ShowBatteryPopupAsync();
        else
            return false;
        return true;
    }
}