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

extern alias UIAComWrapperNetstandard;
using UIAuto = UIAComWrapperNetstandard::System.Windows.Automation;
using Amdocs.Ginger.Common.UIElement;
using System;
using System.Collections.Generic;


namespace GingerCore.Drivers.Common
{
    /// <summary>
    /// Base class for different Control type for each driver, enable to show unified list in Window Explorer Grid
    /// </summary>
    public class TableElementInfo : ElementInfo
    {
        public static class Fields
        {
            public static string RowCount = "RowCount";
            public static string ColumnNames = "ColumnNames";
            public static string MainDict = "MainDict";
        }
       
        // ---------------------------------------------------------------------------------------------------------------------
        //  Value
        // ---------------------------------------------------------------------------------------------------------------------
        private Dictionary<string, UIAuto.AutomationElement[]> mMainDict = new Dictionary<string, UIAuto.AutomationElement[]>();
        public Dictionary<string, UIAuto.AutomationElement[]> MainDict
        {
            get
            {
                if (mMainDict == null) mMainDict = GetMainDict();
                return mMainDict;
            }
            set
            {
                mMainDict = value;
            }
        }
        
        private int mRowCount = 0;
        public int RowCount
        {
            get
            {
                //TODO: fix me value cannot be null then why if?
                if (mRowCount >= 0) mRowCount = GetRowCount();
                return mRowCount;
            }
            set { mRowCount = value; }
        } 

        private List<String> mColumnNames = null;
        public List<String> ColumnNames
        {
            get
            {
                if (mColumnNames == null) mColumnNames = GetColumnNames();
                return mColumnNames;
            }
            set { mColumnNames = value; }
        } 

        // Used for Lazy loading when possible
        public virtual int GetRowCount()
        {
            // we return ElementType unless it was overridden as expected
            // So we keep backward compatibility until all drivers do it correctly
            return mRowCount;
        }

        // Used for Lazy loading when possible
        public virtual List<String> GetColumnNames()
        {
            // we return ElementType unless it was overridden as expected
            // So we keep backward compatibility until all drivers do it correctly
            return mColumnNames;
        }
        public virtual Dictionary<string, UIAuto.AutomationElement[]> GetMainDict()
        {
            return MainDict;
        }
    }
}
