#region License
/*
Copyright © 2014-2026 European Support Limited

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

using Amdocs.Ginger.Common.UIElement;
using Ginger.WindowExplorer.Java;
using GingerWPF.UserControlsLib.UCTreeView;

namespace Ginger.WindowExplorer.HTMLCommon
{
    public class HTMLElementInfoConverter
    {
        internal static ITreeViewItem GetHTMLElementTreeItem(ElementInfo EI)
        {
            if (EI.ElementTypeEnum == eElementType.TextBox)
            {
                HTMLTextBoxTreeItem HTBTI = new HTMLTextBoxTreeItem
                {
                    ElementInfo = EI
                };
                return HTBTI;
            }
            else if (EI.ElementTypeEnum == eElementType.Button)
            {
                HTMLButtonTreeItem HTBTI = new HTMLButtonTreeItem
                {
                    ElementInfo = EI
                };
                return HTBTI;
            }
            else if (EI.ElementTypeEnum == eElementType.TableItem)
            {
                HTMLTDTreeItem HTBTI = new HTMLTDTreeItem
                {
                    ElementInfo = EI
                };
                return HTBTI;
            }
            else if (EI.ElementTypeEnum == eElementType.HyperLink)
            {
                HTMLLinkTreeItem HTLII = new HTMLLinkTreeItem
                {
                    ElementInfo = EI
                };
                return HTLII;
            }
            else if (EI.ElementTypeEnum == eElementType.Label)
            {
                HTMLLabelTreeItem HTBTI = new HTMLLabelTreeItem
                {
                    ElementInfo = EI
                };
                return HTBTI;
            }
            else if (EI.ElementTypeEnum == eElementType.ComboBox)
            {
                HTMLComboBoxTreeItem HCBTI = new HTMLComboBoxTreeItem
                {
                    ElementInfo = EI
                };
                return HCBTI;
            }
            else if (EI.ElementTypeEnum == eElementType.Table)
            {
                HTMLTableTreeItem HTTI = new HTMLTableTreeItem
                {
                    ElementInfo = EI
                };
                return HTTI;
            }
            else if (EI.ElementTypeEnum == eElementType.EditorTable)
            {
                JEditorHTMLTableTreeItem HTTI = new JEditorHTMLTableTreeItem
                {
                    ElementInfo = EI
                };
                return HTTI;
            }
            else if (EI.ElementTypeEnum == eElementType.Div)
            {
                HTMLDivTreeItem HTTI = new HTMLDivTreeItem
                {
                    ElementInfo = EI
                };
                return HTTI;
            }
            else if (EI.ElementTypeEnum == eElementType.Span)
            {
                HTMLSpanTreeItem HTTI = new HTMLSpanTreeItem
                {
                    ElementInfo = EI
                };
                return HTTI;
            }
            else if (EI.ElementTypeEnum == eElementType.Image)
            {
                HTMLImgTreeItem HTTI = new HTMLImgTreeItem
                {
                    ElementInfo = EI
                };
                return HTTI;
            }
            else if (EI.ElementTypeEnum == eElementType.CheckBox)
            {
                HTMLCheckBoxTreeItem HCBTI = new HTMLCheckBoxTreeItem
                {
                    ElementInfo = EI
                };
                return HCBTI;
            }
            else if (EI.ElementTypeEnum == eElementType.RadioButton)
            {
                HTMLRadioButtonTreeItem HTRBI = new HTMLRadioButtonTreeItem
                {
                    ElementInfo = EI
                };
                return HTRBI;
            }
            else if (EI.ElementTypeEnum == eElementType.Iframe)
            {
                HTMLFrameTreeItem HTFI = new HTMLFrameTreeItem
                {
                    ElementInfo = EI
                };
                return HTFI;
            }
            else if (EI.ElementTypeEnum == eElementType.Canvas)
            {
                HTMLCanvasTreeItem HTFI = new HTMLCanvasTreeItem
                {
                    ElementInfo = EI
                };
                return HTFI;
            }
            else
            {
                // If not in above then put generic base
                HTMLElementTreeItemBase h = new HTMLElementTreeItemBase
                {
                    ElementInfo = EI
                };
                return h;
            }
        }
    }
}