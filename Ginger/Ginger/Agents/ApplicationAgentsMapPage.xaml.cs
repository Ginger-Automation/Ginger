#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Amdocs.Ginger.Common.InterfacesLib;

namespace Ginger.Agents
{
    /// <summary>
    /// Interaction logic for ApplicationAgentsMapPage.xaml
    /// </summary>
    public partial class ApplicationAgentsMapPage : Page
    {        
        public ObservableList<ApplicationAgent> ApplicationAgents;
        Context mContext;
        public ApplicationAgentsMapPage(Context context)
        {
            InitializeComponent();
            mContext = context;
            mContext.Runner.PropertyChanged += MGR_PropertyChanged; 
            RefreshApplicationAgentsList();
        }

        private void MGR_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(mContext.Runner.ApplicationAgents))
            {
                RefreshApplicationAgentsList();
            }
        }

        private void RefreshApplicationAgentsList()
        {
            ApplicationAgents = new ObservableList<ApplicationAgent>();

            foreach (ApplicationAgent Apag in mContext.Runner.ApplicationAgents)
            {
                if (mContext.Runner.SolutionApplications.Where(x => x.AppName == Apag.AppName && x.Platform == ePlatformType.NA).FirstOrDefault() == null)
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
                if(((Agent)AG.Agent).Driver!=null && String.IsNullOrEmpty(((Agent)AG.Agent).Driver.ErrorMessageFromDriver))
                    ((Agent)AG.Agent).WaitForAgentToBeReady();
                Agent.eStatus Status = ((Agent)AG.Agent).Status;
                if (Status!= Agent.eStatus.Running && Status!= Agent.eStatus.Starting)
                {
                    string errorMessage = ((Agent)AG.Agent).Driver.ErrorMessageFromDriver;
                    if (String.IsNullOrEmpty(errorMessage))
                        errorMessage = "Failed to Connect the agent";
                    
                    Reporter.ToStatus(eStatusMsgKey.StartAgentFailed,null, errorMessage);
                }
            }
            catch(Exception ex)
            {
                Reporter.ToStatus(eStatusMsgKey.StartAgentFailed, null, ex.Message);
           }         
        }

        private void ConfigAgentButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationAgent AG = (ApplicationAgent)((Button)sender).DataContext;

            ApplicationAgentSelectionPage w = new ApplicationAgentSelectionPage(mContext.Runner, AG);
            w.ShowAsWindow();
        }

        private void CloseAgentButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationAgent AG = (ApplicationAgent)((Button)sender).DataContext;
            ((Agent)AG.Agent).Close();
        }

        private void ExplorerAgentButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationAgent AG = (ApplicationAgent)((Button)sender).DataContext;
            if (AG.Agent != null)
            {
                if (((Agent)AG.Agent).Status == Agent.eStatus.NotStarted)
                    StartAppAgent(AG);
                //TODO: Temporary to launch Web service window, till we merge web services to window explorer
                if (((Agent)AG.Agent).Driver is WebServicesDriver)
                {
                    ((WebServicesDriver)((Agent)AG.Agent).Driver).LauncDriverWindow();
                    return;
                }

                //if (((Agent)AG.Agent).Driver is IWindowExplorer)
                //Once all the driver implementing IwindowExplorer are ready, simply checking is IWindowExplorer will server the purpose and flag IsWindowExplorerSupportReady can be removed
                if (((Agent)AG.Agent).IsWindowExplorerSupportReady)
                {
                    WindowExplorerPage WEP = new WindowExplorerPage(AG, mContext);
                    WEP.ShowAsWindow();
                }               
                else
                {
                    Reporter.ToUser(eUserMsgKey.DriverNotSupportingWindowExplorer, ((Agent)AG.Agent).DriverType);
                }
            }
        }

        private void StartAppAgent(ApplicationAgent AG)
        {
            AutoLogProxy.UserOperationStart("StartAgentButton_Click");
            Reporter.ToStatus(eStatusMsgKey.StartAgent, null, AG.AgentName, AG.AppName);
            if (((Agent)AG.Agent).Status == Agent.eStatus.Running) ((Agent)AG.Agent).Close();

            ((Agent)AG.Agent).ProjEnvironment = mContext.Environment;
            ((Agent)AG.Agent).BusinessFlow = mContext.BusinessFlow; 
            ((Agent)AG.Agent).SolutionFolder =  WorkSpace.Instance.Solution.Folder;
            ((Agent)AG.Agent).DSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            ((Agent)AG.Agent).StartDriver();               
            //For ASCF, launch explorer automatically when launching Agent
            if (((Agent)AG.Agent).IsShowWindowExplorerOnStart && ((Agent)AG.Agent).Status == Agent.eStatus.Running)
            {
                WindowExplorerPage WEP = new WindowExplorerPage(AG, mContext);
                WEP.ShowAsWindow();
            }

            Reporter.HideStatusMessage();
            AutoLogProxy.UserOperationEnd();
        }

        private void AppAgentsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ApplicationAgent AG = (ApplicationAgent)AppAgentsListBox.SelectedItem;
            if (AG == null) return;
            ApplicationAgentSelectionPage w = new ApplicationAgentSelectionPage(mContext.Runner, AG);
            w.ShowAsWindow();
        }

        private void LoadingAgentButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationAgent AG = (ApplicationAgent)((Button)sender).DataContext;
            ((Agent)AG.Agent).Driver.cancelAgentLoading = true;
        }
    }
}
