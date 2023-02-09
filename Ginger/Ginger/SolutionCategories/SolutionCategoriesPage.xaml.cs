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
using Amdocs.Ginger.Common.Repository.SolutionCategories;
using Amdocs.Ginger.CoreNET.Run.SolutionCategory;
using Ginger.UserControls;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.SolutionCategories
{
    public enum eSolutionCategoriesPageMode { OptionalValuesDefinition, ValuesSelection}
    /// <summary>
    /// Interaction logic for SolutionCategoriesPage.xaml
    /// </summary>
    public partial class SolutionCategoriesPage : Page
    {
        eSolutionCategoriesPageMode mPageMode;
        ObservableList<SolutionCategory> mSolutionCategories;
        ObservableList<SolutionCategoryDefinition> mCategoriesDefinitions;
        bool mReadOnly;

        public SolutionCategoriesPage(eSolutionCategoriesPageMode mode, ObservableList<SolutionCategoryDefinition> categoriesDefinitions = null, bool readOnlyMode=false)
        {
            InitializeComponent();

            mPageMode = mode;
            mCategoriesDefinitions = categoriesDefinitions;
            if (WorkSpace.Instance.Solution != null)
            {
                mSolutionCategories = WorkSpace.Instance.Solution.SolutionCategories;
            }
            mReadOnly = readOnlyMode;

            InitGrid();
        }

        private void InitGrid()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(SolutionCategory.CategoryName), Header = "Category", ReadOnly = true, BindingMode= BindingMode.OneWay, WidthWeight = 20 });            

            if (mPageMode == eSolutionCategoriesPageMode.OptionalValuesDefinition)
            {
                view.GridColsView.Add(new GridColView() { Field = nameof(SolutionCategory.Description), Header = "Description", ReadOnly = mReadOnly, WidthWeight = 25 });
                view.GridColsView.Add(new GridColView() { Field = nameof(SolutionCategory.CategoryOptionalValuesString), Header = "Optional Values", WidthWeight = 50, ReadOnly = true, BindingMode = BindingMode.OneWay });
                view.GridColsView.Add(new GridColView() { Field = "Edit", WidthWeight = 5, ReadOnly= mReadOnly, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xMainGrid.Resources["xOpenEditLocalPossibleValuesPage"] });
            }
            else
            {
                SetOptionalValues();
                view.GridColsView.Add(new GridColView() { Field = nameof(SolutionCategoryDefinition.Description), Header = "Description", ReadOnly = true, WidthWeight = 25 });               
                view.GridColsView.Add(new GridColView() { Field = nameof(SolutionCategoryDefinition.SelectedValueID), Header = "Selected Value", ReadOnly = mReadOnly, WidthWeight = 50, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(valuesListField: nameof(SolutionCategoryDefinition.CategoryOptionalValues), selectedValueField: nameof(SolutionCategoryDefinition.SelectedValueID), selectedValuePathField: nameof(SolutionCategoryValue.Guid), displayMemberPathField: nameof(SolutionCategoryValue.Value), style: this.FindResource("$FlatInputComboBoxInGridCellStyle") as Style) });
            }
            xCategoriesGrid.SetAllColumnsDefaultView(view);
            xCategoriesGrid.InitViewItems();
            
            xCategoriesGrid.ShowTitle = Visibility.Collapsed;
            xCategoriesGrid.ShowEdit = Visibility.Collapsed;
            xCategoriesGrid.ShowUpDown = Visibility.Collapsed;
            xCategoriesGrid.ShowAdd = Visibility.Collapsed;
            xCategoriesGrid.ShowClearAll = Visibility.Collapsed;
            xCategoriesGrid.ShowDelete = Visibility.Collapsed;
            xCategoriesGrid.ShowCopyCutPast = Visibility.Collapsed;

            xCategoriesGrid.SetRefreshBtnHandler(new RoutedEventHandler(RefreshCategories));

            if (mPageMode == eSolutionCategoriesPageMode.OptionalValuesDefinition)
            {
                xCategoriesGrid.DataSourceList = mSolutionCategories;
            }
            else
            {
                xCategoriesGrid.DataSourceList = mCategoriesDefinitions;
            }
            
        }

        private void SetOptionalValues()
        {
            foreach (SolutionCategoryDefinition cat in mCategoriesDefinitions)
            {
                SolutionCategory solCat = mSolutionCategories.Where(x => x.Category == cat.Category).FirstOrDefault();
                if (cat != null)
                {
                    cat.CategoryName = solCat.CategoryName;
                    cat.Description = solCat.Description;
                    cat.CategoryOptionalValues = solCat.CategoryOptionalValues;
                }
            }
        }

        private void RefreshCategories(object sender, RoutedEventArgs e)
        {
            InitGrid();
        }

        private void OpenEditLocalPossibleValuesPageButton_Click(object sender, RoutedEventArgs e)
        {
            SolutionCategory currentCat = (SolutionCategory)xCategoriesGrid.CurrentItem;
            SolutionCategoryOptionalValuesEditPage valuesPage = new SolutionCategoryOptionalValuesEditPage(currentCat);
            valuesPage.ShowAsWindow();
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            xCategoriesGrid.Grid.CommitEdit(DataGridEditingUnit.Row, true);
        }
    }
}
