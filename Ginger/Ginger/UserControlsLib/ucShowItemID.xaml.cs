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
using Amdocs.Ginger.Repository;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.UserControlsLib
{
    /// <summary>
    /// Interaction logic for ucShowItemID.xaml
    /// </summary>
    public partial class ucShowItemID : UserControl
    {
        RepositoryItemBase mRepoItem = null;

        public ucShowItemID()
        {
            InitializeComponent();

            InitView();
        }

        public void Init(RepositoryItemBase item)
        {
            mRepoItem = item;
            InitView();
        }

        private void InitView()
        {
            xShowIDBtn.Visibility = Visibility.Visible;
            xIDLbl.Visibility = Visibility.Collapsed;
            xCopyBtn.Visibility = Visibility.Collapsed;
        }

        private void xShowIDBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mRepoItem != null)
            {
                xIDLbl.Text = mRepoItem.Guid.ToString();
                xShowIDBtn.Visibility = Visibility.Collapsed;
                xIDLbl.Visibility = Visibility.Visible;
                xCopyBtn.Visibility = Visibility.Visible;
            }
        }

        private void xCopyBtn_Click(object sender, RoutedEventArgs e)
        {
            GingerCore.General.SetClipboardText(mRepoItem.Guid.ToString());
        }
    }
}
