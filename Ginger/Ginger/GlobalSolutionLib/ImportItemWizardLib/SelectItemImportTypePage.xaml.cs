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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GlobalSolutionLib;
using Amdocs.Ginger.CoreNET.GlobalSolutionLib;
using Ginger.Actions;
using Ginger.SolutionGeneral;
using Ginger.SourceControl;
using Ginger.UserControls;
using GingerCoreNET.SourceControl;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ginger.GlobalSolutionLib.ImportItemWizardLib
{
    /// <summary>
    /// Interaction logic for SelectItemImportTypePage.xaml
    /// </summary>
    public partial class SelectItemImportTypePage : Page, IWizardPage
    {
        ImportItemWizard wiz;
        SourceControlProjectsPage sourceControlProjectsPage;
        public SelectItemImportTypePage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    wiz = (ImportItemWizard)WizardEventArgs.Wizard;
                    ((WizardWindow)wiz.mWizardWindow).WindowState = WindowState.Maximized;
                    GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xGlobalSolutionFolderTextBox, TextBox.TextProperty, wiz, nameof(ImportItemWizard.SolutionFolder));
                    ((WizardWindow)wiz.mWizardWindow).ShowFinishButton(false);
                    sourceControlProjectsPage = new SourceControlProjectsPage(true);
                    xImportFromSourceControlFrame.Content = sourceControlProjectsPage;
                    sourceControlProjectsPage.Width = 1200; 
                    break;
                case EventType.LeavingForNextPage:

                    if (wiz.ImportFromType == GlobalSolution.eImportFromType.SourceControl)
                    {
                        SolutionInfo solutionInfo = (SolutionInfo) sourceControlProjectsPage.SolutionsGrid.grdMain.SelectedItem;
                        if (solutionInfo != null)
                        {
                            if (solutionInfo.ExistInLocaly)
                            {
                                wiz.SolutionFolder = solutionInfo.LocalFolder;
                            }
                            else 
                            {
                                Reporter.ToUser(eUserMsgKey.AskToSelectSolution);
                                WizardEventArgs.CancelEvent = true;
                                return;
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(wiz.SolutionFolder))
                    {
                        Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Please select Solution Folder.");
                        WizardEventArgs.CancelEvent = true;
                        return;
                    }
                    else if (wiz.SolutionFolder.Contains(WorkSpace.Instance.SolutionRepository.SolutionFolder))
                    {
                        Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Please select different Solution Folder.");
                        WizardEventArgs.CancelEvent = true;
                        return;
                    }
                    else 
                    {
                        if (!wiz.SolutionFolder.EndsWith("\\"))
                        {
                            wiz.SolutionFolder = wiz.SolutionFolder + "\\";
                        }
                    }
                    if (!IsValidSolution(wiz.SolutionFolder))
                    {
                        WizardEventArgs.CancelEvent = true;
                        return;
                    }

                    xGlobalSolutionFolderTextBox.Text = wiz.SolutionFolder;
                    GlobalSolutionUtils.Instance.SolutionFolder = wiz.SolutionFolder;
                    wiz.EncryptionKey = GlobalSolutionUtils.Instance.GetEncryptionKey();

                    break;
                default:
                    //Nothing to do
                    break;
            }
        }

        private void ImportFromLocalFolderTypeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (wiz == null)
            {
                return;
            }
            wiz.ImportFromType = GlobalSolution.eImportFromType.LocalFolder;
            ImportFromLocalFolderPanel.Visibility = Visibility.Visible;
            ImportFromSourceControlPanel.Visibility = Visibility.Hidden;
        }

        private void ImportFromSourceControlTypeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (wiz == null)
            {
                return;
            }
            wiz.ImportFromType = GlobalSolution.eImportFromType.SourceControl;
            ImportFromSourceControlPanel.Visibility = Visibility.Visible;
            ImportFromLocalFolderPanel.Visibility = Visibility.Hidden;
        }

        private void SelectSolutionFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string solutionFolder = General.OpenSelectFolderDialog("Select Ginger Solution Folder");
            if (solutionFolder != null)
            {
                xGlobalSolutionFolderTextBox.Text = solutionFolder;
                IsValidSolution(solutionFolder);
            }
        }

        bool IsValidSolution(string solutionFolderPath)
        {
            GlobalSolutionUtils.Instance.SolutionFolder = solutionFolderPath;
            Solution mSolution = GlobalSolutionUtils.Instance.GetSolution();
            if (mSolution != null)
            {
                if (string.IsNullOrEmpty(mSolution.EncryptedValidationString))
                {
                    Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Solution to import items from was created with lower version which do not support Encryption key. " + Environment.NewLine + "To import solution items, upgrade the solution to latest version.");
                    return false;
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Please select solution root folder.");
                return false;
            }
            return true;
        }
    }
}
