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
using Amdocs.Ginger.Common.Enums;
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
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    public class BusinessFlowTreeItem : NewTreeViewItemBase, ITreeViewItem
    {        
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
                if ( WorkSpace.Instance.UserProfile.UserTypeHelper.IsSupportAutomate)
                {   
                    TreeViewUtils.AddMenuItem(mContextMenu, "Automate", Automate, null, eImageType.Automate);
                }
                
                AddItemNodeBasicManipulationsOptions(mContextMenu);
                MenuItem actConversionMenu = TreeViewUtils.CreateSubMenu(mContextMenu, "Conversion");
                TreeViewUtils.AddSubMenuItem(actConversionMenu, "Legacy Actions", ActionsConversionHandler, null, eImageType.Convert);

                AddSourceControlOptions(mContextMenu);

                MenuItem ExportMenu = TreeViewUtils.CreateSubMenu(mContextMenu, "Export", eImageType.Export);
                TreeViewUtils.AddSubMenuItem(ExportMenu, "Export to ALM", ExportToALM, null, eImageType.ALM);
                TreeViewUtils.AddSubMenuItem(ExportMenu, "Export to CSV", ExportToCSV, null, eImageType.CSV);
                if (WorkSpace.Instance.BetaFeatures.BFExportToJava)
                    TreeViewUtils.AddSubMenuItem(ExportMenu, "Export to Java", ExportToJava, null, "");
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
            else
            {
                var items = ((Amdocs.Ginger.Repository.RepositoryFolder<GingerCore.BusinessFlow>)((ITreeViewItem)this).NodeObject()).GetFolderItemsRecursive();
                foreach (var bf in items)
                {
                    lst.Add(bf);
                }
            }

            WizardWindow.ShowWizard(new ActionsConversionWizard(ActionsConversionWizard.eActionConversionType.MultipleBusinessFlow, new Context(), lst), 900, 700, true);
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
            DocumentEditorPage documentEditorPage = new DocumentEditorPage(mBusinessFlow.ExternalID.Replace("~",  WorkSpace.Instance.Solution.Folder), true);
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

        private void ExportToCSV(object sender, System.Windows.RoutedEventArgs e)
        {
            Export.GingerToCSV.BrowseForFilename();
            Export.GingerToCSV.BusinessFlowToCSV(mBusinessFlow);
        }
    }
}
