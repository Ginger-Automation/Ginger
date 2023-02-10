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


using GingerWPFUnitTest.POMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;

namespace GingerTest.POMs.Common
{
    public class PagePOM : GingerPOMBase
    {
        Page mPage;

        public PagePOM(Page page)
        {
            mPage = page;
        }

        // Quick way to get element by AutomationID
        //public UIElementPOM this[string name]
        //{
        //    get
        //    {
        //        DependencyObject dependencyObject = null;
        //        Execute(() => {                    
        //            dependencyObject = FindElementByName(mPage, name);         
        //        });
        //        return new UIElementPOM(dependencyObject);
        //    }
        //}


        public UIElementPOM this[string name]
        {
            get
            {
                DependencyObject dependencyObject = null;
                Execute(() => {                    
                    dependencyObject = FindControlByAutomationId(mPage, name);                    
                });
                return new UIElementPOM(dependencyObject);
            }
        }


        /// <summary>
        /// Recursive search in tree to find the control with automation ID requested
        /// </summary>
        /// <param name="control"></param>
        /// <param name="automationID"></param>
        /// <returns></returns>
        private DependencyObject FindControlByAutomationId(DependencyObject control, string automationID)
        {            
            string AID = (string)control.GetValue(AutomationProperties.AutomationIdProperty);
            if (AID == automationID) return control;
            
            foreach (object subControl in LogicalTreeHelper.GetChildren(control))
            {
                if (subControl is FrameworkElement)
                {
                    FrameworkElement frameworkElement = (FrameworkElement)subControl;
                    if (frameworkElement != null)
                    {
                        DependencyObject DO = FindControlByAutomationId(frameworkElement, automationID);
                        if (DO != null) return DO;
                    }
                }
            }
            
            return null;
        }


    }
}
