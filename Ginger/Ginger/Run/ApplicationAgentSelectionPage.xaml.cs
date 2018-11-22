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
using Amdocs.Ginger.Common;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Run
{
    public partial class ApplicationAgentSelectionPage : Page
    {
        GenericWindow _pageGenericWin = null;
        ApplicationAgent mApplicationAgent;
        GingerRunner mGingerRunner;

        public ApplicationAgentSelectionPage(GingerRunner gingerRunner, ApplicationAgent applicationAgent)
        {
            InitializeComponent();

            mGingerRunner = gingerRunner;
            mApplicationAgent = applicationAgent;

            SetPossibleAgentsGridView();
            SetPossibleAgentsGridData();
        }

        private void SetPossibleAgentsGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = Agent.Fields.Name, Header = "Agent Name", WidthWeight = 100, ReadOnly = true });
            grdPossibleAgents.SetAllColumnsDefaultView(defView);
            grdPossibleAgents.InitViewItems();

            grdPossibleAgents.Grid.SelectionMode = DataGridSelectionMode.Single;
            grdPossibleAgents.RowDoubleClick +=grdPossibleAgents_RowDoubleClick;
        }

        private void SetPossibleAgentsGridData()
        {
            ObservableList<Agent> optionalAgents = new ObservableList<Agent>();
            if (mApplicationAgent != null)
            {
                //find out the target application platform
                ApplicationPlatform ap = (from x in App.UserProfile.Solution.ApplicationPlatforms where x.AppName == mApplicationAgent.AppName select x).FirstOrDefault();
                if (ap != null)
                {
                    ePlatformType appPlatform = ap.Platform;

                    //get the solution Agents which match to this platform
                    //List<Agent> optionalAgentsList = (from p in App.UserProfile.Solution.Agents where p.Platform == appPlatform select p).ToList();
                    List<Agent> optionalAgentsList = (from p in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where p.Platform == appPlatform select p).ToList();
                    if (optionalAgentsList != null && mGingerRunner != null)
                    {
                        //remove already mapped agents
                        List<ApplicationAgent> mappedApps = mGingerRunner.ApplicationAgents.Where(x => x.Agent != null).ToList();
                        foreach (ApplicationAgent mappedApp in mappedApps)
                        {
                            if (mappedApp.Agent.Platform == appPlatform && mappedApp != mApplicationAgent)
                                optionalAgentsList.Remove(mappedApp.Agent);
                        }

                        foreach (Agent agent in optionalAgentsList) optionalAgents.Add(agent);
                    }
                }
            }


            // FIXME : !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // Add Plugin agents
            // if (mApplicationAgent.target - plugin...) search based on type
            // Search plugins            
            var list = from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where x.ServiceId == mApplicationAgent.AppName select x;
            foreach (Agent agent in list)
            {
                optionalAgents.Add(agent);
            }

            if (optionalAgents.Count == 0)
                Reporter.ToUser(eUserMsgKeys.NoOptionalAgent);

            grdPossibleAgents.DataSourceList = optionalAgents;

            //select the current mapped agent in the list
            foreach(Agent agent in optionalAgents)
            {
                if (agent == mApplicationAgent.Agent)
                    grdPossibleAgents.Grid.SelectedItem = agent;
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button mapBtn = new Button();
            mapBtn.Content = "Map";
            mapBtn.Click += new RoutedEventHandler(mapBtn_Click);

            ObservableList<Button> winButtons = new ObservableList<Button>();
            winButtons.Add(mapBtn);

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, "'"+mApplicationAgent.AppName+"'- Agent Mapping", this, winButtons, true, "Cancel");
        }

        private void mapBtn_Click(object sender, RoutedEventArgs e)
        {
            MapSelectedAgent();
        }

        private void grdPossibleAgents_RowDoubleClick(object sender, EventArgs e)
        {
            MapSelectedAgent();
        }

        private void MapSelectedAgent()
        {
            if (grdPossibleAgents.Grid.SelectedItem == null)
            {
                Reporter.ToUser(eUserMsgKeys.NoItemWasSelected);
                return;
            }
            else
            {
                Agent selectedAgent = (Agent)grdPossibleAgents.Grid.SelectedItem;
                mApplicationAgent.Agent = selectedAgent;

                //save last used agent on the Solution Target Applications
                ApplicationPlatform ap = App.UserProfile.Solution.ApplicationPlatforms.Where(x => x.AppName == mApplicationAgent.AppName).FirstOrDefault();
                if (ap != null)
                    ap.LastMappedAgentName = selectedAgent.Name;
            }

            _pageGenericWin.Close();
        }
    }
}
