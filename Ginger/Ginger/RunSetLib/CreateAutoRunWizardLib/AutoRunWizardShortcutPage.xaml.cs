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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Ginger.RunSetLib.CreateAutoRunWizardLib;
using GingerCore.GeneralLib;
using GingerWPF.WizardLib;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.RunSetLib.CreateCLIWizardLib
{
    /// <summary>
    /// Interaction logic for CreateCLIContentPage.xaml
    /// </summary>
    public partial class AutoRunWizardShortcutPage : Page, IWizardPage
    {
        AutoRunWizard mAutoRunWizard;
        //int mExecutionCount = 1;

        public AutoRunWizardShortcutPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mAutoRunWizard = (AutoRunWizard)WizardEventArgs.Wizard;
                    mAutoRunWizard.AutoRunShortcut.CreateShortcut = false;
                    mAutoRunWizard.AutoRunShortcut.ShortcutFileName = WorkSpace.Instance.Solution.Name + "-" + mAutoRunWizard.RunsetConfig.Name + " Execution";
                    xShortcutPathTextbox.Init(mAutoRunWizard.mContext, mAutoRunWizard.AutoRunShortcut, nameof(RunSetAutoRunShortcut.ShortcutFolderPath), isVENeeded: false, isBrowseNeeded: true, browserType: Actions.UCValueExpression.eBrowserType.Folder);
                    BindingHandler.ObjFieldBinding(xShortcutDescriptionTextBox, System.Windows.Controls.TextBox.TextProperty, mAutoRunWizard.AutoRunShortcut, nameof(RunSetAutoRunShortcut.ShortcutFileName));
                    xDesktopRadioButton.IsChecked = true;
                    mAutoRunWizard.CliHelper.ShowAutoRunWindow = false;
                    mAutoRunWizard.AutoRunConfiguration.ParallelExecutionCount = 1;
                    break;

                case EventType.Active:
                    BindingHandler.ObjFieldBinding(xShortcutContentTextBox, System.Windows.Controls.TextBox.TextProperty, mAutoRunWizard.AutoRunShortcut, nameof(RunSetAutoRunShortcut.ShortcutContent), BindingMode: System.Windows.Data.BindingMode.OneWay);
                    BindingHandler.ObjFieldBinding(xExecutionServiceUrlTextBox, System.Windows.Controls.TextBox.TextProperty, mAutoRunWizard.AutoRunConfiguration, nameof(RunSetAutoRunConfiguration.ExecutionServiceUrl));

                    InitNumberPicker();

                    ShowHideCommandPnl();
                    if (mAutoRunWizard.AutoRunConfiguration.AutoRunEexecutorType == eAutoRunEexecutorType.Remote)
                    {
                        xRequestExecutionYesRadioButton.IsChecked = true;
                        xExecutionServiceUrlTextBox.AddValidationRule(new ValidateURLFormat());
                    }
                    else
                    {
                        xRequestExecutionNoRadioButton.IsChecked = true;
                        xExecutionServiceUrlTextBox.RemoveValidations(TextBox.TextProperty);
                    }
                    break;
            }

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xExecuteNow, Expander.VisibilityProperty, WorkSpace.Instance.UserProfile, nameof(WorkSpace.Instance.UserProfile.ShowEnterpriseFeatures), bindingConvertor: new GingerCore.GeneralLib.BoolVisibilityConverter());
        }

        private void InitNumberPicker()
        {
            xNumberPickerControl.PropertyChanged += XNumberPickerControl_PropertyChanged;
            xNumberPickerControl.MinCount = 1;
            xNumberPickerControl.MaxCount = 10;
        }

        private void XNumberPickerControl_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (mAutoRunWizard != null)
            {
                mAutoRunWizard.AutoRunConfiguration.ParallelExecutionCount = xNumberPickerControl.SelectedNumber;
            }
        }

        private void ShowHideCommandPnl()
        {
            if (mAutoRunWizard.AutoRunConfiguration.AutoRunEexecutorType == eAutoRunEexecutorType.Remote )
            {
                xCLICommandPnl.Visibility = Visibility.Collapsed;
                xShortCutCreationConfigsPnl.Visibility = Visibility.Collapsed;
                xCreateShortCutRadioPnl.Visibility = Visibility.Collapsed;

                if (mAutoRunWizard.AutoRunShortcut.StartExecution)
                {
                    xRequestSettingsPnl.Visibility = Visibility.Visible;
                }
            }
            else
            {
                xCreateShortCutRadioPnl.Visibility = Visibility.Visible;
                xCLICommandPnl.Visibility = Visibility.Visible;
                xRequestSettingsPnl.Visibility = Visibility.Collapsed;
            }
        }

        private void XCreateShortCutRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (mAutoRunWizard != null)
            {
                mAutoRunWizard.AutoRunShortcut.CreateShortcut = true;
                xShortCutCreationConfigsPnl.Visibility = Visibility.Visible;
            }
        }

        private void XDoNotCreateShortCutRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (mAutoRunWizard != null)
            {
                mAutoRunWizard.AutoRunShortcut.CreateShortcut = false;
                xShortCutCreationConfigsPnl.Visibility = Visibility.Collapsed;
            }
        }

        private void XDesktopRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (mAutoRunWizard != null)
            {
                mAutoRunWizard.AutoRunShortcut.ShortcutFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                xShortcutPathTextbox.Visibility = Visibility.Collapsed;
            }
        }

        private void XFolderRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (mAutoRunWizard != null)
            {
                mAutoRunWizard.AutoRunShortcut.ShortcutFolderPath = string.Empty;
                xShortcutPathTextbox.Visibility = Visibility.Visible;
            }
        }


        private void xRequestExecutionNoRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (mAutoRunWizard != null)
            {
                xParallelExecutionPnl.Visibility = Visibility.Collapsed;
                xRequestSettingsPnl.Visibility = Visibility.Collapsed;
                mAutoRunWizard.AutoRunShortcut.StartExecution = false;

                ShowHideCommandPnl();
            }
        }

        private void xRequestExecutionYesRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (mAutoRunWizard != null)
            {
                xParallelExecutionPnl.Visibility = Visibility.Visible;
                xRequestSettingsPnl.Visibility = Visibility.Visible;

                mAutoRunWizard.AutoRunShortcut.StartExecution = true;
                ShowHideCommandPnl();
            }
        }

        private void xCopyBtn_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(xShortcutContentTextBox.Text.ToString());
        }
    }
}
