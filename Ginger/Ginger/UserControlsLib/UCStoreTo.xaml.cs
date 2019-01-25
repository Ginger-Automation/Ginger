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
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using Ginger.Actions;
using GingerCore;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using System.Windows.Media;
using GingerCore.DataSource;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.UserControlsLib
{
    /// <summary>
    /// Interaction logic for UCStoreTo.xaml
    /// </summary>
    public partial class UCStoreTo : UserControl, INotifyPropertyChanged
    {
        public static DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register("ItemsSource", typeof(ObservableList<string>), typeof(UCStoreTo), new PropertyMetadata(OnItemsSourcePropertyChanged));

        public static DependencyProperty ItemsSourceGlobalParamProperty =
        DependencyProperty.Register("ItemsSourceGlobalParam", typeof(ObservableList<GingerCoreNET.GeneralLib.General.ComboItem>), typeof(UCStoreTo), new PropertyMetadata(OnItemsSourceGlobalParamPropertyChanged));

        public static DependencyProperty TextProperty =
        DependencyProperty.Register("TextProp", typeof(string), typeof(UCStoreTo),new PropertyMetadata(OnTextPropertyChanged));

        public static DependencyProperty CheckedProperty =
        DependencyProperty.Register("Checked", typeof(string), typeof(UCStoreTo), new PropertyMetadata(OnCheckedPropertyChanged));

        public static DependencyProperty VarLabelProperty =
        DependencyProperty.Register("VarLabel", typeof(string), typeof(UCStoreTo), new PropertyMetadata(OnVarLabelPropertyChanged));

        public static DependencyProperty AllowStoreProperty =
        DependencyProperty.Register("AllowStore", typeof(bool), typeof(UCStoreTo), new PropertyMetadata(OnAllowStorePropertyChanged));
        private string mLabel;
        public UCStoreTo()
        {
            ActReturnValue actR = new ActReturnValue();
            InitializeComponent();
            mLabel = ActReturnValue.eStoreTo.Variable.ToString();
            GingerCore.General.FillComboItemsFromEnumType(cmbStoreTo, actR.StoreTo.GetType());            
            if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>().Count == 0)
            {
                GingerCore.General.DisableComboItem(cmbStoreTo,ActReturnValue.eStoreTo.DataSource);                
                DSConfig.IsEnabled = false;
            }            
        }

        public List<string> ItemsSource
        {
            get { return (List <string>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value);}
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        
        public string TextProp
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }  
        }

        public string Checked
        {
            get { return (string)GetValue(CheckedProperty); }
            set { SetValue(CheckedProperty, value); }
        }
        
        private void DSConfig_Click(object sender, RoutedEventArgs e)
        {
            ActDataSourcePage ADSP;
            if (((Button)sender).DataContext.GetType() == typeof(ActReturnValue))
                 ADSP = new ActDataSourcePage(DSVE, ActReturnValue.Fields.StoreToDataSource);
            else
                ADSP = new ActDataSourcePage(DSVE, ActReturnValue.Fields.StoreToDataSource, "Get Value");
            ADSP.ShowAsWindow();
        }
        private static void OnItemsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as UCStoreTo;
            if (control != null)             
                control.OnItemsSourceChanged((ObservableList<string>)args.NewValue);
        }
        private void OnItemsSourceChanged(ObservableList<string> list)
        {
            string str = "";
            
            list.Remove(str);
            if (list.Count == 0)            
                GingerCore.General.DisableComboItem(cmbStoreTo,(object)mLabel);                           
            VariableList.ItemsSource = list;
        }

        private void chkStoreToValid()//4
        {
            bool isValid = true;
            if ((this.Checked == ActReturnValue.eStoreTo.Variable.ToString() && !GingerCore.General.CheckComboItemExist(VariableList,this.TextProp))
                || (this.Checked == ActReturnValue.eStoreTo.DataSource.ToString() && GingerCore.General.CheckDataSource(this.TextProp,WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>()) != "")
                || (this.Checked == ActReturnValue.eStoreTo.ApplicationModelParameter.ToString() && !GingerCore.General.CheckComboItemExist(xModelsParamsComboBox, this.TextProp, "Value")))
                isValid = false;

            if (isValid== false)
            {
                this.BorderThickness = new Thickness(1);
                this.BorderBrush = Brushes.Red;               
            }
            else
            {
                this.BorderThickness = new Thickness(0);
                this.BorderBrush = null;
            }            
        }
        //Fill ApplicationModelParameter Global Param combobox with values
        private static void OnItemsSourceGlobalParamPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as UCStoreTo;
            if (control != null)
                control.OnItemsSourceGlobalParamChanged((ObservableList<GingerCoreNET.GeneralLib.General.ComboItem>)args.NewValue);
        }
        private void OnItemsSourceGlobalParamChanged(ObservableList<GingerCoreNET.GeneralLib.General.ComboItem> list)
        {
            if (list.Count == 0)            
                GingerCore.General.DisableComboItem(cmbStoreTo,ActReturnValue.eStoreTo.ApplicationModelParameter);
                
            xModelsParamsComboBox.DisplayMemberPath = nameof(GingerCoreNET.GeneralLib.General.ComboItem.text);
            xModelsParamsComboBox.SelectedValuePath = nameof(GingerCoreNET.GeneralLib.General.ComboItem.Value);
            xModelsParamsComboBox.ItemsSource = list.OrderBy(nameof(GingerCoreNET.GeneralLib.General.ComboItem.text));
        }

        private static void OnVarLabelPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as UCStoreTo;
            if (control != null)
                control.OnVarLabelChanged((string)args.NewValue);
        }

        private void OnVarLabelChanged(string label)
        {            
            mLabel = label;
            GingerCore.General.UpdateComboItem(cmbStoreTo, ActReturnValue.eStoreTo.Variable, label);                        
            GingerCore.General.DisableComboItem(cmbStoreTo, (object)mLabel);
            GingerCore.General.DisableComboItem(cmbStoreTo,ActReturnValue.eStoreTo.DataSource);
            GingerCore.General.DisableComboItem(cmbStoreTo, ActReturnValue.eStoreTo.ApplicationModelParameter);            
        }

        private static void OnAllowStorePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as UCStoreTo;
            if (control != null)
                control.OnAllowStoreChanged((bool)args.NewValue);
        }

        private void OnAllowStoreChanged(bool allowStore)
        {
            if(allowStore == true)
            {
                if(VariableList != null && VariableList.Items.Count != 0)
                    GingerCore.General.EnableComboItem(cmbStoreTo, (object)mLabel);                
                if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>().Count == 0)
                {                    
                    GingerCore.General.DisableComboItem(cmbStoreTo,ActReturnValue.eStoreTo.DataSource);                    
                    DSConfig.IsEnabled = false;
                }                    
                else
                {                    
                    GingerCore.General.EnableComboItem(cmbStoreTo,ActReturnValue.eStoreTo.DataSource);                    
                    DSConfig.IsEnabled = true;
                }
            }
        }           

        private static void OnTextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as UCStoreTo;
            if (control != null)
                control.OnTextChanged((string)args.NewValue);           
        }
        private void OnTextChanged(string StoreValue)
        {
            if (String.IsNullOrEmpty(StoreValue))
                return;
            if (cmbStoreTo.SelectedValue == null)
                return;
            if (cmbStoreTo.SelectedValue.ToString() == mLabel && !StoreValue.StartsWith("{DS Name"))                
                GingerCore.General.SelectComboValue(VariableList, StoreValue);
            else if (cmbStoreTo.SelectedValue.ToString() == ActReturnValue.eStoreTo.DataSource.ToString() && StoreValue.StartsWith("{DS Name"))            
                DSVE.Text = StoreValue;
            chkStoreToValid();
        }

        private static void OnCheckedPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)//2
        {            
            var control = (UCStoreTo)sender;
            if (control != null && control.DataContext.GetType() == typeof(ActReturnValue))
                control.OnCheckChanged((string)args.NewValue,"Set Value");
            else
                control.OnCheckChanged((string)args.NewValue, "Get Value");            
        }
        
        private void OnCheckChanged(string StoreTo,string ControlAction)//3
        {
            Guid refGuid = new Guid();
            if (StoreTo == ActReturnValue.eStoreTo.Variable.ToString())
            {
                GingerCore.General.SelectComboValue(cmbStoreTo, mLabel);
                if (String.IsNullOrEmpty(this.TextProp) || this.TextProp == DSVE.Text || Guid.TryParse(this.TextProp, out refGuid) == true)
                {
                    VariableList.SelectedIndex = 0;
                    this.TextProp = VariableList.SelectedItem.ToString();
                }
                else
                    VariableList.Text = this.TextProp;
            }

            else if(StoreTo == ActReturnValue.eStoreTo.ApplicationModelParameter.ToString())
            {
                GingerCore.General.SelectComboValue(cmbStoreTo, ActReturnValue.eStoreTo.ApplicationModelParameter.ToString());
                if (Guid.TryParse(this.TextProp, out refGuid) == false)
                {
                    xModelsParamsComboBox.SelectedIndex = 0;
                    this.TextProp = ((GingerCoreNET.GeneralLib.General.ComboItem)xModelsParamsComboBox.SelectedItem).Value.ToString();
                }
                else
                    xModelsParamsComboBox.SelectedValue = this.TextProp;
            }

            else if (StoreTo == ActReturnValue.eStoreTo.DataSource.ToString())
            {
                GingerCore.General.SelectComboValue(cmbStoreTo, ActReturnValue.eStoreTo.DataSource.ToString());
                if (String.IsNullOrEmpty(this.TextProp) || this.TextProp == VariableList.Text || Guid.TryParse(this.TextProp, out refGuid) == true)
                {
                    ActDataSourcePage ADSP = new ActDataSourcePage(DSVE, this.TextProp, ControlAction);
                    ADSP.ShowAsWindow();
                    this.TextProp = DSVE.Text;
                }
                else
                    DSVE.Text = this.TextProp;
            }

            else if (StoreTo == ActReturnValue.eStoreTo.None.ToString())
            {
                GingerCore.General.SelectComboValue(cmbStoreTo, ActReturnValue.eStoreTo.None.ToString());
                this.TextProp = "";
                DSVE.Text = "";
                VariableList.Text="";
            }
            chkStoreToValid();
        }

        private void DSVE_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.TextProp = DSVE.Text;            
        }

        private void VariableList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(VariableList.SelectedItem != null)
                this.TextProp = VariableList.SelectedItem.ToString();
        }

        private void xModelsParamsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xModelsParamsComboBox.SelectedItem != null)
                this.TextProp = ((GingerCoreNET.GeneralLib.General.ComboItem)xModelsParamsComboBox.SelectedItem).Value.ToString();
        }
        
        private void cmbStoreTo_SelectionChanged(object sender, SelectionChangedEventArgs e)//1
        {
            if (cmbStoreTo.SelectedValue == null)
                return;

            if (cmbStoreTo.SelectedValue.ToString() == mLabel)
            {
                ValueCol.Width = new GridLength(105,GridUnitType.Star);
                if (VariableList != null)
                {
                    VariableList.Visibility = Visibility.Visible;
                    xModelsParamsComboBox.Visibility = Visibility.Hidden;
                    DSConfig.Visibility = Visibility.Hidden;
                    DSVE.Visibility = Visibility.Hidden;
                }
                this.Checked = ActReturnValue.eStoreTo.Variable.ToString(); //"Variable";  
            }
            else if (cmbStoreTo.SelectedValue.ToString() == ActReturnValue.eStoreTo.ApplicationModelParameter.ToString())
            {
                ValueCol.Width = new GridLength(105, GridUnitType.Star);
                if (xModelsParamsComboBox != null)
                {
                    xModelsParamsComboBox.Visibility = Visibility.Visible;
                    VariableList.Visibility = Visibility.Hidden;
                    DSConfig.Visibility = Visibility.Hidden;
                    DSVE.Visibility = Visibility.Hidden;
                }
                this.Checked = ActReturnValue.eStoreTo.ApplicationModelParameter.ToString();
            }
            else if (cmbStoreTo.SelectedValue.ToString() == ActReturnValue.eStoreTo.DataSource.ToString())
            {
                ValueCol.Width = new GridLength(105, GridUnitType.Star);
                if (VariableList != null)
                {
                    VariableList.Visibility = Visibility.Hidden;
                    xModelsParamsComboBox.Visibility = Visibility.Hidden;
                    DSConfig.Visibility = Visibility.Visible;
                    DSVE.Visibility = Visibility.Visible;
                }
                this.Checked = ActReturnValue.eStoreTo.DataSource.ToString();// "DataSource";      
            }
            else if (cmbStoreTo.SelectedValue.ToString() == ActReturnValue.eStoreTo.None.ToString())
            {
                ValueCol.Width = new GridLength(0);
                if (VariableList != null)
                {
                    VariableList.Visibility = Visibility.Hidden;
                    xModelsParamsComboBox.Visibility = Visibility.Hidden;
                    DSConfig.Visibility = Visibility.Hidden;
                    DSVE.Visibility = Visibility.Hidden;
                }

                this.Checked = ActReturnValue.eStoreTo.None.ToString(); //"None"
            }            
        }        
    }
}
