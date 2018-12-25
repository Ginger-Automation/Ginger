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

using amdocs.ginger.GingerCoreNET;
using Ginger.Agents;
using GingerCore;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.UserControlsLib;
using GingerWPFUnitTest.POMs;
using System;
using System.Linq;
using System.Windows.Controls;

namespace GingerTest.POMs
{
    public class AgentsPOM : GingerPOMBase
    {
        private SingleItemTreeViewExplorerPagePOM mTreeView;

        public AgentsPOM(SingleItemTreeViewExplorerPage page)
        {
            mTreeView = new SingleItemTreeViewExplorerPagePOM(page);
        }

        public SingleItemTreeViewExplorerPagePOM AgentsTree { get { return mTreeView; } }

        public Agent CreateAgent(string name, ePlatformType platform, Agent.eDriverType driverType)
        {
            mTreeView.AddButton.Click();
            SleepWithDoEvents(100);

            return CreateAgentOnWizard(name, platform, driverType);
        }

        internal Agent CreateAgent(string folderName, string name, ePlatformType platform, Agent.eDriverType driverType)
        {
            AgentsTree.SelectItem(folderName);
            mTreeView.SelectedItem.ContextMenu["Add New Agent"].Click();

            return CreateAgentOnWizard(name, platform, driverType);            
        }


        Agent CreateAgentOnWizard(string name, ePlatformType platform, Agent.eDriverType driverType)
        {
            WizardPOM wizard = WizardPOM.CurrentWizard;
            //skip intro page
            wizard.NextButton.Click();

            // set name
            wizard.CurrentPage["Name AID"].SetText(name);

            wizard.CurrentPage["Platform Type AID"].SelectValue(platform);
            wizard.CurrentPage["Driver Type AID"].SelectValue(driverType);

            // Driver config page
            wizard.NextButton.Click();
            wizard.Finish();

            // Verify agent appear on tree, might take some time
            bool b = mTreeView.IsItemExist(name);
            if (!b) throw new Exception("Cannot find new agent in tree: " + name);

            Agent agent = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where x.Name == name select x).SingleOrDefault();
            return agent;
        }

        public Agent SelectAgent(string name)
        {
            mTreeView.SelectItem(name);
            Agent agent = (Agent)mTreeView.GetSelectedItemNodeObject();
            return agent;
        }

        public void SelectItem(string header)
        {
            mTreeView.SelectItem(header);            
        }

        public AgentEditPage GetSelectedAgentEditPage()
        {
            return (AgentEditPage)mTreeView.GetSelectedItemEditPage();
        }

        public Agent RenameAgent(string name, string NewName)
        {
            mTreeView.SelectItem(name);
            Page p = mTreeView.GetSelectedItemEditPage();
            Execute(() => 
            {                
                TextBox txt = (TextBox)FindElementByAutomationID<TextBox>(p, "AgentNameTextBox");
                txt.Text = NewName;
            });
            Agent agent = (Agent)mTreeView.GetSelectedItemNodeObject();
            return agent;
        }



        //internal IEnumerable<TreeViewItem> GetRootItems()
        //{
        //    return mTreeView.GetRootItems();
        //}

        public void CurrentItemMenu()
        {
            //mTreeView.GetSelectedItemNodeObject
        }

        internal void AddSubFolder(string name)
        {
            mTreeView.SelectedItem.ContextMenu["Add Sub Folder"].Click();            
            CurrentInputBoxWindow.SetText(name);
            CurrentInputBoxWindow.ClickOK();
            SleepWithDoEvents(500);
        }

        
    }
}
