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
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ginger.Run;
using Ginger.WindowExplorer;
using GingerCore;
using GingerCore.Drivers;
using GingerCore.Platforms;
using GingerCore.Drivers.ASCF;
using GingerCore.Drivers.WebServicesDriverLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger;
using amdocs.ginger.GingerCoreNET;
using GingerCore.DataSource;

namespace Ginger.Agents
{
    /// <summary>
    /// Interaction logic for ApplicationAgentsMapPage.xaml
    /// </summary>
    public partial class ApplicationAgentsMapPage : Page
    {
        private GingerRunner mGR;
        public ObservableList<ApplicationAgent> ApplicationAgents;

        public ApplicationAgentsMapPage(GingerRunner GR)
        {
            InitializeComponent();
            mGR=GR;
            mGR.PropertyChanged += MGR_PropertyChanged; 
            RefreshApplicationAgentsList();
        }

        private void MGR_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(mGR.ApplicationAgents))
            {
                RefreshApplicationAgentsList();
            }
        }

        private void RefreshApplicationAgentsList()
        {
            ApplicationAgents = new ObservableList<ApplicationAgent>();

            foreach (ApplicationAgent Apag in mGR.ApplicationAgents)
            {
                if (mGR.SolutionApplications.Where(x => x.AppName == Apag.AppName && x.Platform == ePlatformType.NA).FirstOrDefault() == null)
                {
                    ApplicationAgents.Add(Apag);
                }
            }

            AppAgentsListBox.ItemsSource = ApplicationAgents;
        }

        private void StartAgentButton_Click(object sender, RoutedEventArgs e)
        {                                    
            ApplicationAgent AG = (ApplicationAgent)((Button)sender).DataContext;
            try
            {
                StartAppAgent(AG);

                //If there is errorMessageFromDriver is populated then do not wait. 
                if(AG.Agent.Driver!=null && String.IsNullOrEmpty(AG.Agent.Driver.ErrorMessageFromDriver))               
                    AG.Agent.WaitForAgentToBeReady();
                Agent.eStatus Status = AG.Agent.Status;
                if (Status!= Agent.eStatus.Running && Status!= Agent.eStatus.Starting)
                {
                    string errorMessage = AG.Agent.Driver.ErrorMessageFromDriver;
                    if (String.IsNullOrEmpty(errorMessage))
                        errorMessage = "Failed to Connect the agent";
                    
                    Reporter.ToGingerHelper(eGingerHelperMsgKey.StartAgentFailed,null, errorMessage);
                }
            }
            catch(Exception ex)
            {
                Reporter.ToGingerHelper(eGingerHelperMsgKey.StartAgentFailed, null, ex.Message);
           }         
        }

        private void ConfigAgentButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationAgent AG = (ApplicationAgent)((Button)sender).DataContext;

            ApplicationAgentSelectionPage w = new ApplicationAgentSelectionPage(mGR, AG);
            w.ShowAsWindow();
        }

        private void CloseAgentButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationAgent AG = (ApplicationAgent)((Button)sender).DataContext;
            AG.Agent.Close();
        }

        private void ExplorerAgentButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationAgent AG = (ApplicationAgent)((Button)sender).DataContext;
            if (AG.Agent != null)
            {
                if (AG.Agent.Status == Agent.eStatus.NotStarted)
                    StartAppAgent(AG);
                //TODO: Temporary to launch Web service window, till we merge web services to window explorer
                if (AG.Agent.Driver is WebServicesDriver)
                {
                    ((WebServicesDriver)AG.Agent.Driver).LauncDriverWindow();
                    return;
                }

                //if (AG.Agent.Driver is IWindowExplorer)
                //Once all the driver implementing IwindowExplorer are ready, simply checking is IWindowExplorer will server the purpose and flag IsWindowExplorerSupportReady can be removed
                if (AG.Agent.IsWindowExplorerSupportReady)
                {
                    WindowExplorerPage WEP = new WindowExplorerPage(AG);
                    WEP.ShowAsWindow();
                }               
                else
                {
                    Reporter.ToUser(eUserMsgKeys.DriverNotSupportingWindowExplorer, AG.Agent.DriverType);
                }
            }
        }

        private void StartAppAgent(ApplicationAgent AG)
        {
            AutoLogProxy.UserOperationStart("StartAgentButton_Click");
            Reporter.ToGingerHelper(eGingerHelperMsgKey.StartAgent, null, AG.AgentName, AG.AppName);
            if (AG.Agent.Status == Agent.eStatus.Running) AG.Agent.Close();

            AG.Agent.ProjEnvironment = App.AutomateTabEnvironment;
            AG.Agent.BusinessFlow = App.BusinessFlow; ;
            AG.Agent.SolutionFolder = App.UserProfile.Solution.Folder;
            AG.Agent.DSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            AG.Agent.StartDriver();               
            //For ASCF, launch explorer automatically when launching Agent
            if (AG.Agent.IsShowWindowExplorerOnStart && AG.Agent.Status == Agent.eStatus.Running)
            {
                WindowExplorerPage WEP = new WindowExplorerPage(AG);
                WEP.ShowAsWindow();
            }

            Reporter.CloseGingerHelper();
            AutoLogProxy.UserOperationEnd();
        }

        private void AppAgentsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ApplicationAgent AG = (ApplicationAgent)AppAgentsListBox.SelectedItem;
            if (AG == null) return;
            ApplicationAgentSelectionPage w = new ApplicationAgentSelectionPage(mGR, AG);
            w.ShowAsWindow();
        }

        private void LoadingAgentButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationAgent AG = (ApplicationAgent)((Button)sender).DataContext;
            AG.Agent.Driver.cancelAgentLoading = true;
        }
    }
}
