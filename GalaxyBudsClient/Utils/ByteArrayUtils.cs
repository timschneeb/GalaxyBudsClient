using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GalaxyBudsClient.Utils;

public static class ByteArrayUtils
{
    public static byte[] Combine(params byte[][] arrays)
    {
        var rv = new byte[arrays.Sum(a => a.Length)];
        var offset = 0;
        foreach (var array in arrays) {
            Buffer.BlockCopy(array, 0, rv, offset, array.Length);
            offset += array.Length;
        }
        return rv;
    }
        
    public static int ValueOfBinaryDigit(byte b, int i) {
        return b & (1 << i);
    }

    public static byte ValueOfLeft(byte b) {
        return (byte) ((b & 240) >> 4);
    }

    public static byte ValueOfRight(byte b) {
        return (byte) (b & 15);
    }

    public static byte[] RTrimBytes(this byte[] bytes)
    {
        if (bytes.Length == 0) return bytes;
        var i = bytes.Length - 1;
        while (bytes[i] == 0)
        {
            i--;
        }
        var copy = new byte[i + 1];
        Array.Copy(bytes, copy, i + 1);
        return copy;
    }

    public static bool IsBufferZeroedOut(IEnumerable<byte>? buffer)
    {
        if (buffer == null)
        {
            return true;
        }
            
        foreach(var value in buffer)
        {
            if (value != 0x00)
            {
                return false;
            }
        }

        return true;
    }
        
    /**
     * Throws ArgumentOutOfRangeException and FormatException in case of bad format
     **/
    public static byte[] HexStringToByteArray(this string? hex)
    {
        if (hex == null)
        {
            return Array.Empty<byte>();
        }
            
        hex = Regex.Replace(hex, @"\s+", "");

        var numberChars = hex.Length;
        var bytes = new byte[numberChars / 2];
            
        for (var i = 0; i < numberChars; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

        return bytes;
    }
}