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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Telemetry;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Repository;
using Ginger.Actions.ActionConversion;
using Ginger.ALM;
using Ginger.BusinessFlowWindows;
using Ginger.GherkinLib;
using Ginger.Import;
using Ginger.Imports.QTP;
using Ginger.Imports.UFT;
using Ginger.UserControlsLib.TextEditor.Gherkin;
using GingerCore;
using GingerCore.Platforms;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static GingerCoreNET.ALMLib.ALMIntegrationEnums;

namespace Ginger.SolutionWindows.TreeViewItems
{
    public enum eBusinessFlowsTreeViewMode
    {
        ReadWrite = 0,
        ReadOnly = 1
    }

    public class BusinessFlowsFolderTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        RepositoryFolder<BusinessFlow> mBusFlowsFolder;
        private ExplorerBusinessFlowsPage mExplorerBusinessFlowsPage;
        public eBusinessFlowsTreeViewMode mViewMode;

        public BusinessFlowsFolderTreeItem(RepositoryFolder<BusinessFlow> repositoryFolder, eBusinessFlowsTreeViewMode viewMode = eBusinessFlowsTreeViewMode.ReadWrite)
        {
            mBusFlowsFolder = repositoryFolder;
            mViewMode = viewMode;
        }

        Object ITreeViewItem.NodeObject()
        {
            return mBusFlowsFolder;
        }
        override public string NodePath()
        {
            return mBusFlowsFolder.FolderFullPath;
        }
        override public Type NodeObjectType()
        {
            return typeof(BusinessFlow);
        }

        StackPanel ITreeViewItem.Header()
        {
            return NewTVItemFolderHeaderStyle(mBusFlowsFolder);
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return GetChildrentGeneric<BusinessFlow>(mBusFlowsFolder);
        }

        public override ITreeViewItem GetTreeItem(object item)
        {
            if (item is BusinessFlow)
            {
                return new BusinessFlowTreeItem((BusinessFlow)item);
            }
            else if (item is RepositoryFolderBase)
            {
                return new BusinessFlowsFolderTreeItem((RepositoryFolder<BusinessFlow>)item);
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error unknown item added to " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows) + " folder");
                throw new NotImplementedException();
            }
        }

