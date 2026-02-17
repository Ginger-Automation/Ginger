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
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.TagsLib
{
    /// <summary>
    /// Interaction logic for TagsSelectorPage.xaml
    /// </summary>
    public partial class TagsSelectorPage : Page
    {
        ObservableList<Guid> mTags;
        ObservableList<SelectedTag> list;
        GenericWindow _pageGenericWin = null;

        public class SelectedTag
        {
            public bool Selected { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public Guid GUID { get; set; }
        }

        public TagsSelectorPage(ObservableList<Guid> Tags)
        {
            InitializeComponent();
            mTags = Tags;
            SetGridView();
            ShowTags();
        }

        private void SetGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = "Selected", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.CheckBox },
                new GridColView() { Field = "Name", Header = "Name", WidthWeight = 50, ReadOnly = true, BindingMode = BindingMode.OneWay },
                new GridColView() { Field = "Description", Header = "Description", WidthWeight = 30, ReadOnly = true, BindingMode = BindingMode.OneWay },
            ]
            };

            TagsGrid.SetAllColumnsDefaultView(view);
            TagsGrid.InitViewItems();
        }

        private void ShowTags()
        {
            // We create a temp list for selection and mark the selected if exist already
            list = [];
            foreach (RepositoryItemTag t in WorkSpace.Instance.Solution.Tags)
            {
                SelectedTag st = new SelectedTag
                {
                    Name = t.Name,
                    Description = t.Description,
                    GUID = t.Guid
                };

                if (mTags.Contains(t.Guid))
                {
                    st.Selected = true;
                }

                list.Add(st);
            }

            TagsGrid.DataSourceList = list;
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool ShowCancelButton = true)
        {
            Button okBtn = new Button
            {
                Content = "Ok"
            };
            okBtn.Click += new RoutedEventHandler(OKButton_Click);
            ObservableList<Button> winButtons = [okBtn];

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, winButtons, ShowCancelButton, "Cancel");
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            mTags.Clear();
            foreach (SelectedTag TA in list)
            {
                if (TA.Selected)
                {
                    mTags.Add(TA.GUID);
                }
            }
            _pageGenericWin.Close();
        }
    }
}
