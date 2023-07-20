using System.Linq;
using System.Text;

namespace ThePBone.OSX.Native
{
    public static class Utils
    {
        public static byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            var offset = 0;
            foreach (byte[] array in arrays) {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
        
        public static string DeepCopy(this string str)
        {
            StringBuilder sb = new();
            sb.Append(str);
            return sb.ToString();
        }

        public static void SwapBytes(this byte[] buf, int i, int j)
        {
            var temp = buf[i];
            buf[i] = buf[j];
            buf[j] = temp;
        }
        
        public static void FixEndiannessOfGuidBytes(this byte[] self)
        {
            self.SwapBytes(0, 3);
            self.SwapBytes(1, 2);
            self.SwapBytes(6, 7);
        }
    }
}