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

using GingerCore;
using GingerWPF.UserControlsLib;
using GingerWPFUnitTest.POMs;
using System;
using System.Windows.Controls;

namespace GingerTest.POMs
{
    public class BusinessFlowPOM : GingerPOMBase
    {
        private SingleItemTreeViewExplorerPagePOM mTreeView;

        public BusinessFlowPOM(SingleItemTreeViewExplorerPage page)
        {
            mTreeView = new SingleItemTreeViewExplorerPagePOM(page);
        }

        public SingleItemTreeViewExplorerPagePOM BusinessFlowsTree { get { return mTreeView; } }

        public BusinessFlow selectBusinessFlow(string name)
        {
            mTreeView.SelectItem(name);
            BusinessFlow businessFlow = (BusinessFlow)mTreeView.GetSelectedItemNodeObject();
            return businessFlow;
        }

        public void GoToAutomate()
        {
            mTreeView.GetSelectedItemEditPage();
        }

        public void AutomatePage(string name)
        {
            var elByName = FindElementByName(mTreeView.GetSelectedItemEditPage(), "xAutomateBtn");

            if (elByName != null)
            {
                if (elByName is Amdocs.Ginger.UserControls.ucButton)
                {
                    Dispatcher.Invoke(() =>
                    {
                        (elByName as Amdocs.Ginger.UserControls.ucButton).DoClick();
                        SleepWithDoEvents(100);
                    });
                }
            }
        }

    }
}

