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
using Ginger.UserControls;
using GingerCore;
using System.Windows;
using System.Windows.Controls;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;
using System;

namespace Ginger.TagsLib
{
    /// <summary>
    /// Interaction logic for TagsEditorPage.xaml
    /// </summary>
    public partial class SolutionTagsEditorPage : Page
    {
        private ObservableList<RepositoryItemTag> mTags;
        GenericWindow genWin = null;

        public SolutionTagsEditorPage(ObservableList<RepositoryItemTag> tags)
        {
            InitializeComponent();

            this.mTags = tags;
            SetTagsGridView();
            TagsGrid.DataSourceList = tags;
            TagsGrid.Grid.CellEditEnding += Grid_CellEditEnding;
        }

        private void Grid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            foreach (RepositoryItemTag tag in mTags)
            {
                RepositoryItemTag newTag = new RepositoryItemTag();
                newTag.Guid = tag.Guid;
                newTag.Name = tag.Name;
                newTag.Description = tag.Description;
                newTag.ItemName = tag.ItemName;
            }
        }

        private void SetTagsGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();

            defView.GridColsView.Add(new GridColView() { Field = RepositoryItemTag.Fields.Name, Header = "Name", WidthWeight = 40 });
            defView.GridColsView.Add(new GridColView() { Field = RepositoryItemTag.Fields.Description, Header = "Description", WidthWeight = 40 });

            TagsGrid.SetAllColumnsDefaultView(defView);
            TagsGrid.InitViewItems();

            TagsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddButton));
        }

        private void AddButton(object sender, RoutedEventArgs e)
        {
            TagsGrid.Grid.CommitEdit(DataGridEditingUnit.Row, true);
            mTags.Add(new RepositoryItemTag());
        }

        public void ShowAsWindow()
        {
            this.Height = 400;
            this.Width = 400;

            Button saveBtn = new Button();
            saveBtn.Content = "Save";
            saveBtn.Click += new RoutedEventHandler(saveBtn_Click);
            ObservableList<Button> winButtons = new ObservableList<Button>();
            winButtons.Add(saveBtn);

            GingerCore.General.LoadGenericWindow(ref genWin, null, eWindowShowStyle.Free, "Solution Tags Edit Page", this, winButtons, closeEventHandler: closeBtn_Click);
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            Reporter.ToGingerHelper(eGingerHelperMsgKey.SaveItem, null, App.UserProfile.Solution.Name, "item");
            TagsGrid.Grid.CommitEdit(DataGridEditingUnit.Row, true);
            CleanUnValidTags();
            App.UserProfile.Solution.Save();
            Reporter.CloseGingerHelper();
        }

        private void closeBtn_Click(object sender, EventArgs e)
        {
            CleanUnValidTags();
            TagsGrid.Grid.CommitEdit(DataGridEditingUnit.Row, true);
            genWin.Close();
        }

        private void CleanUnValidTags()
        {
            //remove empty tags
            for (int i = 0; i < mTags.Count; i++)
                if (string.IsNullOrEmpty(mTags[i].Name))
                {
                    mTags.RemoveAt(i);
                    i--;
                }

            //remove duplicated tags
            for (int i = 0; i < mTags.Count; i++)
                for (int j = i + 1; j < mTags.Count; j++)
                    if (mTags[i].Name.Trim().ToLower() == mTags[j].Name.Trim().ToLower())
                    {
                        mTags.RemoveAt(j);
                        j--;
                    }
        }
    }
}
