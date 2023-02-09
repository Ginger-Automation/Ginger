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
using Amdocs.Ginger.Common.Repository.BusinessFlowLib;
using Amdocs.Ginger.Repository;
using Ginger.BusinessFlowPages;
using Ginger.BusinessFlowPages.ListHelpers;
using Ginger.Repository.AddItemToRepositoryWizard;
using Ginger.Run;
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

namespace Ginger.Repository
{
    /// <summary>
    /// Interaction logic for ActivitiesRepositoryPage.xaml
    /// </summary>
    public partial class ActivitiesRepositoryPage : Page
    {
        readonly RepositoryFolder<Activity> mActivitiesFolder;        
        bool mInTreeModeView = false;
        ObservableList<Guid> mTags = new ObservableList<Guid>();
        RoutedEventHandler mAddActivityHandler;
        Context mContext;
        GenericWindow _pageGenericWin = null;
        ObservableList<Activity> mActivities;
        bool mAddPOMActivity = false;
        public enum ePageViewMode { Default, Selection }

        public enum eActivityType 
        {
            [EnumValueDescription("Regular Activity")]
            Regular,
            [EnumValueDescription("Error Handler")]
            ErrorHandler,
            [EnumValueDescription("CleanUp Activity")]
            CleanUpActivity
        }

        ePageViewMode mViewMode;

        public ActivitiesRepositoryPage(RepositoryFolder<Activity> activitiesFolder, Context context, ObservableList<Guid> Tags=null, RoutedEventHandler AddActivityHandler = null, ePageViewMode viewMode = ePageViewMode.Default)
        {
            InitializeComponent();

            mActivitiesFolder = activitiesFolder;
            mContext = context;

            /*
            if (Tags != null)
            {
                mTags = Tags;
                xActivitiesRepositoryListView.Tags = mTags;
            }

            if (AddActivityHandler != null)
                mAddActivityHandler = AddActivityHandler;
            else
                mAddActivityHandler = AddFromRepository;

            mViewMode = viewMode;
            */

            mAddActivityHandler = AddActivityHandler;

            SetActivitiesRepositoryListView();            
            SetGridAndTreeData();
        }
        public ActivitiesRepositoryPage(ObservableList<Activity> activities, Context context, bool AddPOMActivity = false)
        {
            InitializeComponent();

            mActivities = activities;
            mContext = context;
            mAddPOMActivity = AddPOMActivity;
            SetActivitiesRepositoryListView();
            SetGridAndTreeData();
        }
        private void SetGridAndTreeData()
        {
            xActivitiesRepositoryListView.ListTitleVisibility = Visibility.Hidden;
            ActivitiesListViewHelper mActionsListHelper = null;
            if (mAddPOMActivity)
            {
                mActionsListHelper = new ActivitiesListViewHelper(mContext, General.eRIPageViewMode.AddFromModel);
            }
            else if (mActivities != null)
            {
                mActionsListHelper = new ActivitiesListViewHelper(mContext, General.eRIPageViewMode.Explorer);
            }
            else
            {
                mActionsListHelper = new ActivitiesListViewHelper(mContext, General.eRIPageViewMode.AddFromShardRepository);
            }
            xActivitiesRepositoryListView.SetDefaultListDataTemplate(mActionsListHelper);
            xActivitiesRepositoryListView.ListSelectionMode = SelectionMode.Extended;
            mActionsListHelper.ListView = xActivitiesRepositoryListView;
            if (mActivities != null)//to show pom specific activities
            {
                xActivitiesRepositoryListView.DataSourceList = mActivities;
                return;
            }
            if (mActivitiesFolder.IsRootFolder)
            {
                xActivitiesRepositoryListView.DataSourceList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
            }
            else
            {
                xActivitiesRepositoryListView.DataSourceList = mActivitiesFolder.GetFolderItems();
            }
        }

        public void UpdateBusinessFlow(BusinessFlow bf)
        {
            //xActivitiesRepositoryListView.ClearFilters();
        }

        private void SetActivitiesRepositoryListView()
        {
            xActivitiesRepositoryListView.ItemMouseDoubleClick += grdActivitiesRepository_grdMain_ItemMouseDoubleClick;
            xActivitiesRepositoryListView.ItemDropped += grdActivitiesRepository_ItemDropped;
            xActivitiesRepositoryListView.PreviewDragItem += grdActivitiesRepository_PreviewDragItem;
            xActivitiesRepositoryListView.xTagsFilter.Visibility = Visibility.Visible;
        }

