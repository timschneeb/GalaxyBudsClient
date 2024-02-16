using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace GalaxyBudsClient.Model.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class ModelMetadataAttribute : Attribute
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

    // TODO: all of these Get<Attribute> methods are very similar, they could be refactored into a single method
    internal static class ModelMetadataAttributeExtension
    {
        public static ModelMetadataAttribute? GetModelMetadata<T>(this T e) where T : IConvertible
        {
            if (e is not Enum) 
                return null;
            
            var type = e.GetType();
            foreach (var obj in Enum.GetValues(type))
            {
                if (obj == null || (int)obj != e.ToInt32(CultureInfo.InvariantCulture))
                    continue;
                    
                var memInfo = type.GetMember(type.GetEnumName((int) obj) ?? string.Empty);

                if (memInfo[0]
                        .GetCustomAttributes(typeof(ModelMetadataAttribute), false)
                        .FirstOrDefault() is ModelMetadataAttribute attribute)
                {
                    return attribute;
                }
            }

            return null;
        }
    }
}
