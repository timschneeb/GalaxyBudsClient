using System;
using System.Runtime.InteropServices;
using Serilog;

namespace GalaxyBudsClient.Platform.SpatialAudio;

/// <summary>
/// Low-latency macOS CoreAudio input capture for system audio loopback (e.g. BlackHole 2ch).
/// </summary>
public sealed unsafe class MacOsSpatialAudioCapture : IDisposable
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

    private delegate void AudioQueueInputCallback(
        IntPtr inUserData,
        IntPtr inAQ,
        IntPtr inBuffer,
        IntPtr inStartTime,
        uint inNumberPacketDescriptions,
        IntPtr inPacketDescs);

    [DllImport(AudioToolboxLib)]
    private static extern int AudioQueueNewInput(
        ref AudioStreamBasicDescription inFormat,
        AudioQueueInputCallback inCallbackProc,
        IntPtr inUserData,
        IntPtr inCallbackRunLoop,
        IntPtr inCallbackRunLoopMode,
        uint inFlags,
        out IntPtr outAQ);

    [DllImport(AudioToolboxLib)]
    private static extern int AudioQueueAllocateBuffer(IntPtr inAQ, uint inBufferByteSize, out IntPtr outBuffer);

    [DllImport(AudioToolboxLib)]
    private static extern int AudioQueueEnqueueBuffer(IntPtr inAQ, IntPtr inBuffer, uint inNumPacketDescs, IntPtr inPacketDescs);

    [DllImport(AudioToolboxLib)]
    private static extern int AudioQueueStart(IntPtr inAQ, IntPtr inStartTime);

    [DllImport(AudioToolboxLib)]
    private static extern int AudioQueueStop(IntPtr inAQ, bool inImmediate);

    [DllImport(AudioToolboxLib)]
    private static extern int AudioQueueDispose(IntPtr inAQ, bool inImmediate);

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
    private const int BufferFrames = 1024;
    private const int ChannelCount = 2;

    private IntPtr _audioQueue;
    private readonly IntPtr[] _buffers = new IntPtr[BufferCount];
    private AudioQueueInputCallback? _callback;
    private Action<Span<float>>? _onSamplesCaptured;
    private bool _isRunning;
    private readonly object _lock = new();

    public bool IsRunning => _isRunning;
    public int SampleRate { get; }

    public MacOsSpatialAudioCapture(int sampleRate = 44100)
    {
        SampleRate = sampleRate;
    }

    public void Start(Action<Span<float>> onSamplesCaptured)
    {
        lock (_lock)
        {
            if (_isRunning) return;
            _onSamplesCaptured = onSamplesCaptured;

            var desc = new AudioStreamBasicDescription
            {
                mSampleRate = SampleRate,
                mFormatID = 0x6c70636d, // 'lpcm'
                mFormatFlags = (1 << 0) | (1 << 3), // Float32 | Packed = 0x9
                mBytesPerPacket = 8,
                mFramesPerPacket = 1,
                mBytesPerFrame = 8,
                mChannelsPerFrame = ChannelCount,
                mBitsPerChannel = 32,
                mReserved = 0
            };

            _callback = OnInputBuffer;
            var gcHandle = GCHandle.Alloc(this);
            var userData = GCHandle.ToIntPtr(gcHandle);

            // Strict check: BlackHole must be actively loaded in CoreAudio to prevent fallback to physical microphone
            if (!BlackHoleHelper.IsLoadedInCoreAudio())
            {
                throw new InvalidOperationException(
                    "Driver BlackHole 2ch não está carregado no CoreAudio. " +
                    "Por favor, clique em 'Reiniciar CoreAudio' ou reinicie o sistema antes de ativar o áudio 360.");
            }

            var status = AudioQueueNewInput(
                ref desc,
                _callback,
                userData,
                IntPtr.Zero,
                IntPtr.Zero,
                0,
                out _audioQueue);

            if (status != 0)
            {
                gcHandle.Free();
                throw new InvalidOperationException($"AudioQueueNewInput failed with error {status}");
            }

            // Bind explicitly to BlackHole 2ch so macOS never captures the room microphone
            const string deviceUid = "BlackHole2ch_UID";
            var cfDeviceUid = CFStringCreateWithCharacters(IntPtr.Zero, deviceUid, deviceUid.Length);
            if (cfDeviceUid != IntPtr.Zero)
            {
                try
                {
                    var pUid = cfDeviceUid;
                    // 0x61716364 = 'aqcd' (kAudioQueueProperty_CurrentDevice)
                    var devStatus = AudioQueueSetProperty(_audioQueue, 0x61716364, &pUid, (uint)sizeof(IntPtr));
                    if (devStatus != 0)
                    {
                        Stop();
                        throw new InvalidOperationException($"Falha ao associar a entrada de áudio ao BlackHole 2ch (código {devStatus}). O microfone não será utilizado.");
                    }
                    Log.Information("MacOsSpatialAudioCapture: Conectado com sucesso ao dispositivo BlackHole 2ch ({Uid})", deviceUid);
                }
                finally
                {
                    CFRelease(cfDeviceUid);
                }
            }

            var bufferBytes = (uint)(BufferFrames * ChannelCount * sizeof(float));
            for (var i = 0; i < BufferCount; i++)
            {
                status = AudioQueueAllocateBuffer(_audioQueue, bufferBytes, out _buffers[i]);
                if (status != 0)
                {
                    Stop();
                    throw new InvalidOperationException($"AudioQueueAllocateBuffer failed with error {status}");
                }

                AudioQueueEnqueueBuffer(_audioQueue, _buffers[i], 0, IntPtr.Zero);
            }

            status = AudioQueueStart(_audioQueue, IntPtr.Zero);
            if (status != 0)
            {
                Stop();
                throw new InvalidOperationException($"AudioQueueStart failed with error {status}");
            }

            _isRunning = true;
            Log.Information("MacOsSpatialAudioCapture: Started system audio capture queue ({SampleRate}Hz)", SampleRate);
        }
    }

    private static void OnInputBuffer(
        IntPtr inUserData,
        IntPtr inAQ,
        IntPtr inBuffer,
        IntPtr inStartTime,
        uint inNumberPacketDescriptions,
        IntPtr inPacketDescs)
    {
        if (inUserData == IntPtr.Zero || inBuffer == IntPtr.Zero) return;

        try
        {
            var handle = GCHandle.FromIntPtr(inUserData);
            if (!handle.IsAllocated || handle.Target is not MacOsSpatialAudioCapture capture || !capture._isRunning)
                return;

            var pBuffer = (AudioQueueBuffer*)inBuffer;
            var sampleCount = (int)(pBuffer->mAudioDataByteSize / sizeof(float));
            if (sampleCount > 0 && pBuffer->mAudioData != null)
            {
                var span = new Span<float>(pBuffer->mAudioData, sampleCount);
                capture._onSamplesCaptured?.Invoke(span);
            }

            AudioQueueEnqueueBuffer(inAQ, inBuffer, 0, IntPtr.Zero);
        }
        catch (Exception ex)
        {
            Log.Verbose(ex, "MacOsSpatialAudioCapture: Error in input buffer callback");
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            if (!_isRunning && _audioQueue == IntPtr.Zero) return;
            _isRunning = false;

            Log.Information("MacOsSpatialAudioCapture: Stopping capture queue");
            if (_audioQueue != IntPtr.Zero)
            {
                AudioQueueStop(_audioQueue, true);
                AudioQueueDispose(_audioQueue, true);
                _audioQueue = IntPtr.Zero;
            }

            for (var i = 0; i < BufferCount; i++)
            {
                _buffers[i] = IntPtr.Zero;
            }

            _onSamplesCaptured = null;
        }
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}
