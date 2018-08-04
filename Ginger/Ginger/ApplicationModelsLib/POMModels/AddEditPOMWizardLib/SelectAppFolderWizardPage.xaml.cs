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
using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.WindowExplorer;
using GingerCore;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Interaction logic for SelectAppFolderWizardPage.xaml
    /// </summary>
    public partial class SelectAppFolderWizardPage : Page, IWizardPage
    {
        AddPOMWizard mWizard;

        public SelectAppFolderWizardPage()
        {
            InitializeComponent();

           

        }

        private void Init()
        {
            ObservableList<ApplicationPlatform> TargetApplications = GingerCore.General.ConvertListToObservableList(App.UserProfile.Solution.ApplicationPlatforms.Where(x => x.Platform == ePlatformType.Web).ToList());
            xTargetApplicationComboBox.BindControl<ApplicationPlatform>(mWizard.POM, nameof(ApplicationPOMModel.TargetApplicationKey), TargetApplications, nameof(ApplicationPlatform.AppName), nameof(ApplicationPlatform.Key));
            xEnvTagsViewer.Init(mWizard.POM.TagsKeys);

        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddPOMWizard)WizardEventArgs.Wizard;
                    Init();
                    //xTargetApplicationComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;
                    //xAgentComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;


                    //xAgentComboBox.SelectedValuePath = Agent.Fields.Name;
                    //xAgentComboBox.DisplayMemberPath = Agent.Fields.Name;
                    //xNameTextBox.BindControl(mWizard.POM, nameof(ApplicationPOMModel.Name));
                    //if (mWizard.WinExplorer != null)
                    //{
                    //    if (string.IsNullOrEmpty(mWizard.POM.Name))
                    //    {
                    //        //if title empty get something else maybe the url
                    //        mWizard.POM.Name = mWizard.WinExplorer.GetActiveWindow().Title;
                    //    }
                    //}
                    break;

                case EventType.LeavingForNextPage:

                    string error = string.Empty;
                    if (ValidateFields(ref error))
                    {
                        mWizard.POM.Name = xNameTextBox.Text;
                        mWizard.POM.Description = xDescriptionTextBox.Text;
                        //mWizard.POM.TargetApplicationKey = App.UserProfile.Solution.ApplicationPlatforms.Where(x => x.Guid == ((ApplicationPlatform)xTargetApplicationComboBox.SelectedItem).Guid).Select(x => x.Key).FirstOrDefault(); ;
                        mWizard.POM.TargetApplicationKey = (RepositoryItemKey)xTargetApplicationComboBox.SelectedValue;
                    }
                    else
                    {
                        //WizardEventArgs.AddError(error);
                    }

                    break;
            }
        }

        private bool ValidateFields(ref string error)
        {
            if (string.IsNullOrEmpty(xNameTextBox.Text))
            {
                error = "Name field cannot be empty";
                return false;
            }
            else if (string.IsNullOrEmpty(xDescriptionTextBox.Text))
            {
                error = "Description field cannot be empty";
                return false;
            }
            else if (string.IsNullOrEmpty(xTargetApplicationComboBox.Text))
            {
                error = "Target Application field cannot be empty";
                return false;
            }

            return true;

        }

        //private void xTargetApplicationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    //xAgentComboBox.IsEnabled = true;
        //    ApplicationPlatform ap = (ApplicationPlatform)xTargetApplicationComboBox.SelectedItem;
        //    List<Agent> optionalAgentsList = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where x.Platform == ap.Platform select x).ToList();
        //    //xAgentComboBox.ItemsSource = optionalAgentsList;

        //}

        //private void xStartAgentButton_Click(object sender, RoutedEventArgs e)
        //{
        //    //Agent agent = (Agent)xAgentComboBox.SelectedItem;
        //    //if(agent != null && agent.Status == Agent.eStatus.NotStarted)
        //    //    StartAppAgent(agent);
        //}

        //private void StartAppAgent(Agent agent)
        //{
        //    AutoLogProxy.UserOperationStart("StartAgentButton_Click");
        //    Reporter.ToGingerHelper(eGingerHelperMsgKey.StartAgent, null, agent.Name, "AppName"); //Yuval: change app name to be taken from current app
        //    if (agent.Status == Agent.eStatus.Running) agent.Close();

        //    agent.StartDriver();
        //    if (agent.IsShowWindowExplorerOnStart && agent.Status == Agent.eStatus.Running)
        //    {
        //        WindowExplorerPage WEP = new WindowExplorerPage(new ApplicationAgent());
        //        WEP.ShowAsWindow();
        //    }

        //    Reporter.CloseGingerHelper();
        //    AutoLogProxy.UserOperationEnd();
        //}
    }
}
