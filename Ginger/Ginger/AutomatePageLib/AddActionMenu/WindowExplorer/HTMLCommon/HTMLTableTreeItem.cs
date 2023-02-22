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

using Amdocs.Ginger.Common;
using System.Windows.Controls;
using GingerCore.Actions;
using GingerWPF.UserControlsLib.UCTreeView;
using Ginger.Actions;
using Amdocs.Ginger.Common.UIElement;

namespace Ginger.WindowExplorer.HTMLCommon
{
    class HTMLTableTreeItem : HTMLElementTreeItemBase, ITreeViewItem, IWindowExplorerTreeItem
    {
        Page mHTMLTablePage;
        ObservableList<Act> mAvailableActions = new ObservableList<Act>();
        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader(ElementInfo.ElementTitle, ElementInfo.GetElementTypeImage(eElementType.Table));
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            mAvailableActions.Clear();

            mAvailableActions.Add(new ActTableElement()
            { //TODO:get Row Count
                Description = "Get " + ElementInfo.ElementTitle + " Table RowCount",
                ControlAction = ActTableElement.eTableAction.GetRowCount,
                ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle,
                LocateColTitle = "0",
                ByRowNum = true,
                LocateRowValue = "0",
                LocateRowType = "Row Number"
            });

            return mAvailableActions;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            mHTMLTablePage = new ActTableEditPage(ElementInfo, mAvailableActions);
            return mHTMLTablePage;
        }
        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }
    }
}
