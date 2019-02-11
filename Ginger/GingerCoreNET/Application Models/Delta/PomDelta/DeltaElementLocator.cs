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
        ElementLocator mOriginalElementLocator = null;
        public ElementLocator OriginalElementLocator
        {
            get
            {
                return mOriginalElementLocator;
            }
            set
            {
                mOriginalElementLocator = value;
                mOriginalElementLocator.PropertyChanged += MOriginalElementLocator_PropertyChanged;
            }
        }

        public ElementLocator LatestMatchingElementLocator = null;

        public bool Active { get { return OriginalElementLocator.Active; } }
        public eLocateBy LocateBy { get { return OriginalElementLocator.LocateBy; } }
        public string LocateValue { get { return OriginalElementLocator.LocateValue; } }
        public bool IsAutoLearned { get { return OriginalElementLocator.IsAutoLearned; } }
        public ElementLocator.eLocateStatus LocateStatus { get { return OriginalElementLocator.LocateStatus; } }
        public eImageType StatusIcon { get { return OriginalElementLocator.StatusIcon; } }

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
