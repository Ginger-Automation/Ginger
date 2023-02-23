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

using Amdocs.Ginger.Common.UIElement;
using System;
using System.Collections.Generic;
using System.Linq;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;

namespace GingerCore.Drivers.Common
{
    // Generic class to help any driver who implement IXpath to create Xpath for element and to find element by XPath
    // XPath helper will be used by all drivers to get unifed Xpath in one easy way, the driver need to implement IXPath to get the service from this Class
    // By doing so we don't need to implement Creating Xpath for element in each driver, or finding element by xpath in each driver, we have one unified code to do it
    // We also get unified Xpath style for all driver which can find element usign complex Xpath

    // Sample XPaths - not all implemented the one marked with 'V' are working
    // --------------
    // 1.  Root                           / = root - the App window
    // V 2.  Using Elements Name+index      /Window1/Pane[2]/Button1   - start from root, find element with Name 'Window1' find sub element Name = Pane index 2, find sub element Name = Button1
    // 3.  Using Element Property         /Window1/[LocalizedControlType:button[2]]  - start from root, find element with Name 'Window1' find sub element LocalizedControlType = button index 2
    // 4.  * = All sub elem
    // 5.  Get parent, or sibling  -  2 parents up then get item-  ../../item
    // 6.  Get last element            /bookstore/book[last()]   - get last book
    // 7.  Get element before last     /bookstore/book[last()-1]
    // 8.  Find Element then goto to next sibling
    // 9.  select elem param with attr id = mine   para[@id='mine']   - 
    // 10. select node by child attr val: contents[para/@id='mine']  This selects a 'contents' element that has a child 'para' with an attribute 'id' of value 'mine'.
    // 11. select all descendent   '//'

    //TODO: check if there is other standard in W3C
    // * item with / - in XPath will be changed to: '&#x2F'  - so the split will work when parsing sample below
    //
    //                                                                \_/                     \___/  
    //                                                                 |                      |   |
    // Escape forward slash '/' = '&#x2F' (slash in hex) --> [@name='c:/temp']  ==> [@name='c:&#x2Ftemp']
    
    public class XPathHelper
    {
        string XpathSlash = "&#x2F";  // avodiing conflict of xpath splitter, we replace '/' in value to '/' in hex which is &#x2F        

        IXPath mDriver;
        List<string> mImportnatProperties;  // [0] is Name(Windows)/ID(Web) which means will not have [] and top importnat property        

