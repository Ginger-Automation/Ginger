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
using Amdocs.Ginger.Repository;
using Ginger.Actions;
using Ginger.BusinessFlowPages;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerWPF.DragDropLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Repository
{
    /// <summary>
    /// Interaction logic for ActionsRepositoryPage.xaml
    /// </summary>
    public partial class ActionsRepositoryPage : Page
    {
        readonly RepositoryFolder<Act> mActionsFolder;
        Context mContext;

        public ActionsRepositoryPage(RepositoryFolder<Act> actionsFolder, Context context)
        {
            InitializeComponent();

            mActionsFolder = actionsFolder;
            mContext = context;

            SetActionsGridView();
            SetGridAndTreeData();
        }

        public void UpdateBusinessFlow(BusinessFlow bf)
        {
            xActionsGrid.ClearFilters();
        }


        private void SetGridAndTreeData()
        {
            if (mActionsFolder.IsRootFolder)
            {
                xActionsGrid.DataSourceList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Act>();
            }                
            else
            {
                xActionsGrid.DataSourceList = mActionsFolder.GetFolderItems();
            }                
        }

        private void grdActions_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DrgInfo.DataIsAssignableToType(typeof(Act)))
            {
                // OK to drop
                DragDrop2.SetDragIcon(true);
            }
            else
            {
                // Do Not Drop
                DragDrop2.SetDragIcon(false);
            }
        }

        private void grdActions_ItemDropped(object sender, EventArgs e)
        {
            Act dragedItem = (Act)((DragInfo)sender).Data;
            if (dragedItem != null)
            {
                // App.LocalRepository.AddItemToRepositoryWithPreChecks(dragedItem, null);
                (new SharedRepositoryOperations()).AddItemToRepository(mContext, dragedItem);
                //refresh and select the item
                try
               {
                   xActionsGrid.DataSourceList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Act>();

                    Act dragedItemInGrid = ((IEnumerable<Act>)xActionsGrid.DataSourceList).Where(x => x.Guid == dragedItem.Guid).FirstOrDefault();
                   if (dragedItemInGrid != null)
                       xActionsGrid.Grid.SelectedItem = dragedItemInGrid;
               }
               catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
            }
        }


        private void SetActionsGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = Act.Fields.Description, WidthWeight = 85, AllowSorting=true });         
            view.GridColsView.Add(new GridColView() { Field = "Inst.", WidthWeight = 15, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["ViewInstancesButton"] });
            xActionsGrid.SetAllColumnsDefaultView(view);
            xActionsGrid.InitViewItems();

            xActionsGrid.btnRefresh.Visibility = Visibility.Collapsed;
            xActionsGrid.AddToolbarTool("@LeftArrow_16x16.png", "Add to Actions", new RoutedEventHandler(AddFromRepository));
            xActionsGrid.AddToolbarTool("@Edit_16x16.png", "Edit Item", new RoutedEventHandler(EditAction));
            xActionsGrid.ShowTagsFilter = Visibility.Visible;
            xActionsGrid.RowDoubleClick += grdActions_grdMain_MouseDoubleClick;
            xActionsGrid.ItemDropped += grdActions_ItemDropped;
            xActionsGrid.PreviewDragItem += grdActions_PreviewDragItem;                     
        }
        

        
        private void AddFromRepository(object sender, RoutedEventArgs e)
        {            
            if (mContext.BusinessFlow == null)
            {
                return;
            }

            if (xActionsGrid.Grid.SelectedItems != null && xActionsGrid.Grid.SelectedItems.Count > 0)
            {
                foreach (Act selectedItem in xActionsGrid.Grid.SelectedItems)
                {
                    ActionsFactory.AddActionsHandler(selectedItem, mContext);
                    //mContext.BusinessFlow.AddAct((Act)selectedItem.CreateInstance(true));
                }                
            }
            else
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);                      
        }
        
        private void EditAction(object sender, RoutedEventArgs e)
        {
            if (xActionsGrid.CurrentItem != null)
            {
                Act a = (Act)xActionsGrid.CurrentItem;
                ActionEditPage actedit = new ActionEditPage(a, General.eRIPageViewMode.SharedReposiotry, new GingerCore.BusinessFlow(), new GingerCore.Activity());
                actedit.ShowAsWindow(eWindowShowStyle.Dialog);             
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectItem);
            }
        }

        private void ViewRepositoryItemUsage(object sender, RoutedEventArgs e)
        {
            if (xActionsGrid.Grid.SelectedItem != null)
            {
                RepositoryItemUsagePage usagePage = new RepositoryItemUsagePage((RepositoryItemBase)xActionsGrid.Grid.SelectedItem);
                usagePage.ShowAsWindow();
            }
            else
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);             
        }
        
        private void grdActions_grdMain_MouseDoubleClick(object sender, EventArgs e)
        {
            EditAction(sender, new RoutedEventArgs());
        }
    }
}
