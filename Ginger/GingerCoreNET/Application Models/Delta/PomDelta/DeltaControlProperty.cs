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

        public ControlProperty ElementProperty = null;

        public string Name { get { return ElementProperty.Name; } }
        public string Value { get { return ElementProperty.Value; } }
        
        //public string UpdatedValue { get; set; }        

    }
}
