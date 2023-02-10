#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using Amdocs.Ginger.Common.UIElement;
using GingerCore.Drivers.Common;

namespace GingerCore.Actions.UIAutomation
{ 
    public class UIAElementInfo : ElementInfo
    {  
        //TOOD: if needed do lazy loading and read it from th obj above
        public double XCordinate;
        public double YCordinate;
        public object BoundingRectangle { get; set; }
        public string LocalizedControlType { get; set; }
        public string AutomationId { get; set; }
        public string ClassName { get; set; }
        public string ToggleState { get; set; }
        public string Text { get; set; }
        public bool IsKeyboardFocusable { get; set; } = false;
        public bool IsEnabled { get; set; } = false;
        public bool IsPassword { get; set; } = false;
        public bool IsOffscreen { get; set; } = false;
        public bool IsSelected { get; set; } = false;

        public override string GetAbsoluteXpath()
        {
            return ((IXPath)WindowExplorer).GetXPathHelper(this).GetElementXpathAbsulote(this);
        }

        public override string GetElementTitle()
        {
            // give the element name for Tree View
            // make it user friendly as much as possible just not empty
            // start with the text on the control 
            // then try Name
            // else get control type and automation ID
            
            return ((UIAutomationDriverBase)WindowExplorer).mUIAutomationHelper.GetElementTitle(ElementObject);
        }

        public override string GetElementType()
        {
            return ((UIAutomationDriverBase)WindowExplorer).mUIAutomationHelper.GetElementControlType(ElementObject);
        }

        public override string GetValue()
        {
            //TODO: check and find the best, if no value take maybe Name or something else, maybe change to content instead of value
          return  ((UIAutomationDriverBase)WindowExplorer).mUIAutomationHelper.GetControlPropertyValue(ElementObject,"Value");
        }
    }
}