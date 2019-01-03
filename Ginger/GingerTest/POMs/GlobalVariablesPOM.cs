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

using Ginger;
using Ginger.Variables;
using GingerCore.Variables;
using GingerTest.POMs.Common;
using GingerWPFUnitTest.POMs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GingerTest.POMs
{
    public class GlobalVariablesPOM : GingerPOMBase
    {
        VariablesPage mVariablesPage;
        public GlobalVariablesPOM(VariablesPage page)
        {            
            mVariablesPage = page;
        }

        public void AddStringVariable(string name)
        {
            Page AddVaraibleEditPage = AddVariable("Variable String");
            SetVariableName(AddVaraibleEditPage, name);
            ClickOK();
        }

        private void ClickOK()
        {
            Execute(() => {               
                CurrentGenericWindow.Buttons["Ok"].Click();
            });
        }

        private void SetVariableName(Page page, string name)
        {
            Execute(() => {
                TextBox txt = (TextBox)FindElementByAutomationID<TextBox>(page, "Variable Name AID");
                txt.Text = name;                
            });
        }

        private Page AddVariable(string VariableDisplayName)
        {
            Task.Factory.StartNew(() =>
            {
                Execute(() => {
                    ucGrid grid = (ucGrid)FindElementByAutomationID<ucGrid>(mVariablesPage, "Variables Grid AID");
                    ucGridPOM gridPOM = new ucGridPOM(grid);
                    gridPOM.EnhancedHeaderAddButton.Click();
                });
            });

            Page addVaraiblePage = null;
            Stopwatch st = Stopwatch.StartNew();
            while (addVaraiblePage == null && st.ElapsedMilliseconds < 10000)
            {
                SleepWithDoEvents(100);
                if (GenericWindow.CurrentWindow != null)
                {
                    addVaraiblePage = CurrentGenericWindow.LoadedPage();
                    // return addVaraiblePage;

                    Execute(() =>
                    {
                        ucGrid ucGrid = (ucGrid)FindElementByAutomationID<ucGrid>(addVaraiblePage, "VariablesGrid AID");
                        ucGridPOM ucGridPOM = new ucGridPOM(ucGrid);
                        ucGridPOM.GotoRow("VariableUIType", VariableDisplayName);
                    });

                    Task.Factory.StartNew(() =>
                    {
                        Execute(() => {
                            CurrentGenericWindow.Buttons["Add Variable"].Click();
                        });
                    });

                    SleepWithDoEvents(100);

                    Page p = null;
                    Stopwatch st2 = Stopwatch.StartNew();
                    while (!(p is VariableEditPage) && st2.ElapsedMilliseconds < 10000)
                    {
                        SleepWithDoEvents(100);
                        p = CurrentGenericWindow.LoadedPage();
                        return p;
                    }

                    throw new Exception("timeout Error waiting for var edit page to load");
                }
            }

            throw new Exception("timeout Error cannot find Add variable page");

        }
    }
}
