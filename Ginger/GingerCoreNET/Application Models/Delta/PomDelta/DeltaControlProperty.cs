using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;

namespace GingerCoreNET.Application_Models
{
   
    public class DeltaControlProperty : DeltaItemBase 
    {
        public enum ePropertiesChangesToAvoid
        {
            All,
            OnlySizeAndLocationProperties,
            None
        }

        public ControlProperty OriginalElementProperty = null;
        public ControlProperty LatestMatchingElementProperty = null;
        public ControlProperty ElementPropertyToShow = null;

        public string Name { get { return ElementPropertyToShow.Name; } }
        public string Value { get { return ElementPropertyToShow.Value; } }
        
        //public string UpdatedValue { get; set; }        

    }
}
