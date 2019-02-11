#region License
/*
Copyright © 2014-2018 European Support Limited

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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using HtmlAgilityPack;

namespace GingerCore.Drivers.Common
{
    public class HTMLElementInfo : ElementInfo
    {       
        // add special HTML elem info here
        public static class Fields
        {
            public static string ID = "ID";
            public static string RelXpath = "RelXpath";
            public static string Name = "Name";
        }

        // ---------------------------------------------------------------------------------------------------------------------
        //  ID
        // ---------------------------------------------------------------------------------------------------------------------
        private string mID = null;
        public string ID
        {
            get
            {
                if (mID == null) mID = GetID();
                return mID;
            }
            set { mID = value; }
        }

        // Used for Lazy loading when possible
        public virtual string GetID()
        {
            // we return Name unless it was overridden as expected
            // So we keep backward compatibility until all drivers do it correctly
            return mID;
        }

        // ---------------------------------------------------------------------------------------------------------------------
        //  RelXpath
        // ---------------------------------------------------------------------------------------------------------------------
        private string mRelXpath = null;
        public string RelXpath
        {
            get
            {
                if (mRelXpath == null) mRelXpath = GetRelXpath();
                return mRelXpath;
            }
            set { mRelXpath = value; }
        }   // Developer Name

        // Used for Lazy loading when possible
        public virtual string GetRelXpath()
        {
            // we return Name unless it was overridden as expected
            // So we keep backward compatibility until all drivers do it correctly
            return mRelXpath;
        }

        // ---------------------------------------------------------------------------------------------------------------------
        //  Name
        // ---------------------------------------------------------------------------------------------------------------------
        private string mName = null;
        public string Name
        {
            get
            {
                if (mName == null) mRelXpath = GetName();
                return mName;
            }
            set { mName = value; }
        }   // Developer Name
        
        // Used for Lazy loading when possible
        public virtual string GetName()
        {
            // we return Name unless it was overridden as expected
            // So we keep backward compatibility until all drivers do it correctly
            return mName;
        }

        public HtmlNode HTMLElementObject { get; set; }

        public override string GetAbsoluteXpath()
        {
            if (WindowExplorer == null) return null;
            return ((IXPath)WindowExplorer).GetXPathHelper(this).GetElementXpathAbsulote(this);
        }        
    }
}
