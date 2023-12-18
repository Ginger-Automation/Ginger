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
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.CoreNET.BPMN.Exceptions;
using Amdocs.Ginger.CoreNET.BPMN.Models;
using Amdocs.Ginger.CoreNET.BPMN.Serialization;
using Ginger.Actions.ActionConversion;
using Ginger.ALM;
using Ginger.BusinessFlowWindows;
using Ginger.Exports.ExportToJava;
using Ginger.UserControlsLib.TextEditor;
using Ginger.VisualAutomate;
using GingerCore;
using GingerWPF.BusinessFlowsLib;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.IO;
using Task = System.Threading.Tasks.Task;
using System.Windows;
using System.Windows.Controls;
using GingerCore.Activities;
using MongoDB.Bson;
using System.Linq;
using Ginger.Repository.AddItemToRepositoryWizard;
using Ginger.Repository.ItemToRepositoryWizard;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.CoreNET.BPMN.Conversion;
using Amdocs.Ginger.CoreNET.BPMN.Exportation;
using Ginger.Repository;

namespace Ginger.SolutionWindows.TreeViewItems
{
    public class BusinessFlowTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        private const string BPMNExportPath = @"~\\Documents\BPMN";

        private BusinessFlowViewPage mBusinessFlowViewPage;

        private BusinessFlow mBusinessFlow { get; set; }
        private readonly eBusinessFlowsTreeViewMode mViewMode;

        Object ITreeViewItem.NodeObject()
        {
            return mBusinessFlow;
        }

        override public string NodePath()
        {
            return mBusinessFlow.FileName;
        }

        override public Type NodeObjectType()
        {
            return typeof(BusinessFlow);
        }

        public BusinessFlowTreeItem(BusinessFlow businessFlow, eBusinessFlowsTreeViewMode viewMode = eBusinessFlowsTreeViewMode.ReadWrite)
        {
            mBusinessFlow = businessFlow;
            mViewMode = viewMode;
        }

        StackPanel ITreeViewItem.Header()
        {
            return NewTVItemHeaderStyle(mBusinessFlow);
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
            if (mBusinessFlowViewPage == null)
            {
                mBusinessFlowViewPage = new BusinessFlowViewPage(mBusinessFlow, null, General.eRIPageViewMode.Standalone);
            }
            return mBusinessFlowViewPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();
            if (mViewMode == eBusinessFlowsTreeViewMode.ReadWrite)
            {
                if (WorkSpace.Instance.UserProfile.UserTypeHelper.IsSupportAutomate)
                {
                    TreeViewUtils.AddMenuItem(mContextMenu, "Automate", Automate, null, eImageType.Automate);
                }

                AddItemNodeBasicManipulationsOptions(mContextMenu);
                MenuItem actConversionMenu = TreeViewUtils.CreateSubMenu(mContextMenu, "Conversion", eImageType.Convert);
                TreeViewUtils.AddSubMenuItem(actConversionMenu, "Legacy Actions Conversion", ActionsConversionHandler, null, eImageType.Convert);
                TreeViewUtils.AddSubMenuItem(actConversionMenu, "Clean Inactive Legacy Actions", LegacyActionsRemoveHandler, null, eImageType.Delete);

                AddSourceControlOptions(mContextMenu);

                MenuItem ExportMenu = TreeViewUtils.CreateSubMenu(mContextMenu, "Export", eImageType.Export);
                TreeViewUtils.AddSubMenuItem(ExportMenu, "Export to ALM", ExportToALM, null, eImageType.ALM);
                TreeViewUtils.AddSubMenuItem(ExportMenu, "Map to ALM", MapToALM, null, eImageType.MapALM);
                TreeViewUtils.AddSubMenuItem(ExportMenu, "Export to CSV", ExportToCSV, null, eImageType.CSV);
                TreeViewUtils.AddSubMenuItem(ExportMenu, "Export as Otoma Use Case Zip file", ExportBPMNMenuItem_Click, icon: eImageType.ShareExternal);
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ExportMenu, Expander.VisibilityProperty, WorkSpace.Instance.UserProfile, nameof(WorkSpace.Instance.UserProfile.ShowEnterpriseFeatures), bindingConvertor: new GingerCore.GeneralLib.BoolVisibilityConverter());

                if (WorkSpace.Instance.BetaFeatures.BFExportToJava)
                {
                    TreeViewUtils.AddSubMenuItem(ExportMenu, "Export to Java", ExportToJava, null, "");
                }
            }

