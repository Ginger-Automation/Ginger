using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Amdocs.Ginger.Common.Enums;

namespace Amdocs.Ginger.Common.UIElement
{
    public class DeltaControlProperty 
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

        public ControlProperty OriginalElementProperty = null;
        public ControlProperty LatestMatchingElementProperty = null;

        public string Name { get { return OriginalElementProperty.Name; } }
        public string Value { get { return OriginalElementProperty.Value; } }

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
