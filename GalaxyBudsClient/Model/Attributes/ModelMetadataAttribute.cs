using System;

namespace GalaxyBudsClient.Model.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ModelMetadataAttribute : Attribute
    {
        /**
         * Friendly name, used for display purposes
         */
        public required string Name { get; set; }
        /**
         * Used to detect corresponding device models for firmware update files
         * Firmware archives do not contain model information in their headers,
         * so we check whether the pattern matches the binary
         */
        public required string FwPattern { get; set; }
        /**
         * Used to detect corresponding device models from build strings found in DEBUG_GET_ALL_DATA
         */
        public required string BuildPrefix { get; set; }
    }
}