        public XPathHelper(IXPath Driver, List<string> ImportnatProperties)
        {
            mDriver = Driver;
            mImportnatProperties = ImportnatProperties;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Create XPath
        // -------------------------------------------------------------------------------------------------------------------------------------

        //Go up to root = abs - use one Importnat Prop or index of no importnat prop have value
        public string GetElementXpathAbsulote(ElementInfo EI)
        {            
            string Xpath = "";   
            
            ElementInfo node = EI;            

            // If it is the window then return / = root 
            if (string.IsNullOrEmpty(EI.GetElementType()) || EI.ElementType.ToUpper() == "WINDOW")
            {
                return "/";
            }

            while (node != null)
            {
                if (Xpath.Length > 0)
                {
                    Xpath = @"/" + Xpath;
                } 
               
                string nodepath = GetElemntNodeXpath(node);
                nodepath = nodepath.Replace("/", XpathSlash);                                

                Xpath = nodepath + Xpath;

                // Goto to Parent                
                node = mDriver.GetElementParent(node);
            }

            Xpath = "/" + Xpath; // root start from '/'
            return Xpath;
        }

        //Go up while not unique, try to add attr based on important prop and avoid long Xpath, 
        //Smart = Name='abc' & ControlType=Button & ... small Xpath more props comb but unique
        public string GetElementXpathSmart(ElementInfo EI)
        {
            string SmartXPath = "";
            // temp for now we return all the important properties which have value
            SmartXPath = mDriver.GetElementProperty(EI, mImportnatProperties[0]);

            //TODO: test if unique in tree if yes return

            // else  add more properties
            for (int i = 1; i < mImportnatProperties.Count; i++)
            {
                string val = mDriver.GetElementProperty(EI, mImportnatProperties[0]);
                if (!string.IsNullOrEmpty(val))
                {
                    SmartXPath += "[" + mImportnatProperties[i] + ":" + val + "]";
                }

                //TODO: test if unique in tree if yes return
            }
            return SmartXPath;
        }

        private string GetElemntNodeXpath(ElementInfo EI)
        {
            string XPath = "";

            foreach (string prop in mImportnatProperties)
            {                
                string val = mDriver.GetElementProperty(EI, prop);
                if (!string.IsNullOrEmpty(val))
                {
                    // if it is the first property then it is Name and no need to identify it
                    if (prop == mImportnatProperties[0])
                    {
                        XPath = val + "%INDEX%";   // we don't know if there will be index so prep place
                    }
                    else
                    {
                        val = val.Replace(":", @"\:").Replace("[", @"\[").Replace("]", @"\]");
                        XPath = "[" + prop + ":" + val + "%INDEX%]";
                    }
                    
                    //Need to see if we have similar item with same property under same parent if yes need to add index
                    int? index = GetPropValIndex(EI, prop, val);
                    string SIndex = null;
                    if (index == null || index == 0)
                    {
                        SIndex = "";
                    }
                    else
                    {
                        SIndex = "[" + index + "]";
                    }

                    XPath = XPath.Replace("%INDEX%", SIndex);

                    //TODO: Decide to break if it is unique in parent.childs , otherwise add more props
                    // TODO: check for indexes
                    break;
                }                
            }
            return XPath;
        }

        private int? GetPropValIndex(ElementInfo EI, string prop, string val)
        {
            //first go prev if index is more than 1 then done
            // if found 0 before then go next, if found then return index
            // none found return null - no need for index since unique

            ElementInfo parent = mDriver.GetElementParent(EI);
            List<XpathPropertyCondition> conditions = new List<XpathPropertyCondition>();
            conditions.Add(new XpathPropertyCondition() { PropertyName = prop, Op = XpathPropertyCondition.XpathConditionOperator.Equel, Value = val });

            int? index = null;
            ElementInfo EI11 = mDriver.GetPreviousSibling(EI);
            while (EI11 != null)
            {
                string val1 = mDriver.GetElementProperty(EI11, prop);
                if (val1 == val) 
                {
                    if (index == null)
                    {
                        index = 0; 
                    }
                    else
                    {
                        index++;
                    }
                }
                EI11 = mDriver.GetPreviousSibling(EI11);
            }

            if (index == null) // we didn't find matching prev sibling, so try forward, stop on the first and make it [0]
            {
                ElementInfo EI12 = mDriver.GetNextSibling(EI);
                while (EI12 != null)
                {
                    string val1 = mDriver.GetElementProperty(EI12, prop);
                    if (val1 == val)
                    {
                        index = 0;
                        break;
                    }
                    EI12 = mDriver.GetNextSibling(EI12);
                }
                //no index found so unique, no need for [index]
            }
            return index;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Find Element By XPath
        // -------------------------------------------------------------------------------------------------------------------------------------

        public ElementInfo GetElementByXpath(string XPath)
        {            
            string[] Nodes = XPath.Split('/');
            string OKPath = "";
            ElementInfo EI = null;
            
            foreach (string node in Nodes)
            {
                if (node.ToUpper() == "DESKTOP") { EI = mDriver.UseRootElement(); continue; }
                //Check special case - root, or multiple *
                if (EI == null)   // We begin then need to know if from root or go find anywhere
                {
                    if (node == "") // We start from root, the Xpath started with '/'                        
                    {
                        EI = mDriver.GetRootElement();
                        if (EI == null)
                        {
                            EI = mDriver.UseRootElement();
                        }
                        continue;
                    }                   
                    else
                    {
                        // Need to Find node in all descendant = Find anywhere, Xpath didn't start with '/'
                        EI = mDriver.GetRootElement();
                        EI = FindNodeAnywhere(EI,node);
                        if (EI == null)
                        {
                            throw new Exception("XPath err - XPath OK until element: '" + OKPath + "' , but FindNodeAnywhere failed to locate: '" + node + "'");
                        }
                        continue;
                    }
                }
                string nodePath = node.Replace(XpathSlash, "/");  //move up
                EI = FindNode(EI, nodePath);  
                if (EI == null)
                {
                    //error cannot drill down more

                    throw new Exception("XPath err - XPath OK until element: '" + OKPath + "' , but the next sub element fail: '" + node + "'");
                }
                OKPath += " --> " + node;
            }

            return EI;
        }


        // Find the first node in all descendant BaseElement that match the conditions, start search in BaseEleemnt Children, and drill down to children, childrent children etc.
        // Return the first element found
        // Scan the full try if needed
        // Recursive function
        private ElementInfo FindNodeAnywhere(ElementInfo BaseElement, string NodePath)
        {
            ElementInfo EI = FindNode(BaseElement, NodePath);
            
            List<ElementInfo> children = mDriver.GetElementChildren(BaseElement);
            // we drill per each child till the end, once done move to next child, so faster and less memory.
            foreach (ElementInfo EIChild in children)
            {
                EI = FindNode(EIChild, NodePath);
                if (EI != null) 
                    return EI;
                // Not found in Base Children let's drill down recursively
                EI = FindNodeAnywhere(EIChild, NodePath);
                if (EI != null)
                    return EI;
            }
            return EI;
        }

        // Find node in BaseElement children that match the conditions - one level
        private ElementInfo FindNode(ElementInfo BaseElement, string NodePath)
        {
            List<XpathPropertyCondition> conditions = new List<XpathPropertyCondition>();

            int? index = null;
            string XpathLeftBrck = "&#lb2F";
            string XpathRightBrck = "&#rb2F";
            string XpathCol = "&#col2F";
            NodePath = NodePath.Replace(@"\[", XpathLeftBrck).Replace(@"\]", XpathRightBrck).Replace(@"\:", XpathCol);
            GetNodePathIndex(ref NodePath, ref index);

            //add conditions
            //TODO: currently handle one prop, handle multiple [[a:1][b:2]...]
            string prop = null;
            string val = null;
            GetPropValIndex(NodePath, ref prop, ref val);
            val= val.Replace(XpathLeftBrck,"[").Replace(XpathRightBrck,"]").Replace(XpathCol,":");
            XpathPropertyCondition cond = new XpathPropertyCondition() { PropertyName = prop, Value = val };
            conditions.Add(cond);
            ElementInfo RC;

            if (!NodePath.StartsWith("[") || index != null & index != 0 )
            {
                if (index == null || index == 0)
                {
                    if (BaseElement == null)
                    {
                        throw new Exception("TreeScope element not found: " + NodePath);
                    }
                    RC = mDriver.FindFirst(BaseElement, conditions);
                    return RC;
                }
                else
                {
                    //TODO: fix me best way is to get first then move next per number of index
                    //meanwhile get all and take the index, slow but will work
                    List<ElementInfo> list = mDriver.FindAll(BaseElement, conditions);
                    RC = list[(int)index];
                    return RC;
                }
            }

            RC = mDriver.FindFirst(BaseElement, conditions);
            return RC;
            
            //node can be 'Name' or '[LocalizedControlType:button]' or with index '[LocalizedControlType:button[3]]'
            // or multiple attrs, will handle later - [[LocalizedControlType:button][AutomationID:A123]...]

            // Make sure it is like standard Xpath!!!

            // if no bracket then it is Name or first important property

            // if we have brackets, it is pair of prop value
        }

        private void GetPropValIndex(string NodePath, ref string prop, ref string val)
        {
            if (NodePath.IndexOf(']') > 0)
            {
                string[] a = NodePath.Substring(1, NodePath.Length - 2).Split(':');
                if(a.Length < 3)
                {
                    prop = a[0];
                    val = a[1];
                }
                else
                {
                    int i = NodePath.IndexOf(":");
                    prop = NodePath.Substring(1, i - 1);
                    val = NodePath.Substring(i + 1, NodePath.Length - prop.Length - 3);
                }
            }          
            else if (!NodePath.Contains(':') || NodePath.Contains("file:") || NodePath.Contains('*') || NodePath.IndexOf(':') == NodePath.Length - 1) //TODO add all special chars
            {
                prop = "Name";
                val = NodePath;
            }
            else
            {
                if (NodePath[0] != '[' && NodePath.Contains(':'))
                {
                    prop = "Name";
                    val = NodePath;
                }
                else
                {
                    string[] a = NodePath.Substring(0, NodePath.Length).Split(':');
                    prop = a[0];
                    val = a[1];
                }
            }
        }

        // Get the index if exist and update the NodePath
        private void GetNodePathIndex(ref string NodePath, ref int? index)
        {
            if (string.IsNullOrEmpty(NodePath)) return;            
            int i1 = NodePath.Substring(1).IndexOf('[');
            int i2 = NodePath.IndexOf(']');

            if (i1 > 0 && i2 > 0) // We have index
            {
                string sIDX = NodePath.Substring(i1 + 2, i2 - i1 - 2);
                if(NodePath.EndsWith("]]"))
                    NodePath = NodePath.Substring(0, i1 + 1) + "]";
                else
                    NodePath = NodePath.Substring(0, i1 + 1);
                index = int.Parse(sIDX);
            }
        }

        //return all matching element for the XPath not just the first found
        public List<ElementInfo> GetElementsByXpath(string XPath)
        {
            //TODO: FIXME!!!
            //temp just do find first

            List<ElementInfo> list = new List<ElementInfo>();
            ElementInfo EI =  GetElementByXpath(XPath);
            list.Add(EI);
            return list;
        }

        public string GetElementRelXPath(ElementInfo elemInfo, PomSetting pomSetting=null)
        {
            var relxpath = "";
            string xpath = elemInfo.XPath;
            List<object> elemsList = null;
            try
            {
                while (relxpath.IndexOf("//") == -1 && elemInfo.ElementObject != null)
                {
                    string id = mDriver.GetElementID(elemInfo);
                    if (!string.IsNullOrEmpty(id))
                    {
                        relxpath = xpath.Replace(elemInfo.XPath, "//" + mDriver.GetElementTagName(elemInfo).ToLower() + "[@id='" + id + "']");
                        elemsList = mDriver.GetAllElementsByLocator(eLocateBy.ByRelXPath,relxpath);
                        if (elemsList == null || (elemsList != null && elemsList.Count() < 2)) {
                            continue;
                        }                        
                    }
                    string name = Convert.ToString(mDriver.GetElementProperty(elemInfo,"name"));
                    if (!string.IsNullOrEmpty(name))
                    {
                        if(relxpath == "")
                        { 
                            relxpath = xpath.Replace(elemInfo.XPath, "//" + mDriver.GetElementTagName(elemInfo).ToLower() + "[@name='" + name + "']");
                        }
                        else
                        { 
                            relxpath = xpath.Replace(elemInfo.XPath, "//" + mDriver.GetElementTagName(elemInfo).ToLower() + "[@id='" + id + "' and @name ='" + name + "']");
                        }
                        elemsList = mDriver.GetAllElementsByLocator(eLocateBy.ByRelXPath, relxpath);
                        if (elemsList == null || (elemsList != null && elemsList.Count() < 2))
                        {
                            continue;
                        }
                    }
                    if(relxpath.IndexOf("//") != -1 && elemsList != null)
                    {
                        string path = relxpath;
                        for (int i = 1; i <= elemsList.Count(); i++)
                        {
                            relxpath = "(" + path + ")[" + i + "]";
                            List<object> newElem = mDriver.GetAllElementsByLocator(eLocateBy.ByRelXPath, relxpath);
                            if (newElem != null && newElem.Count() >0 && newElem[0].Equals(elemInfo.ElementObject))
                            {
                                break;
                            }
                        }
                        continue;
                    }
                    if(relxpath== "")
                    {
                        elemInfo = mDriver.GetElementParent(elemInfo,pomSetting);
                        if (elemInfo is HTMLElementInfo && !string.IsNullOrEmpty(((HTMLElementInfo)elemInfo).RelXpath))
                        {
                            relxpath = xpath.Replace(elemInfo.XPath, ((HTMLElementInfo)elemInfo).RelXpath);
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                relxpath = xpath;
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in GetElementRelXPath ::", e);
            }
            if (relxpath == "")
            {
                relxpath = xpath;
            }                
            return relxpath;
        }


        internal string CreateRelativeXpathWithTagNameAndAttributes(ElementInfo elementInfo)
        {
            var relXpath = string.Empty;

            try
            {
                //creating relative xpath with multiple attribute and tagname
                if (elementInfo.Properties.Count > 1)
                {
                    System.Text.StringBuilder elementAttributes = new System.Text.StringBuilder();
                    var fieldInfos = typeof(ElementProperty).GetFields();
                    List<string> propNames = new List<string>();

                    foreach (var fieldInfo in fieldInfos)
                    {
                        propNames.Add(fieldInfo.GetValue(null).ToString());
                    }
                    foreach (var prop in elementInfo.Properties)
                    {
                        if (!propNames.Contains(prop.Name) && !prop.Name.ToLower().Equals("value") && !prop.Name.ToLower().Equals("id") && !prop.Name.ToLower().Equals("name"))
                        {
                            if (!prop.Value.Contains(";"))
                            {
                                elementAttributes.Append(string.Concat("@", prop.Name, "=", "\'", prop.Value, "\'", " ", "and", " "));
                            }
                        }
                    }

                   if(!string.IsNullOrEmpty(elementAttributes.ToString()))
                   {
                        elementAttributes = elementAttributes.Remove(elementAttributes.Length - 5, 5);
                        relXpath = string.Concat("//", mDriver.GetElementTagName(elementInfo), "[", elementAttributes, "]");
                   }
                }
            }
            catch (Exception  ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Error  occured when creating  relative xapth with attributes values", ex);
            }
                
            return relXpath;
            
        }

        internal string CreateRelativeXpathWithTextMatch(ElementInfo elementInfo,bool isExactMatch=true)
        {
            var relXpath = string.Empty;

            // checking svg element
            var isParentContainsSVG = mDriver.GetInnerHtml(elementInfo).Contains("<svg");
            if(isParentContainsSVG)
            {
                return relXpath;
            }

            var tagStartWithName = "*";
            var tagName = mDriver.GetElementTagName(elementInfo);

            if (tagName.ToLower().Equals(eElementType.Label.ToString().ToLower()) ||(tagName.ToLower().Equals(eElementType.Div.ToString().ToLower()) && !isExactMatch))
            {
                tagStartWithName = tagName;
            }
            
            var innerText = mDriver.GetInnerText(elementInfo);
            if (isExactMatch)
            {
                relXpath = string.Concat("//", tagStartWithName, "[text()=", "\'", innerText, "\'", "]");
            }
            else
            {
                relXpath = string.Concat("//", tagStartWithName, "[contains(text(),", "\'", innerText, "\'", ")]");
            }

            return relXpath;
        }

        //creating xpath with previous sibling for ex: //*[text()="Name"]//following::input
        internal string CreateRelativeXpathWithSibling(ElementInfo elementInfo)
        {
            var relXpath = string.Empty;

            var previousSiblingInnerText = mDriver.GetPreviousSiblingInnerText(elementInfo);
            
            if (!string.IsNullOrEmpty(previousSiblingInnerText))
            {
                relXpath = string.Concat("//*[text()=\'", previousSiblingInnerText, "\']//following::",mDriver.GetElementTagName(elementInfo));
            }
            
            return relXpath;
        }
    }
}