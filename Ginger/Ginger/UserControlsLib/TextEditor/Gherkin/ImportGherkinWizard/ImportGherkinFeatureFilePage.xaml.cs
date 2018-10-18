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

using Amdocs.Ginger.Common;
using Ginger.UserControlsLib.TextEditor.Gherkin;
using GingerCore;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System;
using Ginger.Repository;
using Ginger.SolutionWindows;
using GingerWPF.UserControlsLib.UCTreeView;
using Ginger.SolutionWindows.TreeViewItems;
using GingerWPF.WizardLib;
using amdocs.ginger.GingerCoreNET;
using static Ginger.ExtensionMethods;

namespace Ginger.GherkinLib
{
    /// <summary>
    /// Interaction logic for ImportFeaturePage.xaml
    /// </summary>
    public partial class ImportGherkinFeatureFilePage : Page , IWizardPage
    {
        
        ImportGherkinFeatureWizard mWizard;
        
        internal string mFeatureFile { get { return mWizard.mFeatureFile; } }

        public object GherkinTextEditor { get; private set; }
        public BusinessFlow BizFlow { get { return mWizard.BizFlow; } }

        public bool Imported { get { return mWizard.Imported; } }

        public enum eImportGherkinFileContext
        {
            BusinessFlowFolder,
            DocumentsFolder

        }
        GenericWindow genWin;

        public ImportGherkinFeatureFilePage()
        {
            InitializeComponent();            
            FetaureFileName.FileExtensions.Add(".feature");            
            FileContentViewer.Visibility = System.Windows.Visibility.Collapsed;
            FetaureFileName.FilePathTextBox.TextChanged += FilePathTextBox_TextChanged;

            FileContentViewer.SetContentEditorTitleLabel("Selected Gherkin Feature File Preview");
            FileContentViewer.ToolBarRow.Height = new GridLength(0);
            FileContentViewer.ToolBarTray.Visibility = Visibility.Collapsed;
            FileContentViewer.lblTitle.Content = "Text";
            FileContentViewer.toolbar.Visibility = Visibility.Collapsed;
        }       

        private void FilePathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(FetaureFileName.FilePathTextBox.Text))
                return;
            if (Path.GetExtension(FetaureFileName.FilePathTextBox.Text).ToUpper() != ".FEATURE")
            {
                Reporter.ToUser(eUserMsgKeys.GherkinFeatureFileImportOnlyFeatureFileAllowedErrorMessage);
                FetaureFileName.FilePathTextBox.Text = string.Empty;                
                return;
            }
            else
            {
                FileContentViewer.Visibility = System.Windows.Visibility.Visible;
                FileContentViewer.Init(FetaureFileName.FilePathTextBox.Text, new GherkinDcoumentEditor(), false);
            }
            mWizard.mFeatureFile = FetaureFileName.FilePathTextBox.Text;
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            ObservableList<Button> winButtons = new ObservableList<Button>();

            Button ImportButton = new Button();
            ImportButton.Content = "Import";
            ImportButton.Click += new RoutedEventHandler(ImportButton_Click);                    
            winButtons.Add(ImportButton);

            genWin = null;
            this.Height = 400;
            this.Width = 400;
            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, "Import Feature File", this, winButtons);           
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            //mFeatureFile = Import();
             mWizard.Imported = true;
            if (!string.IsNullOrEmpty(mWizard.mFeatureFile) && mWizard.mContext == eImportGherkinFileContext.BusinessFlowFolder)
            {
                GherkinPage GP = new GherkinPage();                
                bool Compiled = GP.Load(mWizard.mFeatureFile);
                //GP.Optimize();
                if (Compiled)
                {
                    string BFName = System.IO.Path.GetFileName(mWizard.mFeatureFile).Replace(".feature", "");
                    GP.CreateNewBF(BFName, mWizard.mFeatureFile);
                    GP.CreateActivities();                    
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(GP.mBizFlow);
                    mWizard.BizFlow = GP.mBizFlow;
                }
                else
                {
                    Reporter.ToUser(eUserMsgKeys.GherkinBusinessFlowNotCreated);
                }
            }
            if (genWin != null)
            {
                genWin.Close();
            }
        }
          
        public void WizardEvent(WizardEventArgs WizardArgs)
        {
            switch (WizardArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (ImportGherkinFeatureWizard)WizardArgs.Wizard;
                    //TO DO::Feature File Cannot BE NULL
                    break;
                case EventType.Validate:
                    if (!File.Exists(FetaureFileName.FilePathTextBox.Text))
                    {
                        // WizardArgs.AddError("File not found - " + FetaureFileName.FilePathTextBox.Text);
                    }
                    break;
                
            }
        }
    }
}
