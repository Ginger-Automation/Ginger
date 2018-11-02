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
