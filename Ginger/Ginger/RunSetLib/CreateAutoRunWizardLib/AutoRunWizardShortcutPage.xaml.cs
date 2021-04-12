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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
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
                    mAutoRunWizard.AutoRunShortcut.CreateShortcut = true;
                    mAutoRunWizard.AutoRunShortcut.ShortcutFileName = WorkSpace.Instance.Solution.Name + "-" + mAutoRunWizard.RunsetConfig.Name + " Execution";
                    xExecuterPathTextbox.Init(mAutoRunWizard.mContext, mAutoRunWizard.AutoRunShortcut, nameof(RunSetAutoRunShortcut.ExecuterFolderPath), isVENeeded: false, isBrowseNeeded: true, browserType: Actions.UCValueExpression.eBrowserType.Folder);
                    xShortcutPathTextbox.Init(mAutoRunWizard.mContext, mAutoRunWizard.AutoRunShortcut, nameof(RunSetAutoRunShortcut.ShortcutFolderPath), isVENeeded: false, isBrowseNeeded: true, browserType: Actions.UCValueExpression.eBrowserType.Folder);
                    BindingHandler.ObjFieldBinding(xShortcutDescriptionTextBox, System.Windows.Controls.TextBox.TextProperty, mAutoRunWizard.AutoRunShortcut, nameof(RunSetAutoRunShortcut.ShortcutFileName));                                       
                    xGingerEXERadioButton.IsChecked = true;
                    xDesktopRadioButton.IsChecked = true;
                    break;

                case EventType.Active:
                    BindingHandler.ObjFieldBinding(xShortcutContentTextBox, System.Windows.Controls.TextBox.TextProperty, mAutoRunWizard.AutoRunShortcut, nameof(RunSetAutoRunShortcut.ShortcutContent), BindingMode: System.Windows.Data.BindingMode.OneWay);
                    break;
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

        private void XGingerEXERadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (mAutoRunWizard != null)
            {
                mAutoRunWizard.AutoRunShortcut.ExecutorType = RunSetAutoRunShortcut.eExecutorType.GingerExe;
            }
        }

        private void XGingerConsoleRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (mAutoRunWizard != null)
            {
                mAutoRunWizard.AutoRunShortcut.ExecutorType = RunSetAutoRunShortcut.eExecutorType.GingerConsole;
                mAutoRunWizard.CliHelper.ShowAutoRunWindow = false;
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
    }
}
