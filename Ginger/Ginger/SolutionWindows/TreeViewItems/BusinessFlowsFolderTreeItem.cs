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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger.ALM;
using Ginger.ALM.QC;
using Ginger.BusinessFlowWindows;
using Ginger.GherkinLib;
using Ginger.Import;
using Ginger.Imports.QTP;
using Ginger.Imports.UFT;
using Ginger.Repository;
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
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    public enum eBusinessFlowsTreeViewMode
    {
        ReadWrite = 0,
        ReadOnly = 1
    }

    public class BusinessFlowsFolderTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        private ExplorerBusinessFlowsPage mExplorerBusinessFlowsPage;
        RepositoryFolder<BusinessFlow> mRepositoryFolder;
        public eBusinessFlowsTreeViewMode mViewMode;


        public BusinessFlowsFolderTreeItem(RepositoryFolder<BusinessFlow> repositoryFolder,  eBusinessFlowsTreeViewMode viewMode = eBusinessFlowsTreeViewMode.ReadWrite)
        {
            mRepositoryFolder = repositoryFolder;
            mViewMode = viewMode;
        }

        Object ITreeViewItem.NodeObject()
        {
            return null;
        }
        override public string NodePath()
        {
            return mRepositoryFolder.FolderFullPath + @"\";
        }
        override public Type NodeObjectType()
        {
            return typeof(BusinessFlow);
        }

        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.NewRepositoryItemTreeHeader(mRepositoryFolder, nameof(RepositoryFolder<BusinessFlow>.DisplayName), eImageType.Folder, GetSourceControlImage(mRepositoryFolder), false);            
            
            //if (IsGingerDefualtFolder)
            //{
                
            //    return TreeViewUtils.CreateItemHeader(mRepositoryFolder.FolderName, "@WorkFlow_16x16.png", Ginger.SourceControl.SourceControlIntegration.GetItemSourceControlImage(Path, ref ItemSourceControlStatus));
                
            //}
            //else
            //{
                
            //    return TreeViewUtils.CreateItemHeader(mRepositoryFolder.FolderName, "@Folder2_16x16.png", Ginger.SourceControl.SourceControlIntegration.GetItemSourceControlImage(Path, ref ItemSourceControlStatus));                
            //}
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return GetChildrentGeneric<BusinessFlow>(mRepositoryFolder, nameof(BusinessFlow.Name));

            // FIXME Recently used

            //List<ITreeViewItem> Childrens = new List<ITreeViewItem>();
            //ObservableList<BusinessFlow> BFs;

            //if (Folder == "Recently Used")
            //{
            //    // Since Recently used is per user we keep temp file in the solution folder using MRU class
            //    BFs = new ObservableList<BusinessFlow>();

            //    string[] BizFlowsFile = App.UserProfile.Solution.RecentlyUsedBusinessFlows.getList();

            //    foreach (string BFfilename in BizFlowsFile)
            //    {
            //        // DO NOT load from file - need to search or get from db repo... so it will all be in sync wherever ths BF is used
            //        //BusinessFlow BF = App.LocalRepository.GetBusinessFlow(BFfilename);
            //        BusinessFlow BF = App.LocalRepository.GetItemByFileName<BusinessFlow>(typeof(BusinessFlow), BFfilename);
            //        if (BF != null && BFs.Contains(BF) == false)
            //            BFs.Add(BF);
            //    }
            //}
            //else
            //{                
            //    BFs = mRepositoryFolder.GetFolderItems();
            //    AddsubFolders(mRepositoryFolder, Childrens);                                                    
            //}

            ////Add Business Flows to tree children             
            //foreach (BusinessFlow BF in BFs)
            //{
            //    BusinessFlowTreeItem BFTI = new BusinessFlowTreeItem(mViewMode);
            //    BFTI.BusinessFlow = BF;
            //    Childrens.Add(BFTI);
            //}

            //return Childrens;
        }

        // TODO: remove after we move to RepositoryFolder
        //private void AddsubFolders(string sDir, List<ITreeViewItem> Childrens)
        //{
        //    try
        //    {
        //        foreach (string d in Directory.GetDirectories(Path))
        //        {
        //            BusinessFlowsFolderTreeItem BFFTI = new BusinessFlowsFolderTreeItem(mViewMode);
        //            string FolderName = System.IO.Path.GetFileName(d);

        //            BFFTI.Folder = FolderName;
        //            BFFTI.Path = d;

        //            Childrens.Add(BFFTI);
        //        }

        //    }
        //    catch (System.Exception excpt)
        //    {
        //        Console.WriteLine(excpt.Message);
        //    }
        //}

        //private void AddsubFolders(RepositoryFolder<BusinessFlow> mRepositoryFolder, List<ITreeViewItem> Childrens)
        //{
        //    try
        //    {
        //        foreach (RepositoryFolder<BusinessFlow> f in mRepositoryFolder.GetSubFolders())
        //        {
        //            BusinessFlowsFolderTreeItem BFFTI = new BusinessFlowsFolderTreeItem(f, mViewMode);                   
        //            Childrens.Add(BFFTI);
        //        }

        //    }
        //    catch (System.Exception excpt)
        //    {
        //        Console.WriteLine(excpt.Message);
        //    }
        //}

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage()
        {
            if (mExplorerBusinessFlowsPage == null)
            {
                mExplorerBusinessFlowsPage = new ExplorerBusinessFlowsPage(mRepositoryFolder);
            }
            return mExplorerBusinessFlowsPage;
        }

        private void UploadBusinessFlows(object sender, System.Windows.RoutedEventArgs e)
        {
            ObservableList<BusinessFlow> BFs = mRepositoryFolder.GetFolderItems();
            UploadBusinessFlowsPage UBFP = new UploadBusinessFlowsPage(BFs);
            UBFP.ShowAsWindow();
        }

        private void ImportSeleniumScript(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.DefaultExt = ".html";
            dlg.Filter = "Recorded Selenium Scripts (*.html)|*.html";
            
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                BusinessFlow BF = SeleniumToGinger.ConvertSeleniumScript(dlg.FileName);
                //string BFFileName = LocalRepository.GetRepoItemFileName(BF, Path);

                //BusinessFlowTreeItem BFTI = new BusinessFlowTreeItem();
                //BFTI.BusinessFlow = BF;

                //mTreeView.Tree.AddChildItemAndSelect(this, BFTI);

                //BF.SaveToFile(BFFileName);
                mRepositoryFolder.AddRepositoryItem(BF);
            }
        }

        private void ALMTSImport(object sender, System.Windows.RoutedEventArgs e)
        {
            ALMIntegration.Instance.ImportALMTests(mRepositoryFolder.FolderFullPath);
        }
        private void ALMTSImportById(object sender, System.Windows.RoutedEventArgs e)
        {
            ALMIntegration.Instance.ImportALMTestsById(mRepositoryFolder.FolderFullPath);
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
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.DefaultExt = ".Ginger.BusinessFlow.xml";
            dlg.Filter = "Ginger Business Flow File|*.Ginger.BusinessFlow.xml";           
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    //copy to Solution Business Flow folder 
                    string importedBFpath = System.IO.Path.Combine(mRepositoryFolder.FolderFullPath, System.IO.Path.GetFileName(dlg.FileName));
                    File.Copy(dlg.FileName, importedBFpath, false);

                    //load it to object
                    importedBF = (BusinessFlow)RepositoryItem.LoadFromFile(typeof(BusinessFlow), importedBFpath);

                    //customize the imported BF
                    importedBF.Guid = Guid.NewGuid();
                    for (int i = 0; i < importedBF.TargetApplications.Count; i++)
                        if (App.UserProfile.Solution.ApplicationPlatforms.Where(x => x.AppName == importedBF.TargetApplications[i].AppName).FirstOrDefault() == null)
                        {
                            importedBF.TargetApplications.RemoveAt(i);//No such Application so Delete it
                            i--;
                        }                    
                    if (importedBF.TargetApplications.Count == 0)
                    {
                        TargetApplication ta = new TargetApplication();
                        ta.AppName = App.UserProfile.Solution.ApplicationPlatforms[0].AppName;
                        importedBF.TargetApplications.Add(ta);
                    }

                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(importedBF);
                    mRepositoryFolder.AddRepositoryItem(importedBF);
                }
                catch(Exception ex)
                {
                    Reporter.ToUser(eUserMsgKeys.StaticErrorMessage, "Failed to copy and load the selected " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " file." + System.Environment.NewLine + "Error: " + System.Environment.NewLine + ex.Message);
                    return;
                }
            }
        }

        public override void AddTreeItem()
        {
            //TODO: change to wizard
            string BizFlowName = string.Empty;
            if (GingerCore.General.GetInputWithValidation("Add " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Name:", ref BizFlowName, System.IO.Path.GetInvalidFileNameChars()))
            {
                BusinessFlow BizFlow = App.CreateNewBizFlow(BizFlowName);

                if (App.UserProfile.Solution.ApplicationPlatforms.Count != 1)
                {
                    EditBusinessFlowAppsPage EBFP = new EditBusinessFlowAppsPage(BizFlow);
                    EBFP.ResetPlatformSelection();
                    EBFP.Title = "Configure " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Target Application(s)";
                    EBFP.ShowAsWindow(eWindowShowStyle.Dialog, false);
                }
                mRepositoryFolder.AddRepositoryItem(BizFlow);                
            }
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            if (mViewMode == eBusinessFlowsTreeViewMode.ReadWrite)
            {
                if (IsGingerDefualtFolder)
                    AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), allowRenameFolder: false, allowDeleteFolder: false);
                else
                    AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                AddSourceControlOptions(mContextMenu, false, false);

                MenuItem importMenu = TreeViewUtils.CreateSubMenu(mContextMenu, "Import");
                TreeViewUtils.AddSubMenuItem(importMenu, "Import External " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), ImportExternalBuinessFlow, null, "@Import_16x16.png");
                TreeViewUtils.AddSubMenuItem(importMenu, "Import ALM Test Set", ALMTSImport, null, "@ALM_16x16.png");
                TreeViewUtils.AddSubMenuItem(importMenu, "Import ALM Test Set By ID", ALMTSImportById, null, "@ALM_16x16.png");
                TreeViewUtils.AddSubMenuItem(importMenu, "Import Gherkin Feature File", ImportGherkinFeature, null, "@FeatureFile_16X16.png");
                TreeViewUtils.AddSubMenuItem(importMenu, "Import Selenium Script", ImportSeleniumScript, null, "");
                TreeViewUtils.AddSubMenuItem(importMenu, "Import QTP Script", ImportAQTPScript, null, "");
                TreeViewUtils.AddSubMenuItem(importMenu, "Import ASAP Script", ImportASAPScript, null, "");
                MenuItem exportMenu = TreeViewUtils.CreateSubMenu(mContextMenu, "Export");
                TreeViewUtils.AddSubMenuItem(exportMenu, "Export All to ALM", ExportAllToALM, null, "@ALM_16x16.png");
            }
            else
            {
                AddFolderNodeBasicManipulationsOptions(mContextMenu, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), true, false, false, false, false, false, false, false, false, false);
            }
        }

        private void ImportGherkinFeature(object sender, RoutedEventArgs e)
        {
            BusinessFlow BF = null;
            if (WorkSpace.Instance.BetaFeatures.ImportGherkinFeatureWizrd)
            {
                WizardWindow.ShowWizard(new ImportGherkinFeatureWizard(mRepositoryFolder.FolderFullPath));                
            }
            else
            {
                // TODO: check test me
                string FeatureFolder = mRepositoryFolder.FolderFullPath;
                if (!mRepositoryFolder.FolderFullPath.EndsWith("BusinessFlows"))
                    FeatureFolder = mRepositoryFolder.FolderFullPath.Substring(mRepositoryFolder.FolderFullPath.IndexOf("BusinessFlows\\") + 14);
                ImportGherkinFeatureFilePage IFP = new ImportGherkinFeatureFilePage(FeatureFolder, ImportGherkinFeatureFilePage.eImportGherkinFileContext.BusinessFlowFolder);
                IFP.ShowAsWindow();
                BF = IFP.BizFlow;

                if (BF != null)
                {
                    //Refresh and select Faetures Folder
                    DocumentsFolderTreeItem DFTI = (DocumentsFolderTreeItem)mTreeView.Tree.GetChildItembyNameandSelect("Documents");
                    DFTI = (DocumentsFolderTreeItem)mTreeView.Tree.GetChildItembyNameandSelect("Features", DFTI);
                    if (mRepositoryFolder.FolderFullPath != "Business Flows")
                    {
                        mTreeView.Tree.GetChildItembyNameandSelect(mRepositoryFolder.FolderFullPath, DFTI);
                    }
                    mTreeView.Tree.RefreshSelectedTreeNodeChildrens();

                    //Select Business Folder
                    mTreeView.Tree.SelectItem(this);
                    mTreeView.Tree.RefreshSelectedTreeNodeChildrens();
                    BusinessFlowTreeItem BFTI = new BusinessFlowTreeItem();
                    BFTI.BusinessFlow = BF;

                    mTreeView.Tree.GetChildItembyNameandSelect(BF.Name, this);
                }
            }
        }

        private void ExportAllToALM(object sender, System.Windows.RoutedEventArgs e)
        {
            ObservableList<BusinessFlow> bfToExport = mRepositoryFolder.GetFolderItems();
            if (bfToExport.Count > 0)
            {
                if (bfToExport.Count == 1)
                    ALMIntegration.Instance.ExportBusinessFlowToALM(bfToExport[0], true);
                else
                {
                    if (ALMIntegration.Instance.ExportAllBusinessFlowsToALM(bfToExport, true, ALMIntegration.eALMConnectType.Auto))
                        Reporter.ToUser(eUserMsgKeys.ExportAllItemsToALMSucceed);
                    else Reporter.ToUser(eUserMsgKeys.ExportAllItemsToALMFailed);
                }
            }
        }
    }
}