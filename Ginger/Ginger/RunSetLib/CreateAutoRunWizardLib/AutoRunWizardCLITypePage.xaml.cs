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
        AutoRunWizard mAutoRunWizard;

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
                    xConfigRadioButton.IsChecked = true;                                    
                    break;

                case EventType.Active:
                    ShowHelp();
                    ShowContent();
                    break;
            }
        }
        
        private void ShowContent()
        {
            xCLIContentTextBox.Text = mAutoRunWizard.AutoRunConfiguration.CLIContent;                    
        }

        private void ShowHelp()
        {
            string helpContent = string.Empty;

            if (xConfigRadioButton.IsChecked == true)
            {
                helpContent = string.Format("Simple text file which contain the execution configurations." + GetRowDown() + "To be used in case {0} already exist in the Solution and only need to trigger it." + GetRowDown() + "Executed by triggering Ginger executer with the argument 'ConfigFile=%ConfigFilePath%', Example: Ginger.exe ConfigFile=\"C:\\Ginger\\Regression1.Ginger.AutoRunConfigs.Config\"", GingerDicser.GetTermResValue(eTermResKey.RunSet));
            }
            else if (xDynamicRadioButton.IsChecked == true)
            {
                helpContent = string.Format("File (XML/JSON) which describes the {0} to be executed." + GetRowDown() + "To be used in case {0} not exist in the Solution and should be created dynamically for execution purposes only, or in case {0} is exist but need to dynamically customized it input values for specific execution." + GetRowDown() + "Executed by triggering Ginger executer with the argument 'Dynamic -f %FilePath%', Example: Ginger.exe Dynamic -f \"C:\\Ginger\\FeatureATesting.Ginger.AutoRunConfigs.xml\"", GingerDicser.GetTermResValue(eTermResKey.RunSet));
            }
            else if (xScriptRadioButton.IsChecked == true)
            {
                helpContent = string.Format("Script file written in C# which implement Ginger execution flow." + GetRowDown() + "Enable to create {0} with loops and much more complex execution." + GetRowDown() + "Executed by triggering Ginger executer with the argument 'Script=%ScriptFilePath%', Example: Ginger.exe Script=\"C:\\Ginger\\FeatureBTesting.Ginger.AutoRunConfigs.script\"", GingerDicser.GetTermResValue(eTermResKey.RunSet));
            }
            else if (xParametersRadioButton.IsChecked == true)
            {
                helpContent = string.Format("Command line arguments only without any file for triggering existing {0} execution" + GetRowDown() + "Executed by triggering Ginger executer with the switchers, Example: Ginger.exe --args --solution \"c:\\ginger\\solutions\\sol1\" --runset Regression", GingerDicser.GetTermResValue(eTermResKey.RunSet));
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
                mAutoRunWizard.AutoRunConfiguration.SelectedCLI = mCLIConfigFile;
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
                ShowContent();
            }

            xConfigFileSettingsPnl.Visibility = Visibility.Visible;
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
            if (mCLIArgs == null)
            {
                mCLIArgs = new CLIArgs();
            }
            if (mAutoRunWizard != null)
            {
                mAutoRunWizard.AutoRunConfiguration.SelectedCLI = mCLIArgs;
                ShowHelp();
                ShowContent();
            }

            xConfigFileSettingsPnl.Visibility = Visibility.Collapsed;
        }

        private void xDynamicFileTypeCombo_DropDownClosed(object sender, EventArgs e)
        {
            ShowContent();
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