            AddGherkinOptions(mContextMenu);
        }

        private void ActionsConversionHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            ObservableList<BusinessFlow> lst = new ObservableList<BusinessFlow>();
            if (((ITreeViewItem)this).NodeObject().GetType().Equals(typeof(GingerCore.BusinessFlow)))
            {
                lst.Add((GingerCore.BusinessFlow)((ITreeViewItem)this).NodeObject());
            }

            WizardWindow.ShowWizard(new ActionsConversionWizard(ActionsConversionWizard.eActionConversionType.MultipleBusinessFlow, new Context(), lst), 900, 700, true);
        }

        /// <summary>
        /// This method helps to execute the funcationality of removeing the legacy actions from the businessflow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void LegacyActionsRemoveHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            ObservableList<BusinessFlowToConvert> lstBFToConvert = new ObservableList<BusinessFlowToConvert>();
            if (((ITreeViewItem)this).NodeObject().GetType().Equals(typeof(GingerCore.BusinessFlow)))
            {
                BusinessFlowToConvert flowToConvert = new BusinessFlowToConvert();
                flowToConvert.BusinessFlow = (GingerCore.BusinessFlow)((ITreeViewItem)this).NodeObject();
                lstBFToConvert.Add(flowToConvert);
            }

            ActionConversionUtils utils = new ActionConversionUtils();
            await Task.Run(() => utils.RemoveLegacyActionsHandler(lstBFToConvert));
        }

        private void VisualAutomate(object sender, RoutedEventArgs e)
        {
            VisualAutomatePage p = new VisualAutomatePage(mBusinessFlow);
            p.ShowAsWindow();
        }

        private void ExportToJava(object sender, RoutedEventArgs e)
        {
            BFToJava j = new BFToJava();
            j.BusinessFlowToJava(mBusinessFlow);
        }

        public void AddGherkinOptions(ContextMenu CM)
        {
            if (mBusinessFlow.Source == BusinessFlow.eSource.Gherkin)
            {
                MenuItem GherkinMenu = TreeViewUtils.CreateSubMenu(CM, "Gherkin");
                //TOD Change Icon
                TreeViewUtils.AddSubMenuItem(GherkinMenu, "Open Feature file", GoToGherkinFeatureFile, null, "@FeatureFile_16X16.png");
            }
        }

        private void GoToGherkinFeatureFile(object sender, RoutedEventArgs e)
        {
            DocumentEditorPage documentEditorPage = new DocumentEditorPage(mBusinessFlow.ExternalID.Replace("~", WorkSpace.Instance.Solution.Folder), true);
            documentEditorPage.Title = "Gherkin Page";
            documentEditorPage.Height = 700;
            documentEditorPage.Width = 1000;
            documentEditorPage.ShowAsWindow();

        }

        //public override void PostDeleteTreeItemHandler()
        //{
        //    if (App.BusinessFlow == mBusinessFlow)
        //    {
        //        if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>().Count != 0)
        //        {
        //            App.BusinessFlow = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>()[0];
        //        }
        //        else
        //        {
        //            App.BusinessFlow = null;
        //        }
        //    }
        //}

        private void Automate(object sender, System.Windows.RoutedEventArgs e)
        {
            App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.Automate, mBusinessFlow);
        }

        private void ExportToALM(object sender, System.Windows.RoutedEventArgs e)
        {
            ALMIntegration.Instance.ExportBusinessFlowToALM(mBusinessFlow, true);
        }
        private void MapToALM(object sender, System.Windows.RoutedEventArgs e)
        {
            ALMIntegration.Instance.MapBusinessFlowToALM(mBusinessFlow, true);
        }

        private void ExportToCSV(object sender, System.Windows.RoutedEventArgs e)
        {
            Export.GingerToCSV.BrowseForFilename();
            Export.GingerToCSV.BusinessFlowToCSV(mBusinessFlow);
        }

        private IEnumerable<ActivitiesGroup> GetActivityGroupsMissingFromSharedRepository()
        {
            ObservableList<ActivitiesGroup> sharedActivitiesGroups = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
            SharedRepositoryOperations.MarkSharedRepositoryItems((IEnumerable<object>)mBusinessFlow.ActivitiesGroups, (IEnumerable<object>)sharedActivitiesGroups);
            return mBusinessFlow.ActivitiesGroups.Where(ag =>
            {
                bool missingFromSharedRepo = !ag.IsSharedRepositoryInstance;
                bool hasActivities = ag.ActivitiesIdentifiers.Any();
                return missingFromSharedRepo && hasActivities;
            });
        }

        private void ExportBPMNMenuItem_Click(object sender, RoutedEventArgs e)
        {
            bool wasAllAddedToSharedRepository = TryAddingMissingActivityGroupsToSharedRepository();
            if (!wasAllAddedToSharedRepository)
            {
                return;
            }
            ExportBusinessFlowBPMN();
        }

        private bool TryAddingMissingActivityGroupsToSharedRepository()
        {
            bool wasAllAddedToSharedRepository; 
            try
            { 
                IEnumerable<ActivitiesGroup> activityGroups = GetActivityGroupsMissingFromSharedRepository();
                bool allActivityGroupsAlreadyInSharedRepository = !activityGroups.Any();
                if (allActivityGroupsAlreadyInSharedRepository)
                {
                    wasAllAddedToSharedRepository = true;
                    return wasAllAddedToSharedRepository;
                }

                eUserMsgSelection userResponse = Reporter.ToUser(eUserMsgKey.AddActivityGroupsToSharedRepositoryForBPMNConversion);
                if (userResponse != eUserMsgSelection.Yes)
                {
                    wasAllAddedToSharedRepository = false;
                    return wasAllAddedToSharedRepository;
                }

                Context context = new()
                {
                    BusinessFlow = mBusinessFlow
                };
                IEnumerable<RepositoryItemBase> activitiesAndGroups = activityGroups
                    .SelectMany(ag => ag.ActivitiesIdentifiers.Select(ai => ai.IdentifiedActivity))
                    .Cast<RepositoryItemBase>()
                    .Concat(activityGroups);
                WizardWindow.ShowWizard(new UploadItemToRepositoryWizard(context, activitiesAndGroups));

                wasAllAddedToSharedRepository = activityGroups.All(ag => ag.IsSharedRepositoryInstance);
                return wasAllAddedToSharedRepository;
            }
            catch(Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.FailedToAddItemsToSharedRepository, "Unexpected error, check logs for more details");
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while adding missing activities and activity groups to shared repository.", ex);
                wasAllAddedToSharedRepository = false;
                return wasAllAddedToSharedRepository;
            }
        }

        private void ExportBusinessFlowBPMN()
        {
            try
            {
                Reporter.ToStatus(eStatusMsgKey.ExportingToBPMNZIP);

                string fullBPMNExportPath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(BPMNExportPath);
                BusinessFlowToBPMNExporter businessFlowToBPMNExporter = new(mBusinessFlow, fullBPMNExportPath);
                string exportPath = businessFlowToBPMNExporter.Export();
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
    }
}
