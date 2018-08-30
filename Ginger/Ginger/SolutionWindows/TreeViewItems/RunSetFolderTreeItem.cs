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
using Ginger.Run;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using Ginger.Repository;
using GingerCore.GeneralLib;
using GingerWPF.TreeViewItemsLib;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class RunSetFolderTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        public RepositoryFolder<RunSetConfig> mRunSetConfig;
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
            return typeof(RunSetConfig);
        }

        StackPanel ITreeViewItem.Header()
        {
            return NewTVItemFolderHeaderStyle(mRunSetConfig);
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return GetChildrentGeneric<RunSetConfig>(mRunSetConfig);
            //List<ITreeViewItem> Childrens = new List<ITreeViewItem>();

            ////Add Run Sets 
            //ObservableList<RunSetConfig> runSets = App.LocalRepository.GetSolutionRunSets(specificFolderPath: Path);
            //AddsubFolders(Path, Childrens);

            //foreach (RunSetConfig RSC in runSets)
            //{
            //    RunSetTreeItem RSTI = new RunSetTreeItem();
            //    RSTI.RunSetConfig = RSC;
            //    Childrens.Add(RSTI);
            //}
            //return Childrens;
        }
        
        //private void AddsubFolders(string sDir, List<ITreeViewItem> Childrens)
        //{
        //    try
        //    {
        //        foreach (string d in Directory.GetDirectories(Path))
        //        {
        //            RunSetFolderTreeItem FolderItem = new RunSetFolderTreeItem();
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
                AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: GingerDicser.GetTermResValue(eTermResKey.RunSet), allowRenameFolder: false, allowDeleteFolder: false);
            else
                AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: GingerDicser.GetTermResValue(eTermResKey.RunSet));
            
            AddSourceControlOptions(mContextMenu, false, false);
        }

        public override void AddTreeItem()
        {
            string runSetName = string.Empty;
            if (InputBoxWindow.GetInputWithValidation(string.Format("Add New {0}", GingerDicser.GetTermResValue(eTermResKey.RunSet)), string.Format("{0} Name:", GingerDicser.GetTermResValue(eTermResKey.RunSet)), ref runSetName, System.IO.Path.GetInvalidPathChars()))
            {
                RunSetConfig Runsets = RunSetOperations.CreateNewRunset(runSetName, Path);

                RunSetTreeItem BFTI = new RunSetTreeItem();
                BFTI.RunSetConfig = Runsets;
                ITreeViewItem addTreeViewItem = mTreeView.Tree.AddChildItemAndSelect(this, BFTI);

                ////Must do the action after the node was added to tree!
                //Runsets.Save();
                ////add BF to cach
                //refresh header- to reflect source control state
                mTreeView.Tree.RefreshHeader(addTreeViewItem);
            }
        }

        //        public static RunSetConfig CreateNewRunset(string runSetName, string runSetFolderPath=null)
        //        {        
        //            RunSetConfig rsc = new RunSetConfig();
        //            rsc.Name = runSetName;
        //            rsc.GingerRunners.Add(new GingerRunner() { Name = "Runner 1" });
        //            if (string.IsNullOrEmpty(runSetFolderPath))
        //                rsc.FileName = LocalRepository.GetRepoItemFileName(rsc);
        //            else
        //                rsc.FileName = LocalRepository.GetRepoItemFileName(rsc, runSetFolderPath);

        //            App.LocalRepository.SaveNewItem(rsc, runSetFolderPath);
        //            App.LocalRepository.AddItemToCache(rsc);
        //            return rsc;
        //        }

        public override void RefreshTreeFolder(Type itemType, string path)
        {
            base.RefreshTreeFolder(typeof(RunSetConfig), path);
        }
    }
}