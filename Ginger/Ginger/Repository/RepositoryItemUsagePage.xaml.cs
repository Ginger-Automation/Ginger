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
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using Ginger.Activities;
using GingerCore.Activities;
using GingerCore.Variables;
using Amdocs.Ginger.Repository;
using amdocs.ginger.GingerCoreNET;
using System.IO;

namespace Ginger.Repository
{
    /// <summary>
    /// Interaction logic for RepositoryItemUsagePage.xaml
    /// </summary>
    public partial class RepositoryItemUsagePage : Page
    {
        GenericWindow _pageGenericWin = null;

        private readonly RepositoryItemBase mRepoItem;
        public ObservableList<RepositoryItemUsage> RepoItemUsages = new ObservableList<RepositoryItemUsage>();
        private bool mIncludeOriginal = false;
        private readonly RepositoryItemBase mOriginalItem;

        public RepositoryItemUsagePage(RepositoryItemBase repoItem, bool includeOriginal=true, RepositoryItemBase originalItem =null)
        {
            InitializeComponent();

            mRepoItem = repoItem;
            mIncludeOriginal = includeOriginal;
            if (originalItem != null)
                mOriginalItem = originalItem;
            else
                mOriginalItem = mRepoItem;

            FindUsages();

            usageGrid.DataSourceList = RepoItemUsages;
            SetUsagesGridView();
        }

