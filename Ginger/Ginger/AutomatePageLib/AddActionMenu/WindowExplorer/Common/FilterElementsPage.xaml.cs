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

using Ginger.UserControls;
using GingerCore;
using System.Windows;
using System.Windows.Controls;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using System.Threading.Tasks;
using Ginger.BusinessFlowsLibNew.AddActionMenu;

namespace Ginger.WindowExplorer.Common
{
    /// <summary>
    /// Interaction logic for FilterElementsWindow.xaml
    /// </summary>
    public partial class FilterElementsPage : Page
    {
        ObservableList<UIElementFilter> CheckedFilteringCreteriaList;
        GenericWindow _GenWin;
        WindowExplorerPage mWindowExlorerPage;
        WindowsExplorerNavPage mWindowsExplorerNavPage;

        public FilterElementsPage(ObservableList<UIElementFilter> filteringCriteriaList, ObservableList<UIElementFilter> checkedFilteringCreteriaList, /*RoutedEventHandler elementSearchEvent,*/ WindowExplorerPage windowExlorerPage = null, WindowsExplorerNavPage windowsExplorerNavPage = null)
        {
            InitializeComponent();
            SetControlsGridView();
            FilterElementsGridView.DataSourceList = filteringCriteriaList;
            CheckedFilteringCreteriaList = checkedFilteringCreteriaList;
            SetCheckedValues();
            mWindowExlorerPage = windowExlorerPage;
            mWindowsExplorerNavPage = windowsExplorerNavPage;
        }

        private void SetCheckedValues()
        {
            if (CheckedFilteringCreteriaList.Count != 0)
                foreach (UIElementFilter Filter in CheckedFilteringCreteriaList)
                {
                    int counter = 0;
                    foreach (UIElementFilter UIEF in FilterElementsGridView.DataSourceList)
                    {
                        if (Filter.ElementType == UIEF.ElementType)
                        {
                            UIEF.Selected = true;
                        }

                        counter++;
                    }
                }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free)
        {
            string Title = "Filter Elements Window";

            ObservableList<Button> winButtons = new ObservableList<Button>();

            Button searchBtn = new Button();
            searchBtn.Content = "Search";
            searchBtn.Click += DoSearch;
            winButtons.Add(searchBtn);

            GingerCore.General.LoadGenericWindow(ref _GenWin, null, windowStyle, Title, this, winButtons);
        }

        private async void DoSearch(object sender, RoutedEventArgs e)
        {
            if (mWindowExlorerPage != null)
            {
                mWindowExlorerPage.DoSearchControls();
            }
            else if (mWindowsExplorerNavPage != null)
            {
                mWindowsExplorerNavPage.DoSearchControls();
            }
            //bool isSearched = await Task.Run(() => mWindowExlorerPage.DoSearchControls());
            _GenWin.Close();
            //int FoundItemsCount = mWindowExlorerPage.WindowControlsGridView.DataSourceList.Count;
            //if (isSearched)
            //{
            //    Amdocs.Ginger.Common.eUserMsgSelection resultCloseWindow = Reporter.ToUser(eUserMsgKey.CloseFilterPage, FoundItemsCount);
            //    if (resultCloseWindow == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            //        _GenWin.Close();
            //}
        }

            private void SetControlsGridView()
        {
            //Set the Tool Bar look
            FilterElementsGridView.ShowTitle = Visibility.Collapsed;
            FilterElementsGridView.ShowAdd = Visibility.Collapsed;
            FilterElementsGridView.ShowClearAll = Visibility.Collapsed;
            FilterElementsGridView.ShowUpDown = Visibility.Collapsed;
            FilterElementsGridView.btnRefresh.Visibility = Visibility.Collapsed;

            FilterElementsGridView.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Check/Uncheck All Elements", new RoutedEventHandler(CheckUnCheckAllElements));

            FilterElementsGridView.ShowEdit = System.Windows.Visibility.Collapsed;
            FilterElementsGridView.ShowDelete = System.Windows.Visibility.Collapsed;

            //Set the Data Grid columns            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(UIElementFilter.Selected), Header= "Selected", WidthWeight = 10, MaxWidth = 50, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = nameof(UIElementFilter.ElementType), Header = "Element Type", WidthWeight = 100 });
            view.GridColsView.Add(new GridColView() { Field = nameof(UIElementFilter.ElementExtraInfo), Header = "Element Extra Info", WidthWeight = 100 });

            FilterElementsGridView.SetAllColumnsDefaultView(view);
            FilterElementsGridView.InitViewItems();
        }

        private void CheckUnCheckAllElements(object sender, RoutedEventArgs e)
        {
            IObservableList filteringCriteriaList = FilterElementsGridView.DataSourceList;


            int selectedItems = CountSelectedItems();
            if (selectedItems < FilterElementsGridView.DataSourceList.Count)            
                foreach (UIElementFilter UIEFActual in filteringCriteriaList)                
                    UIEFActual.Selected = true;
            else if (selectedItems == FilterElementsGridView.DataSourceList.Count)
                foreach (UIElementFilter UIEFActual in filteringCriteriaList)
                    UIEFActual.Selected = false;

            FilterElementsGridView.DataSourceList = filteringCriteriaList;
        }

        private int CountSelectedItems()
        {
            int counter = 0;
            foreach (UIElementFilter UIEFActual in FilterElementsGridView.DataSourceList)
            {
                if (UIEFActual.Selected)
                     counter++;
            }
            return counter;
        }
    }
}
