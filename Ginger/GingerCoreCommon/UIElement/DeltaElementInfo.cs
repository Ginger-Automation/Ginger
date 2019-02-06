using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Common.Enums;

namespace Amdocs.Ginger.Common.UIElement
{
    public enum eDeltaStatus
    {
        Unknown,
        Unchanged,
        Changed,
        Deleted,        
        New
    }

    public class DeltaElementInfo : ElementInfo
    {        
        private eDeltaStatus mDeltaStatus;

        public eDeltaStatus DeltaStatus
        {
            get
            {
                return mDeltaStatus;
            }
            set
            {
                mDeltaStatus = value;
                OnPropertyChanged(nameof(DeltaStatus));
                OnPropertyChanged(nameof(IsNotEqual));
                OnPropertyChanged(nameof(DeltaStatusIcon));
            }
        }

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

        public enum eDeltaExtraDetails
        {
            [EnumValueDescription("")]
            NA,
            [EnumValueDescription("Locators Changed")]
            LocatorsChanged,
            [EnumValueDescription("Properties Changed")]
            PropertiesChanged,
            [EnumValueDescription("Locators And Properties Changed")]
            LocatorsAndPropertiesChanged
        }

        private eDeltaExtraDetails mDeltaExtraDetails;

        public eDeltaExtraDetails DeltaExtraDetails
        {
            get
            {
                return mDeltaExtraDetails;
            }
            set
            {
                mDeltaExtraDetails = value;
                OnPropertyChanged(nameof(DeltaExtraDetails));
            }
        }

        public ElementInfo LatestMatchingElementInfo = null;
    }
}