        private void FindUsages()
        {
            try
            {
                //TODO: check that retrieve also sub folder business flows
                ObservableList<BusinessFlow> BizFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();

                foreach (BusinessFlow BF in BizFlows)
                {
                    string businessFlowName = Amdocs.Ginger.Common.GeneralLib.General.RemoveInvalidFileNameChars(BF.Name);
                    if (mRepoItem is Activity)
                    {
                        foreach (Activity a in BF.Activities)
                        {
                            if (a.ParentGuid == mRepoItem.Guid || a.Guid == mRepoItem.Guid ||
                                        (mRepoItem.ExternalID != null && mRepoItem.ExternalID != string.Empty && mRepoItem.ExternalID != "0" && a.ExternalID == mRepoItem.ExternalID))
                            {
                                //skip original if not needed
                                if (!mIncludeOriginal)
                                    if (a.Guid == mOriginalItem.Guid) continue;

                                Ginger.Repository.RepositoryItemUsage.eUsageTypes type;
                                if (a.Guid == mRepoItem.Guid)
                                    type = RepositoryItemUsage.eUsageTypes.Original;
                                else
                                    type = RepositoryItemUsage.eUsageTypes.Instance;

                                RepositoryItemUsage itemUsage = new RepositoryItemUsage() { HostBusinessFlow = BF, HostBizFlowPath = Path.Combine(BF.ContainingFolder, businessFlowName), UsageItem = a, UsageItemName = a.ActivityName, UsageExtraDetails = "Number of Actions: " + a.Acts.Count().ToString(), UsageItemType = type, Selected = true, Status = RepositoryItemUsage.eStatus.NotUpdated};
                                itemUsage.SetItemPartesFromEnum(typeof(eItemParts));
                                RepoItemUsages.Add(itemUsage);
                            }
                        }
                    }
                    else if (mRepoItem is ActivitiesGroup)
                    {
                        foreach (ActivitiesGroup a in BF.ActivitiesGroups)
                        {
                            if (a.ParentGuid == mRepoItem.Guid || a.Guid == mRepoItem.Guid ||
                                        (mRepoItem.ExternalID != null && mRepoItem.ExternalID != string.Empty && mRepoItem.ExternalID != "0" && a.ExternalID == mRepoItem.ExternalID))
                            {
                                //skip original if not needed
                                if (!mIncludeOriginal)
                                    if (a.Guid == mOriginalItem.Guid) continue;

                                Ginger.Repository.RepositoryItemUsage.eUsageTypes type;
                                if (a.Guid == mRepoItem.Guid)
                                    type = RepositoryItemUsage.eUsageTypes.Original;
                                else
                                    type = RepositoryItemUsage.eUsageTypes.Instance;

                                RepositoryItemUsage itemUsage = new RepositoryItemUsage() { HostBusinessFlow = BF, HostBizFlowPath = Path.Combine(BF.ContainingFolder, businessFlowName), UsageItem = a, UsageItemName = a.Name, UsageExtraDetails = "Number of " + GingerDicser.GetTermResValue(eTermResKey.Activities) + ": " + a.ActivitiesIdentifiers.Count().ToString(), UsageItemType = type, Selected = true, Status = RepositoryItemUsage.eStatus.NotUpdated };
                                itemUsage.SetItemPartesFromEnum(typeof(ActivitiesGroup.eItemParts));
                                RepoItemUsages.Add(itemUsage);
                            }
                        }
                    }
                    else if (mRepoItem is Act)
                    {
                        foreach (Activity activity in BF.Activities)
                        {
                            foreach (Act a in activity.Acts)
                            {
                                if (a.ParentGuid == mRepoItem.Guid || a.Guid == mRepoItem.Guid ||
                                            (mRepoItem.ExternalID != null && mRepoItem.ExternalID != string.Empty && mRepoItem.ExternalID != "0" && a.ExternalID == mRepoItem.ExternalID))
                                {
                                    //skip original if not needed
                                    if (!mIncludeOriginal)
                                        if (a.Guid == mOriginalItem.Guid) continue;

                                    Ginger.Repository.RepositoryItemUsage.eUsageTypes type;
                                    if (a.Guid == mRepoItem.Guid)
                                        type = RepositoryItemUsage.eUsageTypes.Original;
                                    else
                                        type = RepositoryItemUsage.eUsageTypes.Instance;

                                    RepositoryItemUsage itemUsage = new RepositoryItemUsage() { HostBusinessFlow = BF, HostBizFlowPath = Path.Combine(BF.ContainingFolder, businessFlowName), HostActivity = activity, HostActivityName = activity.ActivityName, UsageItem = a, UsageItemName = a.Description, UsageExtraDetails = "", UsageItemType = type, Selected = true, Status = RepositoryItemUsage.eStatus.NotUpdated };
                                    itemUsage.SetItemPartesFromEnum(typeof(Act.eItemParts));
                                    RepoItemUsages.Add(itemUsage);
                                }
                            }
                        }
                    }
                    else if (mRepoItem is VariableBase)
                    {
                        //search on Bus Flow level
                        foreach (VariableBase a in BF.Variables)
                        {
                            if (a.ParentGuid == mRepoItem.Guid || a.Guid == mRepoItem.Guid ||
                                        (mRepoItem.ExternalID != null && mRepoItem.ExternalID != string.Empty && mRepoItem.ExternalID != "0" && a.ExternalID == mRepoItem.ExternalID))
                            {
                                //skip original if not needed
                                if (!mIncludeOriginal)
                                    if (a.Guid == mOriginalItem.Guid) continue;

                                Ginger.Repository.RepositoryItemUsage.eUsageTypes type;
                                if (a.Guid == mRepoItem.Guid)
                                    type = RepositoryItemUsage.eUsageTypes.Original;
                                else
                                    type = RepositoryItemUsage.eUsageTypes.Instance;

                                RepositoryItemUsage itemUsage = new RepositoryItemUsage() { HostBusinessFlow = BF, HostBizFlowPath = Path.Combine(BF.ContainingFolder, businessFlowName), UsageItem = a, UsageItemName = a.Name, UsageExtraDetails = "Current Value: " + a.Value, UsageItemType = type, Selected = true, Status = RepositoryItemUsage.eStatus.NotUpdated };
                                itemUsage.SetItemPartesFromEnum(typeof(VariableBase.eItemParts));
                                RepoItemUsages.Add(itemUsage);
                            }
                        }
                        //search on Activities level
                        foreach (Activity activity in BF.Activities)
                        {
                            foreach (VariableBase a in activity.Variables)
                            {
                                if (a.ParentGuid == mRepoItem.Guid || a.Guid == mRepoItem.Guid ||
                                            (mRepoItem.ExternalID != null && mRepoItem.ExternalID != string.Empty && mRepoItem.ExternalID != "0" && a.ExternalID == mRepoItem.ExternalID))
                                {
                                    //skip original if not needed
                                    if (!mIncludeOriginal)
                                        if (a.Guid == mOriginalItem.Guid) continue;

                                    Ginger.Repository.RepositoryItemUsage.eUsageTypes type;
                                    if (a.Guid == mRepoItem.Guid)
                                        type = RepositoryItemUsage.eUsageTypes.Original;
                                    else
                                        type = RepositoryItemUsage.eUsageTypes.Instance;

                                    RepositoryItemUsage itemUsage = new RepositoryItemUsage() { HostBusinessFlow = BF, HostBizFlowPath = Path.Combine(BF.ContainingFolder, businessFlowName), HostActivity = activity, HostActivityName = activity.ActivityName, UsageItem = a, UsageItemName = a.Name, UsageExtraDetails = "Current Value: " + a.Value, UsageItemType = type, Selected = true, Status = RepositoryItemUsage.eStatus.NotUpdated };
                                    itemUsage.SetItemPartesFromEnum(typeof(VariableBase.eItemParts));
                                    RepoItemUsages.Add(itemUsage);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.GetRepositoryItemUsagesFailed, mRepoItem.GetNameForFileName(), ex.Message);
            }
        }        

        private void SetUsagesGridView()
        {  
            if (mRepoItem is Activity)
                usageGrid.Title = "'" + ((Activity)mRepoItem).ActivityName + "' " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " Repository Item Usages";
            else if (mRepoItem is ActivitiesGroup)
                usageGrid.Title = "'" + ((ActivitiesGroup)mRepoItem).Name + "' " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " Repository Item Usages";
            else if (mRepoItem is Act)
                usageGrid.Title = "'" + ((Act)mRepoItem).Description + "' Action Repository Item Usages";
            else if (mRepoItem is VariableBase)
                usageGrid.Title = "'" + ((VariableBase)mRepoItem).Name + "' " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " Repository Item Usages";

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = RepositoryItemUsage.Fields.Selected, StyleType = GridColView.eGridColStyleType.CheckBox, WidthWeight = 10 });
            view.GridColsView.Add(new GridColView() { Field = RepositoryItemUsage.Fields.HostBizFlowPath, Header = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), WidthWeight = 25, ReadOnly = true });
            if (mRepoItem is Act || mRepoItem is VariableBase)
                view.GridColsView.Add(new GridColView() { Field = RepositoryItemUsage.Fields.HostActivityName, Header = GingerDicser.GetTermResValue(eTermResKey.Activity), WidthWeight = 25, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = RepositoryItemUsage.Fields.UsageItemName, Header = "Usage Name", WidthWeight = 25, ReadOnly = true });           
            view.GridColsView.Add(new GridColView() { Field = RepositoryItemUsage.Fields.UsageExtraDetails, Header = "Usage Extra Details", WidthWeight = 20, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = RepositoryItemUsage.Fields.UsageItemType, Header = "Usage Type", WidthWeight = 20, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = RepositoryItemUsage.Fields.SelectedItemPart, Header = "Part to Update ", StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(RepositoryItemUsage.Fields.ItemParts, RepositoryItemUsage.Fields.SelectedItemPart, false), WidthWeight = 20 });
            view.GridColsView.Add(new GridColView() { Field = RepositoryItemUsage.Fields.Status, WidthWeight = 15, ReadOnly = true });            

            usageGrid.SetAllColumnsDefaultView(view);
            usageGrid.InitViewItems();

            usageGrid.AddToolbarTool("@Checkbox_16x16.png", "Select / Un-Select All", new RoutedEventHandler(SelectUnSelectAll));
            usageGrid.AddToolbarTool("@DropDownList_16x16.png", "Set Same Selected Part to All", new RoutedEventHandler(SetSamePartToAll));
        }

        private void SetSamePartToAll(object sender, RoutedEventArgs e)
        {
            if (usageGrid.CurrentItem != null)
            {
                RepositoryItemUsage a = (RepositoryItemUsage)usageGrid.CurrentItem;
                foreach (RepositoryItemUsage usage in RepoItemUsages)
                    usage.SelectedItemPart = a.SelectedItemPart;
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectItem);
            }
        }

