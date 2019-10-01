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
using amdocs.ginger.GingerCoreNET;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActionsLib.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

using GingerWPF.WizardLib;
using System.Windows;
using System.Windows.Controls;
using Amdocs.Ginger.CoreNET.Execution;


namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Interaction logic for ScanPageWizardPage.xaml
    /// </summary>
    public partial class ScanPageWizardPage : Page, IWizardPage
    {        
        AddPOMWizard mWizard;

        public ScanPageWizardPage()
        {
            InitializeComponent();

            //ObservableList<NewAgent> agents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<NewAgent>();
            //AgentComboBox.ItemsSource = agents;
            //AgentComboBox.DisplayMemberPath = nameof(NewAgent.Name);


        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            mWizard = (AddPOMWizard)WizardEventArgs.Wizard;            
            
        }

        private void ScanCurrentPageButton_Click(object sender, RoutedEventArgs e)
        {
            //NewAgent agent = (NewAgent)AgentComboBox.SelectedItem;
            //if (agent.Status != NewAgent.eStatus.Running)
            //{
            //    agent.LocalGingerGrid = WorkSpace.Instance.LocalGingerGrid;
            //    agent.PlugInsManager = WorkSpace.Instance.PlugInsManager;
            //    agent.StartDriver();                
            //}

            //DriverAction DA = new DriverAction();
            //// DA.InputValues.Add(new ActInputValue() { Param = "screens", Value = "Active" });
            //DA.ID = "TakeScreenShot";
            //agent.RunAction(DA);
            //if (DA.Status != eRunStatus.Failed)
            //{

            //}
            //// agent.RunAction()
            ////TODO: check support for IVisualTestingDriver
            ////Bitmap bmp = ((IVisualTestingDriver)mWizard.WinExplorer).GetScreenShot();
            ////string Title = mWizard.WinExplorer.GetActiveWindow().Title;

            ////ScreenShotViewPage p = new ScreenShotViewPage(Title, bmp);
            ////MainFrame.Content = p;

            ////mWizard.POM.Name = Title;
            ////mWizard.POM.ScreenShot = bmp;

        }
    }
}
