#region License
/*
Copyright © 2014-2023 European Support Limited

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
using Ginger.ApplicationModelsLib.POMModels;
using Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib;
using Ginger.SourceControl;
using GingerCore;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems.ApplicationModelsTreeItems
{
    public class ApplicationPOMsTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        public RepositoryFolder<ApplicationPOMModel> mPOMModelFolder;
        private POMModelsPage mPOMModelsPage;
        private ObservableList<ApplicationPOMModel> mChildPoms = null;

        public ApplicationPOMsTreeItem(RepositoryFolder<ApplicationPOMModel> POMModelFolder)
        {
            mPOMModelFolder = POMModelFolder;
        }

        Object ITreeViewItem.NodeObject()
        {
            return mPOMModelFolder;
        }
        override public string NodePath()
        {
            return mPOMModelFolder.FolderFullPath;
        }
        override public Type NodeObjectType()
        {
            return typeof(ApplicationPOMModel);
        }

        StackPanel ITreeViewItem.Header()
        {
            return NewTVItemFolderHeaderStyle(mPOMModelFolder);
        }

        public override ITreeViewItem GetFolderTreeItem(RepositoryFolderBase folder)
        {
            return new ApplicationPOMsTreeItem((RepositoryFolder<ApplicationPOMModel>)folder);
        }

        public override ITreeViewItem GetTreeItem(object item)
        {
            if (item is ApplicationPOMModel)
            {
                return new ApplicationPOMTreeItem((ApplicationPOMModel)item);
            }

            if (item is RepositoryFolderBase)
            {
                return new ApplicationPOMsTreeItem((RepositoryFolder<ApplicationPOMModel>)item);
            }

            throw new Exception("Error unknown item added to envs folder");
        }


        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return GetChildrentGeneric<ApplicationPOMModel>(mPOMModelFolder);
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {

            if (mPOMModelsPage == null)
            {
                mPOMModelsPage = new POMModelsPage(mPOMModelFolder);
            }
            return mPOMModelsPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            MenuItem addMenu = TreeViewUtils.CreateSubMenu(mContextMenu, "Add Page Objects Model (POM)", eImageType.Add);
            TreeViewUtils.AddSubMenuItem(addMenu, "Learn POM from live page", AddPOM, null, eImageType.Screen);
            TreeViewUtils.AddSubMenuItem(addMenu, "Empty POM", AddEmptyPOM, null, eImageType.ApplicationPOMModel);
            if (mPOMModelFolder.IsRootFolder)
                AddFolderNodeBasicManipulationsOptions(mContextMenu, "Page Objects Model", allowAddNew: false, allowDeleteFolder: false, allowRenameFolder: false, allowRefresh: false, allowDeleteAllItems: true);
            else
                AddFolderNodeBasicManipulationsOptions(mContextMenu, "Page Objects Model", allowAddNew: false, allowRefresh: false);

            AddSourceControlOptions(mContextMenu);
        }

        internal void AddPOM(object sender, RoutedEventArgs e)
        {
            List<ApplicationPlatform> TargetApplications = WorkSpace.Instance.Solution.GetListOfPomSupportedPlatform();
            if (TargetApplications.Count != 0)
            {
                mTreeView.Tree.ExpandTreeItem((ITreeViewItem)this);
                WizardWindow.ShowWizard(new AddPOMWizard(mPOMModelFolder), 1000, 700, DoNotShowAsDialog: true);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.MissingTargetApplication, "Please Add at-least one Target Application that supports POM to continue adding Page Object Models");
            }
        }

        internal void AddEmptyPOM(object sender, RoutedEventArgs e)
        {
            string NewPOMName = string.Empty;
            ApplicationPOMModel emptyPOM = new ApplicationPOMModel() { Name = NewPOMName };
            if (GingerCore.General.GetInputWithValidation("Add Page Object Model", "Page Object Model Name:", ref NewPOMName, null, false, emptyPOM))
            {
                ObservableList<ApplicationPlatform> TargetApplications = GingerCore.General.ConvertListToObservableList(WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => ApplicationPOMModel.PomSupportedPlatforms.Contains(x.Platform)).ToList());
                if (TargetApplications != null && TargetApplications.Count > 0)
                {
                    emptyPOM.TargetApplicationKey = TargetApplications[0].Key;
                }
                emptyPOM.Name = NewPOMName;
                var PomLearnUtils = new Amdocs.Ginger.CoreNET.Application_Models.PomLearnUtils(emptyPOM, pomModelsFolder: mPOMModelFolder);
                PomLearnUtils.SaveLearnedPOM();
            }
        }
    }
}
