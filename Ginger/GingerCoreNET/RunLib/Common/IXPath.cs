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

using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Common.UIElement;
using System.Collections.Generic;

namespace GingerCore.Drivers.Common
{
    //All drivers who want easy XPath creation and finding element should implement this and XpathHelper will do the magic
    public class XpathPropertyCondition
    {
        public enum XpathConditionOperator
        {
            Equel,
            Less,
            More
        }

        public string PropertyName { get; set; }
        public XpathConditionOperator Op { get; set; }
        public string Value;
    }

    public interface IXPath
    {
        XPathHelper GetXPathHelper(ElementInfo info = null);

        ElementInfo GetRootElement();

        ElementInfo UseRootElement();
        
        ElementInfo GetElementParent(ElementInfo ElementInfo, PomSetting pomSetting = null);

        string GetElementProperty(ElementInfo ElementInfo, string PropertyName);        

        List<ElementInfo> GetElementChildren(ElementInfo ElementInfo);

        ElementInfo FindFirst(ElementInfo ElementInfo, List<XpathPropertyCondition> conditions) ;

        List<ElementInfo> FindAll(ElementInfo ElementInfo, List<XpathPropertyCondition> conditions);

        ElementInfo GetPreviousSibling(ElementInfo EI);

        ElementInfo GetNextSibling(ElementInfo EI);

        string GetElementID(ElementInfo EI);

        string GetElementTagName(ElementInfo EI);
        List<object> GetAllElementsByLocator(eLocateBy LocatorType, string LocValue);

        string GetElementXpath(ElementInfo EI);

        string GetInnerHtml(ElementInfo elementInfo);
        object GetElementParentNode(ElementInfo elementInfo);
        string GetInnerText(ElementInfo elementInfo);
        string GetPreviousSiblingInnerText(ElementInfo elementInfo);
    }
}
