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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.Actions.UserControls;
using GingerCore.Actions.VisualTesting;
using GingerCore.Drivers;
using GingerWPF.WizardLib;
using System;
using System.Drawing;
using System.Windows.Controls;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Interaction logic for MapUIElementsWizardPage.xaml
    /// </summary>
    public partial class POMScreenShotWizardPage : Page, IWizardPage
    {       
        AddPOMWizard mWizard;

        public POMScreenShotWizardPage()
        {
            InitializeComponent();
        }

        ScreenShotViewPage mScreenshotPage;

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddPOMWizard)WizardEventArgs.Wizard;                   
                    break;

                case EventType.Active:
                    ShowScreenShot();
                    break;
            }
        }

        public void ShowScreenShot()
        {
            mWizard.IWindowExplorerDriver.UnHighLightElements();
            mWizard.ScreenShot = ((IVisualTestingDriver)mWizard.Agent.Driver).GetScreenShot();
            mScreenshotPage = new ScreenShotViewPage(mWizard.POM.Name, mWizard.ScreenShot);
            MainFrame.Content = mScreenshotPage;
        }


        private void TakeScreenShotButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            ShowScreenShot();
        }
    }
}
