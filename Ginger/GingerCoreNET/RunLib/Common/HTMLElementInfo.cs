#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using Amdocs.Ginger.Repository;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GingerCore.Drivers.Common
{
    public class HTMLElementInfo : ElementInfo
    {


        [IsSerializedForLocalRepository]
        public string TagName { get; set; }

        private string mID = null;

        [IsSerializedForLocalRepository]
        public string ID
        {
            get
            {
                if (mID == null)
                {
                    mID = GetID();
                }
                return mID;
            }
            set
            {
                mID = value;
            }
        }

        public virtual string GetID()
        {
            return mID;
        }

        private string mRelXpath = null;
        public string RelXpath
        {
            get
            {
                if (mRelXpath == null)
                {
                    mRelXpath = GetRelXpath();
                }
                return mRelXpath;
            }
            set
            {
                mRelXpath = value;
            }
        }

        public virtual string GetRelXpath()
        {
            return mRelXpath;
        }


        private string mName = null;

        [IsSerializedForLocalRepository]
        public string Name
        {
            get
            {
                if (mName == null)
                {
                    mName = GetName();
                }
                return mName;
            }
            set { mName = value; }
        }

        public virtual string GetName()
        {
            return mName;
        }
        // Temporary list for XPaths to locate shadow dom Element
        public IList<string> XPathList { get; set; }
        public HtmlNode HTMLElementObject { get; set; }

        public override string GetAbsoluteXpath()
        {
            if (WindowExplorer == null)
            {
                return null;
            }
            //string xPath = ((IXPath)WindowExplorer).GetXPathHelper(this).GetElementXpathAbsulote(this); // Doing Temporary workaround because GetXPathHelper is not working properly for SeleniumDriver
            string xPath = ((IXPath)WindowExplorer).GetElementXpath(this);
            return xPath;
        }

        public HtmlNode LeftofHTMLElementObject { get; set; }
        public HtmlNode RightofHTMLElementObject { get; set; }
        public HtmlNode AboveHTMLElementObject { get; set; }
        public HtmlNode BelowHTMLElementObject { get; set; }

        public HtmlNode NearHTMLElementObject { get; set; }


        public HTMLElementInfo FindParentElementUsingGuid(IList<ElementInfo> AllElements)
        {
            if (AllElements == null)
            {
                return null;
            }
            var ParentPOMGuid = FindParentPOMGuid();

            if (ParentGuid.Equals(Guid.Empty.ToString()))
            {
                return null;
            }

            return (HTMLElementInfo)AllElements.FirstOrDefault((element) => element.Guid.Equals(new Guid(ParentPOMGuid)));
        }


        public string FindParentPOMGuid()
        {
            return this.Properties.FirstOrDefault((prop) => prop.Name.Equals(ElementProperty.ParentPOMGUID))?.Value ?? Guid.Empty.ToString();
        }
    }
}
