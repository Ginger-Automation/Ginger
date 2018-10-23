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

using Amdocs.Ginger.Common.UIElement;
using GingerCore.Drivers.JavaDriverLib;
using GingerWPF.UserControlsLib.UCTreeView;

namespace Ginger.WindowExplorer.Java
{
    public class JavaElementInfoConverter 
    {
        internal static ITreeViewItem GetTreeViewItemFor(ElementInfo CI)
        {
            // TODO verify if pl.Name = ElementInfo

            JavaElementInfo JEI = (JavaElementInfo)CI;
            switch (JEI.ElementType)
            {
                case "javax.swing.JTextField":
                case "javax.swing.JTextPane":
                    JavaTextBoxTreeItem JTBTI = new JavaTextBoxTreeItem();
                    JTBTI.JavaElementInfo = JEI;                    
                    return JTBTI;
                case "javax.swing.JButton":
                    JavaButtonTreeItem JBTI = new JavaButtonTreeItem();
                    JBTI.JavaElementInfo = JEI;                                        
                    return JBTI;
                case "javax.swing.JLabel":
                    JavaLabelTreeItem JLTI = new JavaLabelTreeItem();
                    JLTI.JavaElementInfo = JEI;                                        
                    return JLTI;
                case "com.amdocs.uif.widgets.browser.JxBrowserBrowserComponent":  //  added to support live spy in JxBrowserBrowserComponent
                case "com.amdocs.uif.widgets.browser.JExplorerBrowserComponent":// "com.jniwrapper.win32.ie.aw" :
                    JavaBrowserTreeItem JBRTI = new JavaBrowserTreeItem();
                    JBRTI.JavaElementInfo = JEI;                    
                    return JBRTI;
                case "javax.swing.JCheckBox":
                    JavaCheckBoxTreeItem JCBTI = new JavaCheckBoxTreeItem();
                    JCBTI.JavaElementInfo = JEI;                    
                    return JCBTI;
                case "javax.swing.JRadioButton":
                    JavaRadioButtonTreeItem JRBTI = new JavaRadioButtonTreeItem();
                    JRBTI.JavaElementInfo = JEI;                    
                    return JRBTI;
                case "com.amdocs.uif.widgets.CalendarComponent":
                case "com.amdocs.uif.widgets.DateTimeNative$2":
                case "lt.monarch.swing.JDateField$Editor":
                    JavaDatePickerTreeItem JDP = new JavaDatePickerTreeItem();
                    JDP.JavaElementInfo = JEI;
                    return JDP;
                case "javax.swing.JComboBox":
                case "com.amdocs.uif.widgets.ComboBoxNative$1":
                    JavaComboBoxTreeItem JCoBTI = new JavaComboBoxTreeItem();
                    JCoBTI.JavaElementInfo = JEI;                    
                    return JCoBTI;
                case "javax.swing.JList":
                    JavaListTreeItem JLiTI = new JavaListTreeItem();
                    JLiTI.JavaElementInfo = JEI;                    
                    return JLiTI;
                case "javax.swing.JTable":
                case "com.amdocs.uif.widgets.search.SearchJTable":
                    JavaTableTreeItem JTiTI = new JavaTableTreeItem();
                    JTiTI.JavaElementInfo = JEI;                    
                    return JTiTI;
                case "javax.swing.JScrollPane":
                case"javax.swing.JScrollPane$ScrollBar":
                    JavaScrollTreeItem JSTI = new JavaScrollTreeItem();
                    JSTI.JavaElementInfo = JEI;                    
                    return JSTI;
                case "javax.swing.JTree":
                case "com.amdocs.uif.widgets.TreeNative$SmartJTree":
                    JavaTreeTreeItem JTRTI = new JavaTreeTreeItem();
                    JTRTI.JavaElementInfo = JEI;
                    return JTRTI;
                case "javax.swing.JMenu":
                    JavaMenuTreeItem JMTI = new JavaMenuTreeItem();
                    JMTI.JavaElementInfo = JEI;                    
                    return JMTI;
                case "javax.swing.JTabbedPane":
                case "com.amdocs.uif.widgets.JXTabbedPane":
                    JavaTabTreeItem JTTI = new JavaTabTreeItem();
                    JTTI.JavaElementInfo = JEI;                    
                    return JTTI;
                case "javax.swing.JInternalFrame":
                case "com.amdocs.uif.workspace.MDIWorkspace$27":
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
