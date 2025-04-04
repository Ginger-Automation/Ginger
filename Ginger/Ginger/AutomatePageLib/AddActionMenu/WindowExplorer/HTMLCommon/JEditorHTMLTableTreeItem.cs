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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.Actions._Common.ActUIElementLib;
using Ginger.WindowExplorer.HTMLCommon;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerWPF.UserControlsLib.UCTreeView;
using System.Windows.Controls;

namespace Ginger.WindowExplorer.Java
{
    class JEditorHTMLTableTreeItem : HTMLElementTreeItemBase, ITreeViewItem, IWindowExplorerTreeItem
    {
        UIElementTableConfigPage mHTMLTablePage;
        ObservableList<Act> mAvailableActions = [];
        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader(ElementInfo.ElementTitle, ElementInfo.GetElementTypeImage(eElementType.Table));
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            mAvailableActions.Clear();

            ActUIElement act = new ActUIElement
            {
                AddNewReturnParams = true,
                Description = "Get " + ElementInfo.ElementTitle + " Table RowCount",

                ElementType = eElementType.EditorPane,
                ElementAction = ActUIElement.eElementAction.JEditorPaneElementAction
            };
            act.AddOrUpdateInputParamValue(ActUIElement.Fields.SubElementType, ActUIElement.eSubElementType.HTMLTable.ToString());
            act.AddOrUpdateInputParamValue(ActUIElement.Fields.SubElementAction, ActUIElement.eElementAction.TableAction.ToString());
            act.AddOrUpdateInputParamValue(ActUIElement.Fields.ControlAction, ActUIElement.eTableAction.GetRowCount.ToString());
            act.AddOrUpdateInputParamValue(ActUIElement.Fields.ColSelectorValue, ActUIElement.eTableElementRunColSelectorValue.ColTitle.ToString());
            act.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateColTitle, "0");
            act.AddOrUpdateInputParamValue(ActUIElement.Fields.ByRowNum, "true");
            act.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowValue, "0");
            act.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowType, "Row Number");
            mAvailableActions.Add(act);

            ActUIElement act1 = new ActUIElement
            {
                AddNewReturnParams = true,
                Description = "Get " + ElementInfo.ElementTitle + " Value",

                ElementType = eElementType.EditorPane,
                ElementAction = ActUIElement.eElementAction.JEditorPaneElementAction
            };
            act1.AddOrUpdateInputParamValue(ActUIElement.Fields.SubElementType, ActUIElement.eSubElementType.HTMLTable.ToString());
            act1.AddOrUpdateInputParamValue(ActUIElement.Fields.SubElementAction, ActUIElement.eElementAction.TableCellAction.ToString());
            act1.AddOrUpdateInputParamValue(ActUIElement.Fields.ControlAction, ActUIElement.eTableAction.GetValue.ToString());
            act1.AddOrUpdateInputParamValue(ActUIElement.Fields.ColSelectorValue, ActUIElement.eTableElementRunColSelectorValue.ColTitle.ToString());
            act1.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateColTitle, "0");
            act1.AddOrUpdateInputParamValue(ActUIElement.Fields.ByRowNum, "true");
            act1.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowValue, "0");
            act1.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowType, "Row Number");
            mAvailableActions.Add(act1);

            ActUIElement act2 = new ActUIElement
            {
                AddNewReturnParams = true,
                Description = "Click " + ElementInfo.ElementTitle,

                ElementType = eElementType.EditorPane,
                ElementAction = ActUIElement.eElementAction.JEditorPaneElementAction
            };
            act2.AddOrUpdateInputParamValue(ActUIElement.Fields.SubElementType, ActUIElement.eSubElementType.HTMLTable.ToString());
            act2.AddOrUpdateInputParamValue(ActUIElement.Fields.SubElementAction, ActUIElement.eElementAction.TableCellAction.ToString());
            act2.AddOrUpdateInputParamValue(ActUIElement.Fields.ControlAction, ActUIElement.eTableAction.Click.ToString());
            act2.AddOrUpdateInputParamValue(ActUIElement.Fields.ColSelectorValue, ActUIElement.eTableElementRunColSelectorValue.ColTitle.ToString());
            act2.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateColTitle, "0");
            act2.AddOrUpdateInputParamValue(ActUIElement.Fields.ByRowNum, "true");
            act2.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowValue, "0");
            act2.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowType, "Row Number");
            mAvailableActions.Add(act2);

            return mAvailableActions;
        }
        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }
        ObservableList<ActInputValue> IWindowExplorerTreeItem.GetItemSpecificActionInputValues()
        {
            return mHTMLTablePage.GetTableRelatedInputValues();
        }
        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {

            if (mHTMLTablePage == null)
            {
                mHTMLTablePage = new UIElementTableConfigPage(ElementInfo, mAvailableActions, mContext);
            }
            return mHTMLTablePage;
        }
    }
}
