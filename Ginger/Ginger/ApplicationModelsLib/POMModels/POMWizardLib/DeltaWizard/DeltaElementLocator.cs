using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Amdocs.Ginger.Common.Enums;

namespace Amdocs.Ginger.Common.UIElement
{
    public class DeltaElementLocator
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public ElementLocator OriginalElementLocator = null;
        public ElementLocator LatestMatchingElementLocator = null;

        public bool Active { get { return OriginalElementLocator.Active; } }
        public eLocateBy LocateBy { get { return OriginalElementLocator.LocateBy; } }
        public string LocateValue { get { return OriginalElementLocator.LocateValue; } }
        public bool IsAutoLearned { get { return OriginalElementLocator.IsAutoLearned; } }
        public ElementLocator.eLocateStatus LocateStatus { get { return OriginalElementLocator.LocateStatus; } }
        public eImageType StatusIcon { get { return OriginalElementLocator.StatusIcon; } }

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

        public string UpdatedValue { get; set; }

        public string DeltaExtraDetails { get; set; }
    }
}
