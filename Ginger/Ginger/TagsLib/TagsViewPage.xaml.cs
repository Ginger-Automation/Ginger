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
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using Amdocs.Ginger.Repository;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.TagsLib
{
    /// <summary>
    /// Interaction logic for TagsSelectorPage.xaml
    /// </summary>
    public partial class TagsViewPage : Page
    {
        ObservableList<Guid> mTags;

        public TagsViewPage(ObservableList<Guid> Tags)
        {
            InitializeComponent();
            mTags = Tags;
            ShowTags();
        }

        private void ShowTags()
        {
            TagsListBox.Items.Clear();
            IEnumerable<RepositoryItemTag> ttg =  WorkSpace.Instance.Solution.Tags.ItemsAsEnumerable();
            foreach (Guid g in mTags)
            {
                // Get the Name for solution tags                
                string s = (from x in ttg where x.Guid == g select x.Name).FirstOrDefault();
                TagsListBox.Items.Add(s);
            }
        }

        private void EditTagsButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TagsSelectorPage p = new TagsSelectorPage(mTags);
            p.ShowAsWindow();
            ShowTags();
        }
    }
}
