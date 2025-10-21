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
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using Ginger.GherkinLib;
using Ginger.PlugInsWindows;
using Ginger.UserControlsLib.TextEditor;
using Ginger.UserControlsLib.TextEditor.Gherkin;
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
            return Path;
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

        private List<ITreeViewItem>? _children = null;

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            if (_children == null)
            {
                _children = GetChildrenList();
            }
            else
            {
                List<ITreeViewItem> updatedChildren = [];
                List<ITreeViewItem> newChildren = GetChildrenList();
                foreach (ITreeViewItem child in newChildren)
                {
                    ITreeViewItem? oldChild = _children.FirstOrDefault(o =>
                    {
                        object oldNodeObj = o.NodeObject();
                        object newNodeObj = child.NodeObject();
                        if (oldNodeObj is not string oldNodeObjStr || newNodeObj is not string newNodeObjStr)
                        {
                            return false;
                        }
                        return string.Equals(oldNodeObjStr, newNodeObjStr);
                    });
                    if (oldChild != null)
                    {
                        updatedChildren.Add(oldChild);
                    }
                    else
                    {
                        updatedChildren.Add(child);
                    }
                }
                _children = updatedChildren;
            }
            return _children;
        }

        private List<ITreeViewItem> GetChildrenList()
        {
            List<ITreeViewItem> Childrens = [];

            AddSubFolders(Path, Childrens);

            //Add Current folder Docs 
            foreach (string f in Directory.GetFiles(Path))
            {
                DocumentTreeItem DTI = new DocumentTreeItem
                {
                    FileName = System.IO.Path.GetFileName(f),
                    Path = f
                };

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
                Reporter.ToLog(eLogLevel.ERROR, "Failed to add Document Folder to tree", excpt);
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

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
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

            if (TV.Tree.TreeChildFolderOnly == true)
            {
                AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: "Document", true, false, false, false, false, false, false, true, false, false);
            }
            else
            {
                if (IsGingerDefualtFolder)
                {
                    AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: "Document", allowSaveAll: false, allowAddNew: false, allowCopyItems: false, allowCutItems: false, allowPaste: false, allowRenameFolder: false, allowDeleteFolder: false, allowDeleteAllItems: true);
                }
                else
                {
                    AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: "Document", allowSaveAll: false, allowAddNew: false, allowCopyItems: false, allowCutItems: false, allowPaste: false);
                }

                AddSourceControlOptions(mContextMenu, false, false);

                AddImportsAndCreateDocumentsOptions();
            }

        }

        public override void DeleteAllTreeItems()
        {
            if (Reporter.ToUser(eUserMsgKey.DeleteTreeFolderAreYouSure, mTreeView.Tree.GetSelectedTreeNodeName()) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                List<ITreeViewItem> childNodes = mTreeView.Tree.GetTreeNodeChildsIncludingSubChilds(this);
                childNodes.Reverse();
                foreach (ITreeViewItem node in childNodes)
                {
                    if (node == null)
                    {
                        continue;
                    }

                    if (node.NodeObject() != null)
                    {
                        if (node.NodeObject() is RepositoryFolderBase)
                        {
                            WorkSpace.Instance.SolutionRepository.DeleteRepositoryItemFolder((RepositoryFolderBase)node.NodeObject());
                        }
                        else if (node.NodeObject() is RepositoryItemBase)
                        {
                            ((NewTreeViewItemBase)node).DeleteTreeItem(node.NodeObject(), true, false);
                        }
                        else if (node.NodeObject() is string)
                        {
                            string filepath = System.IO.Path.Combine(WorkSpace.Instance.Solution.ContainingFolderFullPath, "Documents", (string)node.NodeObject());
                            if (File.Exists(filepath))
                            {
                                try
                                {
                                    File.Delete(filepath);
                                }
                                catch (Exception ex)
                                {
                                    Reporter.ToLog(eLogLevel.ERROR, $"Error while deleting file '{filepath}'", ex);
                                }
                            }
                        }
                        else
                        {
                            Reporter.ToLog(eLogLevel.DEBUG, $"cannot delete item, unknown type - {node.NodeObject().GetType().FullName}");
                        }
                    }
                    else
                    {
                        if (Directory.Exists(this.NodePath()))
                        {
                            String[] DocFolderChildItems = Directory.GetDirectories(this.NodePath());
                            foreach (String path in DocFolderChildItems)
                            {
                                Directory.Delete(path, true);
                            }
                        }
                        break;
                    }
                }
            }
            mTreeView.Tree.RefreshSelectedTreeNodeParent();
        }

        private void AddImportsAndCreateDocumentsOptions()
        {
            //Creating the Generic menus
            MenuItem ImportDocumentMenu = TreeViewUtils.CreateSubMenu(mContextMenu, "Import Document");
            MenuItem CreateDocumentMenu = TreeViewUtils.CreateSubMenu(mContextMenu, "Create Document");

            //Creating text and VBS menus
            TreeViewUtils.AddSubMenuItem(ImportDocumentMenu, "Import TXT Document", ImportNewDocument, ".txt", eImageType.Download);
            TreeViewUtils.AddSubMenuItem(ImportDocumentMenu, "Import VBS Document", ImportNewDocument, ".vbs", eImageType.Download);
            TreeViewUtils.AddSubMenuItem(ImportDocumentMenu, "Import Gherkin Feature Document", ImportGherkinFeatureFile, null, eImageType.Download);
            TreeViewUtils.AddSubMenuItem(CreateDocumentMenu, "Create TXT Document", CreateNewDocument, ".txt", eImageType.Add);
            TreeViewUtils.AddSubMenuItem(CreateDocumentMenu, "Create VBS Document", CreateNewDocument, ".vbs", eImageType.Add);
            TreeViewUtils.AddSubMenuItem(CreateDocumentMenu, "Create Gherkin Feature Document", CreateGherkinFeatureFile, null, eImageType.Add);
            TreeViewUtils.AddSubMenuItem(CreateDocumentMenu, "Add Other File Type", CreateNewDocument, "", eImageType.Add);
            AddSolutionPlugInEditorsOptions(ImportDocumentMenu, CreateDocumentMenu);
        }

        private void AddSolutionPlugInEditorsOptions(MenuItem ImportDocumentMenu, MenuItem CreateDocumentMenu)
        {

            ObservableList<PluginPackage> Plugins = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<PluginPackage>();

            foreach (PluginPackage pluginPackage in Plugins)
            {
                pluginPackage.PluginPackageOperations = new PluginPackageOperations(pluginPackage);
                if (string.IsNullOrEmpty(((PluginPackageOperations)pluginPackage.PluginPackageOperations).PluginPackageInfo.UIDLL))
                {
                    continue;
                }

                foreach (ITextEditor TE in PluginTextEditorHelper.GetTextFileEditors(pluginPackage))
                {

                    foreach (string extension in TE.Extensions)
                    {
                        String DocumentName = extension[1..].ToUpper();
                        TreeViewUtils.AddSubMenuItem(CreateDocumentMenu, "Create " + DocumentName + " Document", CreateNewDocument, extension, "@Add_16x16.png");
                        TreeViewUtils.AddSubMenuItem(ImportDocumentMenu, "Import " + DocumentName + " Document", ImportNewDocument, extension, "@Import_16x16.png");
                    }

                }
            }
        }

        private void CreateNewDocument(object sender, RoutedEventArgs e)
        {
            mTreeView.Tree.ExpandTreeItem(this);

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
            if (GingerCore.General.GetInputWithValidation(headerToShow, lblToShow, ref NewFileName, System.IO.Path.GetInvalidFileNameChars(), false, null))
            {
                FullFilePath = System.IO.Path.Combine(Path, NewFileName + FileExtension);
                if (string.IsNullOrEmpty(System.IO.Path.GetExtension(FullFilePath)))
                {
                    FullFilePath = FullFilePath + ".txt";
                }
                if (!System.IO.Directory.Exists(Path))
                {
                    Directory.CreateDirectory(System.IO.Path.GetFullPath(Path));
                }

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
                {
                    Reporter.ToUser(eUserMsgKey.NotifyFileSelectedFromTheSolution, FullFilePath);
                }
            }
        }

        private void AddChildToTree(string FullFilePath)
        {
            DocumentTreeItem DTI = new DocumentTreeItem
            {
                FileName = System.IO.Path.GetFileName(FullFilePath),
                Path = FullFilePath
            };

            ITreeViewItem addTreeViewItem = mTreeView.Tree.AddChildItemAndSelect(this, DTI);
            mTreeView.Tree.RefreshHeader(addTreeViewItem);
        }

        private void ImportNewDocument(object sender, RoutedEventArgs e)
        {
            mTreeView.Tree.ExpandTreeItem(this);

            string FileExtension = ((string)((MenuItem)sender).CommandParameter);

            ImportDocumentPage IDP = new ImportDocumentPage(Path, FileExtension[1..].ToUpper(), FileExtension);
            IDP.ShowAsWindow();
            if (IDP.Imported)
            {
                AddChildToTree(IDP.mPath);
            }
        }

        //Gherkin BDD functions
        private void ImportGherkinFeatureFile(object sender, RoutedEventArgs e)
        {
            ImportGherkinFeatureWizard mWizard = new ImportGherkinFeatureWizard(this, ImportGherkinFeatureFilePage.eImportGherkinFileContext.DocumentsFolder)
            {
                mFolder = this.Path
            };
            WizardWindow.ShowWizard(mWizard);

            if (!String.IsNullOrEmpty(mWizard.FetaureFileName))
            {
                mTreeView.Tree.RefreshSelectedTreeNodeChildrens();
                mTreeView.Tree.GetChildItembyNameandSelect(System.IO.Path.GetFileName(mWizard.FetaureFileName), mTreeView.Tree.CurrentSelectedTreeViewItem);
            }
        }

        private void CreateGherkinFeatureFile(object sender, RoutedEventArgs e)
        {
            string FileName = string.Empty;
            if (GingerCore.General.GetInputWithValidation("New Feature File", "File Name:", ref FileName, System.IO.Path.GetInvalidFileNameChars(), false, null))
            {
                string FullFilePath = System.IO.Path.Combine(this.Path + @"\", FileName + ".feature");
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
                {
                    Reporter.ToUser(eUserMsgKey.GherkinNotifyFeatureFileExists, FullFilePath);
                }
            }
        }

        public override ITreeViewItem AddSubFolder(Type typeOfFolder, string newFolderName, string newFolderPath)
        {
            try
            {
                mTreeView.Tree.ExpandTreeItem(this);

                System.IO.Directory.CreateDirectory(newFolderPath);
                DocumentsFolderTreeItem FolderItem = new DocumentsFolderTreeItem
                {
                    Folder = newFolderName,
                    Path = newFolderPath
                };
                mTreeView.Tree.AddChildItemAndSelect(this, FolderItem);
                return FolderItem;
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Add sub folder failed, error: " + ex.Message);
                return null;
            }
        }

        public override void DeleteTreeFolder()
        {
            if (Reporter.ToUser(eUserMsgKey.DeleteTreeFolderAreYouSure, Folder) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                try
                {
                    if (Directory.Exists(Path))
                    {
                        Directory.Delete(Path, true);
                    }

                    mTreeView.Tree.RefreshSelectedTreeNodeParent();
                }
                catch (Exception ex)
                {
                    Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Delete folder failed, error: " + ex.Message);
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
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                Folder = newFolderName;

                //refresh header and children's (to get new File name)
                mTreeView.Tree.RefreshSelectedTreeNodeParent();
                // RefreshTreeFolder(typeof(DocumentsFolderTreeItem), Path);
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Rename folder failed, error: " + ex.Message);
                return false;
            }

            return true;
        }

        public void RefreshDocumentsFolder()
        {
            try
            {
                //Clearing the cached items
                _children?.Clear();

                //loading the items again from disk
                _children = GetChildrenList();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error refreshing Documents folder at path '{Path}'", ex);
            }
        }

    }
}
