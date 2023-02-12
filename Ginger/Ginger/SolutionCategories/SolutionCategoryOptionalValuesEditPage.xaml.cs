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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Run.SolutionCategory;
using Ginger.UserControls;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionCategories
{
    /// <summary>
    /// Interaction logic for SolutionCategoryOptionalValuesEditPage.xaml
    /// </summary>
    public partial class SolutionCategoryOptionalValuesEditPage : Page
    {
        SolutionCategory mSolutionCategory;
        bool mEditWasDone = true;
        GenericWindow mWin;

        public SolutionCategoryOptionalValuesEditPage(SolutionCategory solutionCategory)
        {
            InitializeComponent();

            mSolutionCategory = solutionCategory;

            InitGrid();
        }

        private void InitGrid()
        {
            this.Title = mSolutionCategory.Category.ToString() + " " + "Category Optional Values";

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(SolutionCategoryValue.Value), WidthWeight = 10 });
            xOptionalValuesGrid.SetAllColumnsDefaultView(view);
            xOptionalValuesGrid.InitViewItems();

            //mParentObject.OptionalValuesList.PropertyChanged += mAMDP_PropertyChanged;
            xOptionalValuesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddOptionalValue));
            xOptionalValuesGrid.SetbtnDeleteHandler(btnDelete_Click);
            xOptionalValuesGrid.SetbtnClearAllHandler(btnClearAll_Click);
            xOptionalValuesGrid.SetbtnCopyHandler(BtnCopyClicked);
            xOptionalValuesGrid.SetbtnPastHandler(BtnPastClicked);

            xOptionalValuesGrid.DataSourceList = mSolutionCategory.CategoryOptionalValues;
        }

        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {
            mSolutionCategory.CategoryOptionalValues.Clear();
        }
        private void AddOptionalValue(object sender, RoutedEventArgs e)
        {
            xOptionalValuesGrid.Grid.CommitEdit(DataGridEditingUnit.Row, true);

            SolutionCategoryValue newVal = new SolutionCategoryValue(string.Empty);
            mSolutionCategory.CategoryOptionalValues.Add(newVal);

            xOptionalValuesGrid.Grid.SelectedItem = newVal;
            xOptionalValuesGrid.Grid.CurrentItem = newVal;

            mEditWasDone = true;
            xOptionalValuesGrid.Grid.CommitEdit(DataGridEditingUnit.Row, true);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            List<SolutionCategoryValue> optionalValuesToRemove = new List<SolutionCategoryValue>();
            foreach (SolutionCategoryValue selectedOV in xOptionalValuesGrid.Grid.SelectedItems)
            { 
                optionalValuesToRemove.Add(selectedOV); 
            }

            foreach (SolutionCategoryValue ov in optionalValuesToRemove)
            {
                mSolutionCategory.CategoryOptionalValues.RemoveItem(ov);
                mEditWasDone = true;
            }
        }

        List<SolutionCategoryValue> mCopiedItems = new List<SolutionCategoryValue>();
        private void BtnCopyClicked(object sender, RoutedEventArgs e)
        {
            mCopiedItems.Clear();
            foreach (SolutionCategoryValue cat in xOptionalValuesGrid.Grid.SelectedItems)
            {
                mCopiedItems.Add(cat);
            }
        }

        private void BtnPastClicked(object sender, RoutedEventArgs e)
        {
            foreach (SolutionCategoryValue cat in mCopiedItems)
            {
                SolutionCategoryValue newCopy= (SolutionCategoryValue)cat.CreateCopy();
                newCopy.Value += "_Copy";
                mSolutionCategory.CategoryOptionalValues.Add(newCopy);
            }
        }

        public bool ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button OKButton = new Button();
            OKButton.Content = "OK";
            OKButton.Click += new RoutedEventHandler(OKButton_Click);

            this.Width = 300;
            this.Height = 300;

            GenericWindow.LoadGenericWindow(ref mWin, null, windowStyle, this.Title, this, new ObservableList<Button> { OKButton }, showClosebtn: false);

            return mEditWasDone;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            xOptionalValuesGrid.Grid.CommitEdit(DataGridEditingUnit.Row, true);
            //remove empty rows
            for (int i = 0; i < xOptionalValuesGrid.Grid.Items.Count; i++)
            {
                SolutionCategoryValue cat = (SolutionCategoryValue)xOptionalValuesGrid.Grid.Items[i];
                if (string.IsNullOrEmpty(cat.Value))
                {                   
                    mSolutionCategory.CategoryOptionalValues.Remove(cat);
                    i--;
                }
            }
            xOptionalValuesGrid.Grid.CommitEdit(DataGridEditingUnit.Row, true);

            mWin.Close();

            mSolutionCategory.PropertyChangedEventHandler();
        }


        //string mOldValue = string.Empty;
        //bool mEditWasDone = false;
        //private void xOptionalValuesGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        //{
        //    mOldValue = ((SolutionCategoryValue)xOptionalValuesGrid.CurrentItem).Value;
        //    mEditWasDone = true;
        //}

        //private void xOptionalValuesGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        //{
        //    SolutionCategoryValue currentOP = null;
        //    if (e.Column.Header.ToString() == nameof(SolutionCategoryValue.Value))
        //    {
        //        currentOP = (SolutionCategoryValue)xOptionalValuesGrid.CurrentItem;
        //    }
        //    if (currentOP != null)
        //    {
        //        if (mOldValue.Equals(GlobalAppModelParameter.CURRENT_VALUE))
        //        {
        //            currentOP.Value = OldValue;
        //        }
        //        else
        //        {
        //            foreach (OptionalValue OP in mParentObject.OptionalValuesList)
        //            {
        //                if (OP != currentOP && OP.Value == currentOP.Value)
        //                {
        //                    currentOP.Value = OldValue;
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //}

    }
}
