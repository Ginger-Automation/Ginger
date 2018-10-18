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

using GingerWPF.WizardLib;
using System.Windows.Controls;
using Ginger.SolutionWindows.TreeViewItems;
using amdocs.ginger.GingerCoreNET;
using static Ginger.GherkinLib.ImportGherkinFeatureFilePage;
using GingerCore;
using GingerWPF.UserControlsLib.UCTreeView;
using Amdocs.Ginger.Common.Enums;
using System.Collections.Generic;
using System.IO;
using Ginger.GherkinLib;

namespace Ginger.UserControlsLib.TextEditor.Gherkin
{
    /// <summary>
    /// Interaction logic for ImportGherkinIntroPage.xaml
    /// </summary>
    /// 
    public partial class ImportGherkinTargetFolder : Page, IWizardPage
    {
        public object mTargetFolder { get; set; }
        eImportGherkinFileContext mContext;
        SingleItemTreeViewSelectionPage mTargetFolderSelectionPage;
        ImportGherkinFeatureWizard wiz;
        public ImportGherkinTargetFolder(eImportGherkinFileContext context)
        {
            InitializeComponent();
            mContext = context;
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    wiz = (ImportGherkinFeatureWizard)WizardEventArgs.Wizard;
                    if (mContext == eImportGherkinFileContext.DocumentsFolder)
                    {
                        BusinessFlowsFolderTreeItem bfsFolder = new BusinessFlowsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<BusinessFlow>(),eBusinessFlowsTreeViewMode.ReadOnly);
                        
                        mTargetFolderSelectionPage = new SingleItemTreeViewSelectionPage(GingerDicser.GetTermResValue(eTermResKey.BusinessFlows), eImageType.BusinessFlow, bfsFolder, SingleItemTreeViewSelectionPage.eItemSelectionType.Folder, true);
                    }
                    else if(mContext == eImportGherkinFileContext.BusinessFlowFolder)
                    {
                        DocumentsFolderTreeItem documentsFolderRoot = new DocumentsFolderTreeItem();
                        documentsFolderRoot.IsGingerDefualtFolder = true;
                        documentsFolderRoot.Path = Path.Combine(WorkSpace.Instance.SolutionRepository.SolutionFolder, "Documents");
                        documentsFolderRoot.Folder = "Documents";
                        mTargetFolderSelectionPage = new SingleItemTreeViewSelectionPage("Documents", eImageType.File, documentsFolderRoot, SingleItemTreeViewSelectionPage.eItemSelectionType.Folder, true);                        

                    }
                    mTargetFolderSelectionPage.OnSelect += MTargetFolderSelectionPage_OnSelectItem;

                    TargetPath.Content = mTargetFolderSelectionPage;
                    break;
                case EventType.LeavingForNextPage:
                    if (mContext == eImportGherkinFileContext.BusinessFlowFolder)
                    { 
                        wiz.featureTargetFolder = (ITreeViewItem)mTargetFolder;
                    }
                    else
                    { 
                        wiz.bizFlowTargetFolder = (ITreeViewItem)mTargetFolder;
                    }
                    break;
                case EventType.Validate:                    
                    if (mTargetFolder == null)
                    {
                        //TODO  : Select target Fodler error
                    }
                    break;                                    
            }

        }
        private void MTargetFolderSelectionPage_OnSelectItem(object sender, SelectionTreeEventArgs e)
        {
            mTargetFolder = e.SelectedItems[0];           
        }        
    }
}
