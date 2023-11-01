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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.CoreNET.BPMN;
using Ginger.Activities;
using Ginger.ALM;
using Ginger.Repository;
using GingerCore.Activities;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
                TreeViewUtils.AddSubMenuItem(exportMenu, "Export to BPMN file", ExportBPMN, icon: eImageType.ShareExternal);

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

        private void ExportBPMN(object sender, RoutedEventArgs e)
        {
            try
            {
                Reporter.ToStatus(eStatusMsgKey.ExportingToBPMNFile);
                string xml = CreateBPMNXMLForActivitiesGroup(mActivitiesGroup);
                string filePath = SaveBPMNXMLFile(filename: mActivitiesGroup.Name, xml);
                string solutionRelativeFilePath = WorkSpace.Instance.SolutionRepository.ConvertFullPathToBeRelative(filePath);
                Reporter.ToUser(eUserMsgKey.ExportToBPMNSuccessful, solutionRelativeFilePath);
            }
            catch (Exception ex)
            {
                if (ex is BPMNConversionException)
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

        private string CreateBPMNXMLForActivitiesGroup(ActivitiesGroup activitiesGroup)
        {
            Reporter.ToLog(eLogLevel.INFO, $"Creating BPMN XML for activities group {activitiesGroup.Name}");
            ActivitiesGroupToBPMNConverter activitiesGroupToBPMNConverter = new(activitiesGroup);
            Collaboration collaboration = activitiesGroupToBPMNConverter.Convert();
            BPMNXMLSerializer serializer = new();
            string xml = serializer.Serialize(collaboration);
            return xml;
        }

        private string SaveBPMNXMLFile(string filename, string xml)
        {
            Reporter.ToLog(eLogLevel.INFO, "Saving BPMN XML file");
            if (!filename.EndsWith(".bpmn", StringComparison.OrdinalIgnoreCase))
            {
                filename += ".bpmn";
            }

            string directoryPath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(BPMNExportPath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string filePath = Path.Combine(directoryPath, filename);
            File.WriteAllText(filePath, xml);
            return filePath;
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
