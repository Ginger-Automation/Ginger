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
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.POM;
using Ginger;
using GingerCore.Drivers;
using GingerCore.GeneralLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace GingerWPF.WizardLib
{
    /// <summary>
    /// Interaction logic for WizardWindow.xaml
    /// </summary>
    public partial class WizardWindow : Window, IWizardWindow
    {

        public static WizardWindow CurrentWizardWindow = null;

        WizardBase mWizard;

        List<ValidationError> mValidationErrors = [];

        private POMUtils _subscribedPOMUtils;

        public static void ShowWizard(WizardBase wizard, double width = 1000, double height = 800, bool DoNotShowAsDialog = false)
        {
            WizardWindow wizardWindow = new WizardWindow(wizard);
            wizardWindow.Dispatcher.Invoke(() =>
            {
                wizardWindow.Height = height;
                wizardWindow.Width = width;
                if (!wizard.IsNavigationListEnabled)
                {
                    SetterBaseCollection sbc = wizardWindow.NavigationList.ItemContainerStyle.Setters;
                    sbc.Add(new Setter(ListBoxItem.IsEnabledProperty, wizard.IsNavigationListEnabled));
                    ((System.Windows.Setter)sbc[^1]).Value = wizard.IsNavigationListEnabled;
                }
                if (DoNotShowAsDialog)
                {
                    wizardWindow.Owner = App.MainWindow;//adding owner so it will come on top
                    wizardWindow.Show();
                }
                else
                {
                    wizardWindow.ShowDialog();
                }
            });
        }

        public WizardWindow(WizardBase wizard)
        {
            InitializeComponent();
            mWizard = wizard;
            wizard.mWizardWindow = this;

            this.Title = wizard.Title;

            // UpdateFinishButton();
            //xFinishButton.IsEnabled = false;

            SetterBaseCollection SBC = NavigationList.ItemContainerStyle.Setters;
            ((System.Windows.Setter)SBC[0]).Value = true;

            WizardEventArgs WizardEventArgs = new WizardEventArgs(mWizard, EventType.Init);
            foreach (WizardPage page in mWizard.Pages)
            {
                // send init event
                page.Page.WizardEvent(WizardEventArgs);

                // TODO: attach validation error handler
                ((Page)page.Page).AddHandler(Validation.ErrorEvent, new RoutedEventHandler(ValidationErrorHandler));
            }

            RefreshCurrentPage();
            NavigationList.ItemsSource = mWizard.Pages;

            CurrentWizardWindow = this;
        }

        private void UpdateFinishButton()
        {
            xFinishButton.IsEnabled = false;
            if (mValidationErrors.Count > 0)
            {
                return;
            }

            if (mWizard.IsLastPage())
            {
                xFinishButton.IsEnabled = true;
            }
        }

        ~WizardWindow()
        {
            CurrentWizardWindow = null;
        }


        void RefreshCurrentPage()
        {
            WizardPage page = mWizard.GetCurrentPage();
            PageFrame.ClearAndSetContent(page.Page);
            tbSubTitle.Text = page.SubTitle;
            // sync the list too
            NavigationList.SelectedItem = page;
        }

        private void ValidationErrorHandler(object sender, EventArgs e)
        {
            ValidationErrorEventArgs validationErrorEventArgs = (ValidationErrorEventArgs)e;
            WizardPage wizardPage = (WizardPage)mWizard.Pages.CurrentItem;

            if (validationErrorEventArgs.Action == ValidationErrorEventAction.Added)
            {
                mValidationErrors.Add(validationErrorEventArgs.Error);
            }
            else
            {
                mValidationErrors.Remove(validationErrorEventArgs.Error);
            }

            if (mValidationErrors.Count == 0)
            {
                wizardPage.HasErrors = false;
            }
            else
            {
                wizardPage.HasErrors = true;
            }


            // mWizard.UpdateButtons();
            //UpdateFinishButton();
        }




        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            bool bErrors = HasValidationsIssues();
            if (bErrors)
            {
                return;
            }

            if (xProcessingImage.Visibility == Visibility.Visible)
            {
                Reporter.ToUser(eUserMsgKey.WizardCantMoveWhileInProcess, "Next");
            }
            else
            {
                mWizard.Next();
                //UpdateFinishButton();
                UpdatePrevNextButton();
                RefreshCurrentPage();
            }
        }


        // Need to be in base
        private bool HasValidationsIssues(Page pageToScan = null)
        {
            //Scan all controls with validations
            errorsFound = false;
            if (pageToScan == null)
            {
                pageToScan = (Page)PageFrame.Content;
            }
            SearchValidationsRecursive(pageToScan);

            return errorsFound;
        }


        bool errorsFound = false;
        public void SearchValidationsRecursive(DependencyObject depObj)
        {
            if (depObj != null)
            {
                int depObjCount = VisualTreeHelper.GetChildrenCount(depObj);
                for (int i = 0; i < depObjCount; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null)
                    {
                        //.NET Controls Validation
                        BindingExpression bindingExpression = null;
                        if (child is TextBox textBox)
                        {
                            bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
                        }
                        else if (child is ComboBox comboBox)
                        {
                            bindingExpression = comboBox.GetBindingExpression(ComboBox.SelectedValueProperty);
                            if (bindingExpression == null)
                            {
                                bindingExpression = comboBox.GetBindingExpression(ComboBox.TextProperty);
                            }
                        }
                        else if (child is Ginger.Agents.ucAgentControl agentControl)
                        {
                            bindingExpression = agentControl.GetBindingExpression(Ginger.Agents.ucAgentControl.SelectedAgentProperty);
                        }

                        if (bindingExpression != null)
                        {
                            // do if there is validation bindingExpression.
                            bindingExpression.UpdateSource();

                            if (bindingExpression.HasValidationError)
                            {
                                errorsFound = true;
                            }
                        }

                        //Custom controls Validations
                        if (errorsFound == false)
                        {
                            if (child is ucGrid)
                            {
                                errorsFound = ((ucGrid)child).HasValidationError();
                            }
                            else if (child is UCTreeView)
                            {
                                errorsFound = ((UCTreeView)child).HasValidationError();
                            }
                        }
                    }

                    if (errorsFound == true)
                    {
                        return;
                    }
                    else
                    {
                        SearchValidationsRecursive(child);
                    }
                }
            }
        }


        private void UpdatePrevNextButton()
        {
            if (mWizard.IsLastPage())
            {
                xNextButton.IsEnabled = false;

                if (mWizard.DisableBackBtnOnLastPage)
                {
                    xPrevButton.IsEnabled = false;
                    return;
                }
            }
            else
            {
                xNextButton.IsEnabled = true;
            }
            if (mWizard.IsFirstPage())
            {
                xPrevButton.IsEnabled = false;
            }
            else
            {
                xPrevButton.IsEnabled = true;
            }
        }


        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (xProcessingImage.Visibility == Visibility.Visible)
            {
                Reporter.ToUser(eUserMsgKey.WizardCantMoveWhileInProcess, "Previous");
            }
            else
            {
                mWizard.Prev();
                UpdatePrevNextButton();
                //UpdateFinishButton();
                RefreshCurrentPage();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            WindowCloseWasHandled = true;

            if (xProcessingImage.Visibility == Visibility.Visible)
            {
                Reporter.ToUser(eUserMsgKey.WizardCantMoveWhileInProcess, "Cancel");
            }
            else
            {
                //if (Reporter.ToUser(eUserMsgKey.WizardSureWantToCancel) == eUserMsgSelection.Yes)
                //{
                mWizard.Cancel();
                if (sender != null && sender is bool && (bool)sender == false)
                {
                    return;//close already been done
                }
                else
                {
                    CloseWizard();
                }
                //}
            }
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            //First we validate all pages are OK
            WindowCloseWasHandled = true;

            if (xProcessingImage.Visibility == Visibility.Visible)
            {
                Reporter.ToUser(eUserMsgKey.WizardCantMoveWhileInProcess, "Finish");
            }
            else
            {
                foreach (WizardPage p in mWizard.Pages)
                {
                    errorsFound = false;
                    if (VisualTreeHelper.GetChildrenCount((Page)p.Page) == 0)
                    {
                        JumpToPage(p);
                    }
                    if (HasValidationsIssues((Page)p.Page))// TODO: focus on the item and highlight
                    {
                        if (mWizard.Pages.CurrentItem != p)
                        {
                            JumpToPage(p);
                        }
                        return;
                    }
                }

                // TODO: verify all pages pass validation

                NavigationList.SelectionChanged -= NavigationList_SelectionChanged;

                mWizard.ProcessFinish();

                CloseWizard();
            }
        }

        private void JumpToPage(WizardPage pageToJumpTo)
        {
            mWizard.Pages.CurrentItem = pageToJumpTo;
            pageToJumpTo.Page.WizardEvent(new WizardEventArgs(mWizard, EventType.Active));
            UpdatePrevNextButton();
            RefreshCurrentPage();
            GingerCore.General.DoEvents();
        }

        private void CloseWizard()
        {
            mWizard.mWizardWindow = null;
            if (_subscribedSeleniumDriver != null)
            {
                _subscribedSeleniumDriver.PropertyChanged -= SeleniumDriver_PropertyChanged;
                _subscribedSeleniumDriver = null;
            }
            if(_subscribedAppiumDriver != null)
            {
                _subscribedAppiumDriver.PropertyChanged -= AppiumDriver_PropertyChanged;
                _subscribedAppiumDriver = null;
            }
            this.Close();
            CurrentWizardWindow = null;
        }

        public void ShowFinishButton(bool isVisible)
        {
            xFinishButton.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void NavigationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mWizard.Pages.CurrentItem = NavigationList.SelectedItem;
            if (!HasValidationsIssues())
            {
                tbSubTitle.Text = ((WizardPage)mWizard.Pages.CurrentItem).SubTitle;
                UpdatePrevNextButton();
                RefreshCurrentPage();
            }
        }

        public void ProcessStarted()
        {
            Dispatcher.Invoke(() =>
            {
                xProcessingImage.Visibility = Visibility.Visible;
            });
        }

        public void ProcessEnded()
        {
            Dispatcher.Invoke(() =>
            {
                xProcessingImage.Visibility = Visibility.Collapsed;
            });
        }


        void IWizardWindow.NextButton(bool isEnabled)
        {
            xNextButton.IsEnabled = isEnabled;
        }

        void IWizardWindow.SetFinishButtonEnabled(bool isEnabled)
        {
            xFinishButton.IsEnabled = isEnabled;
        }

        bool WindowCloseWasHandled = false;

        private void CloseWindowClicked(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!WindowCloseWasHandled)
            {
                //mWizard.Cancel();
                CancelButton_Click(false, null);//false means that window already been closed
            }

        }

        void IWizardWindow.SetPrevButtonEnabled(bool isEnabled)
        {
            xPrevButton.IsEnabled = isEnabled;
        }

        private DispatcherTimer AIFineTunetimer;
        private TimeSpan AIFineTuneelapsedTime; 
        private string AIFineTuneBaseText;
        private void StartAIFineTuneTimer()
        {
            // Create and configure the DispatcherTimer
            AIFineTunetimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // Update every second
            };
            AIFineTunetimer.Tick += AIFineTuneTimer_Tick;

            // Initialize elapsed time
            AIFineTuneelapsedTime = TimeSpan.Zero;
            // Capture base label once and initialize displayed text
            Dispatcher.Invoke(() =>
            {
                string text = xAIProcessingText.Text;
                AIFineTuneBaseText = string.IsNullOrWhiteSpace(text) ? "AI Fine-Tuning Processing" : text;

                xAIProcessingText.Text = $"{AIFineTuneBaseText}";
            });
            // Start the timer
            try
            {
                AIFineTunetimer.Start();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Error while starting the timer", ex);
            }
        }

        private void AIFineTuneTimer_Tick(object sender, EventArgs e)
        {
            // Increment elapsed time by 1 second
            AIFineTuneelapsedTime = AIFineTuneelapsedTime.Add(TimeSpan.FromSeconds(1));

            // Update the timer display
            xAIProcessingText.Text = $"{AIFineTuneBaseText}, {(int)AIFineTuneelapsedTime.TotalMinutes:00}:{AIFineTuneelapsedTime.Seconds:00}";
        }

        // You can stop the timer if needed
        private void AIFineTuneStopTimer()
        {
            if (AIFineTunetimer != null)
            {
                try
                {
                    AIFineTunetimer.Stop();
                    // Reset label to base text when stopping
                    if (!string.IsNullOrWhiteSpace(AIFineTuneBaseText))
                    {
                        xAIProcessingText.Text = AIFineTuneBaseText;
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Error while stopping the timer", ex);
                }
            }
        }

        private SeleniumDriver _subscribedSeleniumDriver;
        public void SubscribeToSeleniumDriver(SeleniumDriver seleniumDriver)
        {
            // Unsubscribe from previous instance if needed
            if (_subscribedPOMUtils != null)
            {
                _subscribedPOMUtils.ProcessingStatusChanged -= POMUtils_ProcessingStatusChanged;
            }

            if (seleniumDriver.POMUtils != null) 
            { 
                seleniumDriver.POMUtils.ProcessingStatusChanged += POMUtils_ProcessingStatusChanged;
                _subscribedPOMUtils = seleniumDriver.POMUtils;
            }
        }

        private void POMUtils_ProcessingStatusChanged(object sender, bool isProcessing)
        {
            if (isProcessing)
            {
                AIProcessStarted();
            }
            else
            {
                AIProcessStopped();
            }
        }

        private void SeleniumDriver_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SeleniumDriver.IsProcessing))
            {
                if (sender is SeleniumDriver seleniumDriverdata && seleniumDriverdata.IsProcessing)
                {
                    Dispatcher.Invoke(AIProcessStarted);
                }
                else
                {
                    Dispatcher.Invoke(AIProcessStopped);
                }
            }
        }

        private GenericAppiumDriver _subscribedAppiumDriver;
        public void SubscribeToAppiumDriver(GenericAppiumDriver AppiumDriver)
        {

            // Unsubscribe from previous instance if needed
            if (_subscribedPOMUtils != null)
            {
                _subscribedPOMUtils.ProcessingStatusChanged -= POMUtils_ProcessingStatusChanged;
            }

            if (AppiumDriver.POMUtils != null) 
            {
                AppiumDriver.POMUtils.ProcessingStatusChanged += POMUtils_ProcessingStatusChanged;
                _subscribedPOMUtils = AppiumDriver.POMUtils;
            }
        }

        private void AppiumDriver_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GenericAppiumDriver.IsProcessing))
            {
                if (sender is GenericAppiumDriver AppiumDriverdata && AppiumDriverdata.IsProcessing)
                {
                    Dispatcher.Invoke(AIProcessStarted);
                }
                else
                {
                    Dispatcher.Invoke(AIProcessStopped);
                }
            }
        }

        public void AIProcessStarted()
        {
            Dispatcher.Invoke(() =>
            {
                xAIProcessingText.Visibility = Visibility.Visible;
            });
            StartAIFineTuneTimer();
        }

        public void AIProcessStopped()
        {
            Dispatcher.Invoke(() =>
            {
                xAIProcessingText.Visibility = Visibility.Collapsed;
            });
            AIFineTuneStopTimer();
        }
    }
}
