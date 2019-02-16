using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;

namespace GingerCoreNET.Application_Models
{
    public class DeltaElementLocator: DeltaItemBase
    {        
        ElementLocator mElementLocator = null;        
        public ElementLocator ElementLocator
        {
            get
            {
                return mElementLocator;
            }
            set
            {
                mElementLocator = value;
                mElementLocator.PropertyChanged += MOriginalElementLocator_PropertyChanged;
            }
        }        
        public bool Active { get { return ElementLocator.Active; } }
        public eLocateBy LocateBy { get { return ElementLocator.LocateBy; } }
        public string LocateValue { get { return ElementLocator.LocateValue; } }
        public bool IsAutoLearned { get { return ElementLocator.IsAutoLearned; } }
        public ElementLocator.eLocateStatus LocateStatus { get { return ElementLocator.LocateStatus; } }
        public eImageType StatusIcon { get { return ElementLocator.StatusIcon; } } 

        private void MOriginalElementLocator_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ElementLocator.StatusIcon))
            {
                OnPropertyChanged(nameof(StatusIcon));
            }
        }
    }
}
