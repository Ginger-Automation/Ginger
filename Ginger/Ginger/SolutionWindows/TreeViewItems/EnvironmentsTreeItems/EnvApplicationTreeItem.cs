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
using Ginger.Environments;
using GingerCore;
using GingerCore.Environments;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class EnvApplicationTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        private ApplicationPage mApplicationPage;
        public ProjEnvironment ProjEnvironment { get; set; }
        public EnvApplication EnvApplication { get; set; }

        Object ITreeViewItem.NodeObject()
        {
            return EnvApplication;
        }
        override public string NodePath()
        {
            return EnvApplication.FileName;
        }
        override public Type NodeObjectType()
        {
            return typeof(EnvApplication);
        }

        StackPanel ITreeViewItem.Header()
        {                        
            return NewTVItemHeaderStyle(EnvApplication);
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return null;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return false;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            ProjEnvironment.SaveBackup();//to mark the env as changed
            ProjEnvironment.StartDirtyTracking();
            if (mApplicationPage == null)
            {
                mApplicationPage = new ApplicationPage(EnvApplication, new Context() { Environment = ProjEnvironment });
            }
            return mApplicationPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            TreeViewUtils.AddMenuItem(mContextMenu, "Save Parent Environment", Save, this, eImageType.Save);
            TreeViewUtils.AddMenuItem(mContextMenu, "Duplicate", Duplicate, null, eImageType.Duplicate);
            TreeViewUtils.AddMenuItem(mContextMenu, "Delete", Delete, null, eImageType.Delete);
            TreeViewUtils.AddMenuItem(mContextMenu, "Share With Other Environments", Share, this, eImageType.Share);            
        }

        private void DeleteEnvTreeItems()
        {
            ProjEnvironment.Applications.Remove(EnvApplication);
            ProjEnvironment.SaveBackup();//to mark the env as changed
        }
            private void Delete(object sender, RoutedEventArgs e)
        {
            DeleteEnvTreeItems();
        }

        public override bool DeleteTreeItem(object item, bool deleteWithoutAsking = false, bool refreshTreeAfterDelete = true)
        {
            var repoItem = item as RepositoryItemBase;
            if (repoItem != null)
            {
                if (!deleteWithoutAsking)
                {
                    if (Reporter.ToUser(eUserMsgKey.DeleteItem, repoItem.GetNameForFileName()) == Amdocs.Ginger.Common.eUserMsgSelection.No)
                    {
                        return false;
                    }
                }
            }
            DeleteEnvTreeItems();
            return true;
        }

            private void Duplicate(object sender, RoutedEventArgs e)
        {
            EnvApplication copy = (EnvApplication)EnvApplication.CreateCopy();
            copy.Name = copy.Name + "_copy";
            ProjEnvironment.Applications.Add(copy);
            ProjEnvironment.SaveBackup();//to mark the env as changed
        }

        private void Share(object sender, System.Windows.RoutedEventArgs e)
        {
            bool appsWereAdded = false;
            ObservableList<ProjEnvironment> envs = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>();
            foreach (ProjEnvironment env in envs)
            {
                if (env != ProjEnvironment)
                {
                    if (env.Applications.Where(x => x.Name == EnvApplication.Name).FirstOrDefault() == null)
                    {
                        EnvApplication app = (EnvApplication)(((RepositoryItemBase)EnvApplication).CreateCopy());
                        env.Applications.Add(app);
                        env.SaveBackup();//to mark the env as changed
                        appsWereAdded = true;
                    }
                }
            }

            if (appsWereAdded)
            {
                Reporter.ToUser(eUserMsgKey.ShareEnvAppWithAllEnvs);
            }
        }

        private void Save(object sender, System.Windows.RoutedEventArgs e)
        {
            base.SaveTreeItem(ProjEnvironment);
        }
    }
}
