using System;
using System.Runtime.InteropServices;
using Serilog;

namespace GalaxyBudsClient.Platform.SpatialAudio;

public sealed unsafe class MacOsSpatialAudioSink : ISpatialAudioSink
{
    private const string AudioToolboxLib = "/System/Library/Frameworks/AudioToolbox.framework/AudioToolbox";

    [StructLayout(LayoutKind.Sequential)]
    private struct AudioStreamBasicDescription
    {
        public double mSampleRate;
        public uint mFormatID;
        public uint mFormatFlags;
        public uint mBytesPerPacket;
        public uint mFramesPerPacket;
        public uint mBytesPerFrame;
        public uint mChannelsPerFrame;
        public uint mBitsPerChannel;
        public uint mReserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AudioQueueBuffer
    {
        public uint mAudioDataBytesCapacity;
        public void* mAudioData;
        public uint mAudioDataByteSize;
        public void* mUserData;
        public uint mPacketDescriptionCapacity;
        public void* mPacketDescriptions;
        public uint mPacketDescriptionCount;
    }

    private delegate void AudioQueueOutputCallback(IntPtr userData, IntPtr aq, IntPtr buffer);

    [DllImport(AudioToolboxLib)]
    private static extern int AudioQueueNewOutput(
        ref AudioStreamBasicDescription inFormat,
        AudioQueueOutputCallback inCallbackProc,
        IntPtr inUserData,
        IntPtr inCallbackRunLoop,
        IntPtr inCallbackRunLoopMode,
        uint inFlags,
        out IntPtr outAq);

    [DllImport(AudioToolboxLib)]
    private static extern int AudioQueueAllocateBuffer(IntPtr inAq, uint inBufferByteSize, out IntPtr outBuffer);

    [DllImport(AudioToolboxLib)]
    private static extern int AudioQueueEnqueueBuffer(IntPtr inAq, IntPtr inBuffer, uint inNumPacketDescs, IntPtr inPacketDescs);

    [DllImport(AudioToolboxLib)]
    private static extern int AudioQueueStart(IntPtr inAq, IntPtr inStartTime);

    [DllImport(AudioToolboxLib)]
    private static extern int AudioQueueStop(IntPtr inAq, bool inImmediate);

    [DllImport(AudioToolboxLib)]
    private static extern int AudioQueueDispose(IntPtr inAq, bool inImmediate);

    [DllImport(AudioToolboxLib)]
    private static extern int AudioQueueSetProperty(
        IntPtr inAQ,
        uint inID,
        void* inData,
        uint inDataSize);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation", CharSet = CharSet.Unicode)]
    private static extern IntPtr CFStringCreateWithCharacters(IntPtr alloc, string str, nint count);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static extern void CFRelease(IntPtr cf);

    private const int BufferCount = 3;
    private const int BufferFrames = 1024; // ~21ms at 48kHz
    private const int ChannelCount = 2;

    private IntPtr _audioQueue;
    private readonly IntPtr[] _buffers = new IntPtr[BufferCount];
    private AudioQueueOutputCallback? _callback;
    private Func<Span<float>, int>? _readSamplesCallback;
    private bool _isRunning;
    private readonly object _lock = new();

    public bool IsRunning => _isRunning;
    public int SampleRate { get; }

    public MacOsSpatialAudioSink(int sampleRate = 48000)
    {
        SampleRate = sampleRate;
    }

