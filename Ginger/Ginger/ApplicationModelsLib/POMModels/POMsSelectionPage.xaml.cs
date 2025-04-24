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
using Ginger.SolutionWindows.TreeViewItems.ApplicationModelsTreeItems;
using Ginger.UserControls;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Ginger.ApplicationModelsLib.POMModels
{
    /// <summary>
    /// Interaction logic for POMsSelectionPage.xaml
    /// </summary>
    public partial class POMsSelectionPage : Page
    {
        public ObservableList<POMBindingObjectHelper> PomModels = [];
        SingleItemTreeViewSelectionPage mApplicationPOMSelectionPage = null;

        public delegate void POMSelectionEventHandler(object sender, Guid guid);

        public event POMSelectionEventHandler POMSelectionEvent;

        public Window OwnerWindow { get; set; }

        ITreeViewItem mItemTypeRootNode;
        public bool ShowTitle { get; set; }

        /// <summary>
        /// This event is used to raise object
        /// </summary>
        public void POMSelectedEvent(Guid guid)
        {
            if (POMSelectionEvent != null)
            {
                POMSelectionEvent?.Invoke(this, guid);
            }
        }

        /// <summary>
        /// Ctor for default settings
        /// </summary>
        public POMsSelectionPage()
        {
            InitializeComponent();
            SetPOMGridView();
        }

        /// <summary>
        /// This method is used to set the columns POM GridView
        /// </summary>
        private void SetPOMGridView()
        {
            xGridPOMListItems.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(POMBindingObjectHelper.ContainingFolder), Header = "Folder", WidthWeight = 100, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true },
                new GridColView() { Field = nameof(POMBindingObjectHelper.ItemName), Header = "Name", WidthWeight = 150, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true },
            ]
            };
            WeakEventManager<ButtonBase, RoutedEventArgs>.RemoveHandler(source: xGridPOMListItems.btnAdd, eventName: nameof(ButtonBase.Click), handler: BtnAdd_Click);
            WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: xGridPOMListItems.btnAdd, eventName: nameof(ButtonBase.Click), handler: BtnAdd_Click);

            xGridPOMListItems.SetAllColumnsDefaultView(view);
            xGridPOMListItems.InitViewItems();
            PomModels = [];
            xGridPOMListItems.DataSourceList = PomModels;
            xGridPOMListItems.Title = "Selected POM's";
            xGridPOMListItems.ShowTitle = ShowTitle ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// This event is used to open the popup for POM selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (mApplicationPOMSelectionPage == null)
            {
                ApplicationPOMsTreeItem appModelFolder;
                RepositoryFolder<ApplicationPOMModel> repositoryFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationPOMModel>();
                appModelFolder = new ApplicationPOMsTreeItem(repositoryFolder);
                mItemTypeRootNode = appModelFolder;
                mApplicationPOMSelectionPage = new SingleItemTreeViewSelectionPage("Page Objects Model Element", eImageType.ApplicationPOMModel, appModelFolder,
                                                                                   SingleItemTreeViewSelectionPage.eItemSelectionType.MultiStayOpenOnDoubleClick, false);

                mItemTypeRootNode.SetTools(mApplicationPOMSelectionPage.xTreeView);
                mApplicationPOMSelectionPage.xTreeView.SetTopToolBarTools(appModelFolder.SaveAllTreeFolderItemsHandler, appModelFolder.AddPOM, RefreshTreeItems);

                WeakEventManager<SingleItemTreeViewSelectionPage, SelectionTreeEventArgs>.AddHandler(source: mApplicationPOMSelectionPage, eventName: nameof(SingleItemTreeViewSelectionPage.SelectionDone), handler: MAppModelSelectionPage_SelectionDone);
            }

            List<object> selectedPOMs = mApplicationPOMSelectionPage.ShowAsWindow(ownerWindow: OwnerWindow);
            AddSelectedPOM(selectedPOMs);
        }

        public void RefreshTreeItems(object sender, RoutedEventArgs e)
        {
            UpdatePOMTree();
        }

        private void UpdatePOMTree()
        {
            mApplicationPOMSelectionPage.xTreeView.Tree.SelectItem(mItemTypeRootNode);
            mApplicationPOMSelectionPage.xTreeView.Tree.RefreshTreeNodeChildrens(mItemTypeRootNode);
        }

        /// <summary>
        /// This event is used to add the selected POM to grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MAppModelSelectionPage_SelectionDone(object sender, SelectionTreeEventArgs e)
        {
            AddSelectedPOM(e.SelectedItems);
        }

        /// <summary>
        /// This method is used to add the selected POM
        /// </summary>
        /// <param name="selectedPOMs"></param>
        private void AddSelectedPOM(List<object> selectedPOMs)
        {
            if (selectedPOMs != null && selectedPOMs.Count > 0)
            {
                foreach (ApplicationPOMModel pom in selectedPOMs)
                {
                    if (!IsPOMAlreadyAdded(pom.ItemName))
                    {
                        PomModels.Add(new POMBindingObjectHelper() { IsChecked = true, ItemName = pom.ItemName, ContainingFolder = pom.ContainingFolder, ItemObject = pom });
                        POMSelectedEvent(pom.Guid);
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKey.StaticInfoMessage, @"""" + pom.ItemName + @""" POM is already added!");
                    }
                }
                xGridPOMListItems.DataSourceList = PomModels;
            }
        }

        /// <summary>
        /// This method is used to check if the POM is added or not
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        private bool IsPOMAlreadyAdded(string itemName)
        {
            bool isPresent = false;
            if (PomModels != null && PomModels.Count > 0)
            {
                var obj = PomModels.FirstOrDefault(x => x.ItemName == itemName);
                if (obj != null)
                {
                    isPresent = !string.IsNullOrEmpty(Convert.ToString(obj.ItemName));
                }
            }
            return isPresent;
        }
    }
}
