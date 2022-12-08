using amdocs.ginger.GingerCoreNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.TelemetryLib
{
    public class UsedFeatureDetail
    {
        public string Name { get; set; }
        [DefaultValueAttribute("")]
        public string PlatformType { get; set; } = string.Empty;
        [DefaultValueAttribute(false)]
        public bool? IsConfigured { get; set; } = false;
        [DefaultValueAttribute(false)]
        public bool? IsUsed { get; set; } = false;

        public UsedFeatureDetail(string name, bool? isConfigured, bool isUsed, string platformType = "")
        {
            Name = name;
            IsConfigured = isConfigured;
            IsUsed = isUsed;
            PlatformType = platformType;
        }

        public static void AddOrModifyFeatureDetail(string featureName, bool? isConfigured, bool isUsed, string platformType="")
        {
            UsedFeatureDetail usedFeatureDetail = WorkSpace.Instance.Telemetry.TelemetrySession.UsedFeatures.Where(x => x.Name == featureName && x.PlatformType == platformType).FirstOrDefault();
            if (usedFeatureDetail != null)
            {
                int index = WorkSpace.Instance.Telemetry.TelemetrySession.UsedFeatures.IndexOf(usedFeatureDetail);
                if (usedFeatureDetail.IsUsed == false) 
                {
                    usedFeatureDetail.IsUsed = isUsed;
                }
                if (usedFeatureDetail.IsConfigured == false)
                {
                    usedFeatureDetail.IsConfigured = isConfigured;
                }
                usedFeatureDetail.PlatformType = platformType;
                WorkSpace.Instance.Telemetry.TelemetrySession.UsedFeatures[index] = usedFeatureDetail;
            }
            else
            {
                usedFeatureDetail = new UsedFeatureDetail(featureName, isConfigured, isUsed, platformType);
                WorkSpace.Instance.Telemetry.TelemetrySession.UsedFeatures.Add(usedFeatureDetail);
            }
        }
    }
}
