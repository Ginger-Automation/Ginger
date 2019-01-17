#region License
/*
Copyright © 2014-2018 European Support Limited

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
using Amdocs.Ginger.Repository;
using Ginger;
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
        AppModelParameter mAMDP;
        HTMLElementInfo mHEI;
        bool editWasDone = false;
        bool mSelectionModePage;
        bool mIsAppModelParam = false;
        bool mIsHTMLElementInfoParam = false;

        public ModelOptionalValuesPage(AppModelParameter AMDP, bool selectionModePage = false)
        {
            InitializeComponent();
            mIsAppModelParam = true;
            mIsHTMLElementInfoParam = false;

            mAMDP = AMDP;
            mSelectionModePage = selectionModePage;

            OptionalValuesGrid.DataSourceList = mAMDP.OptionalValuesList;
            SetOptionalValuesGridView();

            if (!mSelectionModePage)
            {
                mAMDP.PropertyChanged += mAMDP_PropertyChanged;
                OptionalValuesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddOptionalValue));
                OptionalValuesGrid.SetbtnDeleteHandler(btnDelete_Click);
                OptionalValuesGrid.SetbtnClearAllHandler(btnClearAll_Click);
                OptionalValuesGrid.btnCopy.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnCopyClicked));
                OptionalValuesGrid.btnCut.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnCopyClicked));
                OptionalValuesGrid.btnPaste.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnPastClicked));
            }
            this.Title = AMDP.PlaceHolder + " " + "Optional Values:";
        }

        public ModelOptionalValuesPage(HTMLElementInfo HEI, bool selectionModePage = false)
        {
            InitializeComponent();
            mIsAppModelParam = false;
            mIsHTMLElementInfoParam = true;

            mHEI = HEI;
            mSelectionModePage = selectionModePage;

            OptionalValuesGrid.DataSourceList = mHEI.OptionalVals;
            SetOptionalValuesGridView();

            if (!mSelectionModePage)
            {
                mHEI.PropertyChanged += mAMDP_PropertyChanged;
                OptionalValuesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddOptionalValue));
                OptionalValuesGrid.SetbtnDeleteHandler(btnDelete_Click);
                OptionalValuesGrid.SetbtnClearAllHandler(btnClearAll_Click);
                OptionalValuesGrid.btnCopy.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnCopyClicked));
                OptionalValuesGrid.btnCut.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnCopyClicked));
                OptionalValuesGrid.btnPaste.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnPastClicked));
            }
            this.Title = HEI.ElementName.Replace("\n", "_").Replace("\r", "") + " " + "Possible Values";
        }

        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {
            if (mIsAppModelParam)
            {
                for (int i = 0; i < mAMDP.OptionalValuesList.Count; i++)
                {
                    OptionalValue ov = mAMDP.OptionalValuesList[i];
                    if (ov.Value != GlobalAppModelParameter.CURRENT_VALUE)
                    {
                        mAMDP.OptionalValuesList.RemoveItem(ov);
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
            else if(mIsHTMLElementInfoParam)
            {
                for (int i = 0; i < mHEI.OptionalVals.Count; i++)
                {
                    OptionalValue ov = mHEI.OptionalVals[i];
                    if (ov.Value != GlobalAppModelParameter.CURRENT_VALUE)
                    {
                        mHEI.OptionalVals.RemoveItem(ov);
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
                view.GridColsView.Add(new GridColView() { Field = nameof(OptionalValue.Value), ReadOnly = true, WidthWeight = 10 });

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
                    if (mIsAppModelParam)
                    {
                        foreach (OptionalValue OP in mAMDP.OptionalValuesList)
                        {
                            if (OP != CurrentOP && OP.Value == CurrentOP.Value)
                            {
                                CurrentOP.Value = OldValue;
                                Reporter.ToUser(eUserMsgKeys.SpecifyUniqueValue);
                                break;
                            }
                        } 
                    }
                    else if(mIsHTMLElementInfoParam)
                    {
                        foreach (OptionalValue OP in mHEI.OptionalVals)
                        {
                            if (OP != CurrentOP && OP.Value == CurrentOP.Value)
                            {
                                CurrentOP.Value = OldValue;
                                Reporter.ToUser(eUserMsgKeys.SpecifyUniqueValue);
                                break;
                            }
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
            if (mIsAppModelParam)
            {                
                mAMDP.OptionalValuesList.Add(newVal);
            }
            else if(mIsHTMLElementInfoParam)
            {
                mHEI.OptionalVals.Add(newVal);
            }            

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
                SavedDefaultOV = defaultOptionalValue;
        }

        private void BtnPastClicked(object sender, RoutedEventArgs e)
        {
            if (SavedDefaultOV != null)
                SavedDefaultOV.IsDefault = true;
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
                        ((OptionalValue)(OptionalValuesGrid.Grid.Items[0])).IsDefault = true;

                    if (mIsAppModelParam) mAMDP.OptionalValuesList.Remove(OV);
                    if (mIsHTMLElementInfoParam) mHEI.OptionalVals.Remove(OV);
                    i--;
                }
            }

            OptionalValuesGrid.Grid.CommitEdit(DataGridEditingUnit.Row, true);

            mWin.Close();

            if (mIsAppModelParam) mAMDP.OnPropertyChanged(nameof(AppModelParameter.OptionalValuesString));
            if (mIsHTMLElementInfoParam) mHEI.OnPropertyChanged(nameof(HTMLElementInfo.OpValsString));
        }

        private void mAMDP_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            List<OptionalValue> OptionalValuesToRemove = new List<OptionalValue>();
            foreach (OptionalValue selectedOV in OptionalValuesGrid.Grid.SelectedItems)
                OptionalValuesToRemove.Add(selectedOV);

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
                    if (mIsAppModelParam) mAMDP.OptionalValuesList.RemoveItem(OV);
                    if (mIsHTMLElementInfoParam) mHEI.OptionalVals.RemoveItem(OV);
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
                OptionalValuesGrid.ShowToolsBar = Visibility.Collapsed;

            GenericWindow.LoadGenericWindow(ref mWin, null, windowStyle, this.Title, this, new ObservableList<Button> { OKButton }, showClosebtn: false);

            return editWasDone;
        }

        private void DefaultValueRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            OptionalValuesGrid.Grid.CommitEdit(DataGridEditingUnit.Row, true);

            RadioButton isDefaultRb = (RadioButton)sender;

            if (mIsAppModelParam)
            {
                OptionalValue clickedOv = mAMDP.OptionalValuesList.Where(x => x.Guid == (Guid)isDefaultRb.Tag).FirstOrDefault();
                if (clickedOv != null && clickedOv.IsDefault != true)
                    clickedOv.IsDefault = true;

                foreach (OptionalValue nonClickedOv in mAMDP.OptionalValuesList.Where(x => x.Guid != (Guid)isDefaultRb.Tag).ToList())
                    if (nonClickedOv.IsDefault != false)
                        nonClickedOv.IsDefault = false; 
            }
            else if (mIsHTMLElementInfoParam)
            {
                OptionalValue clickedOv = mHEI.OptionalVals.Where(x => x.Guid == (Guid)isDefaultRb.Tag).FirstOrDefault();
                if (clickedOv != null && clickedOv.IsDefault != true)
                    clickedOv.IsDefault = true;

                foreach (OptionalValue nonClickedOv in mHEI.OptionalVals.Where(x => x.Guid != (Guid)isDefaultRb.Tag).ToList())
                    if (nonClickedOv.IsDefault != false)
                        nonClickedOv.IsDefault = false;
            }

            editWasDone = true;
            OptionalValuesGrid.Grid.CommitEdit(DataGridEditingUnit.Row, true);
        }
    }
}
