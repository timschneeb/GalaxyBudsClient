namespace GalaxyBudsClient.Platform.OSX
{
    public static class Utils
    {
        private static void SwapBytes(this byte[] buf, int i, int j)
        {
            (buf[i], buf[j]) = (buf[j], buf[i]);
        }
        
        public static void FixEndiannessOfGuidBytes(this byte[] self)
        {
            self.SwapBytes(0, 3);
            self.SwapBytes(1, 2);
            self.SwapBytes(4, 5);
            self.SwapBytes(6, 7);
        }
    }
}