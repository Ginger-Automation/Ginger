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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.TagsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ginger
{
    /// <summary>
    /// Interaction logic for ucTagsViewer.xaml
    /// </summary>
    public partial class ucTagsViewer : UserControl
    {
        ObservableList<RepositoryItemTag> mFullTagsList;
        bool mUseSolutionTags = false;

        eItemTagsType mItemTagsType;
        ObservableList<Guid> mItemTagsGUID;
        ObservableList<RepositoryItemKey> mItemTagsKey;

        List<RepositoryItemTag> ComboTagsList = new List<RepositoryItemTag>();
        RepositoryItemTag mFullListEditTag = null;
        bool mAddTags = true;


        public enum eItemTagsType
        {
            Guid,
            RepositoryItemKey
        }

        public event EventHandler TagsStackPanlChanged;

        public ucTagsViewer()
        {
            InitializeComponent();
        }

        private void BaseInit(ObservableList<RepositoryItemTag> fullTagsList = null)
        {
            if (WorkSpace.Instance.Solution == null)
                return;

            if (fullTagsList == null)
            {
                mUseSolutionTags = true;
                mFullTagsList = WorkSpace.Instance.Solution.Tags;

                if (mAddTags == true)
                {
                    mFullListEditTag = new RepositoryItemTag() { Name = "Add/Edit Solution Tags..." };
                }
            }
            else
                mFullTagsList = fullTagsList;

            mFullTagsList.CollectionChanged += mFullTagsList_CollectionChanged;

            LoadItemTagsToView();//show current saved tags

            if (mItemTagsType == eItemTagsType.Guid)
                mItemTagsGUID.CollectionChanged += MItemTags_CollectionChanged;
            else
                mItemTagsKey.CollectionChanged += MItemTags_CollectionChanged;


            SetComboTagsSource();
            if (mAddTags == false)
            {
                TagLabel.Content = "Filter By :";
                AddTagBtn1.Content = "Tag...";
            }
        }

        public void Init(ObservableList<Guid> itemTags, ObservableList<RepositoryItemTag> fullTagsList = null)
        {
            mItemTagsType = eItemTagsType.Guid;

            //mItemTags = new ObservableList<object>(itemTags.Cast<object>());
            mItemTagsGUID = itemTags;


            BaseInit(fullTagsList);
        }

        public void Init(ObservableList<RepositoryItemKey> itemTags, ObservableList<RepositoryItemTag> fullTagsList = null)
        {
            mItemTagsType = eItemTagsType.RepositoryItemKey;

            //mItemTags = new ObservableList<object>(itemTags.Cast<object>());
            mItemTagsKey = itemTags;

            BaseInit(fullTagsList);
        }

        public bool AddTags
        {
            get { return mAddTags; }
            set { mAddTags = value; }
        }

        public List<Guid> GetSelectedTagsList()
        {
            List<Guid> tagsGuid = new List<Guid>();
            switch (mItemTagsType)
            {
                case eItemTagsType.Guid:
                    foreach (Guid guid in mItemTagsGUID)
                        tagsGuid.Add(guid);
                    break;
                case eItemTagsType.RepositoryItemKey:
                    foreach (RepositoryItemKey itemKey in mItemTagsKey)
                        tagsGuid.Add(itemKey.Guid);
                    break;
            }

            return tagsGuid;
        }

        private void MItemTags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            LoadItemTagsToView();
            if (TagsStackPanlChanged != null)
            {
                TagsStackPanlChanged.Invoke(sender, new EventArgs());
            }
        }

        private void mFullTagsList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            LoadItemTagsToView();
        }

        //load Items to combobox and filter out all saved tags from the combobox list 
        private void SetComboTagsSource()
        {
            ComboTagsList.Clear();

            TagsComboBox.DisplayMemberPath = RepositoryItemTag.Fields.Name;
            TagsComboBox.SelectedValuePath = nameof(RepositoryItemBase.Guid);
            TagsComboBox.ItemsSource = ComboTagsList;

        }

        private int GetItemTagsCount()
        {
            if (mItemTagsType == eItemTagsType.Guid)
                return mItemTagsGUID.Count;
            else
                return mItemTagsKey.Count;
        }

        //Load saved tags on BF
        private void LoadItemTagsToView()
        {
            TagsStackPanl.Children.Clear();
            IEnumerable<RepositoryItemTag> ttg = mFullTagsList.ItemsAsEnumerable();

            for (int i = 0; i < GetItemTagsCount(); i++)
            {
                // Get the Name for solution tags.
                RepositoryItemTag t = null;
                if (mItemTagsType == eItemTagsType.Guid)
                    t = (from x in ttg where x.Guid == (Guid)mItemTagsGUID[i] select x).FirstOrDefault();
                else
                    t = (from x in ttg where x.Guid == ((RepositoryItemKey)mItemTagsKey[i]).Guid select x).FirstOrDefault();

                if (t != null)
                {
                    // add saved tags
                    ucTag tag = new ucTag(t);
                    tag.xDeleteTagBtn.Click += XDeleteTagBtn_Click;
                    TagsStackPanl.Children.Add(tag);
                }
                else
                {
                    //removing tag which not exist on solution anymore from the item tags list
                    if (mItemTagsType == eItemTagsType.Guid)
                        mItemTagsGUID.RemoveAt(i);
                    else
                        mItemTagsKey.RemoveAt(i);
                    i--;
                }
            }
        }

        private void XDeleteTagBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mItemTagsType == eItemTagsType.Guid)
                mItemTagsGUID.Remove(((RepositoryItemTag)((Button)sender).Tag).Guid);
            else
                mItemTagsKey.Remove(mItemTagsKey.Where(x => x.Guid == ((RepositoryItemTag)((Button)sender).Tag).Guid).FirstOrDefault());
        }

        private void TagsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TagsComboBox.SelectedItem != null)
            {
                if (mFullListEditTag != null && ((RepositoryItemTag)TagsComboBox.SelectedItem).Guid == mFullListEditTag.Guid)
                {
                    //open edit solution page
                    TagsPage page = new TagsPage(TagsPage.eViewMode.SpecificList, mFullTagsList);

                    page.ShowAsWindow();
                }
                else
                {
                    AddSelectedTag();
                }
                TagsComboBox.SelectedItem = null;
                TagsComboBox.Visibility = Visibility.Collapsed;
                AddTagBtn1.Visibility = Visibility.Visible;
            }
        }

        private void AddTagBtn1_Click(object sender, RoutedEventArgs e)
        {
            TagsComboBox.Visibility = Visibility.Visible;
            AddTagBtn1.Visibility = Visibility.Collapsed;

            TagsComboBox.IsDropDownOpen = true;
        }

        // Add tags to stackpanal
        private void AddSelectedTag()
        {
            RepositoryItemTag itag;
            itag = (RepositoryItemTag)TagsComboBox.SelectedItem;
            if (itag != null)
            {
                if (mItemTagsType == eItemTagsType.Guid)
                    mItemTagsGUID.Add(itag.Guid);
                else
                    mItemTagsKey.Add(itag.Key);
            }
        }

        private void TagsComboBox_DropDownOpened(object sender, EventArgs e)
        {
            if (mUseSolutionTags)
            {
                mFullTagsList = WorkSpace.Instance.Solution.Tags;
            }

            RefreshComboSelectionTagsList();
            if (ComboTagsList.Count == 0)
            {
                TagsComboBox.MaxDropDownHeight = 20;
            }
            else
            {
                TagsComboBox.MaxDropDownHeight = 200;
            }
            TagsComboBox.ItemsSource = ComboTagsList;
        }

        private void RefreshComboSelectionTagsList()
        {
            //remove from combobox all Items that are already saved on item else load all Soultion Tags.
            if (GetItemTagsCount() != 0)
            {
                if (mItemTagsType == eItemTagsType.Guid)
                {
                    ComboTagsList = mFullTagsList.Where(y => !mItemTagsGUID.Any(x => y.Guid == (Guid)x)).ToList();
                }
                else
                {
                    ComboTagsList = mFullTagsList.Where(y => !mItemTagsKey.Any(x => y.Guid == ((RepositoryItemKey)x).Guid)).ToList();
                }
            }
            else
            {
                ComboTagsList = mFullTagsList.Where(y => mFullTagsList.Contains(y)).ToList();
            }

            //add dummy edit tag
            if (mFullListEditTag != null)
                ComboTagsList.Add(mFullListEditTag);

            //refresh combo
            TagsComboBox.Items.Refresh();
        }


        private void TagsComboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            TagsComboBox.Visibility = Visibility.Collapsed;
            AddTagBtn1.Visibility = Visibility.Visible;
        }

        private void EditTagBtn_Click(object sender, RoutedEventArgs e)
        {
            TagsPage s = new TagsPage(TagsPage.eViewMode.SpecificList, mFullTagsList);
            s.ShowAsWindow();
        }
    }
}
