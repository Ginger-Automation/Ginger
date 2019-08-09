#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Ginger.BusinessFlowPages;
using Ginger.SolutionWindows.TreeViewItems.ApplicationModelsTreeItems;
using Ginger.UserControls;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.Actions._Common.ActUIElementLib
{
    /// <summary>
    /// Interaction logic for POMElementGridSelectionPage.xaml
    /// </summary>
    public partial class POMElementGridSelectionPage : Page
    {
        public ObservableList<POMBindingObjectHelper> PomModels = new ObservableList<POMBindingObjectHelper>();
        SingleItemTreeViewSelectionPage mApplicationPOMSelectionPage = null;

        public delegate void POMSelectionEventHandler(object sender, string guid);

        public event POMSelectionEventHandler POMSelectionEvent;

        public bool ShowTitle { get; set; }

        /// <summary>
        /// This event is used to raise object
        /// </summary>
        public void POMSelectedEvent(string guid)
        {
            if (POMSelectionEvent != null)
            {
                POMSelectionEvent?.Invoke(this, guid);
            }
        }
        
        /// <summary>
        /// Ctor for default settings
        /// </summary>
        public POMElementGridSelectionPage()
        {
            InitializeComponent();
            SetPOMGridView();            
        }

        /// <summary>
        /// This method is used to set the columns POM GridView
        /// </summary>
        private void SetPOMGridView()
        {
            gridPOMListItems.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(POMBindingObjectHelper.ContainingFolder), Header = "Path", WidthWeight = 100, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(POMBindingObjectHelper.ItemName), Header = "Name", WidthWeight = 150, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            gridPOMListItems.btnAdd.Click -= BtnAdd_Click;
            gridPOMListItems.btnAdd.Click += BtnAdd_Click;
            gridPOMListItems.SetAllColumnsDefaultView(view);
            gridPOMListItems.InitViewItems();
            PomModels = new ObservableList<POMBindingObjectHelper>();
            gridPOMListItems.DataSourceList = PomModels;
            gridPOMListItems.Title = "Selected POM's";
            gridPOMListItems.ShowTitle = ShowTitle ? Visibility.Visible : Visibility.Collapsed;
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
                mApplicationPOMSelectionPage = new SingleItemTreeViewSelectionPage("Page Objects Model Element", eImageType.ApplicationPOMModel, appModelFolder,
                                                                                        SingleItemTreeViewSelectionPage.eItemSelectionType.MultiStayOpenOnDoubleClick, false);
                mApplicationPOMSelectionPage.SelectionDone += MAppModelSelectionPage_SelectionDone;
            }

            List<object> selectedPOMs = mApplicationPOMSelectionPage.ShowAsWindow();
            AddSelectedPOM(selectedPOMs);
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
                        ApplicationPOMModel pomToAdd = (ApplicationPOMModel)pom.CreateCopy(false);
                        PomModels.Add(new POMBindingObjectHelper() { IsChecked = true, ItemName = pomToAdd.ItemName, ContainingFolder = pom.ContainingFolder, ItemObject = pom });
                        POMSelectedEvent(Convert.ToString(pom.Guid));
                    }
                    else
                    {
                        MessageBox.Show(@"""" + pom.ItemName + @""" POM is already added!", "Alert Message");
                    }                    
                }
                gridPOMListItems.DataSourceList = PomModels;                
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
                foreach (var item in PomModels)
                {
                    if (item.ItemName == itemName)
                    {
                        isPresent = true;
                        break;
                    }
                }
            }
            return isPresent;
        }
    }
}
