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
using Amdocs.Ginger.Repository;
using Ginger.ALM;
using Ginger.Repository;
using GingerCore;
using GingerCore.Activities;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class SharedActivitiesGroupsFolderTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        RepositoryFolder<ActivitiesGroup> mActivitiesGroupFolder;

        private ActivitiesGroupsRepositoryPage mActivitiesGroupsRepositoryPage;
        public string Folder { get; set; }
        public string Path { get; set; }
        public enum eActivitiesGroupsItemsShowMode { ReadWrite, ReadOnly }
        private eActivitiesGroupsItemsShowMode mShowMode;

        public SharedActivitiesGroupsFolderTreeItem(eActivitiesGroupsItemsShowMode showMode = eActivitiesGroupsItemsShowMode.ReadWrite)
        {
            mShowMode = showMode;
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
            return typeof(ActivitiesGroup);
        }

        StackPanel ITreeViewItem.Header()
        {
            string ImageFile;
            if (IsGingerDefualtFolder)
            {
                ImageFile = "@Group_16x16.png";
            }
            else
            {
                ImageFile = "@Folder2_16x16.png";
            }
            return TreeViewUtils.CreateItemHeader(Folder, ImageFile, Ginger.SourceControl.SourceControlIntegration.GetItemSourceControlImage(Path, ref ItemSourceControlStatus));
        }


        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return GetChildrentGeneric<ActivitiesGroup>(mActivitiesGroupFolder, nameof(Agent.Name));
            //List<ITreeViewItem> Childrens = new List<ITreeViewItem>();

            //AddsubFolders(Path, Childrens);

            ////Add Activities Group
            //ObservableList<ActivitiesGroup> ActivitiesGroups = App.LocalRepository.GetSolutionRepoActivitiesGroups(specificFolderPath: Path);

            //foreach (ActivitiesGroup activitiesGroup in ActivitiesGroups)
            //{
            //    SharedActivitiesGroupTreeItem SATI = new SharedActivitiesGroupTreeItem(mShowMode);
            //    SATI.mActivitiesGroup = activitiesGroup;
            //    Childrens.Add(SATI);
            //}

            //return Childrens;
        }

        //private void AddsubFolders(string sDir, List<ITreeViewItem> Childrens)
        //{
        //    try
        //    {
        //        foreach (string d in Directory.GetDirectories(Path))
        //        {
        //            SharedActivitiesGroupsFolderTreeItem FolderItem = new SharedActivitiesGroupsFolderTreeItem(mShowMode);
        //            string FolderName = System.IO.Path.GetFileName(d);

        //            FolderItem.Folder = FolderName;
        //            FolderItem.Path = d;

        //            Childrens.Add(FolderItem);
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
            if (mActivitiesGroupsRepositoryPage == null)
            {
                mActivitiesGroupsRepositoryPage = new ActivitiesGroupsRepositoryPage(Path);
            }
            return mActivitiesGroupsRepositoryPage;
        }


        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }


        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            if (mShowMode == eActivitiesGroupsItemsShowMode.ReadWrite)
            {
                if (IsGingerDefualtFolder)
                    AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), allowAddNew: false, allowRenameFolder: false, allowDeleteFolder: false);
                else
                    AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), allowAddNew: false);

                MenuItem exportMenu = TreeViewUtils.CreateSubMenu(mContextMenu, "Export");
                TreeViewUtils.AddSubMenuItem(exportMenu, "Export All to ALM", ExportAllToALM, null, "@ALM_16x16.png");
                
                AddSourceControlOptions(mContextMenu, false, false);
            }
            else
            {
                AddFolderNodeBasicManipulationsOptions(mContextMenu, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), true, false, false, false, false, false, false, false, false,false);
            }
        }

        private void ExportAllToALM(object sender, System.Windows.RoutedEventArgs e)
        {
            ObservableList<ActivitiesGroup> agToExport = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
            if (agToExport.Count > 0)
            {
                if (agToExport.Count == 1)
                    ALMIntegration.Instance.ExportActivitiesGroupToALM(agToExport[0], true);
                else
                {

                    foreach (ActivitiesGroup ag in agToExport)
                        ALMIntegration.Instance.ExportActivitiesGroupToALM(ag, true);
                }
            }
        }
    }
}
