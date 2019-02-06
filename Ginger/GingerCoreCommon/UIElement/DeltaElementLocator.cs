using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Common.Enums;

namespace Amdocs.Ginger.Common.UIElement
{
    public class DeltaElementLocator: ElementLocator
    {
        public eDeltaStatus DeltaStatus { get; set; }

        public eImageType DeltaStatusIcon
        {
            get
            {
                switch (DeltaStatus)
                {
                    case eDeltaStatus.Deleted:
                        return eImageType.Deleted;
                    case eDeltaStatus.Changed:
                        return eImageType.Modified;
                    case eDeltaStatus.New:
                        return eImageType.Added;
                    case eDeltaStatus.Unchanged:
                    default:
                        return eImageType.UnModified;
                }
            }
        }

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

        public string UpdatedValue { get; set; }

        public string DeltaExtraDetails { get; set; }
    }
}
