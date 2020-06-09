using System;
using System.Text.RegularExpressions;
using System.Windows;

namespace Galaxy_Buds_Client.util
{
    static class ByteArrayUtils
    {
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

        public static byte[] HexStringToByteArray(this String hex)
        {
            hex = Regex.Replace(hex, @"\s+", "");

            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            try
            {
                for (int i = 0; i < NumberChars; i += 2)
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Bad Format, uneven count of chars.\nYou need two Hex chars per byte!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return new byte[0];
            }

            return bytes;
        }
    }
}
