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

using Amdocs.Ginger.Common.Enums;
using Ginger.Environments;
using GingerCore.Environments;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
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

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            List<ITreeViewItem> Childrens = new List<ITreeViewItem>();

            // Add Apps            
            foreach (EnvApplication app in ProjEnvironment.Applications.OrderBy(nameof(EnvApplication.Name)))
            {
                EnvApplicationTreeItem EATI = new EnvApplicationTreeItem();
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
                mTreeView.Tree.RefresTreeNodeChildrens(this);
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
            string appName = string.Empty;
            EnvApplication app = new EnvApplication();
            if (GingerCore.General.GetInputWithValidation("Add Application", "Application Name:", ref appName, null, false, app))
            {
                app.Name= appName;
                ProjEnvironment.Applications.Add(app);
            }
        }
    }
}
