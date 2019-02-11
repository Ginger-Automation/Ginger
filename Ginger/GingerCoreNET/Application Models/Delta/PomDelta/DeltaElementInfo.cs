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

        public string ElementName { get { return OriginalElementInfo.ElementName; } }

        public string Description { get { return OriginalElementInfo.Description; } }

        public eElementType ElementTypeEnum { get { return OriginalElementInfo.ElementTypeEnum; } }

        public ElementInfo.eElementStatus ElementStatus { get { return OriginalElementInfo.ElementStatus; } }

        public eImageType StatusIcon { get { return OriginalElementInfo.StatusIcon; } }

        public bool IsAutoLearned { get { return OriginalElementInfo.IsAutoLearned; } }

        public ObservableList<DeltaElementLocator> Locators = new ObservableList<DeltaElementLocator>();

        public ObservableList<DeltaControlProperty> Properties = new ObservableList<DeltaControlProperty>();

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
    }
}
