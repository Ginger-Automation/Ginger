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
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.WebServices.WebAPI;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.ApplicationsModels.ModelsUsages
{
    public partial class ModelItemUsagePage : Page
    {
        GenericWindow _pageGenericWin = null;
        private ApplicationModelBase mModelItem;
        ObservableList<ModelItemUsage> ModelItemUsages = new ObservableList<ModelItemUsage>();
        ApplicationModelBase.eModelUsageUpdateType mUsageUpdateType;
        ApplicationModelBase.eModelParts mModelPart;

        public ModelItemUsagePage(ApplicationModelBase modelItem, ApplicationModelBase.eModelUsageUpdateType usageUpdateType, ApplicationModelBase.eModelParts modelPart)
        {
            InitializeComponent();
            mModelItem = modelItem;
            mUsageUpdateType = usageUpdateType;
            mModelPart = modelPart;

            FindUsages();
            usageGrid.DataSourceList = ModelItemUsages;
            SetUsagesGridView();
        }

        private void SetUsagesGridView()
        {
            usageGrid.ShowTitle = Visibility.Collapsed;

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = ModelItemUsage.Fields.Selected, StyleType = GridColView.eGridColStyleType.CheckBox, WidthWeight = 10 });
            view.GridColsView.Add(new GridColView() { Field = ModelItemUsage.Fields.HostBizFlowPath, Header = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), WidthWeight = 25, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = ModelItemUsage.Fields.HostActivityName, Header = GingerDicser.GetTermResValue(eTermResKey.Activity), WidthWeight = 25, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = ModelItemUsage.Fields.UsageItemName, Header = "Action Description", WidthWeight = 25, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = ModelItemUsage.Fields.UsageExtraDetails, Header = "Usage Extra Details", WidthWeight = 20, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = ModelItemUsage.Fields.SelectedItemPart, Header = "Part to Update ", StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(ModelItemUsage.Fields.ItemParts, ModelItemUsage.Fields.SelectedItemPart, false), WidthWeight = 20 });
            view.GridColsView.Add(new GridColView() { Field = ModelItemUsage.Fields.Status, WidthWeight = 15, ReadOnly = true });

            usageGrid.SetAllColumnsDefaultView(view);
            usageGrid.InitViewItems();

            usageGrid.AddToolbarTool("@Checkbox_16x16.png", "Select / Un-Select All", new RoutedEventHandler(SelectUnSelectAll));
            usageGrid.AddToolbarTool("@DropDownList_16x16.png", "Set Same Selected Part to All", new RoutedEventHandler(SetSamePartToAll));
        }

        private async void FindUsages()
        {
            try
            {
                xProcessingImage.Visibility = Visibility.Visible;

                ObservableList<BusinessFlow> BizFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
                await Task.Run(() =>
                {
                    foreach (BusinessFlow BF in BizFlows)
                    {
                        if (mModelItem is ApplicationAPIModel)
                        {
                            foreach (Activity activity in BF.Activities)
                                foreach (Act act in activity.Acts)
                                {
                                    if (act is ActWebAPIModel && ((ActWebAPIModel)act).APImodelGUID == mModelItem.Guid)
                                    {
                                        ModelItemUsage itemUsage = new ModelItemUsage() { HostBusinessFlow = BF, HostBizFlowPath = Path.Combine(BF.ContainingFolder,BF.Name), HostActivityName = activity.ActivityName, HostActivity = activity, Action = act, UsageItem = act, UsageItemName = act.Description, Selected = true/*, UsageExtraDetails = "Number of " + GingerDicser.GetTermResValue(eTermResKey.Activities) + ": " + act.ActivitiesIdentifiers.Count().ToString()*/, Status = ModelItemUsage.eStatus.NotUpdated };
                                        if (mUsageUpdateType == ApplicationModelBase.eModelUsageUpdateType.SinglePart)
                                        {
                                            if (mModelPart == ApplicationModelBase.eModelParts.ReturnValues)
                                                itemUsage.SetItemPartesFromEnum(typeof(ActReturnValue.eItemParts));
                                        }
                                        else if (mUsageUpdateType == ApplicationModelBase.eModelUsageUpdateType.MultiParts)
                                            itemUsage.SetItemPartesFromEnum(typeof(ApplicationModelBase.eModelParts));

                                        ModelItemUsages.Add(itemUsage);
                                    }
                                }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.GetModelItemUsagesFailed, mModelItem.GetNameForFileName(), ex.Message);
            }
            finally
            {
                xProcessingImage.Visibility = Visibility.Collapsed;
            }
        }

        private void SelectUnSelectAll(object sender, RoutedEventArgs e)
        {
            bool selectValue = false;
            if (ModelItemUsages.Count > 0)
            {
                selectValue = !ModelItemUsages[0].Selected;//decide if to take or not
                foreach (ModelItemUsage usage in ModelItemUsages)
                    usage.Selected = selectValue;
            }
        }

        private void SetSamePartToAll(object sender, RoutedEventArgs e)
        {
            if (usageGrid.CurrentItem != null)
            {
                ModelItemUsage a = (ModelItemUsage)usageGrid.CurrentItem;
                foreach (ModelItemUsage usage in ModelItemUsages)
                    usage.SelectedItemPart = a.SelectedItemPart;
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectItem);
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button UpdateAllButton = new Button();
            UpdateAllButton.Content = "Update All Selected";
            UpdateAllButton.Click += new RoutedEventHandler(UpdateButton_Click);

            Button SaveAllBizFlowsButton = new Button();
            SaveAllBizFlowsButton.Content = "Save All Updated Usages";
            SaveAllBizFlowsButton.Click += new RoutedEventHandler(SaveAllBizFlowsButton_Click);

            ObservableList<Button> winButtons = new ObservableList<Button>();
            winButtons.Add(SaveAllBizFlowsButton);
            winButtons.Add(UpdateAllButton);

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, "'" + mModelItem.Name + "' " + " Model Item Usages", this, winButtons, true, "Close");
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                xProcessingImage.Visibility = Visibility.Visible;

                await Task.Run(() =>
                {
                    foreach (ModelItemUsage usage in ModelItemUsages)
                    {
                        if (usage.Selected && usage.Status == ModelItemUsage.eStatus.NotUpdated || usage.Status == ModelItemUsage.eStatus.UpdateFailed)
                            usage.Status = ModelItemUsage.eStatus.Pending;
                    }

                    //do the update
                    foreach (ModelItemUsage usage in ModelItemUsages)
                    {
                        try
                        {
                            if (usage.Status == ModelItemUsage.eStatus.Pending)
                            {
                                UpdateAllReturnValuesInAction(usage.Action, mModelItem, usage);
                                DeleteOldReturnValuesInAction(usage.Action, mModelItem);
                            }
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Failed to update the model item usage", ex);
                            usage.Status = ModelItemUsage.eStatus.UpdateFailed;
                        }
                    }
                });
            }
            finally
            {
                xProcessingImage.Visibility = Visibility.Collapsed;
            }
        }


        private static void UpdateAllReturnValuesInAction(Act act, ApplicationModelBase apiModel, ModelItemUsage usage)
        {
            ActReturnValue.eItemParts ePartToUpdate = (ActReturnValue.eItemParts)Enum.Parse(typeof(ActReturnValue.eItemParts), usage.SelectedItemPart);
            foreach (ActReturnValue apiARV in apiModel.ReturnValues)
            {
                ActReturnValue actARV = act.ActReturnValues.Where(x => x.Guid == apiARV.Guid).FirstOrDefault();
                if (actARV != null) //Exist already in the action - Update it
                {
                    actARV.Active = apiARV.Active;
                    actARV.Status = ActReturnValue.eStatus.Pending;

                    if (ePartToUpdate == ActReturnValue.eItemParts.ExpectedValue || ePartToUpdate == ActReturnValue.eItemParts.All)
                        actARV.Expected = apiARV.Expected;
                    if (ePartToUpdate == ActReturnValue.eItemParts.Parameter || ePartToUpdate == ActReturnValue.eItemParts.All)
                        actARV.ItemName = apiARV.ItemName;
                    if (ePartToUpdate == ActReturnValue.eItemParts.Path || ePartToUpdate == ActReturnValue.eItemParts.All)
                        actARV.Path = apiARV.Path;
                    if (ePartToUpdate == ActReturnValue.eItemParts.SimulatedActual || ePartToUpdate == ActReturnValue.eItemParts.All)
                        actARV.SimulatedActual = apiARV.SimulatedActual;
                    if (ePartToUpdate == ActReturnValue.eItemParts.StoreTo || ePartToUpdate == ActReturnValue.eItemParts.All)
                        if (!string.IsNullOrEmpty(apiARV.StoreToValue))
                        {
                            actARV.StoreTo = ActReturnValue.eStoreTo.ApplicationModelParameter;
                            actARV.StoreToValue = apiARV.StoreToValue;
                        }

                    usage.Status = ModelItemUsage.eStatus.Updated;

                }
                else //Not exist in the action - add it
                {
                    if (!string.IsNullOrEmpty(apiARV.StoreToValue))
                        apiARV.StoreTo = ActReturnValue.eStoreTo.ApplicationModelParameter;
                    AddNewActReturnValue(act, apiARV);
                }
            }
        }

        private static void AddNewActReturnValue(Act act, ActReturnValue apiARV)
        {
            ActReturnValue newARV = new ActReturnValue();
            newARV.AddedAutomatically = true;
            newARV.Status = ActReturnValue.eStatus.Pending;
            newARV.Guid = apiARV.Guid;
            newARV.Active = apiARV.Active;
            newARV.ItemName = apiARV.ItemName;
            newARV.Path = apiARV.Path;
            newARV.Expected = apiARV.Expected;
            newARV.StoreTo = apiARV.StoreTo;
            newARV.SimulatedActual = apiARV.SimulatedActual;
            act.ActReturnValues.Add(newARV);
        }

        private static void DeleteOldReturnValuesInAction(Act act, ApplicationModelBase apiModel)
        {
            for (int index = 0; index < act.ReturnValues.Count; index++)
            {
                if (act.ReturnValues[index].AddedAutomatically == true)
                {
                    ActReturnValue apiARV = apiModel.ReturnValues.Where(x => x.Guid == act.ReturnValues[index].Guid).FirstOrDefault();
                    if (apiARV == null) //Output value deleted from API - delete it also from action
                    {
                        act.ReturnValues.RemoveAt(index);
                        index--;
                    }
                }
            }
        }

        private async void SaveAllBizFlowsButton_Click(object sender, RoutedEventArgs e)
        {
            xProcessingImage.Visibility = Visibility.Visible;

            try
            {
                await Task.Run(() =>
                {

                    foreach (ModelItemUsage usage in ModelItemUsages)
                    {
                        if (usage.Status == ModelItemUsage.eStatus.Updated || usage.Status == ModelItemUsage.eStatus.SaveFailed)
                        {
                            try
                            {                                
                                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(usage.HostBusinessFlow);
                                usage.Status = ModelItemUsage.eStatus.UpdatedAndSaved;
                            }
                            catch (Exception ex)
                            {
                                usage.Status = ModelItemUsage.eStatus.SaveFailed;
                                Reporter.HideStatusMessage();
                                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                            }
                        }
                    }
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mModelItem);
                });
            }
            finally
            {
                xProcessingImage.Visibility = Visibility.Collapsed;
            }
        }

    }
}
