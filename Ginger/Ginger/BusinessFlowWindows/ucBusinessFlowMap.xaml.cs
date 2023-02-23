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
using System.Text;

namespace Ginger.BusinessFlowWindows
{
    /// <summary>
    /// Interaction logic for ucBusinessFlowMap.xaml
    /// </summary>
    public partial class ucBusinessFlowMap : UserControl
    {
        Object mObjectElementType;
        string mElementTypeFieldName;

        public bool GoToAutomateButtonVisible { get; set; }

        BusinessFlow mBusinessFlow;
        public BusinessFlow BusinessFlow
        {
            get
            {
                return mBusinessFlow;
            }
            set
            {
                mBusinessFlow = value;                                
                xBFTextBox.Text = GetBusinessFlowDisplayPath();
                if (GoToAutomateButtonVisible)
                {
                    xGoToAutomateBtn.Visibility = Visibility.Visible; 
                }
                mBusinessFlowRepositoryKey = new RepositoryItemKey();
                mBusinessFlowRepositoryKey.Guid = mBusinessFlow.Guid;
                mBusinessFlowRepositoryKey.ItemName = mBusinessFlow.Name;
                if (mObjectElementType != null)
                {
                    mObjectElementType.GetType().GetProperty(mElementTypeFieldName).SetValue(mObjectElementType, mBusinessFlowRepositoryKey);
                }
            }
        }

        private string GetBusinessFlowDisplayPath()
        {
            StringBuilder actualPath = new StringBuilder();
            string path = mBusinessFlow.FilePath.Substring(mBusinessFlow.FilePath.IndexOf("BusinessFlows") + "BusinessFlows".Length);
            if (!string.IsNullOrEmpty(path))
            {
                string[] item = path.Split('\\');
                if (item != null && item.Length > 0)
                {
                    for (int i = 0; i <= item.Length - 2; i++)
                    {
                        actualPath.Append(item[i] + "\\");
                    }
                    actualPath.Append(mBusinessFlow.ItemName);
                }
            }
            return actualPath.ToString();
        }

        RepositoryItemKey mBusinessFlowRepositoryKey;
        public RepositoryItemKey BusinessFlowRepositoryKey
        {
            get
            {
                return mBusinessFlowRepositoryKey;
            }
            set
            {
                mBusinessFlowRepositoryKey = value;
            }
        }

        public string TargetApplication { get; set; }

        public delegate void ElementChangedEventHandler();

        public event ElementChangedEventHandler ElementChangedPageEvent;

        public void ElementChangedEvent()
        {
            if (ElementChangedPageEvent != null)
            {
                ElementChangedPageEvent();
            }
        }

        public ucBusinessFlowMap(Object objectElementType, string elementTypeFieldName, bool gotoAutomateButtonVisible = true)
        {
            InitializeComponent();

            mObjectElementType = objectElementType;
            mElementTypeFieldName = elementTypeFieldName;
            GoToAutomateButtonVisible = gotoAutomateButtonVisible;
            object key = mObjectElementType.GetType().GetProperty(mElementTypeFieldName).GetValue(mObjectElementType);
            if (key != null && key.GetType() == typeof(RepositoryItemKey))
            {
                mBusinessFlowRepositoryKey = (RepositoryItemKey)key;
                BusinessFlow = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<BusinessFlow>(mBusinessFlowRepositoryKey.Guid);
                xBFTextBox.Text = GetBusinessFlowDisplayPath();
            }
        }

        private void xSelectBF_Click(object sender, RoutedEventArgs e)
        {
            RepositoryFolder<BusinessFlow> mBFFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<BusinessFlow>();
            BusinessFlowsFolderTreeItem bFsRoot = new BusinessFlowsFolderTreeItem(mBFFolder);

            SingleItemTreeViewSelectionPage selectPage = new SingleItemTreeViewSelectionPage(GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Elements", eImageType.BusinessFlow, bFsRoot,
                                                                                SingleItemTreeViewSelectionPage.eItemSelectionType.Single, true,
                                                                                new Tuple<string, string>(nameof(BusinessFlow.Applications), TargetApplication), UCTreeView.eFilteroperationType.Contains);
          
            List<object> selectedBF = selectPage.ShowAsWindow();
            if (selectedBF != null && selectedBF.Count > 0)
            {
                BusinessFlow = (BusinessFlow)selectedBF[0];             
            }
            else
            {
                if (mBusinessFlow == null)
                {
                    xGoToAutomateBtn.Visibility = Visibility.Hidden; 
                }
            }
            ElementChangedEvent();
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
