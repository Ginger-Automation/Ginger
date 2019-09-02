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

using Amdocs.Ginger.Common;
using GingerWPF.DragDropLib;
using Ginger.UserControls;
using Ginger.Variables;
using GingerCore;
using GingerCore.Variables;
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
    /// InterVariableBaseion logic for VariablesRepositoryPage.xaml
    /// </summary>    
    public partial class VariablesRepositoryPage : Page
    {
        readonly RepositoryFolder<VariableBase> mVariablesFolder;
        BusinessFlow mBusinessFlow;
        Context mContext = new Context();

        public VariablesRepositoryPage(RepositoryFolder<VariableBase> variablesFolder, BusinessFlow businessFlow)
        {
            InitializeComponent();

            mVariablesFolder = variablesFolder;
            mBusinessFlow = businessFlow;
            mContext.BusinessFlow = mBusinessFlow;

            SetVariablesGridView();
            SetGridAndTreeData();
        }

        public void UpdateBusinessFlow(BusinessFlow bf)
        {
            mBusinessFlow = bf;
            mContext.BusinessFlow = mBusinessFlow;
            xVariablesGrid.ClearFilters();
        }

        private void SetGridAndTreeData()
        {
            if (mVariablesFolder.IsRootFolder)
            {
                xVariablesGrid.DataSourceList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<VariableBase>();
            }                
            else
            {
                xVariablesGrid.DataSourceList = mVariablesFolder.GetFolderItems();
            }
            
        }


        private void SetVariablesGridView()
        {                        
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;
            viewCols.Add(new GridColView() { Field = nameof(VariableBase.Name), WidthWeight = 50, AllowSorting = true });
            viewCols.Add(new GridColView() { Field = nameof(VariableBase.Description), WidthWeight = 35, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = "Inst.", WidthWeight = 15, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["ViewInstancesButton"] });           
            xVariablesGrid.SetAllColumnsDefaultView(view);
            xVariablesGrid.InitViewItems();
            
            xVariablesGrid.AddToolbarTool("@LeftArrow_16x16.png", "Add to " + GingerDicser.GetTermResValue(eTermResKey.Variables), new RoutedEventHandler(AddFromRepository));
            xVariablesGrid.AddToolbarTool("@Edit_16x16.png", "Edit Item", new RoutedEventHandler(EditVar));
            xVariablesGrid.ShowTagsFilter = Visibility.Visible;
            
            xVariablesGrid.RowDoubleClick += grdVariables_grdMain_MouseDoubleClick;
            xVariablesGrid.ItemDropped += grdVariables_ItemDropped;
            xVariablesGrid.PreviewDragItem += grdVariables_PreviewDragItem;           
        }

        private void AddFromRepository(object sender, RoutedEventArgs e)
        {
            if (xVariablesGrid.Grid.SelectedItems != null && xVariablesGrid.Grid.SelectedItems.Count > 0)
            {
                foreach (VariableBase selectedItem in xVariablesGrid.Grid.SelectedItems)
                {
                    mBusinessFlow.AddVariable((VariableBase)selectedItem.CreateInstance(true));
                }                    
                
                int selectedActIndex = -1;
                ObservableList<VariableBase> varList = mBusinessFlow.Variables;
                if (varList.CurrentItem != null)
                {
                    selectedActIndex = varList.IndexOf((VariableBase)varList.CurrentItem);
                }
                if (selectedActIndex >= 0)
                {
                    varList.Move(varList.Count - 1, selectedActIndex + 1);
                }
            }
            else
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
        }

        private void EditVar(object sender, RoutedEventArgs e)
        {
            if (xVariablesGrid.CurrentItem != null && xVariablesGrid.CurrentItem.ToString() != "{NewItemPlaceholder}")
            {
                VariableBase selectedVarb = (VariableBase)xVariablesGrid.CurrentItem;
                selectedVarb.NameBeforeEdit = selectedVarb.Name;

                VariableEditPage w = new VariableEditPage(selectedVarb, mContext, false, VariableEditPage.eEditMode.SharedRepository);
                w.ShowAsWindow(eWindowShowStyle.Dialog);

            }
            else
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectVariable);
            }
        }

        private void ViewRepositoryItemUsage(object sender, RoutedEventArgs e)
        {
            if (xVariablesGrid.Grid.SelectedItem != null)
            {
                RepositoryItemUsagePage usagePage = new RepositoryItemUsagePage((RepositoryItemBase)xVariablesGrid.Grid.SelectedItem);
                usagePage.ShowAsWindow();
            }
            else
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
        }

        private void grdVariables_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DrgInfo.DataIsAssignableToType(typeof(VariableBase)))
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

        private void grdVariables_ItemDropped(object sender, EventArgs e)
        {
            VariableBase dragedItem = (VariableBase)((DragInfo)sender).Data;
            if (dragedItem != null)
            {
                (new SharedRepositoryOperations()).AddItemToRepository(mContext, dragedItem);

                //refresh and select the item
                try
                {
                    VariableBase dragedItemInGrid = ((IEnumerable<VariableBase>)xVariablesGrid.DataSourceList).Where(x => x.Guid == dragedItem.Guid).FirstOrDefault();
                    if (dragedItemInGrid != null)
                        xVariablesGrid.Grid.SelectedItem = dragedItemInGrid;
                }
                catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
            }
        }

        private void grdVariables_grdMain_MouseDoubleClick(object sender, EventArgs e)
        {
            EditVar(sender, new RoutedEventArgs());
        }
    }
}
