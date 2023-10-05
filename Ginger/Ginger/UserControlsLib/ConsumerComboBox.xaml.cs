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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Amdocs.Ginger.Common.VariablesLib;
using Amdocs.Ginger.Common;
using GingerCore.Activities;
using GingerCore.Platforms;

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
            this._nodeList = new ObservableCollection<Node>();
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
        }

        private static void OnSelectedConsumerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ConsumerComboBox control = (ConsumerComboBox)d;
            control.SelectNodes();
            control.SetText();
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

        private void ConsumerCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox clickedBox = (CheckBox)sender;

               
                
            
            SetSelectedConsumer();
            SetText();

        }
        #endregion


        #region Methods
        private void SelectNodes()
        {
            if(this.SelectedConsumer !=null && this.SelectedConsumer.Count>0)
            {
                foreach (Consumer consumer in this.SelectedConsumer)
                {
                    Node? node = this._nodeList.FirstOrDefault(n => n.Consumer.ConsumerGuid == consumer.ConsumerGuid);
                    if (node != null)
                    {
                        node.IsSelected = true;
                    }
                }
            }
            
        }

        private void SetSelectedConsumer()
        {
            ObservableList<Consumer> temp = new ObservableList<Consumer>();
            foreach (Node node in this._nodeList)
            {
               
                    if (node.IsSelected)
                    {
                       temp.Add(node.Consumer);
                    }
                
            }
            SelectedConsumer = new ObservableList<Consumer>(temp);
        }

        private void DisplayInConsumer()
        {
            this._nodeList.Clear();
            
            foreach (Consumer keyValue in this.ConsumerSource)
            {
                Node node = new(keyValue);
                this._nodeList.Add(node);
            }
            this.ConsumerCombo.ItemsSource = this._nodeList;
        }

        private void SetText()
        {
            if (this.SelectedConsumer != null)
            {
                StringBuilder displayText = new StringBuilder();
                foreach (Node s in this._nodeList)
                {
                     if (s.IsSelected)
                    {
                        displayText.Append(s.Title);
                        displayText.Append(',');
                    }
                }
                Text = displayText.ToString().TrimEnd(',');
            }
            // set DefaultText if nothing else selected
            if (string.IsNullOrEmpty(Text))
            {
                Text = DefaultText;
            }
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
