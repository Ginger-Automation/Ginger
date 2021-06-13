using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Run.SolutionCategory;
using Ginger.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ginger.SolutionWindows.SolutionCategories
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

            InitGrid();
        }

        private void InitGrid()
        {
            this.Title = mSolutionCategory.ItemName + " " + "Optional Values:";

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();


                view.GridColsView.Add(new GridColView() { Field = nameof(SolutionCategoryValue.Value), WidthWeight = 10 });
            //if (xOptionalValuesGrid.Grid != null)
            //{
            //    xOptionalValuesGrid.Grid.BeginningEdit += xOptionalValuesGrid_BeginningEdit;
            //    xOptionalValuesGrid.Grid.CellEditEnding += xOptionalValuesGrid_CellEditEnding;
            //}
          
            xOptionalValuesGrid.SetAllColumnsDefaultView(view);
            xOptionalValuesGrid.InitViewItems();


            //mParentObject.OptionalValuesList.PropertyChanged += mAMDP_PropertyChanged;
            xOptionalValuesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddOptionalValue));
            xOptionalValuesGrid.SetbtnDeleteHandler(btnDelete_Click);
            xOptionalValuesGrid.SetbtnClearAllHandler(btnClearAll_Click);
            // OptionalValuesGrid.btnCopy.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnCopyClicked));
            //OptionalValuesGrid.btnCut.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnCopyClicked));
            //OptionalValuesGrid.btnPaste.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnPastClicked));

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
            { optionalValuesToRemove.Add(selectedOV); }

            foreach (SolutionCategoryValue ov in optionalValuesToRemove)
            {
                //if (ov != null && !ov.Value.Equals(GlobalAppModelParameter.CURRENT_VALUE))
                //{
                //    if (ov.IsDefault && OptionalValuesGrid.Grid.Items.Count > 1)
                //    {
                //        OptionalValue newDefault = ((OptionalValue)(OptionalValuesGrid.Grid.Items[0]));
                //        newDefault.IsDefault = true;
                //        //binding is disabeled so setting the radio button as check manually
                //        RadioButton rb = (RadioButton)OptionalValuesGrid.GetDataTemplateCellControl<RadioButton>(newDefault, 1);
                //        rb.IsChecked = true;
                //    }

                    mSolutionCategory.CategoryOptionalValues.RemoveItem(ov);
                    mEditWasDone = true;
                //}
            }
        }

        public bool ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button OKButton = new Button();
            OKButton.Content = "OK";
            OKButton.Click += new RoutedEventHandler(OKButton_Click);
          
            GenericWindow.LoadGenericWindow(ref mWin, null, windowStyle, this.Title, this, new ObservableList<Button> { OKButton }, showClosebtn: false);

            return mEditWasDone;
        }

        //SolutionCategoryValue mSavedDefaultOV = null;
        //private void BtnCopyClicked(object sender, RoutedEventArgs e)
        //{
        //    mSavedDefaultOV = null;
        //    SolutionCategoryValue defaultOptionalValue = OptionalValuesGrid.Grid.SelectedItems.Cast<OptionalValue>().ToList().Where(x => x.IsDefault == true).FirstOrDefault();
        //    if (defaultOptionalValue != null)
        //    { mSavedDefaultOV = defaultOptionalValue; }
        //}

        //private void BtnPastClicked(object sender, RoutedEventArgs e)
        //{
        //    if (mSavedDefaultOV != null)
        //    { mSavedDefaultOV.IsDefault = true; }
        //}

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            xOptionalValuesGrid.Grid.CommitEdit(DataGridEditingUnit.Row, true);

            //remove empty rows
            for (int i = 0; i < xOptionalValuesGrid.Grid.Items.Count; i++)
            {
                SolutionCategoryValue OV = (SolutionCategoryValue)xOptionalValuesGrid.Grid.Items[i];
                if (string.IsNullOrEmpty(OV.Value))
                {                   
                    mSolutionCategory.CategoryOptionalValues.Remove(OV);
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
