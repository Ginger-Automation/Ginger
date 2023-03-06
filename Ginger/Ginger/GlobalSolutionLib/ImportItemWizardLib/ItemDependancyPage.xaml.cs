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
using Amdocs.Ginger.Common.GlobalSolutionLib;
using Amdocs.Ginger.CoreNET.GlobalSolutionLib;
using Amdocs.Ginger.Repository;
using Ginger.Actions;
using Ginger.UserControls;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace Ginger.GlobalSolutionLib.ImportItemWizardLib
{
    /// <summary>
    /// Interaction logic for SelectItemImportTypePage.xaml
    /// </summary>
    public partial class ItemDependancyPage : Page, IWizardPage
    {
        ImportItemWizard wiz;
        NewRepositorySerializer newRepositorySerializer = new NewRepositorySerializer();

        public ItemDependancyPage()
        {
            InitializeComponent();
        }

        public async void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    wiz = (ImportItemWizard)WizardEventArgs.Wizard;

                    break;
                case EventType.Active:
                    ((WizardWindow)wiz.mWizardWindow).ShowFinishButton(true);

                    SetDependantItemsListToImportGridView();
                    xDependantItemsToImportGrid.DataSourceList = wiz.SelectedItemsListToImport;
                    await GetSelectedItemsListToImport();
                    break;
                default:
                    //Nothing to do
                    break;
            }
        }


        private void SetDependantItemsListToImportGridView()
        {
            //Set the Data Grid columns            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.Selected), Header = "Select", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.ItemType), Header = "Item Type", WidthWeight = 50, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.ItemName), Header = "Item Name", WidthWeight = 50, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.RequiredFor), Header = "Dependency For", WidthWeight = 50, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.ItemImportSetting), Header = "Import Setting", WidthWeight = 30, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.Comments), Header = "Comments", WidthWeight = 120, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalSolutionItem.ItemExtraInfo), Header = "Item Full Path", WidthWeight = 150, ReadOnly = true });

            xDependantItemsToImportGrid.SetAllColumnsDefaultView(view);
            xDependantItemsToImportGrid.InitViewItems();

            xDependantItemsToImportGrid.SetBtnImage(xDependantItemsToImportGrid.btnMarkAll, "@CheckAllColumn_16x16.png");
            xDependantItemsToImportGrid.btnMarkAll.Visibility = Visibility.Visible;
            xDependantItemsToImportGrid.MarkUnMarkAllActive += MarkUnMarkAllItems;
        }

        public async Task GetSelectedItemsListToImport()
        {
            wiz.SelectedItemsListToImport.ClearAll();
            try
            {
                wiz.ProcessStarted();
                await Task.Run(() =>
                {
                    if (wiz.ItemsListToImport.Where(x => x.Selected).ToList().Count > 0 )
                    {
                        foreach (GlobalSolutionItem item in wiz.ItemsListToImport.Where(x => x.Selected).ToList())
                        {
                            switch (item.ItemType)
                            {
                                case GlobalSolution.eImportItemType.Documents:
                                    GlobalSolutionUtils.Instance.AddItemToSelectedItemsList(item, ref wiz.SelectedItemsListToImport);
                                    break;
                                case GlobalSolution.eImportItemType.DataSources:
                                    GlobalSolutionUtils.Instance.AddItemToSelectedItemsList(item, ref wiz.SelectedItemsListToImport);
                                    break;

                                case GlobalSolution.eImportItemType.Environments:
                                    GlobalSolutionUtils.Instance.AddItemToSelectedItemsList(item, ref wiz.SelectedItemsListToImport);
                                    GlobalSolutionUtils.Instance.AddDependaciesForEnvironment(item, ref wiz.SelectedItemsListToImport, ref wiz.VariableListToImport);
                                    break;
                                case GlobalSolution.eImportItemType.SharedRepositoryActivitiesGroup:
                                    GlobalSolutionUtils.Instance.AddItemToSelectedItemsList(item, ref wiz.SelectedItemsListToImport);
                                    GlobalSolutionUtils.Instance.AddDependaciesForSharedActivityGroup(item, ref wiz.SelectedItemsListToImport, ref wiz.VariableListToImport, ref wiz.EnvAppListToImport);
                                    break;
                                case GlobalSolution.eImportItemType.SharedRepositoryActivities:
                                    GlobalSolutionUtils.Instance.AddItemToSelectedItemsList(item, ref wiz.SelectedItemsListToImport);
                                    GlobalSolutionUtils.Instance.AddDependaciesForSharedActivity(item, ref wiz.SelectedItemsListToImport, ref wiz.VariableListToImport, ref wiz.EnvAppListToImport);
                                    break;
                                case GlobalSolution.eImportItemType.SharedRepositoryActions:
                                    GlobalSolutionUtils.Instance.AddItemToSelectedItemsList(item, ref wiz.SelectedItemsListToImport);
                                    GlobalSolutionUtils.Instance.AddDependaciesForSharedAction(item, ref wiz.SelectedItemsListToImport, ref wiz.VariableListToImport, ref wiz.EnvAppListToImport);
                                    break;
                                case GlobalSolution.eImportItemType.SharedRepositoryVariables:
                                    GlobalSolutionUtils.Instance.AddItemToSelectedItemsList(item, ref wiz.SelectedItemsListToImport);
                                    break;
                                case GlobalSolution.eImportItemType.APIModels:
                                    GlobalSolutionUtils.Instance.AddItemToSelectedItemsList(item, ref wiz.SelectedItemsListToImport);
                                    GlobalSolutionUtils.Instance.AddDependaciesForAPIModel(item, ref wiz.SelectedItemsListToImport, ref wiz.VariableListToImport, ref wiz.EnvAppListToImport);
                                    break;
                                case GlobalSolution.eImportItemType.POMModels:
                                    GlobalSolutionUtils.Instance.AddItemToSelectedItemsList(item, ref wiz.SelectedItemsListToImport);
                                    GlobalSolutionUtils.Instance.AddDependaciesForPOMModel(item, ref wiz.SelectedItemsListToImport, ref wiz.VariableListToImport, ref wiz.EnvAppListToImport);
                                    break;
                                case GlobalSolution.eImportItemType.BusinessFlows:
                                    GlobalSolutionUtils.Instance.AddItemToSelectedItemsList(item, ref wiz.SelectedItemsListToImport);
                                    GlobalSolutionUtils.Instance.AddDependaciesForBusinessFlows(item, ref wiz.SelectedItemsListToImport, ref wiz.VariableListToImport, ref wiz.EnvAppListToImport);
                                    break;
                                case GlobalSolution.eImportItemType.Agents:
                                    GlobalSolutionUtils.Instance.AddItemToSelectedItemsList(item, ref wiz.SelectedItemsListToImport);
                                    GlobalSolutionUtils.Instance.AddDependaciesForAgents(item, ref wiz.SelectedItemsListToImport, ref wiz.VariableListToImport, ref wiz.EnvAppListToImport);
                                    break;
                                default:
                                    GlobalSolutionUtils.Instance.AddItemToSelectedItemsList(item, ref wiz.SelectedItemsListToImport);
                                    break;

                            }
                        }
                    }

                    foreach (GlobalSolutionItem gsi in wiz.SelectedItemsListToImport)
                    {
                        gsi.PropertyChanged -= Item_PropertyChanged;
                        gsi.PropertyChanged += Item_PropertyChanged;
                    }
                });
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
            finally
            {
                wiz.ProcessEnded();
            }
        }

        private void MarkUnMarkAllItems(bool ActiveStatus)
        {
            foreach (GlobalSolutionItem item in xDependantItemsToImportGrid.DataSourceList)
            {
                if (!string.IsNullOrEmpty(item.RequiredFor))
                {
                    continue;
                }
                item.Selected = ActiveStatus;
            }
        }
        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            xInfoMessageLabel.Content = "";
            GlobalSolutionItem solutionItem = (GlobalSolutionItem)sender;
            if (!string.IsNullOrEmpty(solutionItem.RequiredFor))
            {
                if (wiz.SelectedItemsListToImport.Where(x => x.ItemName == solutionItem.RequiredFor).FirstOrDefault().Selected && solutionItem.ItemImportSetting == GlobalSolution.eImportSetting.New)
                {
                    solutionItem.Selected = true;
                    xInfoMessageLabel.Content = "Dependant items with import setting as 'New' can not be unchecked as it must be imported.";
                    return;
                }
                else
                {
                    CheckUncheckDependantItems(solutionItem);
                }
            }
            else 
            {
                CheckUncheckDependantItems(solutionItem);
            }
        }

        void CheckUncheckDependantItems(GlobalSolutionItem solutionItem)
        {
            foreach (GlobalSolutionItem item in wiz.SelectedItemsListToImport.Where(x => x.RequiredFor == solutionItem.ItemName))
            {
                item.Selected = solutionItem.Selected;
                CheckUncheckDependantItems(item);
            }
        }
    }
}
