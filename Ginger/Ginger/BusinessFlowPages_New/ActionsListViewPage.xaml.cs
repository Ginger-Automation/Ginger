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
using Ginger;
using Ginger.BusinessFlowPages.ListViewItems;
using Ginger.UserControlsLib.UCListView;
using GingerCore;
using GingerCore.Actions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GingerWPF.BusinessFlowsLib
{
    /// <summary>
    /// Interaction logic for ActivityActionsPage.xaml
    /// </summary>
    public partial class ActionsListViewPage : Page
    {
        Activity mActivity;
        Context mContext;
        
        public ActionsListViewPage(Activity Activity, Context context)
        {
            InitializeComponent();

            mActivity = Activity;
            mContext = context;
            SetListView();
            //xActionsListView.List.ItemsSource = mActivity.Acts;
            
            //Activity.Acts.PropertyChanged += Acts_PropertyChanged;
            //xActionsListView.List.SelectionChanged += ActionsListBox_SelectionChanged;
        }

        //private void SetListView() //working
        //{
        //    DataTemplate dataTemp = new DataTemplate();

        //    FrameworkElementFactory listItemFac = new FrameworkElementFactory(typeof(ActionListItem));
        //    listItemFac.SetBinding(ActionListItem.ActionProperty, new Binding());

        //    dataTemp.VisualTree = listItemFac;
        //    xActionsListView.List.ItemTemplate = dataTemp; 
        //}

        private void SetListView() 
        {
            xActionsListView.Title = "Actions";
            xActionsListView.ListImageType = Amdocs.Ginger.Common.Enums.eImageType.Action;

            ////TODO: move DataTemplate into ListView
            //DataTemplate dataTemp = new DataTemplate();
            //FrameworkElementFactory listItemFac = new FrameworkElementFactory(typeof(UcListViewItem));
            ////listItemFac.SetValue(UcListViewItem.ParentListProperty, xActionsListView);
            //listItemFac.SetBinding(UcListViewItem.ItemProperty, new Binding());
            //listItemFac.SetValue(UcListViewItem.ItemInfoProperty, new ActionListItemInfo(mContext));
            //dataTemp.VisualTree = listItemFac;
            //xActionsListView.List.ItemTemplate = dataTemp;

            xActionsListView.SetDefaultListDataTemplate(new ActionListItemInfo(mContext));

            xActionsListView.AddBtnVisiblity = Visibility.Collapsed;

            xActionsListView.DataSourceList = mActivity.Acts;
            //xActionsListView.List.ItemsSource = mActivity.Acts;
        }

        public void UpdateActivity(Activity activity)
        {
            if (mActivity != activity)
            {
                mActivity = activity;
                if (mActivity != null)
                {
                    //xActionsListView.DataSourceList = null;
                    xActionsListView.DataSourceList = mActivity.Acts;
                    //xActionsListView.List.ItemsSource = null;
                    //xActionsListView.List.ItemsSource = mActivity.Acts;
                }
                else
                {
                    xActionsListView.DataSourceList = null;
                    //xActionsListView.List.ItemsSource = null;
                }
                //xActionsListView.List.Items.Refresh();
            }
        }

        //private void ActionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    // Make the list synced with the Activity Acts, so if user change action the activity current act is the same
        //    mActivity.Acts.CurrentItem = xActionsListView.List.SelectedItem;
        //    //UpdateFloatingButtons();
        //}

        //private void UpdateFloatingButtons()
        //{
        //    if (xActionsListView.List.SelectedItem != null)
        //    {
        //        ListViewItem lvi = (ListViewItem)xActionsListView.List.ItemContainerGenerator.ContainerFromItem(xActionsListView.List.SelectedItem);
        //        if (lvi != null)
        //        {
        //            Point rel = lvi.TranslatePoint(new Point(0, 0), xActionsListView.List);
        //            FloatingStackPanel.Margin = new Thickness(0, rel.Y, 0, 0);
        //            FloatingStackPanel.Visibility = Visibility.Visible;
        //        }
        //    }
        //    else
        //    {
        //        FloatingStackPanel.Visibility = Visibility.Collapsed;
        //    }
        //}

        //private void Acts_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == "CurrentItem")
        //    {
        //        // since we can get event while GingerRunner is on another thread we need dispatcher
        //        xActionsListView.List.Dispatcher.Invoke(() => {
        //            xActionsListView.List.SelectedItem = mActivity.Acts.CurrentItem;
        //            xActionsListView.List.Refresh();
        //        });
        //    }
        //}

        //private void EditButton_Click(object sender, RoutedEventArgs e)
        //{
        //    Act act = (Act)mActivity.Acts.CurrentItem;
        //    GingerActionEditPage GAEP = new GingerActionEditPage(act);
        //    GAEP.ShowAsWindow();
        //}

        //private void DeleteButton_Click(object sender, RoutedEventArgs e)
        //{
        //    Act act = (Act)mActivity.Acts.CurrentItem;
        //    int index = mActivity.Acts.IndexOf(act);
        //    Act newSelectedAct = null;
        //    if (mActivity.Acts.Count-1 > index)
        //    {
        //        newSelectedAct = (Act)mActivity.Acts[index + 1];
        //    }
        //    else
        //    {
        //        if (index != 0)
        //        {
        //            newSelectedAct = (Act)mActivity.Acts[index - 1];
        //        }                
        //    }
        //    mActivity.Acts.Remove(act);
        //    mActivity.Acts.CurrentItem = newSelectedAct;
        //    //UpdateFloatingButtons();
        //}
    }
}
