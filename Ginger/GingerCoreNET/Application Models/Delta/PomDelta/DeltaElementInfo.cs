using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;

namespace GingerCoreNET.Application_Models
{
    public class DeltaElementInfo: DeltaItemBase
    {
        public ElementInfo OriginalElementInfo = null;
        public ElementInfo LatestMatchingElementInfo = null;
        public ElementInfo ElementInfoToShow = null;

        public string ElementName { get { return ElementInfoToShow.ElementName; } }

        public string Description { get { return ElementInfoToShow.Description; } }

        public eElementType ElementTypeEnum { get { return ElementInfoToShow.ElementTypeEnum; } }

        public ElementInfo.eElementStatus ElementStatus { get { return ElementInfoToShow.ElementStatus; } }

        public eImageType StatusIcon { get { return ElementInfoToShow.StatusIcon; } }

        public string OptionalValuesObjectsListAsString { get { return ElementInfoToShow.OptionalValuesObjectsListAsString; } }

        public bool IsAutoLearned { get { return ElementInfoToShow.IsAutoLearned; } }

        public ObservableList<DeltaElementLocator> Locators = new ObservableList<DeltaElementLocator>();

        public ObservableList<DeltaControlProperty> Properties = new ObservableList<DeltaControlProperty>();

        public object OriginalElementGroup { get { return ElementInfoToShow.ElementGroup; } }

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
    }
}
