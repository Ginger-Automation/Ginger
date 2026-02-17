#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using Ginger.Environments;
using Ginger.SolutionWindows.TreeViewItems.EnvironmentsTreeItems;
using GingerCore.Environments;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    public class EnvironmentTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        private AppsListPage mAppsListPage;
        public ProjEnvironment ProjEnvironment { get; set; }

        Object ITreeViewItem.NodeObject()
        {
            return ProjEnvironment;
        }
        override public string NodePath()
        {
            return ProjEnvironment.FileName;
        }
        override public Type NodeObjectType()
        {
            return typeof(ProjEnvironment);
        }

        StackPanel ITreeViewItem.Header()
        {
            return NewTVItemHeaderStyle(ProjEnvironment);
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
                    ITreeViewItem? oldChild = _children.FirstOrDefault(o => o.NodeObject() == child.NodeObject());
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

            // Add Apps            
            foreach (EnvApplication app in ProjEnvironment.Applications.OrderBy(nameof(EnvApplication.Name)))
            {
                EnvApplicationTreeItem EATI = new EnvApplicationTreeItem();
                app.SetDataFromAppPlatform(WorkSpace.Instance.Solution.ApplicationPlatforms);
                EATI.EnvApplication = app;
                EATI.ProjEnvironment = ProjEnvironment;
                Childrens.Add(EATI);
            }

            //start tracking of children apps
            ProjEnvironment.Applications.CollectionChanged -= Applications_CollectionChanged;
            ProjEnvironment.Applications.CollectionChanged += Applications_CollectionChanged;

            return Childrens;
        }

        private void Applications_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (mTreeView != null) //TODO: add handling to make sure this will never be Null and won't be set only on SetTools
            {
                if (TreeViewItem != null)
                {
                    //we collapse the TreeViewItem so that when it will be expanded, it will be updated automatically
                    TreeViewItem.IsExpanded = false;
                }
            }
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            if (mAppsListPage == null)
            {
                mAppsListPage = new AppsListPage(ProjEnvironment);
            }
            return mAppsListPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            TreeViewUtils.AddMenuItem(mContextMenu, "Add New Application", AddApplication, null, eImageType.Add);
            AddItemNodeBasicManipulationsOptions(mContextMenu);
            AddSourceControlOptions(mContextMenu);
        }

        private void AddApplication(object sender, RoutedEventArgs e)
        {
            var ApplicationPlatforms = WorkSpace.Instance.Solution.ApplicationPlatforms.Where((app) => !ProjEnvironment.CheckIfApplicationPlatformExists(app.Guid, app.AppName))?.ToList();



            string appName = string.Empty;
            ObservableList<ApplicationPlatform> DisplayedApplicationPlatforms = GingerCore.General.ConvertListToObservableList(ApplicationPlatforms);

            EnvironmentApplicationList applicationList = new(DisplayedApplicationPlatforms);
            applicationList.ShowAsWindow();

            IEnumerable<ApplicationPlatform> SelectedApplications = DisplayedApplicationPlatforms.Where((displayedApp) => displayedApp.Selected);

            ProjEnvironment.AddApplications(SelectedApplications);
            ProjEnvironment.OnPropertyChanged(nameof(ProjEnvironment.Applications));

            if (SelectedApplications.Any())
            {

                ObservableList<ProjEnvironment> AllEnvironments = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>();
                if (AllEnvironments.Count > 1)
                {
                    eUserMsgSelection eUserMsg = Reporter.ToUser(eUserMsgKey.PublishApplicationToOtherEnv);

                    if (eUserMsg.Equals(eUserMsgSelection.Yes))
                    {
                        AllEnvironments.ForEach((env) =>
                        {
                            if (!env.Guid.Equals(ProjEnvironment.Guid) && !SelectedApplications.Any((app) => env.CheckIfApplicationPlatformExists(app.Guid, app.AppName)))
                            {
                                env.AddApplications(SelectedApplications);
                                env.OnPropertyChanged(nameof(env.Applications));
                            }
                        });
                    }
                }

                mTreeView?.Tree?.RefreshSelectedTreeNodeParent();
            }
        }




    }
}
