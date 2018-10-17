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
using Amdocs.Ginger.Common.Enums;
using Ginger.GherkinLib;
using Ginger.UserControlsLib.TextEditor;
using GingerCore;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Ginger.UserControlsLib.TextEditor.Gherkin;
using GingerWPF.WizardLib;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.SolutionWindows.TreeViewItems
{
    //public enum eDocumentsItemViewMode
    //{
    //    All = 0,
    //    FoldersOnly = 1
    //};
    class DocumentsFolderTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        public string Folder { get; set; }
                
        string mPath;
        public string Path
        {
            get
            {
                return mPath;
            }
            set
            {
                if (!Directory.Exists(value))
                {
                    Directory.CreateDirectory(value);
                }
                mPath = value;
            }
        }  
        Object ITreeViewItem.NodeObject()
        {
            return null;
        }
        override public string NodePath()
        {
            return Path + @"\";
        }
        override public Type NodeObjectType()
        {
            return null;
        }

        StackPanel ITreeViewItem.Header()
        {                       
            return TreeViewUtils.NewRepositoryItemTreeHeader(null, Folder, eImageType.Folder, GetSourceControlImageByPath(Path), false);
        }


        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            List<ITreeViewItem> Childrens = new List<ITreeViewItem>();

            AddSubFolders(Path, Childrens);

            //Add Current folder Docs 
            foreach (string f in Directory.GetFiles(Path))
            {                
                DocumentTreeItem DTI = new DocumentTreeItem();             
                DTI.FileName = System.IO.Path.GetFileName(f);
                DTI.Path = f;

                Childrens.Add(DTI);
            }

            return Childrens;
        }

        private void AddSubFolders(string sDir, List<ITreeViewItem> Childrens)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(Path))
                {
                    DocumentsFolderTreeItem FolderItem = new DocumentsFolderTreeItem();
                    string FolderName = System.IO.Path.GetFileName(d);

                    FolderItem.Folder = FolderName;
                    FolderItem.Path = d;

                    Childrens.Add(FolderItem);
                }

            }
            catch (System.Exception excpt)
            {
               Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to add Document Folder to tree",excpt,true);
            }
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        //internal void AddDocument(object sender, RoutedEventArgs e)
        //{
        //    CreateNewDocument(sender, e);
           
        //}

        Page ITreeViewItem.EditPage()
        {
            return null;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;            
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            if(TV.Tree.TreeChildFolderOnly == true)
            { 
                AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: "Document", true, false, false, false, false, false, false, true, false, false);
            }
            else
            {
                if (IsGingerDefualtFolder)
                    AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: "Document",allowSaveAll:false, allowAddNew:false,allowCopyItems:false,allowCutItems:false,allowPaste:false, allowRenameFolder: false, allowDeleteFolder: false);
                else
                    AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: "Document", allowSaveAll: false, allowAddNew: false, allowCopyItems: false, allowCutItems: false, allowPaste: false);
                AddSourceControlOptions(mContextMenu, false, false);

                AddImportsAndCreateDocumentsOptions();
            }
            
        }

        private void AddImportsAndCreateDocumentsOptions()
        {
            //Creating the Generic menus
            MenuItem ImportDocumentMenu =  TreeViewUtils.CreateSubMenu(mContextMenu, "Import Document");
            MenuItem CreateDocumentMenu = TreeViewUtils.CreateSubMenu(mContextMenu, "Create Document");

            //Creating text and VBS menus
            TreeViewUtils.AddSubMenuItem(ImportDocumentMenu, "Import TXT Document", ImportNewDocument, ".txt", eImageType.Download);
            TreeViewUtils.AddSubMenuItem(ImportDocumentMenu, "Import VBS Document", ImportNewDocument, ".vbs", eImageType.Download);
            TreeViewUtils.AddSubMenuItem(ImportDocumentMenu, "Import Gherkin Feature Document", ImportGherkinFeatureFile, null, eImageType.Download);
            TreeViewUtils.AddSubMenuItem(CreateDocumentMenu, "Create TXT Document", CreateNewDocument, ".txt", eImageType.Add);
            TreeViewUtils.AddSubMenuItem(CreateDocumentMenu, "Create VBS Document", CreateNewDocument, ".vbs", eImageType.Add);
            TreeViewUtils.AddSubMenuItem(CreateDocumentMenu, "Create Gherkin Feature Document", CreateGherkinFeatureFile, null, eImageType.Add);
            TreeViewUtils.AddSubMenuItem(CreateDocumentMenu, "Add Other File Type", CreateNewDocument, "", eImageType.Add);
        }

        private void CreateNewDocument(object sender, RoutedEventArgs e)
        {
            mTreeView.Tree.ExpandTreeItem((ITreeViewItem)this);

            string FileExtension = ((string)((MenuItem)sender).CommandParameter);
           
            string FileContent = string.Empty;
            
            if (FileExtension == ".txt")
            {
                FileContent = "Some text";
            }
            else if (FileExtension == ".vbs")
            {
                FileContent = Properties.Resources.VBSTemplate;
            }

            string NewFileName = string.Empty;
            string FullFilePath = string.Empty;
            string headerToShow;
            string lblToShow;
            if (string.IsNullOrEmpty(FileExtension))
            {
                headerToShow = "New File";
                lblToShow = "File Name & Extension:";
            }
           else
            {
                headerToShow = string.Format("New {0} File", FileExtension.ToUpper().TrimStart(new char[] { '.' }));
                lblToShow = "File Name:";
            }
            if (GingerCore.General.GetInputWithValidation(headerToShow, lblToShow, ref NewFileName, System.IO.Path.GetInvalidFileNameChars()))
            {
                FullFilePath = System.IO.Path.Combine(Path, NewFileName + FileExtension);
                if (string.IsNullOrEmpty(System.IO.Path.GetExtension(FullFilePath)))
                {
                    FullFilePath = FullFilePath + ".txt";
                }
                if (!System.IO.Directory.Exists(Path))
                    Directory.CreateDirectory(System.IO.Path.GetFullPath(Path));
                if (!System.IO.File.Exists(FullFilePath))
                {
                    using (Stream fileStream = File.Create(System.IO.Path.GetFullPath(FullFilePath)))
                    {
                        fileStream.Close();
                    }
                    File.WriteAllText(FullFilePath, FileContent);
                    AddChildToTree(FullFilePath);
                }
                else
                    Reporter.ToUser(eUserMsgKeys.NotifyFileSelectedFromTheSolution, FullFilePath);
            }
        }

        private void AddChildToTree(string FullFilePath)
        {            
            DocumentTreeItem DTI = new DocumentTreeItem();
            DTI.FileName = System.IO.Path.GetFileName(FullFilePath);
            DTI.Path = FullFilePath;

            ITreeViewItem addTreeViewItem = mTreeView.Tree.AddChildItemAndSelect(this, DTI);
            mTreeView.Tree.RefreshHeader(addTreeViewItem);
        }

        private void ImportNewDocument(object sender, RoutedEventArgs e)
        {
            mTreeView.Tree.ExpandTreeItem((ITreeViewItem)this);

            string FileExtension = ((string)((MenuItem)sender).CommandParameter);

            ImportDocumentPage IDP = new ImportDocumentPage(Path, FileExtension.Substring(1).ToUpper(), FileExtension);
            IDP.ShowAsWindow();
            if (IDP.Imported)
                AddChildToTree(IDP.mPath);
        }

         //Gherkin BDD functions
        private void ImportGherkinFeatureFile(object sender, RoutedEventArgs e)
        {   
            ImportGherkinFeatureWizard mWizard = new ImportGherkinFeatureWizard(this, ImportGherkinFeatureFilePage.eImportGherkinFileContext.DocumentsFolder);
            mWizard.mFolder = this.Path;
            WizardWindow.ShowWizard(mWizard);
            
            if(!String.IsNullOrEmpty(mWizard.FetaureFileName))
            {
                mTreeView.Tree.RefreshSelectedTreeNodeChildrens();
                mTreeView.Tree.GetChildItembyNameandSelect(System.IO.Path.GetFileName(mWizard.FetaureFileName), mTreeView.Tree.CurrentSelectedTreeViewItem);
            }
        }

        private void CreateGherkinFeatureFile(object sender, RoutedEventArgs e)
        { 
            string FileName = string.Empty;
            if (GingerCore.General.GetInputWithValidation("New Feature File", "File Name:", ref FileName, System.IO.Path.GetInvalidFileNameChars()))
            {                
                string FullFilePath = System.IO.Path.Combine(this.Path + @"\" , FileName + ".feature");
                if (!System.IO.File.Exists(FullFilePath))
                {
                    string FileContent = "Feature: Description\r\n\r\n@Tag1 @Tag2\r\n\r\nScenario: Scenario1 Description\r\n       Given \r\n       And \r\n       And \r\n       When \r\n       Then \r\n\r\n\r\n@Tag1 @Tag2\r\n\r\nScenario: Scenario2 Description\r\n       Given \r\n       And \r\n       And \r\n       When \r\n       Then \r\n\r\n@Tag1 @Tag2\r\n\r\n\r\nScenario Outline: eating\r\n  Given there are <start> cucumbers\r\n  When I eat <eat> cucumbers\r\n  Then I should have <left> cucumbers\r\n\r\n  Examples:\r\n    | start | eat | left |\r\n    |  12   |  5  |  7   |\r\n    |  20   |  5  |  15  |";
                    using (Stream fileStream = File.Create(FullFilePath))
                    {
                        fileStream.Close();
                    }
                    File.WriteAllText(FullFilePath, FileContent);
                    
                    mTreeView.Tree.RefreshSelectedTreeNodeChildrens();
                    mTreeView.Tree.GetChildItembyNameandSelect(FileName + ".feature", mTreeView.Tree.CurrentSelectedTreeViewItem);                   
                    
                }
                else
                    Reporter.ToUser(eUserMsgKeys.GherkinNotifyFeatureFileExists, FullFilePath);
            }
        }

        public override ITreeViewItem AddSubFolder(Type typeOfFolder, string newFolderName, string newFolderPath)
        {
            try
            {
                mTreeView.Tree.ExpandTreeItem((ITreeViewItem)this);

               System.IO.Directory.CreateDirectory(newFolderPath);
                DocumentsFolderTreeItem FolderItem = new DocumentsFolderTreeItem();
                FolderItem.Folder = newFolderName;
                FolderItem.Path = newFolderPath;
                mTreeView.Tree.AddChildItemAndSelect((ITreeViewItem)this, (ITreeViewItem)FolderItem);
                return FolderItem;            
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKeys.StaticErrorMessage, "Add sub folder failed, error: " + ex.Message);
                return null;
            }
}

        public override void DeleteTreeFolder()
        {
            if (Reporter.ToUser(eUserMsgKeys.DeleteTreeFolderAreYouSure, Folder) == MessageBoxResult.Yes)
            {
                try
                {
                    if (Directory.Exists(Path))
                        Directory.Delete(Path, true);

                    mTreeView.Tree.RefreshSelectedTreeNodeParent();
                }
                catch (Exception ex)
                {
                    Reporter.ToUser(eUserMsgKeys.StaticErrorMessage, "Delete folder failed, error: " + ex.Message);
                }
            }
        }

        public override bool RenameTreeFolder(string originalName, string newFolderName, string newPath)
        {
            try
            {             
                if (Path != newPath)
                {
                    if (originalName.ToUpper() == newFolderName.ToUpper())//user just changed the name letters case
                    {
                        //move to temp folder
                        string tempTargetPath = System.IO.Path.Combine(Path, originalName);
                        Directory.Move(Path, tempTargetPath);
                        Path = tempTargetPath;
                    }

                    Directory.Move(Path, newPath);
                    if (System.IO.Directory.Exists(newPath) == false)
                        return false;
                }
                else
                {
                    return false;
                }
            
                Folder = newFolderName;

                //refresh header and childerns (to get new File name)
                mTreeView.Tree.RefreshSelectedTreeNodeParent();
               // RefreshTreeFolder(typeof(DocumentsFolderTreeItem), Path);
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKeys.StaticErrorMessage, "Rename folder failed, error: " + ex.Message);
                return false;
            }

            return true;
        }
    }
}
