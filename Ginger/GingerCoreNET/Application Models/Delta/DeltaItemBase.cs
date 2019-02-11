using Amdocs.Ginger.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace GingerCoreNET.Application_Models
{
    public enum eDeltaStatus
    {
        Unknown,
        Unchanged,
        Changed,
        Deleted,
        New
    }

    public class DeltaItemBase
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
                        return eImageType.Unchanged;
                    case eDeltaStatus.Unknown:
                    default:
                        return eImageType.Question;
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

        private string mDeltaExtraDetails;
        public string DeltaExtraDetails
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

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

    }
}
