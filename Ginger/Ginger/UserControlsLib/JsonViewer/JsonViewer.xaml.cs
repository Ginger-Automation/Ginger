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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace JsonViewerDemo.JsonViewer
{
    /// <summary>
    /// Interaction logic for JsonViewer.xaml
    /// </summary>
    public partial class JsonViewer : UserControl
    {
        private const GeneratorStatus Generated = GeneratorStatus.ContainersGenerated;
        private DispatcherTimer _timer;

        public JsonViewer()
        {
            InitializeComponent();
        }

        public void Load(string json)
        {
            JsonTreeView.ItemsSource = null;
            JsonTreeView.Items.Clear();

            var children = new List<JToken>();

            try
            {
                var token = JToken.Parse(json);

                if (token != null)
                {
                    children.Add(token);
                }

                JsonTreeView.ItemsSource = children;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open the JSON string:\r\n" + ex.Message);
            }
        }

        private void JValue_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2)
                return;

            if (sender is TextBlock tb)
            {
                GingerCore.General.SetClipboardText(tb.Text);
            }
        }

        private void ExpandAll(object sender, RoutedEventArgs e)
        {
            ToggleItems(true);
        }

        private void CollapseAll(object sender, RoutedEventArgs e)
        {
            ToggleItems(false);
        }

        private void ToggleItems(bool isExpanded)
        {
            if (JsonTreeView.Items.IsEmpty)
                return;

            var prevCursor = Cursor;
            //System.Windows.Controls.DockPanel.Opacity = 0.2;
            //System.Windows.Controls.DockPanel.IsEnabled = false;
            Cursor = Cursors.Wait;
            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Normal, delegate
            {
                ToggleItems(JsonTreeView, JsonTreeView.Items, isExpanded);
                //System.Windows.Controls.DockPanel.Opacity = 1.0;
                //System.Windows.Controls.DockPanel.IsEnabled = true;
                _timer.Stop();
                Cursor = prevCursor;
            }, Application.Current.Dispatcher);
            _timer.Start();
        }

        private void ToggleItems(ItemsControl parentContainer, ItemCollection items, bool isExpanded)
        {
            var itemGen = parentContainer.ItemContainerGenerator;
            if (itemGen.Status == Generated)
            {
                Recurse(items, isExpanded, itemGen);
            }
            else
            {
                itemGen.StatusChanged += delegate
                {
                    Recurse(items, isExpanded, itemGen);
                };
            }
        }

        private void Recurse(ItemCollection items, bool isExpanded, ItemContainerGenerator itemGen)
        {
            if (itemGen.Status != Generated)
                return;

            foreach (var item in items)
            {
                var tvi = itemGen.ContainerFromItem(item) as TreeViewItem;
                tvi.IsExpanded = isExpanded;
                ToggleItems(tvi, tvi.Items, isExpanded);
            }
        }
    }
}
