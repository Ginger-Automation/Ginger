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
using Ginger.Actions;
using Ginger.BusinessFlowFolder;
using GingerWPF.DragDropLib;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;

namespace Ginger.Repository
{
    /// <summary>
    /// Interaction logic for ActionsRepositoryPage.xaml
    /// </summary>
    public partial class ActionsRepositoryPage : Page
    {
        readonly RepositoryFolder<Act> mActionsFolder;

        public ActionsRepositoryPage(RepositoryFolder<Act> actionsFolder)
        {
            InitializeComponent();

            mActionsFolder = actionsFolder;
            SetActionsGridView();
            SetGridAndTreeData();
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
            if (DragDrop2.DragInfo.DataIsAssignableToType(typeof(Act)))
            {
                // OK to drop                         
                DragDrop2.DragInfo.DragIcon = GingerWPF.DragDropLib.DragInfo.eDragIcon.Copy;
            }
        }

        private void grdActions_ItemDropped(object sender, EventArgs e)
        {
            Act dragedItem = (Act)((DragInfo)sender).Data;
            if (dragedItem != null)
            {
                // App.LocalRepository.AddItemToRepositoryWithPreChecks(dragedItem, null);
                SharedRepositoryOperations.AddItemToRepository(dragedItem);
                //refresh and select the item
                try
               {
                   xActionsGrid.DataSourceList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Act>();

                    Act dragedItemInGrid = ((IEnumerable<Act>)xActionsGrid.DataSourceList).Where(x => x.Guid == dragedItem.Guid).FirstOrDefault();
                   if (dragedItemInGrid != null)
                       xActionsGrid.Grid.SelectedItem = dragedItemInGrid;
               }
               catch (Exception ex) { Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}"); }
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
            if (xActionsGrid.Grid.SelectedItems != null && xActionsGrid.Grid.SelectedItems.Count > 0)
            {
                foreach (Act selectedItem in xActionsGrid.Grid.SelectedItems)
                {
                    App.BusinessFlow.AddAct((Act)selectedItem.CreateInstance(true));
                }
                
                int selectedActIndex = -1;
                ObservableList<Act> actsList = App.BusinessFlow.CurrentActivity.Acts;
                if (actsList.CurrentItem != null)
                {
                    selectedActIndex = actsList.IndexOf((Act)actsList.CurrentItem);
                }
                if (selectedActIndex >= 0)
                {
                    actsList.Move(actsList.Count - 1, selectedActIndex + 1);

                }
            }
            else
                Reporter.ToUser(eUserMsgKeys.NoItemWasSelected);                      
        }
        
        private void EditAction(object sender, RoutedEventArgs e)
        {
            if (xActionsGrid.CurrentItem != null)
            {
                Act a = (Act)xActionsGrid.CurrentItem;
                ActionEditPage actedit = new ActionEditPage(a, General.RepositoryItemPageViewMode.SharedReposiotry, new GingerCore.BusinessFlow(), new GingerCore.Activity());
                actedit.ShowAsWindow(eWindowShowStyle.Dialog);             
            }
            else
            {
                Reporter.ToUser(eUserMsgKeys.AskToSelectItem);
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
                Reporter.ToUser(eUserMsgKeys.NoItemWasSelected);             
        }
        
        private void grdActions_grdMain_MouseDoubleClick(object sender, EventArgs e)
        {
            EditAction(sender, new RoutedEventArgs());
        }
    }
}
