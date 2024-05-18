using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using PortAudioSharp;
using Serilog;

namespace GalaxyBudsClient.Platform;

public static class AudioPlayer
{
    public static async Task PlayStreamAsync(CancellationToken cancellationToken)
    {
        var deviceIndex = PortAudio.DefaultOutputDevice;
        if (deviceIndex == PortAudio.NoDevice)
        {
            Log.Error("No default audio output device found");
            return;
        }

        var info = PortAudio.GetDeviceInfo(deviceIndex);
        Log.Debug($"Using output default device {deviceIndex} ({info.name})");

        var param = new StreamParameters
        {
            device = deviceIndex,
            channelCount = 1,
            sampleFormat = SampleFormat.Int16,
            suggestedLatency = info.defaultLowOutputLatency,
            hostApiSpecificStreamInfo = IntPtr.Zero
        };

        // https://learn.microsoft.com/en-us/dotnet/standard/collections/thread-safe/blockingcollection-overview
        var dataItems = new BlockingCollection<short[]>();
        var queueSamples = (IntPtr samples, int n) =>
        {
            var data = new short[n];
            Marshal.Copy(samples, data, 0, n);
            dataItems.Add(data, cancellationToken);
        };

        var playFinished = false;

        short[]? lastSampleArray = null;
        var lastIndex = 0; // not played

        var sampleRate = 22050; // TODO
        
        try
        {
            using var stream = new Stream(inParams: null, outParams: param, sampleRate: sampleRate,
                framesPerBuffer: 0,
                streamFlags: StreamFlags.ClipOff,
                callback: PlayCallback,
                userData: IntPtr.Zero
            );

            stream.Start();

            //OfflineTtsCallback callback = new OfflineTtsCallback(queueSamples);

            //OfflineTtsGeneratedAudio audio = tts.GenerateWithCallback(options.Text, speed, sid, callback);
            dataItems.CompleteAdding();

            try
            {
                while (!playFinished && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(100, cancellationToken); // 100ms
                }
            }
            catch (TaskCanceledException)
            {
                Log.Debug("Playback task cancelled");
            }

            if (cancellationToken.IsCancellationRequested)
                stream.Abort();
            
        }
        catch (PortAudioException ex)
        {
            Log.Error(ex, "Error playing audio");
        }
        return;

        StreamCallbackResult PlayCallback(IntPtr input, IntPtr output, uint frameCount, ref StreamCallbackTimeInfo timeInfo, StreamCallbackFlags statusFlags, IntPtr userData)
        {
            if (dataItems.IsCompleted && lastSampleArray == null && lastIndex == 0)
            {
                Console.WriteLine("Finished playing");
                playFinished = true;
                return StreamCallbackResult.Complete;
            }

            var expected = Convert.ToInt32(frameCount);
            var i = 0;

            while ((lastSampleArray != null || dataItems.Count != 0) && i < expected)
            {
                var needed = expected - i;

                if (lastSampleArray != null)
                {
                    var remaining = lastSampleArray.Length - lastIndex;
                    if (remaining >= needed)
                    {
                        var thisBlock = lastSampleArray.Skip(lastIndex).Take(needed).ToArray();
                        lastIndex += needed;
                        if (lastIndex == lastSampleArray.Length)
                        {
                            lastSampleArray = null;
                            lastIndex = 0;
                        }

                        Marshal.Copy(thisBlock, 0, IntPtr.Add(output, i * sizeof(short)), needed);
                        return StreamCallbackResult.Continue;
                    }

                    var thisBlock2 = lastSampleArray.Skip(lastIndex).Take(remaining).ToArray();
                    lastIndex = 0;
                    lastSampleArray = null;

                    Marshal.Copy(thisBlock2, 0, IntPtr.Add(output, i * sizeof(short)), remaining);
                    i += remaining;
                    continue;
                }

                if (dataItems.Count != 0)
                {
                    lastSampleArray = dataItems.Take(cancellationToken);
                    lastIndex = 0;
                }
            }

            if (i < expected)
            {
                var sizeInBytes = (expected - i) * 4;
                Marshal.Copy(new byte[sizeInBytes], 0, IntPtr.Add(output, i * sizeof(short)), sizeInBytes);
            }

            return StreamCallbackResult.Continue;
        }
    }
}