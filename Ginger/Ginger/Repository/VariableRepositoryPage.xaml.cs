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

        public VariablesRepositoryPage(RepositoryFolder<VariableBase> variablesFolder)
        {
            InitializeComponent();

            mVariablesFolder = variablesFolder;
            SetVariablesGridView();
            SetGridAndTreeData();
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
            viewCols.Add(new GridColView() { Field = VariableBase.Fields.Name, WidthWeight = 50, AllowSorting = true });
            viewCols.Add(new GridColView() { Field = VariableBase.Fields.Description, WidthWeight = 35, AllowSorting = true });
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
                    App.BusinessFlow.AddVariable((VariableBase)selectedItem.CreateInstance(true));
                }                    
                
                int selectedActIndex = -1;
                ObservableList<VariableBase> varList = App.BusinessFlow.Variables;
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
                Reporter.ToUser(eUserMsgKeys.NoItemWasSelected);
        }

        private void EditVar(object sender, RoutedEventArgs e)
        {
            if (xVariablesGrid.CurrentItem != null && xVariablesGrid.CurrentItem.ToString() != "{NewItemPlaceholder}")
            {
                VariableBase selectedVarb = (VariableBase)xVariablesGrid.CurrentItem;
                selectedVarb.NameBeforeEdit = selectedVarb.Name;

                VariableEditPage w = new VariableEditPage(selectedVarb, false, VariableEditPage.eEditMode.SharedRepository);
                w.ShowAsWindow(eWindowShowStyle.Dialog);

            }
            else
            {
                Reporter.ToUser(eUserMsgKeys.AskToSelectVariable);
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
                Reporter.ToUser(eUserMsgKeys.NoItemWasSelected);
        }

        private void grdVariables_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DragInfo.DataIsAssignableToType(typeof(VariableBase)))
            {
                // OK to drop                         
                DragDrop2.DragInfo.DragIcon = GingerWPF.DragDropLib.DragInfo.eDragIcon.Copy;
            }
        }

        private void grdVariables_ItemDropped(object sender, EventArgs e)
        {
            VariableBase dragedItem = (VariableBase)((DragInfo)sender).Data;
            if (dragedItem != null)
            {
                //App.LocalRepository.AddItemToRepositoryWithPreChecks(dragedItem, null);

                SharedRepositoryOperations.AddItemToRepository(dragedItem);

                //refresh and select the item
                try
                {
                    VariableBase dragedItemInGrid = ((IEnumerable<VariableBase>)xVariablesGrid.DataSourceList).Where(x => x.Guid == dragedItem.Guid).FirstOrDefault();
                    if (dragedItemInGrid != null)
                        xVariablesGrid.Grid.SelectedItem = dragedItemInGrid;
                }
                catch (Exception ex) { Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}"); }
            }
        }

        private void grdVariables_grdMain_MouseDoubleClick(object sender, EventArgs e)
        {
            EditVar(sender, new RoutedEventArgs());
        }
    }
}
