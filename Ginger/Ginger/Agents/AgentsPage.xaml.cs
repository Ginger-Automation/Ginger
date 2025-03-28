#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using GingerCore;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Agents
{
    /// <summary>
    /// Interaction logic for AgentsPage.xaml
    /// </summary>
    public partial class AgentsPage : Page
    {
        //string mFolder;

        public AgentsPage(RepositoryFolder<Agent> agentsFolder)
        {
            InitializeComponent();
            SetAgentsGridView();

            if (agentsFolder.IsRootFolder)
            {
                xAgentsGrd.DataSourceList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>();
            }
            else
            {
                xAgentsGrd.DataSourceList = agentsFolder.GetFolderItems();
            }
        }

        private void SetAgentsGridView()
        {
            xAgentsGrd.SetTitleLightStyle = true;

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(Agent.Name), WidthWeight = 150 },
                new GridColView() { Field = nameof(Agent.Notes), WidthWeight = 80 },
                new GridColView() { Field = nameof(Agent.DriverType), WidthWeight = 150 },
            ]
            };
            xAgentsGrd.SetAllColumnsDefaultView(view);
            xAgentsGrd.InitViewItems();
            xAgentsGrd.ShowTagsFilter = Visibility.Visible;
        }


    }
}
