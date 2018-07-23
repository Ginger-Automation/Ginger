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

using GingerWPF.UserControlsLib.UCTreeView;
using Ginger.WindowExplorer.Java;
using Amdocs.Ginger.Common.UIElement;

namespace Ginger.WindowExplorer.HTMLCommon
{
    public class HTMLElementInfoConverter 
    {
        //Move from here
        internal static ITreeViewItem GetHTMLElementTreeItem(ElementInfo EI)
        {
            if (EI.ElementType.ToUpper() == "INPUT.TEXT" || EI.ElementType.ToUpper() == "TEXTAREA" || EI.ElementType.ToUpper() == "INPUT.UNDEFINED" || EI.ElementType.ToUpper() == "INPUT.PASSWORD" || EI.ElementType.ToUpper() == "INPUT.EMAIL")  // HTML text 
            {
                HTMLTextBoxTreeItem HTBTI = new HTMLTextBoxTreeItem();
                HTBTI.ElementInfo = EI;
                return HTBTI;
            }
            else if (EI.ElementType.ToUpper() == "INPUT.BUTTON" || EI.ElementType.ToUpper() == "BUTTON" || EI.ElementType.ToUpper() == "INPUT.IMAGE" || EI.ElementType.ToUpper() == "LINK" || EI.ElementType.ToUpper() == "INPUT.SUBMIT")  // HTML Button
            {
                HTMLButtonTreeItem HTBTI = new HTMLButtonTreeItem();
                HTBTI.ElementInfo = EI;
                return HTBTI;
            }
            else if (EI.ElementType.ToUpper() == "TD" || EI.ElementType.ToUpper() == "TH")
            {
                HTMLTDTreeItem HTBTI = new HTMLTDTreeItem();
                HTBTI.ElementInfo = EI;
                return HTBTI;

            }
            else if (EI.ElementType.ToUpper() == "LINK" || EI.ElementType.ToUpper() == "A")  // HTML Link
            {
                HTMLLinkTreeItem HTLII = new HTMLLinkTreeItem();
                HTLII.ElementInfo = EI;
                return HTLII;
            }
            else if (EI.ElementType == "LABEL")  // HTML Label
            {
                HTMLLabelTreeItem HTBTI = new HTMLLabelTreeItem();
                HTBTI.ElementInfo = EI;
                return HTBTI;
            }
            else if (EI.ElementType == "SELECT")  // HTML Select/ComboBox
            {
                HTMLComboBoxTreeItem HCBTI = new HTMLComboBoxTreeItem();
                HCBTI.ElementInfo = EI;
                return HCBTI;
            }
            else if (EI.ElementType.ToUpper() == "TABLE")  // HTML Table
            {
                HTMLTableTreeItem HTTI = new HTMLTableTreeItem();
                HTTI.ElementInfo = EI;
                return HTTI;
            }
            else if (EI.ElementType.ToUpper() == "JEDITOR.TABLE")
            {
                JEditorHTMLTableTreeItem HTTI = new JEditorHTMLTableTreeItem();
                HTTI.ElementInfo = EI;
                return HTTI;
            }
            else if (EI.ElementType == "DIV")  // DIV Element
            {
                HTMLDivTreeItem HTTI = new HTMLDivTreeItem();
                HTTI.ElementInfo = EI;
                return HTTI;
            }
            else if (EI.ElementType == "SPAN")  // SPAN Element
            {
                HTMLSpanTreeItem HTTI = new HTMLSpanTreeItem();
                HTTI.ElementInfo = EI;
                return HTTI;
            }
            else if (EI.ElementType == "IMG")  // IMG Element
            {
                HTMLImgTreeItem HTTI = new HTMLImgTreeItem();
                HTTI.ElementInfo = EI;
                return HTTI;
            }
            else if (EI.ElementType == "INPUT.checkbox" || EI.ElementType == "INPUT.CHECKBOX")  // Check Box Element
            {
                HTMLCheckBoxTreeItem HCBTI = new HTMLCheckBoxTreeItem();
                HCBTI.ElementInfo = EI;
                return HCBTI;
            }
            else if (EI.ElementType.ToUpper() == "INPUT.RADIO")  // HTML Radio
            {
                HTMLRadioButtonTreeItem HTRBI = new HTMLRadioButtonTreeItem();
                HTRBI.ElementInfo = EI;
                return HTRBI;
            }
            else if (EI.ElementType.ToUpper() == "IFRAME")  // HTML IFRAME
            {
                HTMLFrameTreeItem HTFI = new HTMLFrameTreeItem();
                HTFI.ElementInfo = EI;
                return HTFI;
            }
            else if (EI.ElementType.ToUpper() == "CANVAS")  // HTML Radio
            {
                HTMLCanvasTreeItem HTFI = new HTMLCanvasTreeItem();
                HTFI.ElementInfo = EI;
                return HTFI;
            }

            // TODO: add all the rest
            else
            {
                // If not in above then put generic base
                HTMLElementTreeItemBase h = new HTMLElementTreeItemBase();
                h.ElementInfo = EI;
                return h;
            }
        }
    }
}