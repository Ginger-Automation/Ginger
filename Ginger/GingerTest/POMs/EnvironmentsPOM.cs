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

using GingerCore.Environments;
using GingerWPF.UserControlsLib;
using System.Collections.Generic;
using System.Windows.Controls;

namespace GingerWPFUnitTest.POMs
{
    public class EnvironmentsPOM : GingerPOMBase
    {
        private SingleItemTreeViewExplorerPagePOM mTreeView;

        public EnvironmentsPOM(SingleItemTreeViewExplorerPage page)
        {
            mTreeView = new SingleItemTreeViewExplorerPagePOM(page);
        }

        public SingleItemTreeViewExplorerPagePOM EnvironmentsTree { get { return mTreeView; } }

        public void CreateEnvironment(string name)
        {
            mTreeView.AddButton.Click();
            SleepWithDoEvents(100);
            WizardPOM wizard = WizardPOM.CurrentWizard;
            //skip intro page
            wizard.NextButton.Click();
            
            // set env name
            wizard.CurrentPage["Name AID"].SetText(name);

            wizard.NextButton.Click();
            wizard.FinishButton.Click();
            SleepWithDoEvents(100);
        }

        public ProjEnvironment SelectEnvironment(string name)
        {
            mTreeView.SelectItem(name);
            ProjEnvironment env = (ProjEnvironment)mTreeView.GetSelectedItemNodeObject();
            return env;
        }

        internal IEnumerable<TreeViewItem> GetRootItems()
        {
            return mTreeView.GetRootItems();            
        }

    }
}
