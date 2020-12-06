using System;
using System.Text.RegularExpressions;

namespace GalaxyBudsClient.Utils
{
    static class ByteArrayUtils
    {
        public static byte[] AddByteToArray(byte[] bArray, byte newByte)
        {
            byte[] newArray = new byte[bArray.Length + 1];
            bArray.CopyTo(newArray, 0);
            newArray[newArray.Length - 1] = newByte;
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

        /**
         * Throws ArgumentOutOfRangeException in case of bad format
         **/
        public static byte[] HexStringToByteArray(this String hex)
        {
            hex = Regex.Replace(hex, @"\s+", "");

            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];

            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

            return bytes;
        }
    }
}
