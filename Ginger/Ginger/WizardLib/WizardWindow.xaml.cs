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

        public static void ShowWizard(WizardBase wizard, double width = 800, bool DoNotShowAsDialog = false)
        {
            WizardWindow wizardWindow = new WizardWindow(wizard);
            wizardWindow.Width = width;
            
            if(DoNotShowAsDialog)
            {
                wizardWindow.Topmost = true;
                wizardWindow.Show();
            }
            else
            {
                wizardWindow.ShowDialog();
            }
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
                ((IWizardPage)page.Page).WizardEvent(WizardEventArgs);

                // TODO: attach validation error handler
                ((Page)page.Page).AddHandler(Validation.ErrorEvent, new RoutedEventHandler(ValidationErrorHandler));
            }

            RefreshCurrentPage();
            NavigationList.ItemsSource = mWizard.Pages;

            CurrentWizardWindow = this;            
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
            //UpdateFinishButton();
        }




        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            bool bErrors = HasValidationsIssues();
            if (bErrors)
            {                
                return;
            }

            mWizard.Next();
            //UpdateFinishButton();
            UpdatePrevNextButton();
            RefreshCurrentPage();
        }


        // Need to be in base
        private bool HasValidationsIssues(Page pageToScan=null)
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
                        if (child is TextBox)
                        {
                            TextBox textBox = (TextBox)child;
                            bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
                        }
                        else if (child is ComboBox)
                        {
                            ComboBox comboBox = (ComboBox)child;
                            bindingExpression = comboBox.GetBindingExpression(ComboBox.SelectedValueProperty);
                            if(bindingExpression == null)
                            {
                                bindingExpression = comboBox.GetBindingExpression(ComboBox.TextProperty);
                            }
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
                xNextButton.IsEnabled = false;
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
            mWizard.Prev();
            UpdatePrevNextButton();
            //UpdateFinishButton();
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
            xNextButton.IsEnabled = isEnabled;
        }
    }
}
