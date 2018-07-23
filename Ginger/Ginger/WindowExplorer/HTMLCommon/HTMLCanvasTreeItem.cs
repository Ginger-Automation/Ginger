#region License
/*
Copyright © 2014-2018 European Support Limited

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

namespace Ginger.WindowExplorer.HTMLCommon
{
    public class HTMLCanvasTreeItem : HTMLElementTreeItemBase, ITreeViewItem, IWindowExplorerTreeItem
    {
        StackPanel ITreeViewItem.Header()
        {
            string ImageFileName = "@Canvas16x16.png";
            string Title = this.ElementInfo.ElementTitle;            
            return TreeViewUtils.CreateItemHeader(Title, ImageFileName);
        }

        HTMLCanvasElementPage mHTMLCanvasElementPage;
        Page ITreeViewItem.EditPage()
        {
                if (mHTMLCanvasElementPage == null)
                {
                    mHTMLCanvasElementPage = new HTMLCanvasElementPage(actList, ElementInfo);
                }
                return mHTMLCanvasElementPage;
        }

        ObservableList<Act> actList = new ObservableList<Act>();
        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            actList.Add(new ActGenElement()
            {
                Description = "Click by X Y inside the " + this.ElementInfo.ElementTitle + " Element",
                GenElementAction = ActGenElement.eGenElementAction.XYClick
            });

            actList.Add(new ActGenElement()
            {
                Description = "DoubleClick by X Y inside the  " + this.ElementInfo.ElementTitle + " Element",
                GenElementAction = ActGenElement.eGenElementAction.XYDoubleClick
            });

            actList.Add(new ActGenElement()
            {
                Description = "SendKeys by X Y inside the " + this.ElementInfo.ElementTitle + " TextBox",
                GenElementAction = ActGenElement.eGenElementAction.XYSendKeys
            });

            AddGeneralHTMLActions(actList);
            return actList;
        }
    }
}
