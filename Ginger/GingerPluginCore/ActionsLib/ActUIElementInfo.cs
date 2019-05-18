using System;
using System.Collections.Generic;
using System.Text;
using static Amdocs.Ginger.Plugin.Core.ActionsLib.ActInfo;

namespace Amdocs.Ginger.Plugin.Core.ActionsLib
{
    public struct ActUIElementInfo
    {
        public eLocateBy LocateBy { get; set; }
        public string LocateValue { get; set; }

        public String TargetLocateValue { get; set; }
        public eElementAction ElementAction { get; set; }
        public eLocateBy TargetLocateBy { get; set; }
        public eElementDragDropType DragDropType { get; set; }

        public eElementType ElementType { get; set; }
        public int XCoordinate { get; set; }
        public int YCoordinate { get; set; }

        public eElementAction ClickType { get; set; }

        public eElementAction ValidationType { get; set; }
      
        public eLocateBy ValidationElementLocateby { get; set; }
        public string ValidationElementLocatorValue { get; set; }
        public bool LoopThroughClicks { get; set; }
        public string Value { get; set; }
    }
}
