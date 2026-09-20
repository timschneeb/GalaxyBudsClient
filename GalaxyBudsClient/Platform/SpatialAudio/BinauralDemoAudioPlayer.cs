using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace GalaxyBudsClient.Platform.SpatialAudio;

/// <summary>
/// Interactive audio player for demonstrating 360 spatial audio anchored in front of the screen.
/// Simulates binaural hearing (ILD - Interaural Level Difference & ITD - Interaural Time Difference).
/// </summary>
public class BinauralDemoAudioPlayer : IDisposable
{
    private CancellationTokenSource? _playCts;
    private bool _isPlaying;

    public bool IsPlaying
    {
        get => _isPlaying;
        private set => _isPlaying = value;
    }

    public event EventHandler<bool>? PlaybackStateChanged;

    public async Task StartAsync()
    {
        if (_isPlaying)
            return;

        _isPlaying = true;
        PlaybackStateChanged?.Invoke(this, true);
        _playCts = new CancellationTokenSource();

        try
        {
            await Task.Run(() => PlaybackLoop(_playCts.Token));
        }
        catch (OperationCanceledException)
        {
            // Normal exit
        }
        catch (Exception ex)
        {
            Log.Error(ex, "BinauralDemoAudioPlayer: Playback error");
        }
        finally
        {
            _isPlaying = false;
            PlaybackStateChanged?.Invoke(this, false);
        }
    }

    public void Stop()
    {
        if (!_isPlaying)
            return;

        _playCts?.Cancel();
        _playCts?.Dispose();
        _playCts = null;
        _isPlaying = false;
        PlaybackStateChanged?.Invoke(this, false);
    }

    private void PlaybackLoop(CancellationToken token)
    {
        var tempFile = Path.Combine(Path.GetTempPath(), "gbc_spatial_demo.wav");

        while (!token.IsCancellationRequested)
        {
            var yaw = SpatialAudioService.Instance.CurrentYaw;

            // Generate a 1.2-second spatial chime centered at the screen
            GenerateSpatialChimeWav(tempFile, yaw);

            if (token.IsCancellationRequested)
                break;

            PlayWavFile(tempFile, token);

            // Interval between chimes
            try
            {
                Task.Delay(1300, token).Wait(token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        try
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    private static void PlayWavFile(string path, CancellationToken token)
    {
        try
        {
            if (OperatingSystem.IsMacOS())
            {
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "afplay",
                    Arguments = $"\"{path}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });

                if (process != null)
                {
                    while (!process.WaitForExit(100))
                    {
                        if (token.IsCancellationRequested)
                        {
                            process.Kill();
                            break;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "BinauralDemoAudioPlayer: afplay failed");
        }
    }

    private static void GenerateSpatialChimeWav(string filePath, float yawDegrees)
    {
        const int sampleRate = 44100;
        const double durationSeconds = 1.0;
        var totalSamples = (int)(sampleRate * durationSeconds);

        // Compute Interaural Level Difference (ILD)
        // Sound source is fixed at 0° (screen).
        // Head yaw: when head turns left (yaw < 0), sound arrives at right ear first and louder.
        var angleRad = (yawDegrees * Math.PI) / 180.0;
        var pan = Math.Sin(angleRad); // -1.0 (hard right in ear) to +1.0 (hard left in ear)

        // Equal power panning
        var leftVol = Math.Cos((pan + 1.0) * Math.PI / 4.0);
        var rightVol = Math.Sin((pan + 1.0) * Math.PI / 4.0);

        // Compute Interaural Time Difference (ITD): maximum human delay is ~0.65ms (~29 samples)
        var maxDelaySamples = (int)(sampleRate * 0.00065);
        var delaySamples = (int)(pan * maxDelaySamples);
        var leftDelay = delaySamples > 0 ? delaySamples : 0;
        var rightDelay = delaySamples < 0 ? -delaySamples : 0;

        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        using var writer = new BinaryWriter(stream);

        // WAV Header
        writer.Write("RIFF"u8);
        writer.Write(36 + (totalSamples * 4));
        writer.Write("WAVEfmt "u8);
        writer.Write(16); // Subchunk1Size (16 for PCM)
        writer.Write((short)1); // AudioFormat (1 = PCM)
        writer.Write((short)2); // NumChannels (2 = Stereo)
        writer.Write(sampleRate);
        writer.Write(sampleRate * 4); // ByteRate
        writer.Write((short)4); // BlockAlign
        writer.Write((short)16); // BitsPerSample
        writer.Write("data"u8);
        writer.Write(totalSamples * 4);

        // Harmonic spatial chime: 528Hz (C) + 792Hz (G) harmonic bell
        for (var i = 0; i < totalSamples; i++)
        {
            var t = (double)i / sampleRate;
            var envelope = Math.Exp(-4.5 * t); // Percussive decay

            var sample1 = Math.Sin(2.0 * Math.PI * 528.0 * t);
            var sample2 = 0.5 * Math.Sin(2.0 * Math.PI * 792.0 * t);
            var sample3 = 0.25 * Math.Sin(2.0 * Math.PI * 1056.0 * t);
            var tone = (sample1 + sample2 + sample3) * envelope * 0.45;

            // Apply delay and volume per channel
            var leftIdx = i - leftDelay;
            var rightIdx = i - rightDelay;

            var leftSample = leftIdx >= 0 ? (short)(tone * leftVol * 32767.0) : (short)0;
            var rightSample = rightIdx >= 0 ? (short)(tone * rightVol * 32767.0) : (short)0;

            writer.Write(leftSample);
            writer.Write(rightSample);
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
