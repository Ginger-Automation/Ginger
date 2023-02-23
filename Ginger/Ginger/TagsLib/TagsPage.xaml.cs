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
using Ginger.UserControls;
using GingerCore;
using System.Windows;
using System.Windows.Controls;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;
using System;
using Ginger.UserControlsLib;

namespace Ginger.TagsLib
{
    /// <summary>
    /// Interaction logic for TagsEditorPage.xaml
    /// </summary>
    public partial class TagsPage : GingerUIPage
    {
        public enum eViewMode { Solution, SpecificList }

        readonly eViewMode mViewMode;
        private ObservableList<RepositoryItemTag> mTags;
        GenericWindow genWin = null;

        public TagsPage(eViewMode viewMode, ObservableList<RepositoryItemTag> tags = null)
        {
            InitializeComponent();

            mViewMode = viewMode;
            mTags = tags;
            CurrentItemToSave = WorkSpace.Instance.Solution;
            if (mViewMode == eViewMode.Solution)
            {
                 WorkSpace.Instance.PropertyChanged += WorkSpacePropertyChanged;
            }

            SetTagsGridView();
            SetGridData();
        }

        private void WorkSpacePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WorkSpace.Solution))
            {
                SetGridData();
            }                
        }

        private void SetGridData()
        {
            if (mViewMode == eViewMode.Solution)
            {
                if ( WorkSpace.Instance.Solution != null)
                {
                    mTags =  WorkSpace.Instance.Solution.Tags;
                }
                else
                {
                    mTags = new ObservableList<RepositoryItemTag>();
                }
            }
            xTagsGrid.DataSourceList = mTags;
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
            if (mViewMode == eViewMode.Solution)
            {
                xTagsGrid.SetGridEnhancedHeader(Amdocs.Ginger.Common.Enums.eImageType.Tag, "Tags", saveAllHandler: saveBtn_Click, addHandler: AddButton, true);
                xTagsGrid.ShowUpDown = Visibility.Visible;
                xTagsGrid.ShowAdd = Visibility.Collapsed;
            }
            else
            {
                xTagsGrid.ShowTitle = Visibility.Collapsed;
                xTagsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddButton));
            }

            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = RepositoryItemTag.Fields.Name, Header = "Name", WidthWeight = 40 });
            defView.GridColsView.Add(new GridColView() { Field = RepositoryItemTag.Fields.Description, Header = "Description", WidthWeight = 40 });
            xTagsGrid.SetAllColumnsDefaultView(defView);
            xTagsGrid.InitViewItems();

            xTagsGrid.Grid.CellEditEnding += Grid_CellEditEnding;
        }

        private void AddButton(object sender, RoutedEventArgs e)
        {
            xTagsGrid.Grid.CommitEdit(DataGridEditingUnit.Row, true);
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
            xTagsGrid.Grid.CommitEdit(DataGridEditingUnit.Row, true);
            CleanUnValidTags();
             WorkSpace.Instance.Solution.SolutionOperations.SaveSolution(true, SolutionGeneral.Solution.eSolutionItemToSave.Tags);
        }

        private void closeBtn_Click(object sender, EventArgs e)
        {
            CleanUnValidTags();
            xTagsGrid.Grid.CommitEdit(DataGridEditingUnit.Row, true);
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
