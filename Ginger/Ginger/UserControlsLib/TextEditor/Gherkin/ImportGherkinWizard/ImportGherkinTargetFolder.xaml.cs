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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.Enums;
using Ginger.SolutionWindows.TreeViewItems;
using GingerCore;
using GingerCore.GeneralLib;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using System.IO;
using System.Windows.Controls;
using static Ginger.GherkinLib.ImportGherkinFeatureFilePage;

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
                        BusinessFlowsFolderTreeItem bfsFolder = new BusinessFlowsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<BusinessFlow>(), eBusinessFlowsTreeViewMode.ReadOnly);

                        mTargetFolderSelectionPage = new SingleItemTreeViewSelectionPage(GingerDicser.GetTermResValue(eTermResKey.BusinessFlows), eImageType.BusinessFlow, bfsFolder, SingleItemTreeViewSelectionPage.eItemSelectionType.Folder, true);
                    }
                    else if (mContext == eImportGherkinFileContext.BusinessFlowFolder)
                    {
                        DocumentsFolderTreeItem documentsFolderRoot = new DocumentsFolderTreeItem
                        {
                            IsGingerDefualtFolder = true,
                            Path = Path.Combine(WorkSpace.Instance.SolutionRepository.SolutionFolder, "Documents"),
                            Folder = "Documents"
                        };
                        mTargetFolderSelectionPage = new SingleItemTreeViewSelectionPage("Documents", eImageType.File, documentsFolderRoot, SingleItemTreeViewSelectionPage.eItemSelectionType.Folder, true);
                    }
                    mTargetFolderSelectionPage.xTreeView.xTreeViewTree.ValidationRules.Add(UCTreeView.eUcTreeValidationRules.NoItemSelected);

                    mTargetFolderSelectionPage.OnSelect += MTargetFolderSelectionPage_OnSelectItem;

                    TargetPath.ClearAndSetContent(mTargetFolderSelectionPage);
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
            }

        }
        private void MTargetFolderSelectionPage_OnSelectItem(object sender, SelectionTreeEventArgs e)
        {
            mTargetFolder = e.SelectedItems[0];
        }
    }
}
