using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform.Model;
using Serilog;

namespace GalaxyBudsClient.Platform;

/// <summary>
/// Unlocks account-free multipoint coexistence on Galaxy Buds.
///
/// Samsung's firmware only lets a second device keep its audio link if that device's <c>asVer</c>
/// record field is 2 or 3; anything else is force-released with reason 0xA9 as soon as the second
/// audio connection comes up. <c>asVer</c> is written exclusively by the <c>MDE_VERSION</c> handler
/// (opcode 0x0B) on the SMEP channel.
///
/// Non-Samsung hosts never send that message, which is why connecting from a PC or Mac normally
/// kicks the phone off. The account hash carried by the same message is not part of the gate for
/// this host (verified on hardware — a wrong hash and a zeroed hash both keep the phone connected,
/// while asVer=0 drops it immediately), so we use the version-only form that leaves the account
/// fields untouched.
///
/// The peer records holding <c>asVer</c> live in the earbuds' RAM, not in flash: booting clears
/// every slot and writes no default, and the routine that repopulates a record when a peer connects
/// restores the account fields from storage but reads <c>asVer</c> straight back out of the live
/// record. So an ordinary disconnect keeps the value and a trip to the charging case loses it, and
/// no sequence of frames makes it durable. That makes this a per-power-session write rather than a
/// one-time patch, which is why it runs on every connection.
/// </summary>
public static class MultipointPatcher
{
    /// <summary>
    /// MDE_VERSION SET blob, version-only variant:
    /// <code>
    ///   04 03 04 | 00 00 | 0B | 02
    ///   |  |  |     |      |    `-- asVer = 2
    ///   |  |  |     |      `------- opcode 0x0B = MDE_VERSION
    ///   |  |  |     `-------------- reserved prefix
    ///   |  |  `-------------------- blob length (4)
    ///   |  `----------------------- TLV type 0x03 (blob)
    ///   `-------------------------- TLV tag 0x04
    /// </code>
    /// Wrapped into WRITE_PROPERTY (0x43) inside an UNK_SPP_ALT (0x01) SMEP frame by
    /// <see cref="SppAlternativeMessage"/>, this encodes to the exact frame captured from a Galaxy
    /// phone and replayed successfully on hardware:
    /// <code>FC 0B 00 01 43 04 03 04 00 00 0B 02 1E AF CC</code>
    /// </summary>
    private static readonly byte[] MdeVersionAsVer2 = [0x04, 0x03, 0x04, 0x00, 0x00, 0x0B, 0x02];

    private const byte MdeVersionOpcode = 0x0B;

    private static readonly SemaphoreSlim Lock = new(1, 1);

    /// <summary>Time to wait for the SMEP channel to report itself connected.</summary>
    private const int ConnectTimeoutMs = 10000;

    /// <summary>
    /// The earbuds' SPP4 server can take a moment to accept connections, so the first attempts
    /// routinely fail with a generic RFCOMM error. Retrying a few times is expected, not exceptional.
    /// </summary>
    private const int ConnectAttempts = 4;

    private const int ConnectRetryDelayMs = 1500;

    /// <summary>Delay before tearing down a freshly established session for the patch sequence.</summary>
    private const int SettleDelayMs = 3000;

    /// <summary>Grace period for a backend to actually tear its stream down before reconnecting.</summary>
    private const int DisconnectSettleMs = 500;

    /// <summary>
    /// How long to keep listening for state frames after the write. The earbuds answer within a
    /// few hundred milliseconds, but they send a burst rather than a single frame.
    /// </summary>
    private const int VerifyWindowMs = 3000;

    /// <summary>
    /// Restoring the regular session raises <see cref="BluetoothImpl.Connected"/> again. Without a
    /// window in which that echo is ignored, the hook would immediately patch itself in a loop.
    /// It also has to expire on its own, so that a failed restore cannot mute the hook for good.
    /// </summary>
    private const int SelfReconnectSuppressMs = 15000;

