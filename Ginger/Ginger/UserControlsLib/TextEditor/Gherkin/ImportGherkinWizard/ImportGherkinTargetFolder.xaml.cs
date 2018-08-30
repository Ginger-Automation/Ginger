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

namespace Ginger.UserControlsLib.TextEditor.Gherkin
{
    /// <summary>
    /// Interaction logic for ImportGherkinIntroPage.xaml
    /// </summary>
    /// 
    public partial class ImportGherkinTargetFolder : Page, IWizardPage
    {
        eImportGherkinFileContext mContext;
        SingleItemTreeViewSelectionPage mBusFlowsSelectionPage = null;
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
                    ImportGherkinFeatureWizard wiz = (ImportGherkinFeatureWizard)WizardEventArgs.Wizard;
                    if (mContext == eImportGherkinFileContext.DocumentsFolder)
                    {
                        BusinessFlowsFolderTreeItem bfsFolder;
                        if (WorkSpace.Instance.BetaFeatures.BFUseSolutionRepositry)
                        {
                            bfsFolder = new BusinessFlowsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<BusinessFlow>());
                        }
                        else
                        {
                            bfsFolder = new BusinessFlowsFolderTreeItem();//create new tree each time for now to allow refresh
                        }
                        bfsFolder.Folder = GingerDicser.GetTermResValue(eTermResKey.BusinessFlows);
                        bfsFolder.Path = App.UserProfile.Solution.BusinessFlowsMainFolder;
                        bfsFolder.IsGingerDefualtFolder = true;
                        
                        mBusFlowsSelectionPage = new SingleItemTreeViewSelectionPage(GingerDicser.GetTermResValue(eTermResKey.BusinessFlows), eImageType.BusinessFlow, bfsFolder, SingleItemTreeViewSelectionPage.eItemSelectionType.Folder,false);
                        mBusFlowsSelectionPage.SelectionDone += MBusFlowsSelectionPage_SelectionDone;

                        TargetPath.Content = mBusFlowsSelectionPage;
                        //List<object> selectedBfs = mBusFlowsSelectionPage.ShowAsWindow();
                        //AddSelectedBuinessFlows(selectedBfs);
                    }
                    break;
            }
        }
        private void MBusFlowsSelectionPage_SelectionDone(object sender, SelectionTreeEventArgs e)
        {
            //AddSelectedBuinessFlows(e.SelectedItems);
        }
    }
}
