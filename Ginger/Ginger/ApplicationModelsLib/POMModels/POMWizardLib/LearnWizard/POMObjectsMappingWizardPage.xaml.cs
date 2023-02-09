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

using Amdocs.Ginger.Common;
using GingerWPF.WizardLib;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Interaction logic for LearnWizardPage.xaml
    /// </summary>
    public partial class POMObjectsMappingWizardPage : Page, IWizardPage
    {
        public AddPOMWizard mWizard;                            
        PomAllElementsPage mPomAllElementsPage = null;        

        public POMObjectsMappingWizardPage()
        {
            InitializeComponent();                       
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddPOMWizard)WizardEventArgs.Wizard;
                    if (!mWizard.ManualElementConfiguration)
                    {                        
                        InitilizePomElementsMappingPage();
                    }
                    break;

                case EventType.Active:
                    if (mPomAllElementsPage.mAgent == null)
                    {
                        mPomAllElementsPage.SetAgent(mWizard.mPomLearnUtils.Agent);
                    }

                    if (mWizard.ManualElementConfiguration)
                    {
                        xReLearnButton.Visibility = Visibility.Hidden;
                        mPomAllElementsPage.mappedUIElementsPage.MainElementsGrid.ValidationRules.Clear();
                    }
                    else
                    {
                        mPomAllElementsPage.mappedUIElementsPage.MainElementsGrid.ValidationRules.Clear();
                        mPomAllElementsPage.mappedUIElementsPage.MainElementsGrid.ValidationRules.Add(ucGrid.eUcGridValidationRules.CantBeEmpty);

                        xReLearnButton.Visibility = Visibility.Visible;
                        
                        Learn();
                    }
                    break;

                case EventType.LeavingForNextPage:
                case EventType.Finish:
                    mPomAllElementsPage.FinishEditInAllGrids();
                    if (mPomAllElementsPage != null)
                    {
                        mPomAllElementsPage.StopSpy();
                    }
                    mWizard.mPomLearnUtils.ClearStopLearning();
                    break;
                case EventType.Cancel:
                    if (mPomAllElementsPage != null)
                    {
                        mPomAllElementsPage.StopSpy();
                    }
                    mWizard.mPomLearnUtils.ClearStopLearning();
                    break;
            }
        }

        private async void Learn()
        {
            if (!mWizard.IsLearningWasDone)
            {
                try
                {
                    mWizard.IsLearningWasDone = false;
                    mWizard.ProcessStarted();
                    xReLearnButton.Visibility = Visibility.Collapsed;
                    xStopLoadButton.ButtonText = "Stop";
                    xStopLoadButton.IsEnabled = true;
                    mWizard.mPomLearnUtils.ClearStopLearning();
                    xStopLoadButton.Visibility = Visibility.Visible;

                    await mWizard.mPomLearnUtils.Learn();

                    mWizard.IsLearningWasDone = true;
                }
                catch (Exception ex)
                {
                    Reporter.ToUser(eUserMsgKey.POMWizardFailedToLearnElement, ex.Message);
                    mWizard.IsLearningWasDone = false;
                }
                finally
                {
                    xStopLoadButton.Visibility = Visibility.Collapsed;
                    xReLearnButton.Visibility = Visibility.Visible;
                    mWizard.ProcessEnded();
                }
            }
        }


        private void InitilizePomElementsMappingPage()
        {
            if (mPomAllElementsPage == null)
            {
                mPomAllElementsPage = new PomAllElementsPage(mWizard.mPomLearnUtils.POM, PomAllElementsPage.eAllElementsPageContext.AddPOMWizard,false);
                mPomAllElementsPage.ShowTestAllElementsButton = Visibility.Collapsed;
                mPomAllElementsPage.mappedUIElementsPage.MainElementsGrid.ValidationRules.Add(ucGrid.eUcGridValidationRules.CantBeEmpty);
                xPomElementsMappingPageFrame.Content = mPomAllElementsPage;
            }
        }

        private void StopButtonClicked(object sender, RoutedEventArgs e)
        {
            xStopLoadButton.ButtonText = "Stopping...";
            xStopLoadButton.IsEnabled = false;
            mWizard.mPomLearnUtils.StopLearning();
        }
         

        private void ReLearnButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.POMWizardReLearnWillDeleteAllElements) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                mWizard.IsLearningWasDone = false;
                Learn();
            }
        }
    }
}
