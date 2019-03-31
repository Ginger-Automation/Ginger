#region License
/*
Copyright © 2014-2019 European Support Limited

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

using Ginger;
using GingerCore;
using GingerCore.Actions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GingerWPF.BusinessFlowsLib
{
    /// <summary>
    /// Interaction logic for ActivityActionsPage.xaml
    /// </summary>
    public partial class ActivityActionsPage : Page
    {
        Activity mActivity;
        public ActivityActionsPage(Activity Activity)
        {
            InitializeComponent();

            mActivity = Activity;
            ActionsListBox.ItemsSource = mActivity.Acts;
            
            Activity.Acts.PropertyChanged += Acts_PropertyChanged;
            ActionsListBox.SelectionChanged += ActionsListBox_SelectionChanged;
        }

        private void SetListView()
        {
            GridView GV = new GridView();
            //TODO: add row num

            GridViewColumn GVC1 = new GridViewColumn() { Header = "Description", Width = 250, DisplayMemberBinding = new Binding(nameof(Act.Description)) };
            GV.Columns.Add(GVC1);

            GridViewColumn GVC2 = new GridViewColumn() { Header = "Status", Width = 100, DisplayMemberBinding = new Binding(nameof(Act.Status)) };
            GV.Columns.Add(GVC2);
         
            GridViewColumn GVC3 = new GridViewColumn() { Header = "Elapsed", Width = 50, DisplayMemberBinding = new Binding(nameof(Act.Elapsed)) };
            GV.Columns.Add(GVC3);

            GridViewColumn GVC4 = new GridViewColumn() { Header = "Error", Width = 100, DisplayMemberBinding = new Binding(nameof(Act.Error)) };
            GV.Columns.Add(GVC4);

            //Hide the List View header
            System.Windows.Style style = new System.Windows.Style();
            style.Setters.Add(new Setter() { Property = VisibilityProperty, Value = Visibility.Collapsed });
            GV.ColumnHeaderContainerStyle = style;

            ActionsListBox.View = GV;
        }

        private void ActionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Make the list synced with the Activity Acts, so if user change action the activity current act is the same
            mActivity.Acts.CurrentItem = ActionsListBox.SelectedItem;            
            UpdateFloatingButtons();            
        }

        private void UpdateFloatingButtons()
        {
            if (ActionsListBox.SelectedItem != null)
            {
                ListViewItem lvi = (ListViewItem)ActionsListBox.ItemContainerGenerator.ContainerFromItem(ActionsListBox.SelectedItem);
                if (lvi != null)
                {
                    Point rel = lvi.TranslatePoint(new Point(0, 0), ActionsListBox);
                    FloatingStackPanel.Margin = new Thickness(0, rel.Y, 0, 0);
                    FloatingStackPanel.Visibility = Visibility.Visible;
                }
            }
            else
            {
                FloatingStackPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void Acts_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentItem")
            {
                // since we can get event while GingerRunner is on another thread we need dispatcher
                ActionsListBox.Dispatcher.Invoke(() => {
                    ActionsListBox.SelectedItem = mActivity.Acts.CurrentItem;
                    ActionsListBox.Refresh();
                });
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Act act = (Act)mActivity.Acts.CurrentItem;
            GingerActionEditPage GAEP = new GingerActionEditPage(act);
            GAEP.ShowAsWindow();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Act act = (Act)mActivity.Acts.CurrentItem;
            int index = mActivity.Acts.IndexOf(act);
            Act newSelectedAct = null;
            if (mActivity.Acts.Count-1 > index)
            {
                newSelectedAct = (Act)mActivity.Acts[index + 1];
            }
            else
            {
                if (index != 0)
                {
                    newSelectedAct = (Act)mActivity.Acts[index - 1];
                }                
            }
            mActivity.Acts.Remove(act);
            mActivity.Acts.CurrentItem = newSelectedAct;
            UpdateFloatingButtons();
        }
    }
}