    public void Start(Func<Span<float>, int> readStereoSamplesCallback)
    {
        lock (_lock)
        {
            if (_isRunning) return;
            _readSamplesCallback = readStereoSamplesCallback;

            var desc = new AudioStreamBasicDescription
            {
                mSampleRate = SampleRate,
                mFormatID = 0x6c70636d, // 'lpcm'
                mFormatFlags = (1 << 0) | (1 << 3), // kAudioFormatFlagIsFloat (1) | kAudioFormatFlagIsPacked (8) = 0x9
                mBytesPerPacket = 8,
                mFramesPerPacket = 1,
                mBytesPerFrame = 8,
                mChannelsPerFrame = ChannelCount,
                mBitsPerChannel = 32,
                mReserved = 0
            };

            _callback = OnBufferComplete;
            var err = AudioQueueNewOutput(ref desc, _callback, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, out _audioQueue);
            if (err != 0)
            {
                Log.Error("MacOsSpatialAudioSink: AudioQueueNewOutput failed with error {Error}", err);
                return;
            }

            // Route audio explicitly to Galaxy Buds so it doesn't loop back into BlackHole or system default
            var targetUid = CoreAudioDeviceHelper.FindOutputDeviceUid("Buds");
            if (!string.IsNullOrEmpty(targetUid))
            {
                var cfTargetUid = CFStringCreateWithCharacters(IntPtr.Zero, targetUid, targetUid.Length);
                if (cfTargetUid != IntPtr.Zero)
                {
                    try
                    {
                        var pUid = cfTargetUid;
                        // 0x61716364 = 'aqcd' (kAudioQueueProperty_CurrentDevice)
                        var devStatus = AudioQueueSetProperty(_audioQueue, 0x61716364, &pUid, (uint)sizeof(IntPtr));
                        if (devStatus == 0)
                        {
                            Log.Information("MacOsSpatialAudioSink: Áudio 360 direcionado com sucesso aos Galaxy Buds ({Uid})", targetUid);
                        }
                        else
                        {
                            Log.Warning("MacOsSpatialAudioSink: Não foi possível vincular a saída ao {Uid}, status {Status}", targetUid, devStatus);
                        }
                    }
                    finally
                    {
                        CFRelease(cfTargetUid);
                    }
                }
            }

            var bufferBytes = (uint)(BufferFrames * ChannelCount * sizeof(float));
            for (var i = 0; i < BufferCount; i++)
            {
                var allocErr = AudioQueueAllocateBuffer(_audioQueue, bufferBytes, out _buffers[i]);
                if (allocErr != 0)
                {
                    Log.Error("MacOsSpatialAudioSink: AudioQueueAllocateBuffer failed with {Error}", allocErr);
                }
                FillAndEnqueueBuffer(_buffers[i]);
            }

            var startErr = AudioQueueStart(_audioQueue, IntPtr.Zero);
            if (startErr != 0)
            {
                Log.Error("MacOsSpatialAudioSink: AudioQueueStart failed with {Error}", startErr);
            }
            _isRunning = true;
            Log.Information("MacOsSpatialAudioSink: Started low-latency audio queue ({SampleRate}Hz)", SampleRate);
        }
    }

    private void FillAndEnqueueBuffer(IntPtr bufferPtr)
    {
        var buffer = (AudioQueueBuffer*)bufferPtr;
        var maxSamples = (int)(buffer->mAudioDataBytesCapacity / sizeof(float));
        var floatSpan = new Span<float>(buffer->mAudioData, maxSamples);

        var samplesFilled = _readSamplesCallback?.Invoke(floatSpan) ?? 0;
        if (samplesFilled <= 0)
        {
            floatSpan.Clear();
            samplesFilled = maxSamples;
        }

        buffer->mAudioDataByteSize = (uint)(samplesFilled * sizeof(float));
        var enqErr = AudioQueueEnqueueBuffer(_audioQueue, bufferPtr, 0, IntPtr.Zero);
        if (enqErr != 0)
        {
            Log.Warning("MacOsSpatialAudioSink: AudioQueueEnqueueBuffer failed with {Error}", enqErr);
        }
    }

    private void OnBufferComplete(IntPtr userData, IntPtr aq, IntPtr bufferPtr)
    {
        if (!_isRunning) return;
        FillAndEnqueueBuffer(bufferPtr);
    }

    public void Stop()
    {
        lock (_lock)
        {
            if (!_isRunning) return;
            _isRunning = false;

            if (_audioQueue != IntPtr.Zero)
            {
                AudioQueueStop(_audioQueue, true);
                AudioQueueDispose(_audioQueue, true);
                _audioQueue = IntPtr.Zero;
            }

            _readSamplesCallback = null;
            Log.Information("MacOsSpatialAudioSink: Stopped");
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
