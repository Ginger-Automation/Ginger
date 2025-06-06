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
using Amdocs.Ginger.Repository;
using Ginger.ApplicationModelsLib.POMModels;
using Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib;
using Ginger.ApplicationModelsLib.POMModels.POMWizardLib.UpdateMultipleWizard;
using Ginger.External.Katalon;
using Ginger.GeneralLib;
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
        private ApplicationPlatform? _applicationPlatform;

        public ApplicationPOMsTreeItem(RepositoryFolder<ApplicationPOMModel> POMModelFolder)
        {
            mPOMModelFolder = POMModelFolder;
        }

        public ApplicationPOMsTreeItem(RepositoryFolder<ApplicationPOMModel> POMModelFolder, ApplicationPlatform? applicationPlatform) : this(POMModelFolder)
        {
            _applicationPlatform = applicationPlatform;
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
            if (WorkSpace.Instance.UserProfile.ShowEnterpriseFeatures)
            {
                TreeViewUtils.AddSubMenuItem(addMenu, "Learn POM From MockUP's", AddScreenshotPOM, null, eImageType.Microchip);
            }
            if (mPOMModelFolder.IsRootFolder)
            {
                AddFolderNodeBasicManipulationsOptions(mContextMenu, "Page Objects Model", allowAddNew: false, allowDeleteFolder: false, allowRenameFolder: false, allowRefresh: false, allowDeleteAllItems: true, allowMultiPomUpdate: true);
            }
            else
            {
                AddFolderNodeBasicManipulationsOptions(mContextMenu, "Page Objects Model", allowAddNew: false, allowRefresh: false);
            }
            MenuItem importMenu = TreeViewUtils.CreateSubMenu(mContextMenu, "Import", eImageType.ImportFile);
            TreeViewUtils.AddSubMenuItem(importMenu, "Katalon Object-Repository", ImportFromKatalonObjectRepository, CommandParameter: null!, icon: eImageType.Katalon);

            MenuItem updateMultiPomMenu = TreeViewUtils.CreateSubMenu(mContextMenu, "Update Multiple POM", eImageType.ApplicationPOMModel);
            TreeViewUtils.AddSubMenuItem(updateMultiPomMenu, "Update Multiple POM", UpdateMultiplePOM, null, eImageType.ApplicationPOMModel);

            AddSourceControlOptions(mContextMenu);
        }

        private void ImportFromKatalonObjectRepository(object sender, RoutedEventArgs e)
        {
            WizardWindow.ShowWizard(new ImportKatalonObjectRepositoryWizard(mPOMModelFolder), width: 1000);
        }

        internal void AddPOM(object sender, RoutedEventArgs e)
        {
            List<ApplicationPlatform> TargetApplications = WorkSpace.Instance.Solution.GetListOfPomSupportedPlatform();
            if (TargetApplications.Count != 0)
            {
                mTreeView.Tree.ExpandTreeItem(this);
                WizardWindow.ShowWizard(new AddPOMWizard(mPOMModelFolder), 1000, 700, DoNotShowAsDialog: true);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.MissingTargetApplication, $"Please Add at-least one {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} that supports POM to continue adding Page Object Models");
            }
        }

        internal void AddEmptyPOM(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplicationPOMModel emptyPOM = new();

                ObservableList<ApplicationPlatform> TargetApplications = null;
                var applicationPlatforms = WorkSpace.Instance.Solution.ApplicationPlatforms;

                if (applicationPlatforms != null)
                {
                    TargetApplications = new ObservableList<ApplicationPlatform>(applicationPlatforms.Where(x => ApplicationPOMModel.PomSupportedPlatforms.Contains(x.Platform)));
                }

                if (TargetApplications == null || TargetApplications.Count == 0)
                {
                    Reporter.ToUser(eUserMsgKey.MissingTargetApplication, $"Please Add at-least one {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} that supports POM to continue adding Page Object Models");
                    return;
                }

                EmptyPOMPage emptyPOMPage = new EmptyPOMPage(emptyPOM, TargetApplications, "Create POM");
                emptyPOMPage.ShowAsWindow();

                if (emptyPOMPage.targetApplicationValue != null && !string.IsNullOrEmpty(emptyPOMPage.pomNameValue))
                {
                    var PomLearnUtils = new Amdocs.Ginger.CoreNET.Application_Models.PomLearnUtils(emptyPOM, pomModelsFolder: mPOMModelFolder);
                    PomLearnUtils.SaveLearnedPOM();
                }
                else
                {
                    return;
                }

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error creating Empty POM", ex);
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Failed to create Empty POM");
            }
        }

        internal void UpdateMultiplePOM(object sender, RoutedEventArgs e)
        {
            List<ApplicationPlatform> TargetApplications = WorkSpace.Instance.Solution.GetListOfPomSupportedPlatform();
            if (TargetApplications.Count != 0)
            {
                mTreeView.Tree.ExpandTreeItem(this);
                WizardWindow.ShowWizard(new UpdateMultiplePomWizard(mPOMModelFolder), 1200, 800, DoNotShowAsDialog: true);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.MissingTargetApplication, $"Please Add at-least one {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} that supports POM to continue adding Page Object Models");
            }
        }
        /// <summary>
        /// Opens the screenshot-based POM learning wizard after verifying that at least one POM-supported platform exists
        /// </summary>
        internal void AddScreenshotPOM(object sender, RoutedEventArgs e)
        {
            List<ApplicationPlatform> TargetApplications = WorkSpace.Instance.Solution.GetListOfPomSupportedPlatform();
            if (TargetApplications.Count != 0)
            {
                mTreeView.Tree.ExpandTreeItem(this);
                WizardWindow.ShowWizard(new AddPOMFromScreenshotWizard(mPOMModelFolder), 1200, 800, DoNotShowAsDialog: true);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.MissingTargetApplication, $"Please Add at-least one {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} that supports POM to continue adding Page Object Models");
            }
        }

    }
}
