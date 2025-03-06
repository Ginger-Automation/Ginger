#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Telemetry;
using Amdocs.Ginger.CoreNET.BPMN.Exceptions;
using Amdocs.Ginger.CoreNET.BPMN.Exportation;
using Ginger.Activities;
using Ginger.ALM;
using Ginger.Repository;
using GingerCore.Activities;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class SharedActivitiesGroupTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        private const string BPMNExportPath = @"~\\Documents\BPMN";

        private readonly ActivitiesGroup mActivitiesGroup;
        private ActivitiesGroupPage mActivitiesGroupPage;
        private SharedActivitiesGroupsFolderTreeItem.eActivitiesGroupsItemsShowMode mShowMode;

        public SharedActivitiesGroupTreeItem(ActivitiesGroup activitiesGroup, SharedActivitiesGroupsFolderTreeItem.eActivitiesGroupsItemsShowMode showMode = SharedActivitiesGroupsFolderTreeItem.eActivitiesGroupsItemsShowMode.ReadWrite)
        {
            mActivitiesGroup = activitiesGroup;
            mShowMode = showMode;
        }

        Object ITreeViewItem.NodeObject()
        {
            return mActivitiesGroup;
        }
        override public string NodePath()
        {
            return mActivitiesGroup.FileName;
        }
        override public Type NodeObjectType()
        {
            return typeof(ActivitiesGroup);
        }

        StackPanel ITreeViewItem.Header()
        {
            return NewTVItemHeaderStyle(mActivitiesGroup);
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return null;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return false;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            if (mActivitiesGroupPage == null)
            {
                mActivitiesGroupPage = new ActivitiesGroupPage(mActivitiesGroup, null, ActivitiesGroupPage.eEditMode.SharedRepository);
            }
            return mActivitiesGroupPage;
        }


        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }


        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();
            if (mShowMode == SharedActivitiesGroupsFolderTreeItem.eActivitiesGroupsItemsShowMode.ReadWrite)
            {
                AddItemNodeBasicManipulationsOptions(mContextMenu);

                TreeViewUtils.AddMenuItem(mContextMenu, "View Repository Item Usage", ShowUsage, imageType: eImageType.InstanceLink);

                MenuItem exportMenu = TreeViewUtils.CreateSubMenu(mContextMenu, "Export");
                TreeViewUtils.AddSubMenuItem(exportMenu, "Export All to ALM", ExportToALM, icon: "@ALM_16x16.png");
                TreeViewUtils.AddSubMenuItem(exportMenu, "Export to BPMN file", ExportBPMNMenuItem_Click, icon: eImageType.ShareExternal);

                AddSourceControlOptions(mContextMenu);
            }
            else
            {
                TreeViewUtils.AddMenuItem(mContextMenu, "View Repository Item Usage", ShowUsage, imageType: eImageType.InstanceLink);
            }

        }

        private void ShowUsage(object sender, RoutedEventArgs e)
        {
            RepositoryItemUsagePage usagePage = new RepositoryItemUsagePage(mActivitiesGroup);
            usagePage.ShowAsWindow();
        }

        private void ExportBPMNMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ExportActivityGroupToBPMN();
        }

        private void ExportActivityGroupToBPMN()
        {
            try
            {
                Reporter.ToStatus(eStatusMsgKey.ExportingToBPMNFile);

                string fullBPMNExportPath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(BPMNExportPath);
                ActivitiesGroupToBPMNExporter activitiesGroupToBPMNExporter = new(mActivitiesGroup, fullBPMNExportPath);

                int? activityCount = null;
                int? actionCount = null;
                try
                {
                    if (mActivitiesGroup.ActivitiesIdentifiers != null)
                    {
                        activityCount = mActivitiesGroup.ActivitiesIdentifiers.Where(a => a.IdentifiedActivity != null && a.IdentifiedActivity.Active).Count();

                        actionCount = mActivitiesGroup
                            .ActivitiesIdentifiers
                            .Select(ai => ai.IdentifiedActivity)
                            .Where(activity => activity != null && activity.Active && activity.Acts != null)
                            .SelectMany(activity => activity.Acts.Where(act => act != null && act.Active))
                            .Count();
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, $"error while capturing '{FeatureId.ExportActivitiesGroupBPMN}' feature metadata", ex);
                }

                string exportPath = string.Empty;
                using (IFeatureTracker featureTracker = Reporter.StartFeatureTracking(FeatureId.ExportActivitiesGroupBPMN))
                {
                    if (activityCount != null)
                    {
                        featureTracker.Metadata.Add("ActivityCount", activityCount.ToString());
                    }
                    if (actionCount != null)
                    {
                        featureTracker.Metadata.Add("ActionCount", actionCount.ToString());
                    }
                    exportPath = activitiesGroupToBPMNExporter.Export();
                }

                string solutionRelativeExportPath = WorkSpace.Instance.SolutionRepository.ConvertFullPathToBeRelative(exportPath);

                Reporter.ToUser(eUserMsgKey.ExportToBPMNSuccessful, solutionRelativeExportPath);
            }
            catch (Exception ex)
            {
                if (ex is BPMNException)
                {
                    Reporter.ToUser(eUserMsgKey.GingerEntityToBPMNConversionError, ex.Message);
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.GingerEntityToBPMNConversionError, "Unexpected Error, check logs for more details.");
                }
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while exporting BPMN", ex);
            }
            finally
            {
                Reporter.HideStatusMessage();
            }
        }

        public override bool DeleteTreeItem(object item, bool deleteWithoutAsking = false, bool refreshTreeAfterDelete = true)
        {
            if (SharedRepositoryOperations.CheckIfSureDoingChange(mActivitiesGroup, "delete") == true)
            {
                return (base.DeleteTreeItem(mActivitiesGroup, deleteWithoutAsking, refreshTreeAfterDelete));
            }
            return false;
        }

        private void ExportToALM(object sender, System.Windows.RoutedEventArgs e)
        {
            ALMIntegration.Instance.ExportActivitiesGroupToALM(mActivitiesGroup, true);
        }
    }
}
