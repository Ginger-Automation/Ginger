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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.Actions;
using Ginger.BusinessFlowPages;
using Ginger.BusinessFlowPages.ListHelpers;
using Ginger.Repository.AddItemToRepositoryWizard;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerWPF.DragDropLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Repository
{
    /// <summary>
    /// Interaction logic for ActionsRepositoryPage.xaml
    /// </summary>
    public partial class ActionsRepositoryPage : Page
    {
        readonly RepositoryFolder<Act> mActionsFolder;
        Context mContext;

        public ActionsRepositoryPage(RepositoryFolder<Act> actionsFolder, Context context)
        {
            InitializeComponent();

            mActionsFolder = actionsFolder;
            mContext = context;

            SetActionsGridView();
            SetGridAndTreeData();
        }

        public void UpdateBusinessFlow(BusinessFlow bf)
        {
            //xActionListView.ClearFilters();
        }


        private void SetGridAndTreeData()
        {
            xActionListView.ListTitleVisibility = Visibility.Hidden;
            ActionsListViewHelper mActionsListHelper = new ActionsListViewHelper(mContext, General.eRIPageViewMode.AddFromShardRepository);

            xActionListView.SetDefaultListDataTemplate(mActionsListHelper);
            xActionListView.ListSelectionMode = SelectionMode.Extended;
            mActionsListHelper.ListView = xActionListView;

            if (mActionsFolder.IsRootFolder)
            {
                xActionListView.DataSourceList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Act>();
            }
            else
            {
                xActionListView.DataSourceList = mActionsFolder.GetFolderItems();
            }
        }

        private void grdActions_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DrgInfo.DataIsAssignableToType(typeof(Act)))
            {
                // OK to drop
                DragDrop2.SetDragIcon(true);
            }
            else
            {
                // Do Not Drop
                DragDrop2.SetDragIcon(false);
            }
        }

        private void grdActions_ItemDropped(object sender, EventArgs e)
        {
            Act dragedItem = (Act)((DragInfo)sender).Data;
            if (dragedItem != null)
            {                
                WizardWindow.ShowWizard(new UploadItemToRepositoryWizard(mContext, dragedItem));
                //refresh and select the item
                try
               {
                   xActionListView.DataSourceList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Act>();

                    Act dragedItemInGrid = ((IEnumerable<Act>)xActionListView.DataSourceList).Where(x => x.Guid == dragedItem.Guid).FirstOrDefault();
                   if (dragedItemInGrid != null)
                       xActionListView.List.SelectedItem = dragedItemInGrid;
               }
               catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
            }
        }


        private void SetActionsGridView()
        {
            xActionListView.ItemMouseDoubleClick += grdActions_grdMain_ItemMouseDoubleClick;
            xActionListView.ItemDropped += grdActions_ItemDropped;
            xActionListView.PreviewDragItem += grdActions_PreviewDragItem;
            xActionListView.xTagsFilter.Visibility = Visibility.Visible;
        }
                       
        private void EditAction(object sender, RoutedEventArgs e)
        {
            if (xActionListView.CurrentItem != null)
            {
                Act a = (Act)xActionListView.CurrentItem;
                ActionEditPage actedit = new ActionEditPage(a, General.eRIPageViewMode.SharedReposiotry, new GingerCore.BusinessFlow(), new GingerCore.Activity());
                actedit.ShowAsWindow(eWindowShowStyle.Dialog);             
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectItem);
            }
        }
        
        private void grdActions_grdMain_ItemMouseDoubleClick(object sender, EventArgs e)
        {
            EditAction(sender, new RoutedEventArgs());
        }
    }
}
