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

using Amdocs.Ginger.Common;
using GingerCore.GeneralLib;
using GingerWPF.WizardLib;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Interaction logic for LearnWizardPage.xaml
    /// </summary>
    public partial class POMObjectsMappingWizardPage : Page, IWizardPage
    {
        public AddPOMWizard mWizard;
        public AddPOMFromScreenshotWizard mScreenshotWizard;
        PomAllElementsPage mPomAllElementsPage = null;
        bool isCallFromScreenShotPage;

        public POMObjectsMappingWizardPage(bool isCallFromScreenshot=false)
        {
            isCallFromScreenShotPage = isCallFromScreenshot;
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    if(isCallFromScreenShotPage)
                    {
                        mScreenshotWizard = (AddPOMFromScreenshotWizard)WizardEventArgs.Wizard;
                        if (!mScreenshotWizard.ManualElementConfiguration)
                        {
                            InitilizePomElementsMappingPage();
                        }
                    }
                    else
                    {
                        mWizard = (AddPOMWizard)WizardEventArgs.Wizard;
                        if (!mWizard.ManualElementConfiguration)
                        {
                            InitilizePomElementsMappingPage();
                        }
                    }
                    
                    break;

                case EventType.Active:
                    if(isCallFromScreenShotPage)
                    {
                        if (mPomAllElementsPage.mAgent == null)
                        {
                            mPomAllElementsPage.SetAgent(mScreenshotWizard.mPomLearnUtils.Agent);
                        }

                        if (mScreenshotWizard.ManualElementConfiguration)
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
                    }
                    else
                    {
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
                    }
                    
                    break;

                case EventType.LeavingForNextPage:
                case EventType.Finish:
                    if(isCallFromScreenShotPage)
                    {
                        mPomAllElementsPage.FinishEditInAllGrids();
                        if (mPomAllElementsPage != null)
                        {
                            mPomAllElementsPage.StopSpy();
                        }
                        mScreenshotWizard.mPomLearnUtils.ClearStopLearning();
                    }
                    else
                    {
                        mPomAllElementsPage.FinishEditInAllGrids();
                        if (mPomAllElementsPage != null)
                        {
                            mPomAllElementsPage.StopSpy();
                        }
                        mWizard.mPomLearnUtils.ClearStopLearning();
                    }
                    
                    break;
                case EventType.Cancel:
                    if(isCallFromScreenShotPage)
                    {
                        if (mPomAllElementsPage != null)
                        {
                            mPomAllElementsPage.StopSpy();
                        }
                        mScreenshotWizard.mPomLearnUtils.ClearStopLearning();
                    }
                    else
                    {
                        if (mPomAllElementsPage != null)
                        {
                            mPomAllElementsPage.StopSpy();
                        }
                        mWizard.mPomLearnUtils.ClearStopLearning();
                    }
                    
                    break;
            }
        }

        private DispatcherTimer timer;
        private TimeSpan elapsedTime;
        private void StartTimer()
        {
            // Create and configure the DispatcherTimer
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // Update every second
            };
            timer.Tick += Timer_Tick;

            // Initialize elapsed time
            elapsedTime = TimeSpan.Zero;

            // Start the timer
            try
            {
                timer.Start();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Error while starting the timer", ex);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Increment elapsed time by 1 second
            elapsedTime = elapsedTime.Add(TimeSpan.FromSeconds(1));

            // Update the timer display
            timerText.Text = $"{(int)elapsedTime.TotalMinutes:00}:{elapsedTime.Seconds:00}";
        }

        // You can stop the timer if needed
        private void StopTimer()
        {
            if (timer != null)
            {
                try
                {
                    timer.Stop();
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Error while stopping the timer", ex);
                }
            }
        }

        private void BringToFocus()
        {
            try
            {
                Window window = Window.GetWindow(this);
                if (window != null)
                {
                    window.Activate();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Error while bring Ginger window to front", ex);
            }
        }

        private async void Learn()
        {
            if(isCallFromScreenShotPage)
            {
                if (!mScreenshotWizard.IsLearningWasDone)
                {
                    try
                    {
                        mScreenshotWizard.IsLearningWasDone = false;
                        mScreenshotWizard.ProcessStarted();
                        StartTimer();
                        xReLearnButton.Visibility = Visibility.Collapsed;
                        xStopLoadButton.ButtonText = "Stop";
                        xStopLoadButton.IsEnabled = true;
                        mScreenshotWizard.mPomLearnUtils.ClearStopLearning();
                        xStopLoadButton.Visibility = Visibility.Visible;

                        await mScreenshotWizard.mPomLearnUtils.Learn();

                        BringToFocus();

                        mScreenshotWizard.IsLearningWasDone = true;
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToUser(eUserMsgKey.POMWizardFailedToLearnElement, ex.Message);
                        mScreenshotWizard.IsLearningWasDone = false;
                    }
                    finally
                    {
                        xStopLoadButton.Visibility = Visibility.Collapsed;
                        xReLearnButton.Visibility = Visibility.Visible;
                        mScreenshotWizard.ProcessEnded();
                        StopTimer();
                        Reporter.ToLog(eLogLevel.INFO, $"Total time taken to learn '{mScreenshotWizard.mPomLearnUtils.POM.Name}' POM is {(int)elapsedTime.TotalMinutes:00}:{elapsedTime.Seconds:00}");
                    }
                }
            }
            else
            {
                if (!mWizard.IsLearningWasDone)
                {
                    try
                    {
                        mWizard.IsLearningWasDone = false;
                        mWizard.ProcessStarted();
                        StartTimer();
                        xReLearnButton.Visibility = Visibility.Collapsed;
                        xStopLoadButton.ButtonText = "Stop";
                        xStopLoadButton.IsEnabled = true;
                        mWizard.mPomLearnUtils.ClearStopLearning();
                        xStopLoadButton.Visibility = Visibility.Visible;

                        await mWizard.mPomLearnUtils.Learn();

                        BringToFocus();

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
                        StopTimer();
                        Reporter.ToLog(eLogLevel.INFO, $"Total time taken to learn '{mWizard.mPomLearnUtils.POM.Name}' POM is {(int)elapsedTime.TotalMinutes:00}:{elapsedTime.Seconds:00}");
                    }
                }
            }
            
        }


        private void InitilizePomElementsMappingPage()
        {
            if(isCallFromScreenShotPage)
            {
                if (mPomAllElementsPage == null)
                {
                    mPomAllElementsPage = new PomAllElementsPage(mScreenshotWizard.mPomLearnUtils.POM, PomAllElementsPage.eAllElementsPageContext.AddPOMWizard, false)
                    {
                        ShowTestAllElementsButton = Visibility.Collapsed
                    };
                    mPomAllElementsPage.mappedUIElementsPage.MainElementsGrid.ValidationRules.Add(ucGrid.eUcGridValidationRules.CantBeEmpty);
                    xPomElementsMappingPageFrame.ClearAndSetContent(mPomAllElementsPage);
                }
            }
            else
            {
                if (mPomAllElementsPage == null)
                {
                    mPomAllElementsPage = new PomAllElementsPage(mWizard.mPomLearnUtils.POM, PomAllElementsPage.eAllElementsPageContext.AddPOMWizard, false)
                    {
                        ShowTestAllElementsButton = Visibility.Collapsed
                    };
                    mPomAllElementsPage.mappedUIElementsPage.MainElementsGrid.ValidationRules.Add(ucGrid.eUcGridValidationRules.CantBeEmpty);
                    xPomElementsMappingPageFrame.ClearAndSetContent(mPomAllElementsPage);
                }
            }
            
        }

        private void StopButtonClicked(object sender, RoutedEventArgs e)
        {
            
            if (isCallFromScreenShotPage)
            {
                xStopLoadButton.ButtonText = "Stopping...";
                xStopLoadButton.IsEnabled = false;
                mScreenshotWizard.mPomLearnUtils.StopLearning();
            }
            else
            {
                xStopLoadButton.ButtonText = "Stopping...";
                xStopLoadButton.IsEnabled = false;
                mWizard.mPomLearnUtils.StopLearning();
            }
            
        }


        private void ReLearnButtonClicked(object sender, RoutedEventArgs e)
        {
            if(isCallFromScreenShotPage)
            {
                if (Reporter.ToUser(eUserMsgKey.POMWizardReLearnWillDeleteAllElements) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                {
                    mScreenshotWizard.IsLearningWasDone = false;
                    Learn();
                }
            }
            else
            {
                if (Reporter.ToUser(eUserMsgKey.POMWizardReLearnWillDeleteAllElements) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                {
                    mWizard.IsLearningWasDone = false;
                    Learn();
                }
            }
            
        }
    }
}
