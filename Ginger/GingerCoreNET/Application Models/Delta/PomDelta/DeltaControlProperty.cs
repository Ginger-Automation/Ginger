using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;

namespace GingerCoreNET.Application_Models
{
    public class DeltaControlProperty : DeltaItemBase 
    {
        public ControlProperty OriginalElementProperty = null;
        public ControlProperty LatestMatchingElementProperty = null;

        public string Name { get { return OriginalElementProperty.Name; } }
        public string Value { get { return OriginalElementProperty.Value; } }
        
        //public string UpdatedValue { get; set; }        

    }
}
