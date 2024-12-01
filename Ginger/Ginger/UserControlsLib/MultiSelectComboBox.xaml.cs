#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Amdocs.Ginger.Common.VariablesLib;
using Amdocs.Ginger.CoreNET.ActionsLib.UI.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.UserControlsLib
{
    /// <summary>
    /// Interaction logic for MultiSelectComboBox.xaml
    /// </summary>
    public partial class MultiSelectComboBox : UserControl
    {
        public ObservableCollection<Node> _nodeList;
        private object obj;
        private string AttrName;
        private bool ShowEnumDesc = false;
        public MultiSelectComboBox()
        {
            InitializeComponent();
            _nodeList = [];
        }

        #region Dependency Properties

        public static readonly DependencyProperty ItemsSourceProperty =
             DependencyProperty.Register("ItemsSource", typeof(Dictionary<string, object>), typeof(MultiSelectComboBox), new FrameworkPropertyMetadata(null,
        new PropertyChangedCallback(MultiSelectComboBox.OnItemsSourceChanged)));

        public static readonly DependencyProperty SelectedItemsProperty =
         DependencyProperty.Register("SelectedItems", typeof(ObservableList<OperationValues>), typeof(MultiSelectComboBox), new FrameworkPropertyMetadata(null,
         new PropertyChangedCallback(MultiSelectComboBox.OnSelectedItemsChanged)));

        public static readonly DependencyProperty TextProperty =
           DependencyProperty.Register("Text", typeof(string), typeof(MultiSelectComboBox), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register("DefaultText", typeof(string), typeof(MultiSelectComboBox), new UIPropertyMetadata(string.Empty));

        public static DependencyProperty OperationSelectedValuesProperty =
        DependencyProperty.Register("OperationSelectedItems", typeof(ObservableList<OperationValues>), typeof(MultiSelectComboBox), new PropertyMetadata(OnOperationSelectedValuesPropertyChanged));

        public Dictionary<string, object> ItemsSource
        {
            get { return (Dictionary<string, object>)GetValue(ItemsSourceProperty); }
            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        public ObservableList<OperationValues> SelectedItems
        {
            get { return (ObservableList<OperationValues>)GetValue(SelectedItemsProperty); }
            set
            {
                SetValue(SelectedItemsProperty, value);
            }
        }

        public ObservableList<OperationValues> OperationSelectedItems
        {
            get { return (ObservableList<OperationValues>)GetValue(OperationSelectedValuesProperty); }
            set { SetValue(OperationSelectedValuesProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string DefaultText
        {
            get { return (string)GetValue(DefaultTextProperty); }
            set { SetValue(DefaultTextProperty, value); }
        }
        #endregion

        #region Events
        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiSelectComboBox control = (MultiSelectComboBox)d;
            control.DisplayInControl();
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiSelectComboBox control = (MultiSelectComboBox)d;
            control.SelectNodes();
            control.SetText();
        }

        private static void OnOperationSelectedValuesPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is MultiSelectComboBox control)
            {
                control.OperationSelectedValuesPropertyChanged((ObservableList<OperationValues>)args.NewValue);
            }
        }

        private void OperationSelectedValuesPropertyChanged(ObservableList<OperationValues> oprationSelectedValues)
        {
            OnPropertyChanged(nameof(OperationSelectedItems));
            SelectedItems = OperationSelectedItems;
            if (this.ItemsSource.Count > 0 && this.SelectedItems != null && this.ItemsSource.Count == this.SelectedItems.Count)
            {
                foreach (Node node in _nodeList)
                {
                    if (node.Title == "All")
                    {
                        node.IsSelected = true;
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event EventHandler ItemCheckBoxClick;

        public void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox clickedBox = (CheckBox)sender;

            if (clickedBox.Content.ToString() == "All")
            {
                foreach (Node node in _nodeList)
                {
                    node.IsSelected = (bool)clickedBox.IsChecked;
                }

            }
            else
            {
                int _selectedCount = 0;
                if (ShowEnumDesc)
                {
                    foreach (Node s in _nodeList)
                    {
                        if (s.IsSelected && s._title != "All")
                        {
                            _selectedCount++;
                        }
                    }
                    if (_selectedCount == _nodeList.Count - 1)
                    {
                        _nodeList.FirstOrDefault(i => i._title == "All").IsSelected = true;
                    }
                    else
                    {
                        _nodeList.FirstOrDefault(i => i._title == "All").IsSelected = false;
                    }
                }
                else
                {
                    foreach (Node s in _nodeList)
                    {
                        if (s.IsSelected && s.Title != "All")
                        {
                            _selectedCount++;
                        }
                    }
                    if (_selectedCount == _nodeList.Count - 1)
                    {
                        _nodeList.FirstOrDefault(i => i.Title == "All").IsSelected = true;
                    }
                    else
                    {
                        _nodeList.FirstOrDefault(i => i.Title == "All").IsSelected = false;
                    }

                }


            }
            SetSelectedItems();
            SetText();
            ItemCheckBoxClick?.Invoke(sender: clickedBox, new EventArgs());
        }
        #endregion


        #region Methods
        private void SelectNodes()
        {
            foreach (OperationValues keyValue in SelectedItems)
            {
                if (ShowEnumDesc)
                {
                    Node node = _nodeList.FirstOrDefault(i => i._title == keyValue.Value);
                    if (node != null)
                    {
                        node.IsSelected = true;
                    }
                }
                else
                {
                    Node node = _nodeList.FirstOrDefault(i => i.Title == keyValue.Value);
                    if (node != null)
                    {
                        node.IsSelected = true;
                    }
                }

            }
        }

        private void SetSelectedItems()
        {
            ObservableList<OperationValues> temp = [];
            if (SelectedItems == null)
                SelectedItems = [];
            SelectedItems.Clear();
            foreach (Node node in _nodeList)
            {
                if (node._ShowEnumDesc)
                {
                    if (node.IsSelected && node._title != "All")
                    {
                        if (this.ItemsSource.Count > 0)
                        {
                            Guid guidOutput;
                            bool isValid = Guid.TryParse(this.ItemsSource[node._title].ToString(), out guidOutput);
                            if (isValid)
                            {
                                SelectedItems.Add(new OperationValues() { Value = node._title, Guid = guidOutput });
                                temp.Add(new OperationValues() { Value = node._title, Guid = guidOutput });
                            }
                            else
                            {
                                SelectedItems.Add(new OperationValues() { Value = node._title });
                                temp.Add(new OperationValues() { Value = node._title });
                            }
                        }

                    }
                }
                else
                {
                    if (node.IsSelected && node.Title != "All")
                    {
                        if (this.ItemsSource.Count > 0)
                        {
                            Guid guidOutput;
                            bool isValid = Guid.TryParse(this.ItemsSource[node.Title].ToString(), out guidOutput);
                            if (isValid)
                            {
                                SelectedItems.Add(new OperationValues() { Value = node.Title, Guid = guidOutput });
                                temp.Add(new OperationValues() { Value = node.Title, Guid = guidOutput });
                            }
                            else
                            {
                                SelectedItems.Add(new OperationValues() { Value = node.Title });
                                temp.Add(new OperationValues() { Value = node.Title });
                            }
                        }

                    }
                }

            }
            OperationSelectedItems = new ObservableList<OperationValues>(temp);
        }

        public void Init(object obj, string AttrName, bool ShowEnumDesc = false)
        {
            //// If the VE is on stand alone form:
            this.obj = obj;
            this.AttrName = AttrName;
            this.ShowEnumDesc = ShowEnumDesc;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(this, OperationSelectedValuesProperty, obj, AttrName);
        }

        private void DisplayInControl()
        {
            _nodeList.Clear();

            var objectlist = this.ItemsSource.Select(x => x.Value);
            this.ShowEnumDesc = objectlist.Any(x => x is OperationValues && !string.IsNullOrEmpty(((OperationValues)x).DisplayName));
            if (this.ItemsSource.Count > 0)
            {
                _nodeList.Add(new Node("All", this.ShowEnumDesc));
            }
            foreach (KeyValuePair<string, object> keyValue in this.ItemsSource)
            {
                Node node = new Node(keyValue.Key, this.ShowEnumDesc);
                _nodeList.Add(node);
            }
            MultiSelectCombo.ItemsSource = _nodeList;
        }

        private void SetText()
        {
            if (this.SelectedItems != null)
            {
                StringBuilder displayText = new StringBuilder();
                bool allSelected = _nodeList.Count(x => x.IsSelected) == _nodeList.Count - 1;
                foreach (Node s in _nodeList)
                {
                    if (s._ShowEnumDesc)
                    {
                        if (allSelected || (s.IsSelected && s._title == "All"))
                        {
                            displayText = new StringBuilder();
                            displayText.Append("All");
                            break;
                        }
                        else if (s.IsSelected && s._title != "All")
                        {
                            displayText.Append(s.Title);
                            displayText.Append(',');
                        }
                    }
                    else
                    {
                        if (allSelected || (s.IsSelected && s.Title == "All"))
                        {
                            displayText = new StringBuilder();
                            displayText.Append("All");
                            break;
                        }
                        else if (s.IsSelected && s.Title != "All")
                        {
                            displayText.Append(s.Title);
                            displayText.Append(',');
                        }
                    }
                }
                this.Text = displayText.ToString().TrimEnd(new char[] { ',' });
            }
            // set DefaultText if nothing else selected
            if (string.IsNullOrEmpty(this.Text))
            {
                this.Text = this.DefaultText;
            }
        }


        #endregion
    }

    public class Node : INotifyPropertyChanged
    {

        public string _title;
        private bool _isSelected;
        public bool _ShowEnumDesc = false;
        #region ctor
        public Node(string title, bool ShowEnumDesc)
        {
            Title = title;
            _ShowEnumDesc = ShowEnumDesc;
        }
        #endregion

        #region Properties
        public string Title
        {
            get
            {
                return _ShowEnumDesc ? GingerCore.General.GetEnumValueDescription(typeof(ActAccessibilityTesting.eTags), _title) : _title;
            }
            set
            {
                _title = value;
                NotifyPropertyChanged("Title");
            }
        }
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                NotifyPropertyChanged("IsSelected");
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