    private static DateTime _suppressUntil = DateTime.MinValue;

    /// <summary>
    /// The SMEP channel is only exposed by models that also support renaming, which is the same
    /// set of devices this patch applies to.
    /// </summary>
    public static bool IsSupportedByCurrentDevice =>
        BluetoothImpl.Instance.DeviceSpec.Supports(Features.Rename);

    public static void Init()
    {
        BluetoothImpl.Instance.Connected += OnConnected;
    }

    private static async void OnConnected(object? sender, EventArgs e)
    {
        try
        {
            if (!Settings.Data.AutoEnableMultipoint)
                return;

            /* Our own restore reconnects, and patching that would never terminate */
            if (DateTime.UtcNow < _suppressUntil)
                return;

            if (!IsSupportedByCurrentDevice)
            {
                Log.Debug("MultipointPatcher: {Model} has no SMEP channel, skipping",
                    BluetoothImpl.Instance.CurrentModel);
                return;
            }

            /* Let the regular session settle before tearing it down for the patch sequence */
            await Task.Delay(SettleDelayMs);
            if (!BluetoothImpl.Instance.IsConnected)
                return;

            await ApplyAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "MultipointPatcher: Unhandled error in the auto-patch hook");
        }
    }

    /// <summary>
    /// Runs the full patch sequence: drop the regular session, open the SMEP channel, write
    /// MDE_VERSION, then restore the regular session. Safe to call while connected or disconnected.
    /// </summary>
    /// <returns>true if the message was written, false if any step failed.</returns>
    public static async Task<bool> ApplyAsync()
    {
        if (!await Lock.WaitAsync(0))
        {
            Log.Warning("MultipointPatcher: A patch sequence is already running");
            return false;
        }

        try
        {
            /*
             * Connecting blocks the calling thread on some backends (macOS polls the RFCOMM channel
             * for up to 3s inside the native call), and callers reach us from the UI thread, so the
             * whole sequence has to run off it.
             */
            return await Task.Run(RunAsync);
        }
        finally
        {
            Lock.Release();
        }
    }

    private static async Task<bool> RunAsync()
    {
        var bt = BluetoothImpl.Instance;
        var wasConnected = bt.IsConnected;

        try
        {
            Log.Information("MultipointPatcher: Writing MDE_VERSION (asVer=2) over the SMEP channel");

            if (bt.IsConnected)
            {
                await bt.DisconnectAsync();
                /* Backends refuse a new connection while their stream is still up */
                await Task.Delay(DisconnectSettleMs);
            }

            if (!await Dispatcher.UIThread.InvokeAsync(() => bt.SetAltMode(true)))
            {
                Log.Error("MultipointPatcher: Could not switch to alt mode");
                return false;
            }

            if (!await ConnectAltAsync())
            {
                Log.Error("MultipointPatcher: Could not open the SMEP channel");
                return false;
            }

            var reported = await WriteAndReadBackAsync();

            /* SendAltAsync swallows send failures, so the channel state is our only feedback */
            if (!bt.IsConnectedAlternative)
            {
                Log.Error("MultipointPatcher: SMEP channel dropped while writing MDE_VERSION");
                return false;
            }

            switch (reported)
            {
                case 2 or 3:
                    Log.Information("MultipointPatcher: MDE_VERSION written and verified, asVer={AsVer}", reported);
                    return true;
                case { } other:
                    Log.Error("MultipointPatcher: Wrote asVer=2, but the earbuds report {AsVer}", other);
                    return false;
                default:
                    /* The write itself did not fail, so this is not an error, but nothing confirms it either */
                    Log.Warning("MultipointPatcher: MDE_VERSION written, but the earbuds reported no state to verify it against");
                    return true;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "MultipointPatcher: Failed to apply the multipoint unlock");
            return false;
        }
        finally
        {
            await RestoreAsync(wasConnected);
        }
    }

    /// <summary>
    /// Writes MDE_VERSION and collects the state frames the earbuds push in response.
    /// </summary>
    /// <returns>the asVer the earbuds report afterwards, or null if they reported nothing.</returns>
    private static async Task<byte?> WriteAndReadBackAsync()
    {
        var bt = BluetoothImpl.Instance;

        /*
         * A write is answered with a burst of state frames describing the record both before and
         * after the change - one run that set asVer=2 reported 1, 1, 2, 2 - so the last frame is
         * the new value and any earlier one may still be the old one.
         */
        byte? asVer = null;
        void OnStateFrame(object? s, SppAlternativeMessage msg)
        {
            if (TryReadAsVer(msg, out var value))
                asVer = value;
        }

        bt.MessageReceivedAlternative += OnStateFrame;
        try
        {
            await SppAlternativeMessage.WritePropertyAsync(MdeVersionAsVer2);
            await Task.Delay(VerifyWindowMs);
        }
        finally
        {
            bt.MessageReceivedAlternative -= OnStateFrame;
        }

        return asVer;
    }

    /// <summary>
    /// Reads the stored asVer out of a state frame: a NOTIFY_PROPERTY whose payload starts with
    /// <c>02 05 4C 0B</c> and carries the value this host is gated on at offset 6. The earbuds push
    /// one whenever a peer record is re-evaluated, which a write reliably triggers.
    /// </summary>
    private static bool TryReadAsVer(SppAlternativeMessage msg, out byte asVer)
    {
        asVer = 0;

        if (msg.Id != MsgIds.NOTIFY_PROPERTY)
            return false;

        var payload = msg.Payload;
        if (payload.Length < 10 ||
            payload[0] != 0x02 || payload[1] != 0x05 ||
            payload[2] != 0x4C || payload[3] != MdeVersionOpcode)
            return false;

        asVer = payload[6];
        return true;
    }

    private static async Task<bool> ConnectAltAsync()
    {
        var bt = BluetoothImpl.Instance;

        for (var attempt = 1; attempt <= ConnectAttempts; attempt++)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            void OnAltConnected(object? s, EventArgs e) => tcs.TrySetResult(true);
            void OnAltError(object? s, BluetoothException e) => tcs.TrySetResult(false);
            void OnAltDisconnected(object? s, string e) => tcs.TrySetResult(false);

            bt.ConnectedAlternative += OnAltConnected;
            bt.BluetoothErrorAlternative += OnAltError;
            bt.DisconnectedAlternative += OnAltDisconnected;

            try
            {
                if (await bt.ConnectAsync(null, true))
                {
                    var winner = await Task.WhenAny(tcs.Task, Task.Delay(ConnectTimeoutMs));
                    if (winner == tcs.Task && tcs.Task.Result)
                        return true;
                }
            }
            finally
            {
                bt.ConnectedAlternative -= OnAltConnected;
                bt.BluetoothErrorAlternative -= OnAltError;
                bt.DisconnectedAlternative -= OnAltDisconnected;
            }

            Log.Warning("MultipointPatcher: SMEP channel attempt {Attempt}/{Total} failed",
                attempt, ConnectAttempts);

            if (attempt < ConnectAttempts)
                await Task.Delay(ConnectRetryDelayMs);
        }

        return false;
    }

    private static async Task RestoreAsync(bool reconnect)
    {
        var bt = BluetoothImpl.Instance;
        try
        {
            if (bt.AlternativeModeEnabled)
            {
                await bt.DisconnectAsync(true);
                await Task.Delay(DisconnectSettleMs);
            }

            if (!await Dispatcher.UIThread.InvokeAsync(() => bt.SetAltMode(false)))
            {
                Log.Error("MultipointPatcher: Stuck in alt mode, cannot restore the regular connection");
                return;
            }

            if (reconnect)
            {
                _suppressUntil = DateTime.UtcNow.AddMilliseconds(SelfReconnectSuppressMs);
                await bt.ConnectAsync();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "MultipointPatcher: Failed to restore the regular connection");
        }
    }
}