        internal void AddItemHandler(object sender, RoutedEventArgs e)
        {
            AddTreeItem();
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            if (mExplorerBusinessFlowsPage == null)
            {
                mExplorerBusinessFlowsPage = new ExplorerBusinessFlowsPage(mBusFlowsFolder);
            }
            return mExplorerBusinessFlowsPage;
        }


        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            if (mTreeView.Tree.TreeChildFolderOnly == true)
            {
                AddFolderNodeBasicManipulationsOptions(mContextMenu, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), true, false, false, false, false, false, false, true, false, false, true);
            }
            else if (mViewMode == eBusinessFlowsTreeViewMode.ReadWrite)
            {
                if (mBusFlowsFolder.IsRootFolder)
                {
                    AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), allowRenameFolder: false, allowDeleteFolder: false, allowRefresh: false, allowDeleteAllItems: true,allowAttributeUpdate:true);
                }
                else
                {
                    AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), allowRefresh: false, allowAttributeUpdate: true);
                }

                MenuItem actConversionMenu = TreeViewUtils.CreateSubMenu(mContextMenu, "Conversion", eImageType.Convert);
                TreeViewUtils.AddSubMenuItem(actConversionMenu, "Legacy Actions Conversion", ActionsConversionHandler, null, eImageType.Convert);
                TreeViewUtils.AddSubMenuItem(actConversionMenu, "Clean Inactive Legacy Actions", LegacyActionsRemoveHandler, null, eImageType.Delete);

                AddSourceControlOptions(mContextMenu, false, false);

                MenuItem importMenu = TreeViewUtils.CreateSubMenu(mContextMenu, "Import");
                TreeViewUtils.AddSubMenuItem(importMenu, "Import External" + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), ImportExternalBuinessFlow, null, eImageType.ImportFile);
                TreeViewUtils.AddSubMenuItem(importMenu, "Import ALM Test Set", ALMTSImport, null, "@ALM_16x16.png");
                TreeViewUtils.AddSubMenuItem(importMenu, "Import ALM Test Set By ID", ALMTSImportById, null, "@ALM_16x16.png");
                TreeViewUtils.AddSubMenuItem(importMenu, "Import Gherkin Feature File", ImportGherkinFeature, null, "@FeatureFile_16X16.png");
                MenuItem exportMenu = TreeViewUtils.CreateSubMenu(mContextMenu, "Export");
                TreeViewUtils.AddSubMenuItem(exportMenu, "Export All to ALM", ExportAllToALM, null, "@ALM_16x16.png");
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(importMenu, Expander.VisibilityProperty, WorkSpace.Instance.UserProfile, nameof(WorkSpace.Instance.UserProfile.ShowEnterpriseFeatures), bindingConvertor: new GingerCore.GeneralLib.BoolVisibilityConverter());
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(exportMenu, Expander.VisibilityProperty, WorkSpace.Instance.UserProfile, nameof(WorkSpace.Instance.UserProfile.ShowEnterpriseFeatures), bindingConvertor: new GingerCore.GeneralLib.BoolVisibilityConverter());
            }
            else
            {
                AddFolderNodeBasicManipulationsOptions(mContextMenu, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), false, false, false, false, false, false, false, false, false, false, true);
            }
        }

        private void ActionsConversionHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            ObservableList<BusinessFlow> lst = [];
            var items = ((Amdocs.Ginger.Repository.RepositoryFolder<GingerCore.BusinessFlow>)((ITreeViewItem)this).NodeObject()).GetFolderItemsRecursive();
            foreach (var bf in items)
            {
                lst.Add(bf);
            }

            WizardWindow.ShowWizard(new ActionsConversionWizard(ActionsConversionWizard.eActionConversionType.MultipleBusinessFlow, new Context(), lst), 900, 700, true);
        }

        /// <summary>
        /// This method helps to execute the funcationality of removeing the legacy actions from the businessflow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void LegacyActionsRemoveHandler(object sender, RoutedEventArgs e)
        {
            ObservableList<BusinessFlowToConvert> lstBFToConvert = [];
            var items = ((RepositoryFolder<BusinessFlow>)((ITreeViewItem)this).NodeObject()).GetFolderItemsRecursive();
            foreach (var bf in items)
            {
                BusinessFlowToConvert flowToConvert = new BusinessFlowToConvert
                {
                    BusinessFlow = bf
                };
                lstBFToConvert.Add(flowToConvert);
            }
            ActionConversionUtils utils = new ActionConversionUtils();

            await Task.Run(() => utils.RemoveLegacyActionsHandler(lstBFToConvert));
        }

        private void ImportSeleniumScript(object sender, System.Windows.RoutedEventArgs e)
        {
            if (General.SetupBrowseFile(new System.Windows.Forms.OpenFileDialog()
            {
                DefaultExt = ".html",
                Filter = "Recorded Selenium Scripts (*.html)|*.html"
            }, false) is string fileName)
            {
                BusinessFlow BF = SeleniumToGinger.ConvertSeleniumScript(fileName);
                mBusFlowsFolder.AddRepositoryItem(BF);
            }
        }

        private void ALMTSImport(object sender, System.Windows.RoutedEventArgs e)
        {
            Reporter.AddFeatureUsage(FeatureId.ALM, new TelemetryMetadata()
            {
                { "Type", ALMIntegration.Instance.GetALMType().ToString() },
                { "Operation", "ImportBusinessFlow" },
            });
            ALMIntegration.Instance.ImportALMTests(mBusFlowsFolder.FolderFullPath);
        }

        private void ALMTSImportById(object sender, System.Windows.RoutedEventArgs e)
        {
            Reporter.AddFeatureUsage(FeatureId.ALM, new TelemetryMetadata()
            {
                { "Type", ALMIntegration.Instance.GetALMType().ToString() },
                { "Operation", "ImportBusinessFlowById" },
            });
            ALMIntegration.Instance.ImportALMTestsById(mBusFlowsFolder.FolderFullPath);
        }

        private void ImportASAPScript(object sender, System.Windows.RoutedEventArgs e)
        {
            UFTImportPage UFTIP = new UFTImportPage();
            UFTIP.ShowAsWindow();
        }

        private void ImportAQTPScript(object sender, RoutedEventArgs e)
        {
            QTPImportPage QTPIP = new QTPImportPage();
            QTPIP.ShowAsWindow();
        }

        private void ImportExternalBuinessFlow(object sender, System.Windows.RoutedEventArgs e)
        {
            BusinessFlow importedBF = null;
            //open dialog for selecting the BF file
            if (General.SetupBrowseFile(new System.Windows.Forms.OpenFileDialog()
            {
                DefaultExt = ".Ginger.BusinessFlow.xml",
                Filter = "Ginger Business Flow File|*.Ginger.BusinessFlow.xml"
            }, false) is string fileName)
            {
                try
                {
                    //copy to Solution Business Flow folder 
                    string importedBFpath = System.IO.Path.Combine(mBusFlowsFolder.FolderFullPath, System.IO.Path.GetFileName(fileName));
                    File.Copy(fileName, importedBFpath, false);

                    //load it to object
                    importedBF = (BusinessFlow)RepositoryItem.LoadFromFile(typeof(BusinessFlow), importedBFpath);

                    //customize the imported BF
                    importedBF.Guid = Guid.NewGuid();
                    for (int i = 0; i < importedBF.TargetApplications.Count; i++)
                    {
                        if (WorkSpace.Instance.Solution.ApplicationPlatforms.FirstOrDefault(x => x.AppName == importedBF.TargetApplications[i].Name) == null)
                        {
                            importedBF.TargetApplications.RemoveAt(i);//No such Application so Delete it
                            i--;
                        }
                    }
                    if (importedBF.TargetApplications.Count == 0)
                    {
                        TargetApplication ta = new TargetApplication
                        {
                            AppName = WorkSpace.Instance.Solution.ApplicationPlatforms[0].AppName
                        };
                        importedBF.TargetApplications.Add(ta);
                    }

                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(importedBF);
                    mBusFlowsFolder.AddRepositoryItem(importedBF);
                }
                catch (Exception ex)
                {
                    Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Failed to copy and load the selected " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " file." + System.Environment.NewLine + "Error: " + System.Environment.NewLine + ex.Message);
                    return;
                }
            }
        }

        public override void AddTreeItem()
        {
            //TODO: change to wizard
            string BizFlowName = string.Empty;
            if (WorkSpace.Instance.Solution.ApplicationPlatforms == null || WorkSpace.Instance.Solution.ApplicationPlatforms.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.MissingTargetApplication, $"The default Application Platform Info is missing, please go to Solution level to add at least one {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)}");
                return;
            }

            BusinessFlow BizFlow = WorkSpace.Instance.GetNewBusinessFlow(BizFlowName);

            if (GingerCore.General.GetInputWithValidation("Add " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Name:", ref BizFlowName, null, false, BizFlow))
            {
                BizFlow = WorkSpace.Instance.GetNewBusinessFlow(BizFlowName);
                if (WorkSpace.Instance.Solution.ApplicationPlatforms.Count != 1)
                {
                    EditBusinessFlowAppsPage EBFP = new EditBusinessFlowAppsPage(BizFlow, true);
                    EBFP.ResetPlatformSelection();
                    EBFP.Title = $"Select {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} for Default {GingerDicser.GetTermResValue(eTermResKey.Activity)}";
                    EBFP.ShowAsWindow(eWindowShowStyle.Dialog, false);
                }
                else
                {
                    BizFlow.TargetApplications.Add(new TargetApplication() { AppName = WorkSpace.Instance.Solution.MainApplication });
                    BizFlow.CurrentActivity.TargetApplication = BizFlow.TargetApplications[0].Name;
                }

                mBusFlowsFolder.AddRepositoryItem(BizFlow);
            }
        }

        private void ImportGherkinFeature(object sender, RoutedEventArgs e)
        {
            ImportGherkinFeatureWizard mWizard = new ImportGherkinFeatureWizard(this, ImportGherkinFeatureFilePage.eImportGherkinFileContext.BusinessFlowFolder);
            WizardWindow.ShowWizard(mWizard);

            if (mWizard.BizFlow != null)
            {
                //Select Business Folder
                mTreeView.Tree.SelectItem(this);
                mTreeView.Tree.RefreshSelectedTreeNodeChildrens();
                mTreeView.Tree.GetChildItembyNameandSelect(mWizard.BizFlow.Name, this);
            }
        }

        private void ExportAllToALM(object sender, System.Windows.RoutedEventArgs e)
        {
            ObservableList<BusinessFlow> bfToExport = mBusFlowsFolder.GetFolderItems();
            if (bfToExport.Count > 0)
            {
                if (bfToExport.Count == 1)
                {
                    _ = bfToExport[0].Activities;//Loading Activity for Export to ALM
                    bool wasSuccessful = ALMIntegration.Instance.ExportBusinessFlowToALM(bfToExport[0], true);
                    if (wasSuccessful)
                    {
                        Reporter.AddFeatureUsage(FeatureId.ALM, new TelemetryMetadata()
                        {
                            { "Type", ALMIntegration.Instance.GetALMType().ToString() },
                            { "Operation", "ExportBusinessFlow" },
                        });
                    }
                }
                else
                {
                    if (ALMIntegration.Instance.ExportAllBusinessFlowsToALM(bfToExport, true, eALMConnectType.Auto))
                    {
                        foreach (var _ in bfToExport)
                        {
                            Reporter.AddFeatureUsage(FeatureId.ALM, new TelemetryMetadata()
                            {
                                { "Type", ALMIntegration.Instance.GetALMType().ToString() },
                                { "Operation", "ExportBusinessFlow" },
                            });
                        }
                        Reporter.ToUser(eUserMsgKey.ExportAllItemsToALMSucceed);
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKey.ExportAllItemsToALMFailed);
                    }
                }
            }
        }
    }
}