        private void ActivityType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = ((ComboBox)sender).SelectedItem;

            var allActiivtyType = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();

            if (selectedItem.ToString().Equals("All"))
            {
                xActivitiesRepositoryListView.DataSourceList = allActiivtyType;
                return;
            }
            selectedItem = ((GingerCore.GeneralLib.ComboEnumItem)selectedItem).Value.ToString();

            ObservableList<Activity> activities = new ObservableList<Activity>();
            foreach (Activity item in allActiivtyType)
            {
                if (selectedItem.Equals(eActivityType.ErrorHandler.ToString()) && item.GetType() == typeof(ErrorHandler))
                {
                    activities.Add(item);
                }
                else if (selectedItem.Equals(eActivityType.CleanUpActivity.ToString()) && item.GetType() == typeof(CleanUpActivity))
                {
                    activities.Add(item);
                }
                else if (selectedItem.Equals(eActivityType.Regular.ToString()) && item.GetType() == typeof(Activity))
                {
                    activities.Add(item);
                }
 
            }

            xActivitiesRepositoryListView.DataSourceList = activities;
        }

        private void EditActivity(object sender, RoutedEventArgs e)
        {
            if (xActivitiesRepositoryListView.CurrentItem != null)
            {
                Activity activity = (Activity)xActivitiesRepositoryListView.CurrentItem;
                GingerWPF.BusinessFlowsLib.ActivityPage window = null;
                Context context = new Context()
                {
                    Activity = activity,
                    Runner = new GingerExecutionEngine(new GingerRunner())
                };
                if (activity.IsAutoLearned)
                {
                    window = new GingerWPF.BusinessFlowsLib.ActivityPage(activity, context, General.eRIPageViewMode.View);
                }
                else
                {
                    window = new GingerWPF.BusinessFlowsLib.ActivityPage(activity, context, General.eRIPageViewMode.SharedReposiotry);
                }
                window.ShowAsWindow();
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectItem);
            }
        }

        public void ShowAsWindow(Window ownerWindow, eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, ePageViewMode viewMode = ePageViewMode.Selection)
        {
            ObservableList<Button> winButtons = new ObservableList<Button>();

            if (viewMode == ePageViewMode.Selection)
            {
                Button addButton = new Button();
                addButton.Content = "Add Selected";
                addButton.Click += SendSelected;

                winButtons.Add(addButton);
                xActivitiesRepositoryListView.AddHandler(DataGridRow.MouseDoubleClickEvent, new RoutedEventHandler(SendSelected));
            }

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, ownerWindow, windowStyle, "Shared " + GingerDicser.GetTermResValue(eTermResKey.Activities), this, winButtons, true, "Close");
        }

        private void SendSelected(object sender, RoutedEventArgs e)
        {
            if (mAddActivityHandler != null)
            {
                if (sender is ucGrid)
                {
                    mAddActivityHandler(sender, e);
                }
                else
                {
                    mAddActivityHandler(xActivitiesRepositoryListView, e);
                }
            }
            _pageGenericWin.Close();
        }

        private void grdActivitiesRepository_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DrgInfo.DataIsAssignableToType(typeof(Activity)))
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

        private void grdActivitiesRepository_ItemDropped(object sender, EventArgs e)
        {
            Activity dragedItem = (Activity)((DragInfo)sender).Data;
            if (dragedItem != null)
            {
                ////check if the Activity is part of a group which not exist in Activity repository
                WizardWindow.ShowWizard(new UploadItemToRepositoryWizard(mContext, dragedItem));

                //refresh and select the item
                try
                {
                    Activity dragedItemInGrid = ((IEnumerable<Activity>)xActivitiesRepositoryListView.DataSourceList).Where(x => x.Guid == dragedItem.Guid).FirstOrDefault();
                    if (dragedItemInGrid != null)
                        xActivitiesRepositoryListView.List.SelectedItem = dragedItemInGrid;
                }
                catch(Exception ex){ Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
            }
        }

        private void grdActivitiesRepository_grdMain_ItemMouseDoubleClick(object sender, EventArgs e)
        {
            EditActivity(sender, new RoutedEventArgs());
        }
    }
}
