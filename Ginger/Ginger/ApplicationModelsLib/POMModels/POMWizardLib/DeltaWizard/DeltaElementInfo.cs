using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    public class DeltaElementInfo
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

        public ElementInfo OriginalElementInfo = null;
        public ElementInfo LatestMatchingElementInfo = null;

        public string ElementName { get { return OriginalElementInfo.ElementName; } }

        public string Description { get { return OriginalElementInfo.Description; } }

        public eElementType ElementTypeEnum { get { return OriginalElementInfo.ElementTypeEnum; } }

        public ElementInfo.eElementStatus ElementStatus { get { return OriginalElementInfo.ElementStatus; } }

        public eImageType StatusIcon { get { return OriginalElementInfo.StatusIcon; } }

        public bool IsAutoLearned { get { return OriginalElementInfo.IsAutoLearned; } }

        public ObservableList<DeltaElementLocator> Locators = new ObservableList<DeltaElementLocator>();

        public ObservableList<DeltaControlProperty> Properties = new ObservableList<DeltaControlProperty>();

        private bool mIsSelected = false;
        public bool IsSelected
        {
            get
            {
                return mIsSelected;
            }
            set
            {
                if (mIsSelected != value)
                {
                    mIsSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }


        public object OriginalElementGroup { get { return OriginalElementInfo.ElementGroup; } }

        public object mElementGroup = null;
        public object SelectedElementGroup
        {
            get
            {
                if (mElementGroup == null)
                {
                    mElementGroup = OriginalElementGroup;
                }
                return mElementGroup;
            }
            set
            {
                if (mElementGroup != value)
                {
                    mElementGroup = value;
                    OnPropertyChanged(nameof(SelectedElementGroup));
                }
            }
        }

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
    }
}
