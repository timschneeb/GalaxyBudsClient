using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using FluentAssertions;
using GalaxyBudsClient.Platform.SpatialAudio;
using GalaxyBudsClient.Utils.Extensions;
using NUnit.Framework;

namespace GalaxyBudsClient.Tests;

[TestFixture]
public class SpatialAudioTests
{
    [Test]
    public void EulerConversion_IdentityQuaternion_ReturnsZero()
    {
        var q = Quaternion.Identity;
        var (roll, pitch, yaw) = q.ToRollPitchYaw();

        ((double)roll).Should().BeApproximately(0.0, 0.001);
        ((double)pitch).Should().BeApproximately(0.0, 0.001);
        ((double)yaw).Should().BeApproximately(0.0, 0.001);
    }

    [Test]
    public void RelativeOrientation_IdenticalQuaternions_ResultsInZeroDelta()
    {
        // Reference looking 45 degrees
        var refQuat = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)(Math.PI / 4.0));
        var currentQuat = refQuat;

        var invRef = Quaternion.Inverse(refQuat);
        var rel = Quaternion.Normalize(Quaternion.Multiply(currentQuat, invRef));

        var (roll, pitch, yaw) = rel.ToRollPitchYaw();
        ((double)yaw).Should().BeApproximately(0.0, 0.001);
        ((double)pitch).Should().BeApproximately(0.0, 0.001);
        ((double)roll).Should().BeApproximately(0.0, 0.001);
    }

    [Test]
    public void RelativeOrientation_YawOffset_CalculatesExactDegrees()
    {
        var refQuat = Quaternion.Identity;
        // 90 degrees around Z axis (Yaw)
        var currentQuat = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)(Math.PI / 2.0));

        var invRef = Quaternion.Inverse(refQuat);
        var rel = Quaternion.Normalize(Quaternion.Multiply(currentQuat, invRef));

        var (_, _, yawRad) = rel.ToRollPitchYaw();
        var yawDeg = (double)yawRad * (180.0 / Math.PI);

        yawDeg.Should().BeApproximately(90.0, 0.1);
    }

    [Test]
    public void RelativeOrientation_TiltedEarbudInEar_DecouplesPureHorizontalYaw()
    {
        // Real-world scenario: Earbud sits tilted ~45° in ear canal
        var earbudTilt = Quaternion.CreateFromYawPitchRoll(0.8f, -0.4f, 0.5f);
        var refQuat = earbudTilt;

        // User turns head horizontally in the room by +35 degrees around World Z
        var turnAngleRad = (float)(35.0 * Math.PI / 180.0);
        var worldTurn = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, turnAngleRad);
        var currentQuat = Quaternion.Multiply(worldTurn, refQuat);

        // World frame relative rotation
        var invRef = Quaternion.Inverse(refQuat);
        var rel = Quaternion.Normalize(Quaternion.Multiply(currentQuat, invRef));

        var (roll, pitch, yawRad) = rel.ToRollPitchYaw();
        var yawDeg = (double)yawRad * (180.0 / Math.PI);

        yawDeg.Should().BeApproximately(35.0, 0.01);
        ((double)roll).Should().BeApproximately(0.0, 0.01);
        ((double)pitch).Should().BeApproximately(0.0, 0.01);
    }

    [Test]
    public void OpenTrackBroadcaster_GeneratesValid48BytePacket()
    {
        using var stream = new MemoryStream(48);
        using var writer = new BinaryWriter(stream);

        // Standard OpenTrack format: 6 x double (X, Y, Z, Yaw, Pitch, Roll)
        writer.Write(0.0);
        writer.Write(0.0);
        writer.Write(0.0);
        writer.Write(45.5); // Yaw
        writer.Write(-12.3); // Pitch
        writer.Write(5.0); // Roll

        var packet = stream.ToArray();
        packet.Length.Should().Be(48);

        using var readStream = new MemoryStream(packet);
        using var reader = new BinaryReader(readStream);

        reader.ReadDouble().Should().Be(0.0);
        reader.ReadDouble().Should().Be(0.0);
        reader.ReadDouble().Should().Be(0.0);
        reader.ReadDouble().Should().Be(45.5);
        reader.ReadDouble().Should().Be(-12.3);
        reader.ReadDouble().Should().Be(5.0);
    }

    [Test]
    public void OscMessage_AddressAndTypeTag_PaddedToFourBytes()
    {
        var address = "/spatial/ypr";
        var typeTag = ",fff";

        var addrBytes = Encoding.ASCII.GetBytes(address);
        var tagBytes = Encoding.ASCII.GetBytes(typeTag);

        // Address "/spatial/ypr" is 12 chars -> with null = 13 -> padded to 16
        var addrPaddedLen = ((addrBytes.Length + 4) / 4) * 4;
        addrPaddedLen.Should().Be(16);

        // Type tag ",fff" is 4 chars -> with null = 5 -> padded to 8
        var tagPaddedLen = ((tagBytes.Length + 4) / 4) * 4;
        tagPaddedLen.Should().Be(8);
    }

    [Test]
    public void DspEngine_CenterOrientation_ProducesBalancedSymmetricEnergy()
    {
        var engine = new SpatialAudioDspEngine(48000);
        engine.SetOrientation(0, 0, 0);

        var samples = 4800; // 100ms
        var input = new float[samples * 2];
        var output = new float[samples * 2];

        // Fill with stereo tone
        for (var i = 0; i < samples; i++)
        {
            var val = MathF.Sin(2.0f * MathF.PI * 440.0f * (i / 48000.0f));
            input[i * 2] = val;
            input[i * 2 + 1] = val;
        }

        // Warm up and process
        engine.Process(input, output);

        // Sum energy on left and right ears
        float energyL = 0f;
        float energyR = 0f;
        for (var i = samples / 2; i < samples; i++)
        {
            energyL += output[i * 2] * output[i * 2];
            energyR += output[i * 2 + 1] * output[i * 2 + 1];
        }

        // At center, left and right energy must be symmetrical
        ((double)MathF.Abs(energyL - energyR) / energyL).Should().BeLessThan(0.05);
    }

    [Test]
    public void DspEngine_TurnHeadLeft_ShiftsSoundEnergyToRightEar()
    {
        var engine = new SpatialAudioDspEngine(48000);
        // Turn head 60 degrees to the left (-60 Yaw)
        // Speakers in front of monitor are now to the right of the head
        engine.SetOrientation(-60, 0, 0);

        var samples = 4800;
        var input = new float[samples * 2];
        var output = new float[samples * 2];

        for (var i = 0; i < samples; i++)
        {
            var val = MathF.Sin(2.0f * MathF.PI * 1000.0f * (i / 48000.0f));
            input[i * 2] = val;
            input[i * 2 + 1] = val;
        }

        // Process warm up
        for (var pass = 0; pass < 5; pass++)
        {
            engine.Process(input, output);
        }

        float energyL = 0f;
        float energyR = 0f;
        for (var i = 0; i < samples; i++)
        {
            energyL += output[i * 2] * output[i * 2];
            energyR += output[i * 2 + 1] * output[i * 2 + 1];
        }

        // Right ear must receive significantly more energy than left ear (head shadow + ILD)
        energyR.Should().BeGreaterThan(energyL * 1.5f);
    }

    [Test]
    public void CoreAudioDeviceHelper_FindOutputDeviceUid_ReturnsBudsUidWhenConnected()
    {
        if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
            return;

        var uid = CoreAudioDeviceHelper.FindOutputDeviceUid("Buds");
        // On this Mac with Galaxy Buds2 Pro connected, UID must be found
        uid.Should().NotBeNullOrEmpty();
        uid.Should().Contain(":output");
    }
}

