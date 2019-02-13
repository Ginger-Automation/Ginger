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
        public ElementLocator OriginalElementLocator = null;
        public ElementLocator LatestMatchingElementLocator = null;


        ElementLocator mElementLocatorToShow = null;        
        public ElementLocator ElementLocatorToShow
        {
            get
            {
                return mElementLocatorToShow;
            }
            set
            {
                mElementLocatorToShow = value;
                mElementLocatorToShow.PropertyChanged += MOriginalElementLocator_PropertyChanged;
            }
        }

        

        public bool Active { get { return ElementLocatorToShow.Active; } }
        public eLocateBy LocateBy { get { return ElementLocatorToShow.LocateBy; } }
        public string LocateValue { get { return ElementLocatorToShow.LocateValue; } }
        public bool IsAutoLearned { get { return ElementLocatorToShow.IsAutoLearned; } }
        public ElementLocator.eLocateStatus LocateStatus { get { return ElementLocatorToShow.LocateStatus; } }
        public eImageType StatusIcon { get { return ElementLocatorToShow.StatusIcon; } }

        //public string UpdatedValue { get; set; }   

        private void MOriginalElementLocator_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ElementLocator.StatusIcon))
            {
                OnPropertyChanged(nameof(StatusIcon));
            }
        }
    }
}
