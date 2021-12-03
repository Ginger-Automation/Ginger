#region License
/*
Copyright © 2014-2021 European Support Limited

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
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using GingerCore;
using GingerCore.GeneralLib;
using GingerWPF.WizardLib;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.RunSetLib.CreateCLIWizardLib
{
    /// <summary>
    /// Interaction logic for CreateCLIChooseTypePage.xaml
    /// </summary>
    public partial class AutoRunWizardCLITypePage : Page, IWizardPage
    {
        private AutoRunWizard mAutoRunWizard;
        private bool mResetCLIContent;
        private string mTempCLIContent;

        public AutoRunWizardCLITypePage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {            
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mAutoRunWizard = (AutoRunWizard)WizardEventArgs.Wizard;

                    BindingHandler.ObjFieldBinding(xConfigurationNameTextBox, System.Windows.Controls.TextBox.TextProperty, mAutoRunWizard.AutoRunConfiguration, nameof(RunSetAutoRunConfiguration.ConfigName));
                    xConfigurationPathTextbox.Init(mAutoRunWizard.mContext, mAutoRunWizard.AutoRunConfiguration, nameof(RunSetAutoRunConfiguration.ConfigFileFolderPath), isVENeeded: false, isBrowseNeeded: true, browserType: Actions.UCValueExpression.eBrowserType.Folder);
                    xParametersRadioButton.IsChecked = true;
                    BindingHandler.ObjFieldBinding(xCLIContentTextBox, TextBox.TextProperty, mAutoRunWizard.AutoRunConfiguration, nameof(mAutoRunWizard.AutoRunConfiguration.CLIContent), BindingMode: System.Windows.Data.BindingMode.TwoWay);
                    mAutoRunWizard.AutoRunConfiguration.CLIContent = mAutoRunWizard.AutoRunConfiguration.GetCLIContent();
                    mTempCLIContent = mAutoRunWizard.AutoRunConfiguration.CLIContent;
                    mResetCLIContent = false;
                    break;

                case EventType.Active:
                    if (mAutoRunWizard.AutoRunConfiguration.SelectedCLI.Verb != "run")
                    {
                        xCLIContentTextBox.AddValidationRule(new ValidateJsonFormat());
                    }
                    ResetCLIContent(mResetCLIContent);
                    ShowHelp();
                    ShowContent();
                    break;

                case EventType.Prev:
                    if (mTempCLIContent != mAutoRunWizard.AutoRunConfiguration.CLIContent && WizardEventArgs.Wizard.GetCurrentPage().Page == this  && Reporter.ToUser(eUserMsgKey.RunsetAutoConfigBackWarn, "Configuartions customizations will be lost,do you want to go back?") == eUserMsgSelection.No)
                    {
                        WizardEventArgs.CancelEvent = true;
                    }
                    else if(WizardEventArgs.Wizard.GetCurrentPage().Page == this && mTempCLIContent != mAutoRunWizard.AutoRunConfiguration.CLIContent)
                    {
                        WizardEventArgs.CancelEvent = false;
                        ResetCLIContent(mResetCLIContent = true);
                        mTempCLIContent = mAutoRunWizard.AutoRunConfiguration.CLIContent;
                        ShowContent();
                    }
                    else
                    {
                        WizardEventArgs.CancelEvent = false;
                    }
                    break;
                case EventType.LeavingForNextPage:
                    mResetCLIContent = false;
                    break;
            }
        }

        private void ResetCLIContent(bool resetCLIContent)
        {
            if (resetCLIContent)
            {
                mAutoRunWizard.AutoRunConfiguration.CLIContent = mAutoRunWizard.AutoRunConfiguration.GetCLIContent();
                mTempCLIContent = mAutoRunWizard.AutoRunConfiguration.CLIContent;
            }
        }

        private void ShowContent()
        {
            xCLIContentTextBox.Text = mAutoRunWizard.AutoRunConfiguration.CLIContent;                    
        }

        private void ShowHelp()
        {
            string helpContent = string.Empty;
            /*
            if (xConfigRadioButton.IsChecked == true)
            {
                helpContent = string.Format("Simple text file which contain the execution configurations." + GetRowDown() + "To be used in case {0} already exist in the Solution and only need to trigger it." + GetRowDown() + "Executed by triggering Ginger executer with the argument 'ConfigFile=%ConfigFilePath%', Example: Ginger.exe ConfigFile=\"C:\\Ginger\\Regression1.Ginger.AutoRunConfigs.Config\"", GingerDicser.GetTermResValue(eTermResKey.RunSet));
            }*/
            if (xDynamicRadioButton.IsChecked == true)
            {
                helpContent = "Execute customized or virtual Run set via configuration file.";
            }
            else if (xScriptRadioButton.IsChecked == true)
            {
                helpContent = string.Format("Script file written in C# which implement Ginger execution flow." + GetRowDown() + "Enable to create {0} with loops and much more complex execution." + GetRowDown() + "Executed by triggering Ginger executer with the argument 'Script=%ScriptFilePath%', Example: Ginger.exe Script=\"C:\\Ginger\\FeatureBTesting.Ginger.AutoRunConfigs.script\"", GingerDicser.GetTermResValue(eTermResKey.RunSet));
            }
            else if (xParametersRadioButton.IsChecked == true)
            {
                helpContent = "Execute existing Run set as is using simple arguments.";
            }
            if (xRequestRadioButton.IsChecked == true)
            {
                helpContent = "Execute customized or virtual Run set on remote/cloud.";
            }

            xCLITypeHelpTextBlock.Text = helpContent;
        }

        private string GetRowDown()
        {
            return Environment.NewLine + Environment.NewLine;
        }

        CLIConfigFile mCLIConfigFile;
        private void xConfigRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (mCLIConfigFile == null)
            {
                mCLIConfigFile = new CLIConfigFile();
            }

            if (mAutoRunWizard != null)
            {
                ResetCLIContent(mResetCLIContent=true);
                ShowHelp();
                ShowContent();
            }

            xConfigFileSettingsPnl.Visibility = Visibility.Visible;
        }        

        CLIDynamicFile mCLIDynamicFile;
        private void XDynamicRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (mCLIDynamicFile == null)
            {
                mCLIDynamicFile = new CLIDynamicFile(CLIDynamicFile.eFileType.JSON);
                GingerCore.General.FillComboFromEnumObj(xDynamicFileTypeCombo, mCLIDynamicFile.FileType);
                BindingHandler.ObjFieldBinding(xDynamicFileTypeCombo, ComboBox.TextProperty, mCLIDynamicFile, nameof(CLIDynamicFile.FileType));
            }            

            if (mAutoRunWizard != null)
            {
                mAutoRunWizard.AutoRunConfiguration.SelectedCLI = mCLIDynamicFile;
                ShowHelp();
                ResetCLIContent(mResetCLIContent=true);
                ShowContent();
            }
            xConfigFileSettingsPnl.Visibility = Visibility.Visible;
            xDynamicFileTypeCombo.Visibility = Visibility.Collapsed;
            xCLIContentTextBox.AddValidationRule(new ValidateJsonFormat());
        }

        CLIScriptFile mCLIScriptFile;
        private void XScriptRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (mCLIScriptFile == null)
            {
                mCLIScriptFile = new CLIScriptFile();
            }
            if (mAutoRunWizard != null)
            {
                mAutoRunWizard.AutoRunConfiguration.SelectedCLI = mCLIScriptFile;
                ShowHelp();
                ShowContent();
            }

            xConfigFileSettingsPnl.Visibility = Visibility.Visible;
        }

        CLIArgs mCLIArgs;
        private void XParametersRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            xCLIContentTextBox.RemoveValidations(TextBox.TextProperty);

            if (mCLIArgs == null)
            {
                mCLIArgs = new CLIArgs();
            }
            if (mAutoRunWizard != null)
            {
                mAutoRunWizard.AutoRunConfiguration.SelectedCLI = mCLIArgs;
                ShowHelp();
                ResetCLIContent(mResetCLIContent=true);
                ShowContent();
            }

        }

        private void xDynamicFileTypeCombo_DropDownClosed(object sender, EventArgs e)
        {
            ShowContent();
        }

        CLIRequestAPI mCLIRequest;
        private void xRequestRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (mCLIRequest == null)
            {
                mCLIRequest = new CLIRequestAPI();
            }
            if (mAutoRunWizard != null)
            {
                mAutoRunWizard.AutoRunConfiguration.SelectedCLI = mCLIRequest;
                mAutoRunWizard.AutoRunConfiguration.IsRequestAPIExecution = true;
                ShowHelp();
                ResetCLIContent(mResetCLIContent=true);
                ShowContent();
            }
            xConfigFileSettingsPnl.Visibility = Visibility.Collapsed;
            xCLIContentTextBox.AddValidationRule(new ValidateJsonFormat());
        }

        private void xRequestRadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            mAutoRunWizard.AutoRunConfiguration.IsRequestAPIExecution = false;
        }

        private void xCopyBtn_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(xCLIContentTextBox.Text.ToString());
        }


        //CLIExcel mCLIExcel;
        //private void XExcelRadioButton_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (mCLIExcel == null)
        //    {
        //        mCLIExcel = new CLIExcel();
        //    }
        //    mAutoRunWizard.SelectedCLI = mCLIExcel;
        //    mCliText = @"Using excel to create and control a runset with data and store information" + Environment.NewLine;                        
        //    ShowContent();
        //}
    }
}
