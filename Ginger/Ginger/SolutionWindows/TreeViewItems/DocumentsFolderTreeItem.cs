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
using Ginger.GherkinLib;
using Ginger.PlugInsLib;
using Ginger.UserControlsLib.TextEditor;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using GingerCore.Actions.PlugIns;
using GingerCore.GeneralLib;
using GingerCore.SourceControl;
using GingerPlugIns.TextEditorLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using GingerWPF.TreeViewItemsLib;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class DocumentsFolderTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        public string Folder { get; set; }
        public string Path { get; set; }
        
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
            // return TreeViewUtils.CreateItemHeader(mAgentsFolder, nameof(RepositoryFolder<Agent>.DisplayName), eImageType.Folder, GetSourceControlImage(mAgentsFolder), false);

            string ImageFile;
            if (IsGingerDefualtFolder)
            {
                ImageFile = "@Documents_16x16.png";
            }
            else
            {
                ImageFile = "@Folder2_16x16.png";
            }

            return TreeViewUtils.CreateItemHeader(Folder, ImageFile, Ginger.SourceControl.SourceControlIntegration.GetItemSourceControlImage(Path, ref ItemSourceControlStatus));
        }


        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            List<ITreeViewItem> Childrens = new List<ITreeViewItem>();

            AddsubFolders(Path, Childrens);

            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }
         
            //Add Docs 
            foreach (string f in Directory.GetFiles(Path))
            {                
                DocumentTreeItem DTI = new DocumentTreeItem();             
                DTI.FileName = System.IO.Path.GetFileName(f);
                DTI.Path = f;

                Childrens.Add(DTI);
            }

            return Childrens;
        }

        private void AddsubFolders(string sDir, List<ITreeViewItem> Childrens)
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
                Console.WriteLine(excpt.Message);
            }
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        internal void AddDocument(object sender, RoutedEventArgs e)
        {
            CreateNewDocument(sender, e);
        }

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

            if (IsGingerDefualtFolder)
                AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: "Document",allowSaveAll:false, allowAddNew:false,allowCopyItems:false,allowCutItems:false,allowPaste:false, allowRenameFolder: false, allowDeleteFolder: false);
            else
                AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: "Document", allowSaveAll: false, allowAddNew: false, allowCopyItems: false, allowCutItems: false, allowPaste: false);
            AddSourceControlOptions(mContextMenu, false, false);
            if(this.Folder == "Documents" || this.Path.Contains("\\Documents\\Features")) 
                AddGherkinOptions(mContextMenu);

            AddImportsAndCreateDocumentsOptions();
        }

        private void AddImportsAndCreateDocumentsOptions()
        {
            //Creating the Generic menus
            MenuItem ImportDocumentMenu =  TreeViewUtils.CreateSubMenu(mContextMenu, "Import Document");
            MenuItem CreateDocumentMenu = TreeViewUtils.CreateSubMenu(mContextMenu, "Create Document");

            //Creating text and VBS menus
            TreeViewUtils.AddSubMenuItem(ImportDocumentMenu, "Import txt Document", ImportNewDocument, ".txt", "@Import_16x16.png");
            TreeViewUtils.AddSubMenuItem(ImportDocumentMenu, "Import VBS Document", ImportNewDocument, ".vbs", "@Import_16x16.png");
            TreeViewUtils.AddSubMenuItem(CreateDocumentMenu, "Create txt Document", CreateNewDocument, ".txt", "@Add_16x16.png");
            TreeViewUtils.AddSubMenuItem(CreateDocumentMenu, "Create VBS Document", CreateNewDocument, ".vbs", "@Add_16x16.png");

           // AddSolutionPlugInEditorsOptions(ImportDocumentMenu, CreateDocumentMenu);
        }

        public void AddGherkinOptions(ContextMenu CM)
        {
            MenuItem GherkinMenu = TreeViewUtils.CreateSubMenu(CM, "Gherkin");
            //TOD Change Icon
            TreeViewUtils.AddSubMenuItem(GherkinMenu, "Create Feature file", CreateGherkinFeatureFile, null, "@FeatureFile_16X16.png");
            TreeViewUtils.AddSubMenuItem(GherkinMenu, "Import Feature file", ImportGherkinFeatureFile, null, "@FeatureFile_16X16.png");
        }

        //private void AddSolutionPlugInEditorsOptions(MenuItem ImportDocumentMenu, MenuItem CreateDocumentMenu)
        //{
        //   ObservableList<PlugInWrapper> PlugInLists = App.LocalRepository.GetSolutionPlugIns();

        //    foreach (PlugInWrapper PIW in PlugInLists)
        //    {
        //        foreach (PlugInTextFileEditorBase PITFEB in PIW.TextEditors())
        //        {
        //            foreach (string extension in PITFEB.Extensions)
        //            {
        //                String DocumentName = extension.Substring(1).ToUpper();
        //                TreeViewUtils.AddSubMenuItem(CreateDocumentMenu, "Create " + DocumentName + " Document", CreateNewDocument, extension, "@Add_16x16.png");
        //                TreeViewUtils.AddSubMenuItem(ImportDocumentMenu, "Import " + DocumentName + " Document", ImportNewDocument, extension, "@Import_16x16.png");
        //            }
        //        }
        //    }
        //}

        private void CreateNewDocument(object sender, RoutedEventArgs e)
        {
            string FileContent = string.Empty;
            string FileExtension = ((string)((MenuItem)sender).CommandParameter);
            if (FileExtension == ".txt")
            {
                FileContent = "Some text";
            }
            else if (FileExtension == ".vbs")
            {
                FileContent = Properties.Resources.VBSTemplate;
            }
            else
            {
                FileContent = PlugInsIntegration.GetTamplateContentByPlugInExtension(FileExtension);
            }

            string NewFileName = string.Empty;
            string FullFilePath = string.Empty;
            if (GingerCore.General.GetInputWithValidation("New " + FileExtension.Substring(1).ToUpper() + " File", "File Name:", ref NewFileName, System.IO.Path.GetInvalidFileNameChars()))
            {
                FullFilePath = Path + @"\" + NewFileName + FileExtension;
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
            string FileExtension = ((string)((MenuItem)sender).CommandParameter);

            ImportDocumentPage IDP = new ImportDocumentPage(Path, FileExtension.Substring(1).ToUpper(), FileExtension);
            IDP.ShowAsWindow();
            if (IDP.Imported)
                AddChildToTree(IDP.mPath);
        }

         //Gherkin BDD functions
        private void ImportGherkinFeatureFile(object sender, RoutedEventArgs e)
        {
            string FeatureFolder = Folder;
            if (this.Path.IndexOf("Documents\\Features\\") > 0)
                FeatureFolder = this.Path.Substring(this.Path.IndexOf("Documents\\Features\\") + 19);
            ImportGherkinFeatureFilePage IFP = new ImportGherkinFeatureFilePage(FeatureFolder, ImportGherkinFeatureFilePage.eImportGherkinFileContext.DocumentsFolder);
            IFP.ShowAsWindow();
            string featureFile = IFP.mFeatureFile;

            if(!String.IsNullOrEmpty(featureFile))
            {               
                if(Folder == "Documents")
                {
                    DocumentsFolderTreeItem DFTI = new DocumentsFolderTreeItem();
                    DFTI.Path = App.UserProfile.Solution.Folder + "Documents" + @"\" + "Features";
                    DFTI.Folder = "Features";
                    mTreeView.Tree.RefreshSelectedTreeNodeChildrens();
                    mTreeView.Tree.GetChildItembyNameandSelect("Features", this);                    
                }
                mTreeView.Tree.RefreshSelectedTreeNodeChildrens();
                mTreeView.Tree.GetChildItembyNameandSelect(System.IO.Path.GetFileName(featureFile), mTreeView.Tree.CurrentSelectedTreeViewItem);
            }         
        }

        private void CreateGherkinFeatureFile(object sender, RoutedEventArgs e)
        { 
            string FileName = string.Empty;
            if (GingerCore.General.GetInputWithValidation("New Feature File", "File Name:", ref FileName, System.IO.Path.GetInvalidFileNameChars()))
            {
                string FullDirectoryPath = "";
                DocumentsFolderTreeItem DFTI = null;
                if (this.Folder == "Documents")
                {
                    FullDirectoryPath = App.UserProfile.Solution.Folder + "Documents" + @"\" + "Features";                    
                    
                    if (!System.IO.Directory.Exists(FullDirectoryPath))
                    {
                        System.IO.Directory.CreateDirectory(FullDirectoryPath);
                        DFTI = new DocumentsFolderTreeItem();
                        DFTI.Path = FullDirectoryPath;
                        DFTI.Folder = "Features";
                        mTreeView.Tree.AddChildItemAndSelect(this, DFTI);
                        mTreeView.Tree.RefreshSelectedTreeNodeChildrens();
                    }
                    else
                    {
                        DFTI = (DocumentsFolderTreeItem)mTreeView.Tree.GetChildItembyNameandSelect("Features",this);                        
                    }
                    
                }
                else
                    FullDirectoryPath = this.Path;

                string FullFilePath = FullDirectoryPath + @"\" + FileName + ".feature";
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
    }
}
