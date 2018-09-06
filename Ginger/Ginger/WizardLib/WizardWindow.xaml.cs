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

using Ginger;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace GingerWPF.WizardLib
{
    /// <summary>
    /// Interaction logic for WizardWindow.xaml
    /// </summary>
    public partial class WizardWindow : Window, IWizardWindow
    {

        public static WizardWindow CurrentWizardWindow = null;

        WizardBase mWizard;

        List<ValidationError> mValidationErrors = new List<ValidationError>();

        public static void ShowWizard(WizardBase wizard, double width = 800)
        {
            WizardWindow wizardWindow = new WizardWindow(wizard);
            //wizardWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            //wizardWindow.Top = 0;
            //wizardWindow.MaxWidth = 500;
            wizardWindow.Width = width;
           wizardWindow.ShowDialog();
        }

        public WizardWindow(WizardBase wizard)
        {
            InitializeComponent();
            mWizard = wizard;
            wizard.mWizardWindow = this;

            this.Title = wizard.Title;

            // UpdateFinishButton();
            FinishButton.IsEnabled = false;

            SetterBaseCollection SBC = NavigationList.ItemContainerStyle.Setters;
            ((System.Windows.Setter)SBC[0]).Value = true;                        

            WizardEventArgs WizardEventArgs = new WizardEventArgs(mWizard, EventType.Init);                        
            foreach (WizardPage page in mWizard.Pages)
            {                
                // send init event
                ((IWizardPage)page.Page).WizardEvent(WizardEventArgs);

                // TODO: attach validation error handler
                ((Page)page.Page).AddHandler(Validation.ErrorEvent, new RoutedEventHandler(ValidationErrorHandler));
            }

            RefreshCurrentPage();
            NavigationList.ItemsSource = mWizard.Pages;

            CurrentWizardWindow = this;            
        }

        private void UpdateFinishButton()
        {
            FinishButton.IsEnabled = false;
            if (mValidationErrors.Count > 0) return;
            FinishButton.IsEnabled = true;
        }

        ~WizardWindow()
        {
            CurrentWizardWindow = null;
        }


        void RefreshCurrentPage()
        {
            WizardPage page = mWizard.GetCurrentPage();
            PageFrame.Content = page.Page;
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
            UpdateFinishButton();
        }




        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            bool bErrors = HasValidationsIssues();
            UpdateFinishButton();
            if (bErrors)
            {                
                return;
            }            

            mWizard.Next();
            UpdatePrevNextButton();

            RefreshCurrentPage();
        }


        // Need to be in base
        private bool HasValidationsIssues()
        {
            //Scan all controls with validations

            errorsFound = false;            
            SearchValidationsRecursive((Page)PageFrame.Content);

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
                        if (child is TextBox)
                        {
                            TextBox textBox = (TextBox)child;
                            bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
                        }
                        else if (child is ComboBox)
                        {
                            ComboBox comboBox = (ComboBox)child;
                            bindingExpression = comboBox.GetBindingExpression(ComboBox.SelectedValueProperty);
                        }
                        else if (child is Ginger.Agents.ucAgentControl)
                        {
                            Ginger.Agents.ucAgentControl agentControl = (Ginger.Agents.ucAgentControl)child;
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

                        //Custome controls Validations
                        if (errorsFound == false)
                        {
                            if (child is ucGrid)
                                errorsFound = ((ucGrid)child).HasValidationError();
                        }
                    }

                    if (errorsFound == true)
                        return;
                    else
                        SearchValidationsRecursive(child);
                }
            }
        }


        private void UpdatePrevNextButton()
        {
            
            if (mWizard.IsLastPage())
            {
                NextButton.IsEnabled = false;
            }
            else
            {
                NextButton.IsEnabled = true;
            }
            
            if (mWizard.IsFirstPage())
            {
                PrevButton.IsEnabled = false;
            }
            else
            {
                PrevButton.IsEnabled = true;
            }

        }
        

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            mWizard.Prev();
            UpdatePrevNextButton();

            RefreshCurrentPage();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            mWizard.Cancel();
            CloseWizard();
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            //First we validate all pages are OK
            foreach (WizardPage p in mWizard.Pages)
            {
                errorsFound = false;
                SearchValidationsRecursive((Page)p.Page);                 
                if (mValidationErrors.Count > 0 || errorsFound)// TODO: focus on the item and highlight
                {
                    mWizard.Pages.CurrentItem = p;
                    UpdatePrevNextButton();
                    RefreshCurrentPage();
                    return;
                }
            }
            


            // TODO: verify all apges pass validation

            NavigationList.SelectionChanged -= NavigationList_SelectionChanged;

            mWizard.ProcessFinish();

            CloseWizard();

            //if (mWizard.mWizardWindow == null)
            //{
            //    // If no page cancelled the Finish then all OK and we can close
            //    CurrentWizardWindow = null;                
            //    mWizard = null;

            //}
            //else
            //{
            //    UpdatePrevNextButton();
            //    RefreshCurrentPage();
            //    NavigationList.SelectionChanged += NavigationList_SelectionChanged;
            //}
        }

        private void CloseWizard()
        {
            mWizard.mWizardWindow = null;
            this.Close();
            CurrentWizardWindow = null;
        }

       

        private void NavigationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mWizard.Pages.CurrentItem = NavigationList.SelectedItem;
            tbSubTitle.Text= ((WizardPage)mWizard.Pages.CurrentItem).SubTitle;
            UpdatePrevNextButton();
            RefreshCurrentPage();
        }

        public void ProcessStarted()
        {
            xProcessingImage.Visibility = Visibility.Visible;
        }

        public void ProcessEnded()
        {
            xProcessingImage.Visibility = Visibility.Collapsed;
        }
        

        void IWizardWindow.NextButton(bool isEnabled)
        {
            NextButton.IsEnabled = isEnabled;
        }
    }
}
