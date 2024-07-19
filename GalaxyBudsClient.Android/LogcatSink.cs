using Android.Util;
using System;
using System.IO;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace GalaxyBudsClient.Android;

internal class LogcatSink(
    string tag)
    : ILogEventSink
{
    private readonly string _tag = tag ?? throw new ArgumentNullException(nameof(tag));

    private static readonly MessageTemplateTextFormatter Formatter = new("{Message:lj}{NewLine}{Exception}");
    
    public void Emit(LogEvent logEvent)
    {
        using var writer = new StringWriter();

        Formatter.Format(logEvent, writer);

        var msg = writer.ToString();

        switch (logEvent.Level)
        {
            case LogEventLevel.Verbose:
                Log.Verbose(_tag, msg);
                break;
            case LogEventLevel.Debug:
                Log.Debug(_tag, msg);
                break;
            case LogEventLevel.Information:
                Log.Info(_tag, msg);
                break;
            case LogEventLevel.Warning:
                Log.Warn(_tag, msg);
                break;
            case LogEventLevel.Fatal:
            case LogEventLevel.Error:
                Log.Error(_tag, msg);
                break;
        }
    }
}