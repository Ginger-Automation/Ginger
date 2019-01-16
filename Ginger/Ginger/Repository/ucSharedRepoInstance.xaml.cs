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
using Amdocs.Ginger.Repository;
using Ginger.Actions;
using Ginger.Repository;
using Ginger.Variables;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Variables;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ginger
{
    /// <summary>
    /// Interaction logic for ucSharedRepoInstance.xaml
    /// </summary>
    public partial class ucSharedRepoInstance : UserControl
    {
        RepositoryItemBase mItem = null;
        RepositoryItemBase mLinkedRepoItem=null;
        BusinessFlow mBusinessFlow = null;
        bool mLinkIsByExternalID = false;
        bool mLinkIsByParentID = false;

        public ucSharedRepoInstance()
        {
            InitializeComponent();

            EditLinkedRepoItemBtn.Visibility = Visibility.Collapsed;
            UpdateRepoBtn.Visibility = Visibility.Collapsed;
        }

        public void Init(RepositoryItemBase item, BusinessFlow containingBusinessFlow)
        {
            mItem = item;
            mBusinessFlow = containingBusinessFlow;
            SetRepoLinkStatus();
        }

        private void SetRepoLinkStatus()
        {
            mLinkedRepoItem = null;
            mLinkIsByExternalID = false;
            mLinkIsByParentID = false;

            EditLinkedRepoItemBtn.Visibility = Visibility.Collapsed;
            UpdateRepoBtn.Visibility = Visibility.Collapsed;

            //get the item from shared repo if exist
            mLinkedRepoItem = SharedRepositoryOperations.GetMatchingRepoItem(mItem, null, ref mLinkIsByExternalID, ref mLinkIsByParentID);
            if (mLinkedRepoItem == null)
            {
                LinkStatusImage.Source = General.GetResourceImage("@StarGray_24x24.png");
                LinkStatusImage.ToolTip = "The item is not linked to Shared Repository."+ Environment.NewLine +"Click to add it to Shared Repository.";
                UpdateRepoBtn.ToolTip = "Upload to Shared Repository";
            }
            else
            {
                LinkStatusImage.Source = General.GetResourceImage("@Star_24x24.png");
                if (mLinkIsByParentID || mLinkIsByExternalID)
                {
                    LinkStatusImage.ToolTip = "The item is linked to the Shared Repository item: '" + Path.Combine(mLinkedRepoItem.ContainingFolder,mLinkedRepoItem.ItemName) + "'." + Environment.NewLine + "Click to un-link it.";
                }
                else
                {
                    LinkStatusImage.ToolTip = "The item is linked to the Shared Repository item: '" + Path.Combine(mLinkedRepoItem.ContainingFolder, mLinkedRepoItem.ItemName) + "'.";
                }
                UpdateRepoBtn.ToolTip = "Overwrite Shared Repository linked item";
            }
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            EditLinkedRepoItemBtn.Visibility = Visibility.Collapsed;
            if (mLinkedRepoItem != null)
            {
                EditLinkedRepoItemBtn.Visibility = Visibility.Visible;
                UpdateRepoBtn.Visibility = Visibility.Visible;
            }
            else
            {
                EditLinkedRepoItemBtn.Visibility = Visibility.Collapsed;
                UpdateRepoBtn.Visibility = Visibility.Visible;
            }
        }

        private void EditLinkedRepoItemBtn_Click(object sender, RoutedEventArgs e)
        {
            //TODO find a way to make in generic
            if (mLinkedRepoItem is Activity)
                (new BusinessFlowWindows.ActivityEditPage((Activity)mLinkedRepoItem, General.RepositoryItemPageViewMode.SharedReposiotry)).ShowAsWindow(startupLocationWithOffset: true);
            else if (mLinkedRepoItem is VariableBase)
                (new VariableEditPage((VariableBase)mLinkedRepoItem, false, VariableEditPage.eEditMode.SharedRepository)).ShowAsWindow(eWindowShowStyle.Dialog, startupLocationWithOffset: true);
            else if (mLinkedRepoItem is Act)
                (new ActionEditPage((Act)mLinkedRepoItem, General.RepositoryItemPageViewMode.SharedReposiotry, new GingerCore.BusinessFlow(), new GingerCore.Activity())).ShowAsWindow(startupLocationWithOffset: true);
            else if (mLinkedRepoItem is GingerCore.Activities.ActivitiesGroup)
                (new Activities.ActivitiesGroupPage((GingerCore.Activities.ActivitiesGroup)mLinkedRepoItem, Activities.ActivitiesGroupPage.eEditMode.SharedRepository)).ShowAsWindow(startupLocationWithOffset: true);
        }

        private void LinkStatusImageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mLinkedRepoItem != null)
            {
                if (mLinkIsByParentID || mLinkIsByExternalID)
                {
                    if (Reporter.ToUser(eUserMsgKeys.AskIfSureWantToDeLink) == Amdocs.Ginger.Common.MessageBoxResult.Yes)
                    {
                        mItem.ParentGuid = Guid.Empty;
                        mItem.ExternalID = string.Empty;
                        mItem.IsSharedRepositoryInstance = false;
                        SetRepoLinkStatus();
                    }
                }
            }
            else
            {
                //App.LocalRepository.AddItemToRepositoryWithPreChecks(mItem, mBusinessFlow);
                Repository.SharedRepositoryOperations.AddItemToRepository(mItem);
                SetRepoLinkStatus();            
            }
        }

        private void UpdateRepoBtn_Click(object sender, RoutedEventArgs e)
        {
            //App.LocalRepository.AddItemToRepositoryWithPreChecks(mItem, mBusinessFlow);
            Repository.SharedRepositoryOperations.AddItemToRepository(mItem);
            SetRepoLinkStatus();
        }
    }
}
