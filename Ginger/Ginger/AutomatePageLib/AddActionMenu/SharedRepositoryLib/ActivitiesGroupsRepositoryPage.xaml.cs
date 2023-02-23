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
using Amdocs.Ginger.Repository;
using Ginger.Activities;
using Ginger.BusinessFlowPages;
using Ginger.BusinessFlowPages.ListHelpers;
using Ginger.Repository.AddItemToRepositoryWizard;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Activities;
using GingerWPF.DragDropLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.Repository
{
    /// <summary>
    /// Interaction logic for ActivitiesGroupsRepositoryPage.xaml
    /// </summary>
    public partial class ActivitiesGroupsRepositoryPage : Page
    {
        readonly RepositoryFolder<ActivitiesGroup> mActivitiesGroupFolder;      
        bool mInTreeModeView = false;

        Context mContext = null;

        public ActivitiesGroupsRepositoryPage(RepositoryFolder<ActivitiesGroup> activitiesGroupFolder, Context context)
        {
            InitializeComponent();

            mActivitiesGroupFolder = activitiesGroupFolder;
            mContext = context;

            SetActivitiesRepositoryGridView();            
            SetGridAndTreeData();
        }

        private void SetGridAndTreeData()
        {
            xActivitiesGroupsRepositoryListView.ListTitleVisibility = Visibility.Hidden;
            ActivitiesGroupsListViewHelper mActionsListHelper = new ActivitiesGroupsListViewHelper(mContext, General.eRIPageViewMode.AddFromShardRepository);

            xActivitiesGroupsRepositoryListView.SetDefaultListDataTemplate(mActionsListHelper);
            xActivitiesGroupsRepositoryListView.ListSelectionMode = SelectionMode.Extended;
            mActionsListHelper.ListView = xActivitiesGroupsRepositoryListView;

            if (mActivitiesGroupFolder.IsRootFolder)
            {
                xActivitiesGroupsRepositoryListView.DataSourceList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
            }                
            else
            {
                xActivitiesGroupsRepositoryListView.DataSourceList = mActivitiesGroupFolder.GetFolderItems();
            }                
        }

        public void UpdateBusinessFlow(BusinessFlow bf)
        {
            //xActivitiesGroupsRepositoryListView.ClearFilters();
        }

        private void SetActivitiesRepositoryGridView()
        {
            xActivitiesGroupsRepositoryListView.ItemMouseDoubleClick += grdActivitiesGroupsRepository_grdMain_ItemMouseDoubleClick;
            xActivitiesGroupsRepositoryListView.ItemDropped += grdActivitiesGroupsRepository_ItemDropped;
            xActivitiesGroupsRepositoryListView.PreviewDragItem += grdActivitiesGroupsRepository_PreviewDragItem;
            xActivitiesGroupsRepositoryListView.xTagsFilter.Visibility = Visibility.Visible;
            
        }

        private void EditActivityGroup(object sender, RoutedEventArgs e)
        {
            if (xActivitiesGroupsRepositoryListView.CurrentItem != null)
            {
                ActivitiesGroup activityGroup = (ActivitiesGroup)xActivitiesGroupsRepositoryListView.CurrentItem;
                BusinessFlow currentBF = null;
                if(mContext != null)
                {
                    currentBF = mContext.BusinessFlow;
                }

                ActivitiesGroupPage mActivitiesGroupPage = new ActivitiesGroupPage(activityGroup, currentBF, ActivitiesGroupPage.eEditMode.SharedRepository);
                mActivitiesGroupPage.ShowAsWindow();
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectItem);
            }
        }

        private void grdActivitiesGroupsRepository_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DrgInfo.DataIsAssignableToType(typeof(ActivitiesGroup))
                || DragDrop2.DrgInfo.DataIsAssignableToType(typeof(CollectionViewGroup)))
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

        
        private void grdActivitiesGroupsRepository_ItemDropped(object sender, EventArgs e)
        {
            try
            {
                ActivitiesGroup dragedItem = null;

                if (((DragInfo)sender).Data is ActivitiesGroup)
                {
                    dragedItem = (ActivitiesGroup)((DragInfo)sender).Data;
                }
                else if (((DragInfo)sender).Data is CollectionViewGroup)
                {
                    dragedItem = mContext.BusinessFlow.ActivitiesGroups.Where(x=>x.Name == ((DragInfo)sender).Header).FirstOrDefault();
                }

                if (dragedItem != null)
                {
                    //add the Group and it Activities to repository                    
                    List<RepositoryItemBase> list = new List<RepositoryItemBase>();
                    list.Add(dragedItem);
                    foreach (ActivityIdentifiers activityIdnt in dragedItem.ActivitiesIdentifiers)
                    {
                        list.Add(activityIdnt.IdentifiedActivity);
                    }
                    WizardWindow.ShowWizard(new UploadItemToRepositoryWizard(mContext, list));                  

                    //refresh and select the item
                    ActivitiesGroup dragedItemInGrid = ((IEnumerable<ActivitiesGroup>)xActivitiesGroupsRepositoryListView.DataSourceList).Where(x => x.Guid == dragedItem.Guid).FirstOrDefault();
                    if (dragedItemInGrid != null)
                        xActivitiesGroupsRepositoryListView.List.SelectedItem = dragedItemInGrid;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to drop " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " into Shared Repository", ex);
            }
        }
        

        private void grdActivitiesGroupsRepository_grdMain_ItemMouseDoubleClick(object sender, EventArgs e)
        {
            EditActivityGroup(sender, new RoutedEventArgs());
        }
    }
}
