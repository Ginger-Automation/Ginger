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
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.ApplicationModelsLib;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Drivers.Common;
using GingerWPF.UserControlsLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace GingerWPF.ApplicationModelsLib.APIModelWizard
{
    public partial class ModelOptionalValuesPage : Page
    {
        GenericWindow mWin;
        IParentOptionalValuesObject mParentObject;
        bool editWasDone = false;
        bool mSelectionModePage;

        public ModelOptionalValuesPage(IParentOptionalValuesObject parObj, bool selectionModePage = false)
        {
            InitializeComponent();

            mParentObject = parObj;
            mSelectionModePage = selectionModePage;

            OptionalValuesGrid.DataSourceList = mParentObject.OptionalValuesList;
            SetOptionalValuesGridView();

            if (!mSelectionModePage)
            {
                mParentObject.OptionalValuesList.PropertyChanged += mAMDP_PropertyChanged;
                OptionalValuesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddOptionalValue));
                OptionalValuesGrid.SetbtnDeleteHandler(btnDelete_Click);
                OptionalValuesGrid.SetbtnClearAllHandler(btnClearAll_Click);
                OptionalValuesGrid.btnCopy.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnCopyClicked));
                OptionalValuesGrid.btnCut.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnCopyClicked));
                OptionalValuesGrid.btnPaste.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnPastClicked));
            }
            this.Title = parObj.ElementName + " " + "Optional Values:";
        }

        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < mParentObject.OptionalValuesList.Count; i++)
            {
                OptionalValue ov = mParentObject.OptionalValuesList[i];
                if (ov.Value != GlobalAppModelParameter.CURRENT_VALUE)
                {
                    mParentObject.OptionalValuesList.RemoveItem(ov);
                    i--;
                }
                else
                {
                    ov.IsDefault = true;
                    //binding is disabled so setting the radio button as check manually
                    RadioButton rb = (RadioButton)OptionalValuesGrid.GetDataTemplateCellControl<RadioButton>(ov, 1);
                    rb.IsChecked = true;
                }
            }
        }

        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
               
        private void SetOptionalValuesGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            if (!mSelectionModePage)
            {
                view.GridColsView.Add(new GridColView() { Field = nameof(OptionalValue.Value), WidthWeight = 10 });
                if (OptionalValuesGrid.Grid != null)
                {
                    OptionalValuesGrid.Grid.BeginningEdit += grdMain_BeginningEdit;
                    OptionalValuesGrid.Grid.CellEditEnding += grdMain_CellEditEnding;
                }
            }
            else
            { view.GridColsView.Add(new GridColView() { Field = nameof(OptionalValue.Value), ReadOnly = true, WidthWeight = 10 }); }

            view.GridColsView.Add(new GridColView() { Field = nameof(OptionalValue.IsDefault), Header = "Default", StyleType = GridColView.eGridColStyleType.Template, HorizontalAlignment = HorizontalAlignment.Center, CellTemplate = (DataTemplate)this.pageGrid.Resources["DefaultValueTemplate"], WidthWeight = 1 });
            OptionalValuesGrid.SetAllColumnsDefaultView(view);
            OptionalValuesGrid.InitViewItems();
        }

        string OldValue = string.Empty;
        private void grdMain_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            OldValue = ((OptionalValue)OptionalValuesGrid.CurrentItem).Value;
            editWasDone = true;
        }

        private void grdMain_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            OptionalValue CurrentOP = null;
            if (e.Column.Header.ToString() == nameof(OptionalValue.Value))
                CurrentOP = (OptionalValue)OptionalValuesGrid.CurrentItem;

            if (CurrentOP != null)
            {
                if (OldValue.Equals(GlobalAppModelParameter.CURRENT_VALUE))
                {
                    CurrentOP.Value = OldValue;
                }
                else
                {
                    foreach (OptionalValue OP in mParentObject.OptionalValuesList)
                    {
                        if (OP != CurrentOP && OP.Value == CurrentOP.Value)
                        {
                            CurrentOP.Value = OldValue;
                            break;
                        }
                    }
                }
            }
        }

        private void AddOptionalValue(object sender, RoutedEventArgs e)
        {
            OptionalValuesGrid.Grid.CommitEdit(DataGridEditingUnit.Row, true);

            OptionalValue newVal = new OptionalValue(string.Empty);
            newVal.IsDefault = true;
            mParentObject.OptionalValuesList.Add(newVal);

            OptionalValuesGrid.Grid.SelectedItem = newVal;
            OptionalValuesGrid.Grid.CurrentItem = newVal;

            editWasDone = true;
            OptionalValuesGrid.Grid.CommitEdit(DataGridEditingUnit.Row, true);
        }

        OptionalValue SavedDefaultOV = null;
        private void BtnCopyClicked(object sender, RoutedEventArgs e)
        {
            SavedDefaultOV = null;
            OptionalValue defaultOptionalValue = OptionalValuesGrid.Grid.SelectedItems.Cast<OptionalValue>().ToList().Where(x => x.IsDefault == true).FirstOrDefault();
            if (defaultOptionalValue != null)
            { SavedDefaultOV = defaultOptionalValue; }
        }

        private void BtnPastClicked(object sender, RoutedEventArgs e)
        {
            if (SavedDefaultOV != null)
            { SavedDefaultOV.IsDefault = true; }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            OptionalValuesGrid.Grid.CommitEdit(DataGridEditingUnit.Row, true);

            for (int i = 0; i < OptionalValuesGrid.Grid.Items.Count; i++)
            {
                OptionalValue OV = (OptionalValue)OptionalValuesGrid.Grid.Items[i];
                if (string.IsNullOrEmpty(OV.Value))
                {
                    if (OV.IsDefault)
                    { ((OptionalValue)(OptionalValuesGrid.Grid.Items[0])).IsDefault = true; }

                    mParentObject.OptionalValuesList.Remove(OV);
                    i--;
                }
            }

            OptionalValuesGrid.Grid.CommitEdit(DataGridEditingUnit.Row, true);

            mWin.Close();

            mParentObject.PropertyChangedEventHandler();
        }

        private void mAMDP_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            mParentObject.PropertyChangedEventHandler();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            List<OptionalValue> OptionalValuesToRemove = new List<OptionalValue>();
            foreach (OptionalValue selectedOV in OptionalValuesGrid.Grid.SelectedItems)
            { OptionalValuesToRemove.Add(selectedOV); }

            foreach (OptionalValue OV in OptionalValuesToRemove)
            {
                if (OV != null && !OV.Value.Equals(GlobalAppModelParameter.CURRENT_VALUE))
                {
                    if (OV.IsDefault && OptionalValuesGrid.Grid.Items.Count > 1)
                    {
                        OptionalValue newDefault = ((OptionalValue)(OptionalValuesGrid.Grid.Items[0]));
                        newDefault.IsDefault = true;
                        //binding is disabeled so setting the radio button as check manually
                        RadioButton rb = (RadioButton)OptionalValuesGrid.GetDataTemplateCellControl<RadioButton>(newDefault, 1);
                        rb.IsChecked = true;
                    }

                    mParentObject.OptionalValuesList.RemoveItem(OV);
                    editWasDone = true;
                }
            }
        }
        
        public bool ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button OKButton = new Button();
            OKButton.Content = "OK";
            OKButton.Click += new RoutedEventHandler(OKButton_Click);

            if (mSelectionModePage)
            { OptionalValuesGrid.ShowToolsBar = Visibility.Collapsed; }

            GenericWindow.LoadGenericWindow(ref mWin, null, windowStyle, this.Title, this, new ObservableList<Button> { OKButton }, showClosebtn: false);

            return editWasDone;
        }

        private void DefaultValueRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            OptionalValuesGrid.Grid.CommitEdit(DataGridEditingUnit.Row, true);

            RadioButton isDefaultRb = (RadioButton)sender;

            OptionalValue clickedOv = mParentObject.OptionalValuesList.Where(x => x.Guid == (Guid)isDefaultRb.Tag).FirstOrDefault();
            if (clickedOv != null && clickedOv.IsDefault != true)
            { clickedOv.IsDefault = true; }

            foreach (OptionalValue nonClickedOv in mParentObject.OptionalValuesList.Where(x => x.Guid != (Guid)isDefaultRb.Tag).ToList())
            {
                if (nonClickedOv.IsDefault != false)
                { nonClickedOv.IsDefault = false; }
            }

            editWasDone = true;
            OptionalValuesGrid.Grid.CommitEdit(DataGridEditingUnit.Row, true);
        }
    }
}
