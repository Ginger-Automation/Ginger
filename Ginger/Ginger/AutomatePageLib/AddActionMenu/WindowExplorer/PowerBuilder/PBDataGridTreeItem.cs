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

using Amdocs.Ginger.Common;
using Ginger.Actions;
using GingerCore.Actions;
using System.Windows.Controls;
using GingerWPF.UserControlsLib.UCTreeView;
using Ginger.Drivers.PowerBuilder;

namespace Ginger.WindowExplorer.PowerBuilder
{
    public class PBDataGridTreeItem : PBControlTreeItemBase, ITreeViewItem, IWindowExplorerTreeItem
    {
        ObservableList<Act> mAvailableActions = new ObservableList<Act>();
        ActTableEditPage mActTableEditPage = null;

        StackPanel ITreeViewItem.Header()
        {
            string ImageFileName = "@Grid_16x16.png";
            return TreeViewUtils.CreateItemHeader(UIAElementInfo.ElementTitle, ImageFileName);
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            if(mAvailableActions.Count == 0)
            {
                mAvailableActions.Clear();
                mAvailableActions.Add(new ActTableElement()
                { //TODO:get Row Count
                    Description = "Get " + UIAElementInfo.ElementTitle + " Table RowCount",
                    ControlAction = ActTableElement.eTableAction.GetRowCount,
                    ColSelectorValue = ActTableElement.eRunColSelectorValue.ColNum,
                    LocateColTitle = "0",
                    ByRowNum = true,
                    LocateRowValue = "0",
                    LocateRowType = "Row Number"
                });
            }            
            return mAvailableActions;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            if (mActTableEditPage == null)
                mActTableEditPage = new ActTableEditPage(base.UIAElementInfo, mAvailableActions);            
            return mActTableEditPage;
        }
    }
}
