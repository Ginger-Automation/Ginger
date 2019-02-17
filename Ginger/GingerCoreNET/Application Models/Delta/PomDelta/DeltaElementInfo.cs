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
        public ElementInfo ElementInfo = null;

        public string ElementName { get { return ElementInfo.ElementName; } }

        public string Description { get { return ElementInfo.Description; } }

        public eElementType ElementTypeEnum { get { return ElementInfo.ElementTypeEnum; } }

        public eImageType ElementTypeImage { get { return ElementInfo.ElementTypeImage; } }

        public ElementInfo.eElementStatus ElementStatus { get { return ElementInfo.ElementStatus; } }

        public eImageType StatusIcon { get { return ElementInfo.StatusIcon; } }

        public string OptionalValuesObjectsListAsString { get { return ElementInfo.OptionalValuesObjectsListAsString; } }

        public bool IsAutoLearned { get { return ElementInfo.IsAutoLearned; } }

        public ObservableList<DeltaElementLocator> Locators = new ObservableList<DeltaElementLocator>();

        public ObservableList<DeltaControlProperty> Properties = new ObservableList<DeltaControlProperty>();

        public object OriginalElementGroup { get { return ElementInfo.ElementGroup; } }

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
