using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Common.Enums;
using static Amdocs.Ginger.Common.UIElement.ElementInfo;

namespace Amdocs.Ginger.Common.UIElement
{
    public class HTMLElementProperty : ControlProperty
    {


        public eDeltaStatus DeltaStatus { get; set; }

        public eImageType DeltaStatusIcon
        {
            get
            {
                switch (DeltaStatus)
                {
                    case ElementInfo.eDeltaStatus.Deleted:
                        return eImageType.Deleted;
                    case ElementInfo.eDeltaStatus.Modified:
                        return eImageType.Modified;
                    case ElementInfo.eDeltaStatus.New:
                        return eImageType.Added;
                    case ElementInfo.eDeltaStatus.Unchanged:
                    default:
                        return eImageType.UnModified;
                }
            }
        }

        public string DeltaExtraDetails { get; set; }

        public string UpdatedValue { get; set; }

        public bool IsNotEqual
        {
            get
            {
                if (DeltaStatus == eDeltaStatus.Unchanged)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

        }

    }
}
