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

using System.Windows;
using System.Windows.Controls;
using GingerWPF.UserControlsLib.UCTreeView;
using Amdocs.Ginger.Repository;
using amdocs.ginger.GingerCoreNET;
using GingerCore;
using Amdocs.Ginger.Common.Enums;
using System.Collections.Generic;
using Ginger.SolutionWindows.TreeViewItems;
using System;
using Amdocs.Ginger.Common;

namespace Ginger.BusinessFlowWindows
{
    /// <summary>
    /// Interaction logic for xUcBusinessFlowMap.xaml
    /// </summary>
    public partial class xUcBusinessFlowMap : UserControl
    {
        public BusinessFlow mBusinessFlow;
        public string TargetApplication { get; set; }

        public xUcBusinessFlowMap()
        {
            InitializeComponent(); 
        }

        private void xSelectBF_Click(object sender, RoutedEventArgs e)
        {
            RepositoryFolder<BusinessFlow> mBFFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<BusinessFlow>();
            BusinessFlowsFolderTreeItem bFsRoot = new BusinessFlowsFolderTreeItem(mBFFolder);

            SingleItemTreeViewSelectionPage selectPage = new SingleItemTreeViewSelectionPage(GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Elements", eImageType.BusinessFlow, bFsRoot,
                                                                                SingleItemTreeViewSelectionPage.eItemSelectionType.Single, true,
                                                                                new Tuple<string, string>(nameof(BusinessFlow.Applications), TargetApplication));
          
            List<object> selectedBF = selectPage.ShowAsWindow();
            if (selectedBF != null && selectedBF.Count > 0)
            {
                mBusinessFlow = (BusinessFlow)selectedBF[0];
                string pathToShow = mBusinessFlow.FilePath.Substring(0, mBusinessFlow.FilePath.LastIndexOf("\\")).Substring(mBusinessFlow.ContainingFolderFullPath.Length) + @"\" + mBusinessFlow.ItemName;
                xBFTextBox.Text = pathToShow;
                xGoToAutomateBtn.Visibility = Visibility.Visible;
            }
            else
            {
                if (mBusinessFlow == null)
                {
                    xGoToAutomateBtn.Visibility = Visibility.Hidden; 
                }
            }
        }

        private void xGoToAutomateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mBusinessFlow != null)
            {
                App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.Automate, mBusinessFlow); 
            }
        }
    }
}
