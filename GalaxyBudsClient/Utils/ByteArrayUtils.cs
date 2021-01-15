using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;

namespace GalaxyBudsClient.Utils
{
    public static class ByteArrayUtils
    {
        public static byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays) {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
        
        public static int ValueOfBinaryDigit(byte b, int i) {
            return b & (1 << i);
        }

        public static byte[] AddByteToArray(byte[] bArray, byte newByte)
        {
            byte[] newArray = new byte[bArray.Length + 1];
            bArray.CopyTo(newArray, 0);
            newArray[^1] = newByte;
            return newArray;
        }

        public static Byte[] RTrimBytes(this Byte[] bytes)
        {
            if (bytes.Length == 0) return bytes;
            var i = bytes.Length - 1;
            while (bytes[i] == 0)
            {
                i--;
            }
            Byte[] copy = new Byte[i + 1];
            Array.Copy(bytes, copy, i + 1);
            return copy;
        }

        public static bool IsBufferZeroedOut(ArrayList? buffer)
        {
            if (buffer == null)
            {
                return true;
            }
            
            foreach(var value in buffer)
            {
                if (value is byte b && b != 0x00)
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
                return new byte[0];
            }
            
            hex = Regex.Replace(hex, @"\s+", "");

            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

            return bytes;
        }
    }
}
