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

using Amdocs.Ginger.Common.UIElement;
using GingerCore.Drivers.JavaDriverLib;
using GingerWPF.UserControlsLib.UCTreeView;

namespace Ginger.WindowExplorer.Java
{
    public class JavaElementInfoConverter 
    {
        internal static ITreeViewItem GetTreeViewItemFor(ElementInfo CI)
        {
          
            JavaElementInfo JEI = (JavaElementInfo)CI;
            switch (JEI.ElementTypeEnum)
            {
                case eElementType.TextBox:
                    JavaTextBoxTreeItem JTBTI = new JavaTextBoxTreeItem();
                    JTBTI.JavaElementInfo = JEI;                    
                    return JTBTI;
                case eElementType.Button:
                    JavaButtonTreeItem JBTI = new JavaButtonTreeItem();
                    JBTI.JavaElementInfo = JEI;                                        
                    return JBTI;
                case eElementType.Label:
                    JavaLabelTreeItem JLTI = new JavaLabelTreeItem();
                    JLTI.JavaElementInfo = JEI;                                        
                    return JLTI;
                case eElementType.Browser:
                    JavaBrowserTreeItem JBRTI = new JavaBrowserTreeItem();
                    JBRTI.JavaElementInfo = JEI;                    
                    return JBRTI;
                case eElementType.CheckBox:
                    JavaCheckBoxTreeItem JCBTI = new JavaCheckBoxTreeItem();
                    JCBTI.JavaElementInfo = JEI;                    
                    return JCBTI;
                case eElementType.RadioButton:
                    JavaRadioButtonTreeItem JRBTI = new JavaRadioButtonTreeItem();
                    JRBTI.JavaElementInfo = JEI;                    
                    return JRBTI;
                case eElementType.DatePicker:
                    JavaDatePickerTreeItem JDP = new JavaDatePickerTreeItem();
                    JDP.JavaElementInfo = JEI;
                    return JDP;
                case eElementType.ComboBox:
                    JavaComboBoxTreeItem JCoBTI = new JavaComboBoxTreeItem();
                    JCoBTI.JavaElementInfo = JEI;                    
                    return JCoBTI;
                case eElementType.List:
                    JavaListTreeItem JLiTI = new JavaListTreeItem();
                    JLiTI.JavaElementInfo = JEI;                    
                    return JLiTI;
                case eElementType.Table:
                    JavaTableTreeItem JTiTI = new JavaTableTreeItem();
                    JTiTI.JavaElementInfo = JEI;                    
                    return JTiTI;
                case eElementType.ScrollBar:
                    JavaScrollTreeItem JSTI = new JavaScrollTreeItem();
                    JSTI.JavaElementInfo = JEI;                    
                    return JSTI;
                case eElementType.TreeView:
                    JavaTreeTreeItem JTRTI = new JavaTreeTreeItem();
                    JTRTI.JavaElementInfo = JEI;
                    return JTRTI;
                case eElementType.MenuItem:
                    JavaMenuTreeItem JMTI = new JavaMenuTreeItem();
                    JMTI.JavaElementInfo = JEI;                    
                    return JMTI;
                case eElementType.Tab:
                    JavaTabTreeItem JTTI = new JavaTabTreeItem();
                    JTTI.JavaElementInfo = JEI;                    
                    return JTTI;
                case eElementType.Iframe:
                    JavaInternalFrameTitleTreeItem JIFTTI = new JavaInternalFrameTitleTreeItem();
                    JIFTTI.JavaElementInfo = JEI;
                    return JIFTTI;
                default:
                    JavaElementTreeItem JETI = new JavaElementTreeItem();
                    JETI.JavaElementInfo = JEI;                    
                    return JETI;
            }	
        }
        }
    }
