#region License
/*
Copyright © 2014-2022 European Support Limited

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
using Amdocs.Ginger.Repository;
using Ginger.Actions.UserControls;
using Ginger.BusinessFlowWindows;
using Ginger.Repository;
using GingerCore;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Interaction logic for SelectAppFolderWizardPage.xaml
    /// </summary>
    public partial class POMAutoLearnedActivitiesWizardPage : Page, IWizardPage
    {
        AddPOMWizard mWizard;
        ActivitiesRepositoryPage mActivitiesRepositoryViewPage;
        ucBusinessFlowMap mBusinessFlowControl;

        public POMAutoLearnedActivitiesWizardPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddPOMWizard)WizardEventArgs.Wizard;
                    
                    break;
                case EventType.Active:
                    //save pom and activities temporary
                    SaveActivitiesAndPOMTemporary();
                    ShowAutoLearnedActivities();
                    break;
                case EventType.LeavingForNextPage:
                    DeleteTemporaryPOM();
                    break;
                default:
                    //nothing to do
                    break;
            }
        }

        public void ShowAutoLearnedActivities()
        {
            mActivitiesRepositoryViewPage = new ActivitiesRepositoryPage(mWizard.mPomLearnUtils.GetAutoLearnedActivities(), new Context());
            xSharedActivitiesFrame.Content = mActivitiesRepositoryViewPage;
        }
        public void SaveActivitiesAndPOMTemporary()
        {
            mWizard.mPomLearnUtils.SaveTemporaryPOM();
        }
        public void DeleteTemporaryPOM()
        {
            mWizard.mPomLearnUtils.DeleteTemporaryPOM();
        }

    }
}
