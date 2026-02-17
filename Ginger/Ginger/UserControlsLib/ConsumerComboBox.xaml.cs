#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using GingerCore.Activities;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ginger.UserControlsLib
{
    /// <summary>
    /// Interaction logic for ConsumerComboBox.xaml
    /// </summary>
    public partial class ConsumerComboBox : UserControl
    {
        private ObservableCollection<Node> _nodeList;
        public ConsumerComboBox()
        {
            InitializeComponent();
            _nodeList = [];
            if (ConsumerSource != null)
            {
                CollectionChangedEventManager.AddHandler(ConsumerSource, ConsumerSource_CollectionChanged);
            }
            SetText();
        }

        #region Dependency Properties

        public static readonly DependencyProperty ConsumerSourceProperty =
             DependencyProperty.Register("ConsumerSource", typeof(ObservableList<Consumer>), typeof(ConsumerComboBox), new FrameworkPropertyMetadata(null,
        new PropertyChangedCallback(ConsumerComboBox.OnConsumerSourceChanged)));

        public static readonly DependencyProperty SelectedConsumerProperty =
         DependencyProperty.Register("SelectedConsumer", typeof(ObservableList<Consumer>), typeof(ConsumerComboBox), new FrameworkPropertyMetadata(null,
         new PropertyChangedCallback(ConsumerComboBox.OnSelectedConsumerChanged)));

        public static readonly DependencyProperty TextProperty =
           DependencyProperty.Register("Text", typeof(string), typeof(ConsumerComboBox), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register("DefaultText", typeof(string), typeof(ConsumerComboBox), new UIPropertyMetadata(string.Empty));


        public ObservableList<Consumer> ConsumerSource
        {
            get { return (ObservableList<Consumer>)GetValue(ConsumerSourceProperty); }
            set
            {
                SetValue(ConsumerSourceProperty, value);
            }
        }

        public ObservableList<Consumer> SelectedConsumer
        {
            get { return (ObservableList<Consumer>)GetValue(SelectedConsumerProperty); }
            set
            {
                SetValue(SelectedConsumerProperty, value);
            }
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

        private static void OnConsumerSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ConsumerComboBox control = (ConsumerComboBox)d;
            control.DisplayInConsumer();
            control.SelectNodes();
            control.SetSelectedConsumer();

            if (e.OldValue is not null and ObservableList<Consumer> oldConsumerSource)
            {
                CollectionChangedEventManager.RemoveHandler(oldConsumerSource, control.ConsumerSource_CollectionChanged);
            }

            if (control.ConsumerSource != null)
            {
                CollectionChangedEventManager.RemoveHandler(control.ConsumerSource, control.ConsumerSource_CollectionChanged);
                CollectionChangedEventManager.AddHandler(control.ConsumerSource, control.ConsumerSource_CollectionChanged);
            }
        }

        private void ConsumerSource_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            DisplayInConsumer();
            SelectNodes();
            SetSelectedConsumer();
        }

        private static void OnSelectedConsumerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ConsumerComboBox control = (ConsumerComboBox)d;
            control.SelectNodes();
            control.SetText();
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler? handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void ConsumerGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //to handle click of entire ComboBox item we also intercept Grid MouseUp event
            Grid grid = (Grid)sender;
            CheckBox checkBox = (CheckBox)grid.Children[0];
            if (checkBox.IsChecked == null)
            {
                return;
            }

            bool isChecked = (bool)checkBox.IsChecked;
            checkBox.IsChecked = !isChecked;
            if (isChecked)
            {
                checkBox.RaiseEvent(new RoutedEventArgs(CheckBox.UncheckedEvent));
            }
            else
            {
                checkBox.RaiseEvent(new RoutedEventArgs(CheckBox.CheckedEvent));
            }

            //prevent ComboBox dropdown from closing after clicking an item
            e.Handled = true;
        }

        private void CheckBox_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            SetSelectedConsumer();
            SetText();
        }
        #endregion


        #region Methods
        private void SelectNodes()
        {
            if (SelectedConsumer != null && SelectedConsumer.Count > 0)
            {
                foreach (Consumer consumer in SelectedConsumer)
                {
                    Node? node = _nodeList.FirstOrDefault(n => n.Consumer.ConsumerGuid == consumer.ConsumerGuid);
                    if (node != null)
                    {
                        node.IsSelected = true;
                    }
                }
            }
        }

        private void SetSelectedConsumer()
        {
            if (SelectedConsumer == null)
            {
                return;
            }

            ObservableList<Consumer> temp = [];
            foreach (Node node in _nodeList)
            {
                if (node.IsSelected)
                {
                    temp.Add(node.Consumer);
                }
            }
            SelectedConsumer.ClearAll();
            foreach (Consumer consumer in temp)
            {
                SelectedConsumer.Add(consumer);
            }
        }

        private void DisplayInConsumer()
        {
            _nodeList.Clear();

            foreach (Consumer keyValue in this.ConsumerSource)
            {
                Node node = new Node(keyValue);
                _nodeList.Add(node);
            }
            ConsumerCombo.ItemsSource = _nodeList;
        }

        private void SetText()
        {
            if (this.SelectedConsumer != null)
            {
                StringBuilder displayText = new StringBuilder();
                foreach (Consumer consumer in SelectedConsumer)
                {
                    if (consumer.Name == null)
                    {
                        consumer.Name = GetConsumerName(consumer.ConsumerGuid);
                    }
                    displayText.Append(consumer.Name);
                    displayText.Append(',');
                }
                Text = General.EscapeAccessKey(displayText.ToString().TrimEnd(','));
            }
            // set DefaultText if nothing else selected
            if (string.IsNullOrEmpty(Text))
            {
                Text = General.EscapeAccessKey(DefaultText);
            }
        }

        private string? GetConsumerName(Guid consumerGuid)
        {
            return
                WorkSpace
                .Instance
                .Solution
                .GetSolutionTargetApplications()
                .FirstOrDefault(targetApp => targetApp.Guid == consumerGuid)
                ?.Name;
        }


        #endregion

        public class Node : INotifyPropertyChanged
        {

            public string Title => _consumer.Name;

            private bool _isSelected;
            public bool IsSelected
            {
                get
                {
                    return _isSelected;
                }
                set
                {
                    _isSelected = value;
                    NotifyPropertyChanged(nameof(IsSelected));
                }
            }

            private Consumer _consumer;
            public Consumer Consumer
            {
                get
                {
                    return _consumer;
                }
                set
                {
                    _consumer = value;
                    NotifyPropertyChanged(nameof(Consumer));
                }
            }

            public Node(Consumer consumer)
            {
                _consumer = consumer;
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void NotifyPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

        }
    }
}
