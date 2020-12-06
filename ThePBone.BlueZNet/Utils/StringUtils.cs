using System;

namespace ThePBone.BlueZNet.Utils
{
    public class StringUtils
    {
        public static string ExpandUuid(string uuid)
        {
            uuid = uuid.ToLower();
            
            switch (uuid.Length)
            {
                case 4:
                    return $"0000{uuid}-0000-1000-8000-00805f9b34fb";
                case 8:
                    return $"{uuid}-0000-1000-8000-00805f9b34fb";
                case 36:
                    return uuid;
                default:
                    throw new ArgumentException($"'{uuid}' is not a valid UUID.");
            }
        }
    }
}