        private void SelectUnSelectAll(object sender, RoutedEventArgs e)
        {
            bool selectValue = false;
            if (RepoItemUsages.Count > 0)
            {
                selectValue = !RepoItemUsages[0].Selected;//decide if to take or not
                foreach (RepositoryItemUsage usage in RepoItemUsages)
                    usage.Selected = selectValue;
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button UpdateAllButton = new Button();
            UpdateAllButton.Content = "Update All Selected";
            UpdateAllButton.Click += new RoutedEventHandler(UpdateAllButton_Click);

            Button SaveAllBizFlowsButton = new Button();
            SaveAllBizFlowsButton.Content = "Save All Updated Usages";
            SaveAllBizFlowsButton.Click += new RoutedEventHandler(SaveAllBizFlowsButton_Click);

            ObservableList<Button> winButtons = new ObservableList<Button>();
            winButtons.Add(SaveAllBizFlowsButton);
            winButtons.Add(UpdateAllButton);
                        
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, "Repository Item Usage", this, winButtons, true, "Close");
        }

        private async void UpdateAllButton_Click(object sender, RoutedEventArgs e)
        {           
            await Task.Run(() =>
            {

                //update status to pending
                foreach (RepositoryItemUsage usage in RepoItemUsages)
                {
                    if (usage.Selected && usage.Status == RepositoryItemUsage.eStatus.NotUpdated || usage.Status == RepositoryItemUsage.eStatus.UpdateFailed)
                        usage.Status = RepositoryItemUsage.eStatus.Pending;
                }

                //do the update
                foreach (RepositoryItemUsage usage in RepoItemUsages)
                {
                    try
                    {
                        if (usage.Status == RepositoryItemUsage.eStatus.Pending)
                        {

                            if (usage.HostActivity != null)
                                mRepoItem.UpdateInstance(usage.UsageItem, usage.SelectedItemPart, usage.HostActivity);
                            else
                                mRepoItem.UpdateInstance(usage.UsageItem, usage.SelectedItemPart, usage.HostBusinessFlow);


                            usage.Status = RepositoryItemUsage.eStatus.Updated;
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to update the repository item usage", ex);
                        usage.Status = RepositoryItemUsage.eStatus.UpdateFailed;
                    }
                }

                Reporter.ToUser(eUserMsgKey.UpdateRepositoryItemUsagesSuccess);
            });
        }

        private async void SaveAllBizFlowsButton_Click(object sender, RoutedEventArgs e)
        {
           await Task.Run(() =>
           {
              foreach (RepositoryItemUsage usage in RepoItemUsages)
               {
                   if (usage.Status == RepositoryItemUsage.eStatus.Updated ||
                          usage.Status == RepositoryItemUsage.eStatus.SaveFailed)
                   {
                       try
                       {
                           Reporter.ToStatus(eStatusMsgKey.SaveItem, null, usage.HostBusinessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                           WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(usage.HostBusinessFlow);
                           usage.Status = RepositoryItemUsage.eStatus.UpdatedAndSaved;
                           Reporter.HideStatusMessage();
                       }
                       catch (Exception ex)
                       {
                           usage.Status = RepositoryItemUsage.eStatus.SaveFailed;
                           Reporter.HideStatusMessage();
                           Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                       }
                   }
               }
           }
            );
        }
    }
}
