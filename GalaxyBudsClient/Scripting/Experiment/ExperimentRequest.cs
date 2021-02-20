using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Org.BouncyCastle.Crypto;
using Serilog;

namespace GalaxyBudsClient.Scripting.Experiment
{
    public enum Environment
    {
        ProductionOnly,
        Production,
        Internal,
    }
    
    public class ExperimentRequest
    {
        public long Id { set; get; }
        public string? Name { set; get; }
        public Environment? Environment { set; get; }

        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public Models[] TargetDevices { set; get; } = new Models[0];
        public int? TimeConstraint { set; get; }
        public int? MinimumRevision { set; get; }
        public int? MaximumRevision { set; get; }
        public string? MinimumAppVersion { set; get; }
        public string? MaximumAppVersion { set; get; }
        
        public string? Signature { set; get; }
        public string? Script { set; get; }
    }

    public static class ExperimentRequestFilters
    {
        private const string PublicSigningKey =
            @"-----BEGIN PUBLIC KEY-----
            MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAmPvttLEiqvE9alr32EEe
            8FBNfvYSi+sGWpSDwCpcc1g2rcKgTV9tPHXtF76AAu/htkUGxb/0KVUGKquLbpq1
            10XGnQILO5z0ivvBCiekeEO2GV+CFzIS5OmFdMt5A7cVzHrGFQ3QVutmJP+8N6wU
            kXFluL/iaBEGKbFOoRfAsk99dyVRDHolIXA7Ueb5ksEyehZNFTmr9CrEmo3rcLSe
            PkqQBFkYu3w14qspP9mw0UwreNf2diPpVTMIOXmX+6FDu2R+vaK3sq90UILEXQXc
            wGf54D0Q5oRjJu0HOYPEayAxNK7h7buOXK7Ymct1yamuqS2sxlhggIA0advkplzS
            cwIDAQAB
            -----END PUBLIC KEY-----";
        
        public static ExperimentRequest? VerifyDecode(this ExperimentRequest item)
        {
            try
            {
                item.Signature = Crypto.RsaDecryptWithPublic(item.Signature ?? string.Empty, PublicSigningKey);
                if (item.Signature != $"Experiment{item.Id}")
                {
                    Log.Error("ExperimentRequest.VerifyDecode: Unknown signature, discarding entry!");
                    return null;
                }

                item.Script = Encoding.UTF8.GetString(Convert.FromBase64String(item.Script ?? string.Empty));
                return item;
            }
            catch (InvalidCipherTextException ex)
            {
                Log.Error($"ExperimentRequest.VerifyDecode: Malformed cipher text, discarding entry! ({ex.Message})");
                return null;
            }
        }
        
        public static IEnumerable<ExperimentRequest> VerifyDecode(this IEnumerable<ExperimentRequest> items)
        {
            return items.Select(VerifyDecode).Where(x => x != null).Select(x => x!);
        }
        
        public static bool FilterByEnvironment(ExperimentRequest item)
        {
#if DEBUG
            return item.Environment != Environment.ProductionOnly;
#else
            return item.Environment != Environment.Internal;
#endif
        }
        
        public static bool FilterByVersion(ExperimentRequest item)
        {
            var pass = true;
            var revision = DeviceMessageCache.Instance.ExtendedStatusUpdate?.Revision ?? 0;
            var currentVersion =
                new Version(Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? string.Empty);

            if (item.MinimumRevision != null && revision < item.MinimumRevision)
            {
                pass = false;
            }
            if (item.MaximumRevision != null && revision > item.MaximumRevision)
            {
                pass = false;
            }
            if (item.MinimumAppVersion != null && currentVersion < new Version(item.MinimumAppVersion))
            {
                pass = false;
            }
            if (item.MaximumAppVersion != null && currentVersion > new Version(item.MaximumAppVersion))
            {
                pass = false;
            }

            return pass;
        }
        
        public static bool IsNotDone(ExperimentRequest item)
        {
            return !(SettingsProvider.Instance.Experiments.FinishedIds?.ToList()?.Contains(item.Id) ?? false);
        }
    }
}