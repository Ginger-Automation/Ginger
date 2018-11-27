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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using mshtml;
using SHDocVw;
using GingerCore.Drivers.Common;
using GingerCore.Actions;
using HtmlAgilityPack;
using GingerCore.Actions.Common;
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Automation;
using Amdocs.Ginger.Common.UIElement;

namespace GingerCore.Drivers.PBDriver
{
    public class HTMLHelper : IXPath
    {
        public delegate void DomEvent(IHTMLEventObj obj);
        public InternetExplorer browserObject;
        HTMLElementInfo CurrentWindowRootElement;
        DispHTMLDocument dispHtmlDocument;
        IHTMLElement CurrentHighlightedElement;
        string currHighlightedAttr;
        IHTMLElement currentFrame;
        HTMLDocument currentFrameDocument;
        HtmlAgilityPack.HtmlDocument HAPDocument;
        mshtml.HTMLDocument mHtmlDocument;
        XPathHelper mXPathHelper;
        List<string> ImportantProperties = new List<string>();
        mshtml.IHTMLDocument3 sourceDoc;
        string documentContents;
        public IHTMLDocument2 frameContent;
        public DispHTMLDocument frameDocument;
        List<ElementInfo> frameElementsList = new List<ElementInfo>();
        AutomationElement AEBrowser;

        public List<ElementInfo> GetVisibleElement()
        {
            List<ElementInfo> list = GetElementList();
            return list;
        }

        public HTMLHelper(InternetExplorer IE,AutomationElement AE)
        {
            browserObject = IE;
            AEBrowser = AE;
            dispHtmlDocument = (DispHTMLDocument)browserObject.Document;
            mHtmlDocument = browserObject.Document;
            sourceDoc = (mshtml.IHTMLDocument3)browserObject.Document;
            documentContents = sourceDoc.documentElement.outerHTML;
            HAPDocument = new HtmlAgilityPack.HtmlDocument();
            HAPDocument.LoadHtml(documentContents);
            CurrentWindowRootElement = GetHtmlElementInfo(dispHtmlDocument.documentElement);          
            currentFrame = null;
            currentFrameDocument = null;
            InitXpathHelper();
        }

        public void injectScriptCode(mshtml.HTMLDocument doc, string JSCode)
        {
            var script = (IHTMLScriptElement)doc.createElement("SCRIPT");
            script.type = "text/javascript";

            script.text = JSCode;

            InjectJSScript(doc, script);
        }

        public void InjectJSScript(mshtml.HTMLDocument doc, IHTMLScriptElement JavaSCript)
        {
            IHTMLElementCollection nodes = doc.getElementsByTagName("head");
            foreach (IHTMLElement elem in nodes)
            {
                var head = (HTMLHeadElement)elem;
                head.appendChild((IHTMLDOMNode)JavaSCript);
            }
        }

        public List<ElementInfo> GetElementChildren(ElementInfo Ei)
        {
            List<ElementInfo> EIlist = new List<ElementInfo>();
            IHTMLElement node = null;
            IHTMLDOMNode domNode = null;
            HTMLElementInfo HTMLEI;
            IHTMLElementCollection coll;
            Reporter.ToLog(eAppReporterLogLevel.INFO, "GetElementChildren::" + Ei.XPath);
            if (Ei.ElementObject == null)
            {
                if(currentFrameDocument !=null)
                    domNode = currentFrameDocument.firstChild;
                else
                    domNode = dispHtmlDocument.firstChild;         
                node = domNode as IHTMLElement;
                coll = node.children;

                while (coll.length == 0 && domNode.nextSibling != null)
                {
                    domNode = domNode.nextSibling;
                    node = domNode as IHTMLElement;
                    coll = node.children;
                }
            }
            else
            {
                node = (IHTMLElement)Ei.ElementObject;
                coll = node.children;
            }

            foreach (IHTMLElement h1 in coll)
            {
                Reporter.ToLog(eAppReporterLogLevel.INFO, "HTMLElementInfo1::" + h1.className);
                HTMLEI = GetHtmlElementInfo(h1);
                HTMLEI.WindowExplorer = Ei.WindowExplorer;
                HTMLEI.ElementObject = h1;
                EIlist.Add(HTMLEI);
                Reporter.ToLog(eAppReporterLogLevel.INFO, "HTMLElementInfo2::" + HTMLEI.XPath);
            }
            if (node.tagName.ToLower().Equals("iframe"))
            {
                try
                {
                    InitFrame(node);
                    ElementInfo htmlRootEI = new ElementInfo();
                    htmlRootEI.XPath = "/";
                    htmlRootEI.WindowExplorer = Ei.WindowExplorer;
                    EIlist = GetElementChildren(htmlRootEI);                    
                }
                catch (Exception e)
                {
                    Reporter.ToLog(eAppReporterLogLevel.INFO, "frameDocument Exception1::" + e.Message);
                }
            }
            return EIlist;
        }

        internal HtmlNode GetActNode(ActTableElement act)
        {
            HtmlNode CurAE = null;
            try
            {
                string LocValueCalculated = act.LocateValueCalculated;
                CurAE = FindNodeByLocator(act.LocateBy, LocValueCalculated);
            }
            catch (ContextMarshalException e)
            {
                act.ExInfo += "Invalid Current Window. Please do switch do the correct window using Switch window before running the action";
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
            }
            return CurAE;
        }

        private HtmlNode FindNodeByLocator(eLocateBy locateBy, string LocValueCalculated)
        {
            HtmlNode HEle = null;
            sourceDoc = (mshtml.IHTMLDocument3)browserObject.Document;
            documentContents = sourceDoc.documentElement.outerHTML;
            HAPDocument = new HtmlAgilityPack.HtmlDocument();
            HAPDocument.LoadHtml(documentContents);

            switch (locateBy)
            {
                case eLocateBy.ByID:
                    HEle = HAPDocument.GetElementbyId(LocValueCalculated);

                    break;
                case eLocateBy.ByName:
                case eLocateBy.ByRelXPath:
                case eLocateBy.ByXPath:
                    HEle = HAPDocument.DocumentNode.SelectSingleNode(LocValueCalculated);
                    if(HEle == null)
                    {
                        IHTMLElement htmlEle = GetElementByXPath(LocValueCalculated);
                        HTMLElementInfo HEI = GetHtmlElementInfo(htmlEle);
                        string xpath = GetElementXPath(HEI);
                        HEle = HAPDocument.DocumentNode.SelectSingleNode(xpath);
                    }
                    break;
                default:
                    throw new Exception("Locator not implement - " + locateBy.ToString());
            }

            return HEle;
        }

        internal void HandleWidgetTableControlAction(ActTableElement actWWC, object htmlTableElement)
        {
            DataTable nodeDataTable = HTMLTableToDataTable((HtmlNode)htmlTableElement);

            int RowCount = -1, RowNumber = -1;
            RowCount = nodeDataTable.Rows.Count;
            if(RowCount==0)
            {
                actWWC.Error = "Widget Table has no rows ";
                return;
            }
            if (actWWC.ControlAction.Equals(ActTableElement.eTableAction.GetRowCount))
            {
                string value = RowCount.ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    actWWC.AddOrUpdateReturnParamActual("Actual", value);
                    actWWC.ExInfo = value;
                }
                else
                {
                    actWWC.Error = "Unable to get Row Count";
                }
                return;
            }
            switch (actWWC.LocateRowType)
            {
                case "Row Number":
                    RowNumber = Convert.ToInt32(
                        actWWC.GetInputParamCalculatedValue(ActTableElement.Fields.LocateRowValue));
                    if (RowNumber >= RowCount)
                    {
                        actWWC.Error = "Given Row Number " + RowNumber + " is not present in Column";
                        return;
                    }
                    HandleWidgetNodeTableAction(nodeDataTable,actWWC, RowNumber,false);
                    break;

                case "Any Row":
                    Random rnd = new Random();
                    RowNumber = rnd.Next(0, RowCount);
                    HandleWidgetNodeTableAction(nodeDataTable,actWWC, RowNumber,false);

                    break;

                case "By Selected Row":
                    break;

                case "Where":
                    HandleWidgetNodeTableAction(nodeDataTable,actWWC, RowNumber,true);

                    break;
                default:
                    actWWC.Error = "Action  - " + actWWC.LocateRowType + " not supported";
                    break;
            }
        }

        #region HtmlAgility Pack-HtmlNode
        private int GetElementWithWhereClasue(DataTable nodeDataTable, ActTableElement actWWC)
        {
            //int rowIndex = 0;
            string whereColTitle = actWWC.WhereColumnTitle;
            string whereColValue = actWWC.WhereColumnValue;
            try
            {
                List<HtmlNode> whereColumnValues = nodeDataTable.AsEnumerable().
                    Select(c => c.Field<HtmlNode>(whereColTitle)).ToList<HtmlNode>();
                int index = -1;
                string innerText = string.Empty;
                string outerHtml = string.Empty;
                for (index=0;index< whereColumnValues.Count;index++)
                {
                    innerText = whereColumnValues[index].InnerText;
                    innerText = Regex.Replace(innerText, @"\t|\n|\r", "");

                    outerHtml = whereColumnValues[index].OuterHtml;
                    outerHtml = Regex.Replace(outerHtml, @"\t|\n|\r", "");

                    if (innerText.Equals(whereColValue)|| innerText.Contains(whereColValue))
                        return index;
                    if (outerHtml.Equals(whereColValue) || outerHtml.Contains(whereColValue))
                        return index;
                }
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
            return -1;
        }

        private static HtmlNode GetElementWOWhereClasue(DataTable nodeDataTable,
            ActTableElement.eRunColSelectorValue ColSelectorValue, string LocateColTitle, int RowNo)
        {
            HtmlNode HAE = null;
            if (ColSelectorValue.Equals(ActTableElement.eRunColSelectorValue.ColNum))
            {
                HAE = (HtmlNode)nodeDataTable.Rows[RowNo][Convert.ToInt32(LocateColTitle)];
            }
            if (ColSelectorValue.Equals(ActTableElement.eRunColSelectorValue.ColTitle))
            {
                HAE = (HtmlNode)nodeDataTable.Rows[RowNo][LocateColTitle];
            }

            return HAE;
        }
        private void HandleWidgetNodeTableAction(DataTable nodeDataTable,ActTableElement actTableElement, int RowNo,bool isWhereClause)
        {
            HtmlNode HAE = null;

            if (isWhereClause)
            {
                RowNo = GetElementWithWhereClasue(nodeDataTable, actTableElement);
            }
            if (RowNo == -1)
            {
                actTableElement.Error = "Matching where criteria not found";
                return;
            }
            HAE = GetElementWOWhereClasue(nodeDataTable, actTableElement.ColSelectorValue, actTableElement.LocateColTitle, RowNo);

            if (HAE == null)
            {
                actTableElement.Error = "Element not Found - " + actTableElement.LocateRowType +
                    " " + actTableElement.LocateColTitle;
                return;
            }
            IHTMLElement IHAE = GetIHTMLElementFromHtmlNode(HAE);

            IHTMLElementAction(actTableElement, IHAE);
        }

        private void IHTMLElementAction(ActTableElement actTableElement, IHTMLElement HAE)
        {
            string value = string.Empty;
            switch (actTableElement.ControlAction)
            {
                case ActTableElement.eTableAction.GetValue:
                    value=GetValue(HAE, actTableElement.Value);
                    if (value!=null)
                    {
                        actTableElement.AddOrUpdateReturnParamActual("Actual", value);
                        actTableElement.ExInfo = value;
                    }
                    else
                        actTableElement.Error += "Unable to Get Value";
                    break;
                case ActTableElement.eTableAction.SetValue:

                    value = actTableElement.Value;
                    bool flag=SetValue(HAE, value, "innerText");
                    if (HAE.innerText.Equals(value))
                    {
                        actTableElement.ExInfo = value + " - Value set";
                    }
                    else
                        actTableElement.Error += "Unable to Set Value";
                    break;

                case ActTableElement.eTableAction.Click:
                    bool status = Click(HAE);

                    if (!status)
                    {
                        actTableElement.Error += "Unable to Click";
                    }
                    else
                        actTableElement.ExInfo += "Clicked Successfully";
                    break;
                    
                default:
                    actTableElement.Error = "Action  - " + actTableElement.ControlAction +
                        " not supported for Grids";
                    break;
            }
        }

        private IHTMLElement GetIHTMLElementFromHtmlNode(HtmlNode hAE)
        {
            IHTMLElement HAE = null;
            HTMLDocument mHtmlDocument = browserObject.Document;
            IHTMLElementCollection HAEColl= mHtmlDocument.all;
            string ihtmlInnerHTML = string.Empty;

            string htmlNodeInnerHTML = hAE.InnerHtml.ToLower().Replace("\r\n","");
            htmlNodeInnerHTML=Regex.Replace(htmlNodeInnerHTML, @"(\s+|@|&|'|\\|\(|\)|<|>|#)", "");
            htmlNodeInnerHTML = Regex.Replace(htmlNodeInnerHTML, @"\\", "");
            htmlNodeInnerHTML = Regex.Replace(htmlNodeInnerHTML, @"""","");

            foreach (IHTMLElement item in HAEColl)
            {
                ihtmlInnerHTML=item.innerHTML == null ? string.Empty : item.innerHTML.ToLower().Replace("\r\n", "");
                if (string.IsNullOrEmpty(ihtmlInnerHTML)) continue;
                ihtmlInnerHTML = Regex.Replace(ihtmlInnerHTML, @"(\s+|@|&|'|\\|\(|\)|<|>|#)", "");
                ihtmlInnerHTML = Regex.Replace(ihtmlInnerHTML, @"\\", "");
                ihtmlInnerHTML = Regex.Replace(ihtmlInnerHTML, @"""", "");
                if (ihtmlInnerHTML == htmlNodeInnerHTML)
                {
                    HAE = item;
                    break;
                }
            }
            IHTMLElementCollection childHAECollection = HAE.children;
            foreach (IHTMLElement cellChild in childHAECollection)
            {

                return cellChild;
            }
            return HAE;
        }

        #endregion

        private DataTable HTMLTableToDataTable(HtmlNode htmlTableNode)
        {
            DataTable nodeDataTable = new DataTable();
            DataRow rowNodeTable = null;
            bool collHeaderCols = true;
            int colSeq = -1;
            bool rowSepartor = false;
            int ColCount = 0;
            int cellsCount = -1;
            HtmlNodeCollection nodeCollection = htmlTableNode.SelectNodes(".//tr");
            foreach (HtmlNode row in nodeCollection)
            {
                if (row != null)
                {
                    if (collHeaderCols)
                    {
                    }
                    else
                    {
                        rowNodeTable = nodeDataTable.NewRow();
                    }
                    cellsCount = row.SelectNodes("th|td").Cast<HtmlNode>().Count();
                    if (cellsCount == 1)
                    {
                        foreach (HtmlNode cell in row.SelectNodes("th|td").Cast<HtmlNode>())
                        {
                            if (string.IsNullOrEmpty(cell.InnerText))
                            {
                                rowSepartor = true;
                                break;
                            }
                            else
                            {
                                string cellText = string.IsNullOrEmpty(cell.InnerText) ? string.Empty : cell.InnerText;
                                cellText = Regex.Replace(cellText, @"\t|\n|\r", "");
                                if (collHeaderCols)
                                {
                                    ColCount++;
                                    nodeDataTable.Columns.Add(cellText,typeof(HtmlNode));
                                }
                                else
                                {
                                    rowNodeTable[colSeq++] = cell;

                                    if (ColCount == colSeq + 1 && ColCount > cellsCount)
                                    {
                                        rowNodeTable[colSeq++] = htmlTableNode.OwnerDocument.CreateElement("TD");
                                    }
                                }
                            }
                        }
                    }
                    if (rowSepartor)
                    {
                        rowSepartor = false;
                        continue;
                    }
                    if ((cellsCount > 1 && cellsCount == ColCount  || collHeaderCols) && cellsCount != 1)
                    {
                        foreach (HtmlNode cell in row.SelectNodes("th|td").Cast<HtmlNode>())
                        {
                            if (cell == null) { continue; }
                            string cellText = string.IsNullOrEmpty(cell.InnerText) ? string.Empty : cell.InnerText;
                            cellText = Regex.Replace(cellText, @"\t|\n|\r", "");
                            if (collHeaderCols)
                            {
                                ColCount++;
                                nodeDataTable.Columns.Add(cellText, typeof(HtmlNode));
                            }
                            else
                            {
                                rowNodeTable[colSeq++] = cell;

                                if (ColCount == colSeq + 1 && ColCount > cellsCount)
                                {
                                    rowNodeTable[colSeq++] = htmlTableNode.OwnerDocument.CreateElement("TD");
                                }
                            }
                        }
                    }
                    if (cellsCount > 1 && cellsCount < ColCount)
                    {
                        foreach (HtmlNode cell in row.SelectNodes("th|td").Cast<HtmlNode>())
                        {
                            if (cell == null) { continue; }
                            string cellText = string.IsNullOrEmpty(cell.InnerText) ? string.Empty : cell.InnerText;
                            cellText = Regex.Replace(cellText, @"\t|\n|\r", "");
                            if (collHeaderCols)
                            {
                                ColCount++;
                                nodeDataTable.Columns.Add(cellText, typeof(HtmlNode));
                            }
                            else
                            {
                                rowNodeTable[colSeq++] = cell;

                                if (ColCount == colSeq + 1 && ColCount > cellsCount)
                                {
                                    rowNodeTable[colSeq++] = htmlTableNode.OwnerDocument.CreateElement("TD");
                                }
                            }
                        }
                    }
                    colSeq = 0;
                }
                if(!collHeaderCols)
                    nodeDataTable.Rows.Add(rowNodeTable.ItemArray);
                collHeaderCols = false;
            }
            return nodeDataTable;
        }
        
        public HTMLElementInfo GetHtmlElementInfo(IHTMLElement h1)
        {
            HTMLElementInfo EI = new HTMLElementInfo();
            EI.ElementTitle = getElementTitle(h1);
            EI.ID = getElementId(h1);
            EI.Value = getElementValue(h1);
            EI.Name = getElementName(h1);
            EI.ElementType = getElementType(h1);
            EI.ElementTypeEnum= GetElementTypeEnum(EI.ElementType);
            EI.Path = "";
            EI.ElementObject = h1;
            EI.XPath = getXPath(h1);
            EI.RelXpath = "";
            return EI;
        }

        public HTMLElementInfo GetHtmlElementInfo(IHTMLDOMNode h2)
        {
            Dictionary<string, string> ElemTyp = new Dictionary<string, string>()
            { { "html" ,"HTML"},{ "head" ,"HEAD"},{ "body" ,"BODY"},{ "title" ,"TITLE"},{ "text", "label" },{ "form","FORM"} };
            IHTMLDOMNode h1 = (IHTMLDOMNode)h2;
            HTMLElementInfo EI = new HTMLElementInfo();
            try
            {
                string val = h1.nodeValue.ToString();
                if (val.Length == 0)
                    val = h1.nodeName;
                EI.ElementTitle = val;
                EI.ID = val;
                EI.Value = val;
                EI.Name = val;
                EI.ElementType = ElemTyp.Where(a => h1.nodeName.ToLower().Contains(a.Key)).Select(b => b.Value).ToString();
                EI.ElementTypeEnum = GetElementTypeEnum(EI.ElementType);
                EI.Path = "";
                EI.XPath = "";
                EI.RelXpath = "";
                EI.ElementObject = h1;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
            return EI;
        }

        private string ValidateXPath(string xpath)
        {
            var index = xpath.LastIndexOf("/");
            var lastPath = xpath.Substring(index);

            if (lastPath.Contains("#"))
            {
                xpath = xpath.Substring(0, index);
                lastPath = lastPath.Replace("#", "");
                lastPath = lastPath.Replace("[", "()[");
                xpath = xpath + lastPath;
            }
            return xpath;
        }

        public List<ElementInfo> GetElementList()
        {
            List<ElementInfo> list = new List<ElementInfo>();
            frameElementsList.Clear();
            AddDocumentsAllElements(dispHtmlDocument);
            list.AddRange(frameElementsList);
            frameElementsList.Clear();
            return list;
        }

        void AddDocumentsAllElements(DispHTMLDocument DispDoc)
        {
            IHTMLElementCollection ElementCollection;
            HTMLElementInfo HTMLEI;
            IHTMLFrameBase2 iframeBase;
            IHTMLWindow2 domNode2;
            IHTMLDOMNode domNode;

            ElementCollection = DispDoc.all;
            foreach(IHTMLElement h1 in ElementCollection)
            {
                HTMLEI = new HTMLElementInfo();
                HTMLEI = GetHtmlElementInfo(h1);
                frameElementsList.Add(HTMLEI);

                if (h1.tagName.Equals("iframe") || h1.tagName.Equals("IFRAME"))
                {
                    try
                    {
                        domNode = h1 as IHTMLDOMNode;
                        iframeBase = domNode as IHTMLFrameBase2;
                        domNode2 = iframeBase.contentWindow;
                        frameContent = (IHTMLDocument2)domNode2.document;

                        DispDoc = (DispHTMLDocument)frameContent;
                        AddDocumentsAllElements(DispDoc);
                    }
                    catch(Exception ex)
                    {
                        if (InitFrame(h1) == "true")
                        {
                            AddDocumentsAllElements(currentFrameDocument);
                        }
                        Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                    }
                }
            }
        }
        public static string getXPath(mshtml.IHTMLElement element)
        {
            if (element == null)
                return "";
            mshtml.IHTMLElement currentNode = element;
            ArrayList path = new ArrayList();

            while (currentNode != null)
            {
                string pe = getNode(currentNode);
                if (pe != null)
                {
                    path.Add(pe);
                }
                currentNode = currentNode.parentElement;
            }
            path.Reverse();
            return join(path, "/");
        }

        public static string join(ArrayList items, string delimiter)
        {
            StringBuilder sb = new StringBuilder();
            foreach (object item in items)
            {
                if (item == null)
                    continue;

                sb.Append(delimiter);
                sb.Append(item);
            }
            return sb.ToString();
        }

        public static string getNode(mshtml.IHTMLElement node)
        {
            string nodeExpr = node.tagName.ToLower();
            if (nodeExpr == null)  // Eg. node = #text
                return null;

            // Find rank of node among its type in the parent
            int rank = 1;
            mshtml.IHTMLDOMNode nodeDom = node as mshtml.IHTMLDOMNode;
            mshtml.IHTMLDOMNode psDom = nodeDom.previousSibling;
            if(psDom!=null && psDom.nodeName == "#text")
                psDom = psDom.previousSibling;
            mshtml.IHTMLElement ps = psDom as mshtml.IHTMLElement;
            while (ps != null)
            {
                if (ps.tagName == node.tagName)
                {
                    rank++;
                }
                psDom = psDom.previousSibling;
                if (psDom!=null && psDom.nodeName == "#text")
                    psDom = psDom.previousSibling;
                ps = psDom as mshtml.IHTMLElement;
            }
            nodeExpr += "[" + rank + "]";
            return nodeExpr;
        }
        
        public string getElementValue(IHTMLElement h1)
        {
            string ElementValue = string.Empty;
            string tagName = h1.tagName;
            string text = h1.innerText;
            if (tagName == "select")
            {
                return "set to " + text;
            }
            if (tagName == "span")
            {
                return "set to " + text;
            }
            if (tagName == "A" || tagName == "a")
            {
                if (text != null)
                {
                    if (text.Contains("\n"))
                    {
                        text = text.Replace("\n", "");
                    }
                    if (text.Contains("\r"))
                    {
                        text = text.Replace("\r", "");
                    }
                    return text;
                }
                else
                {
                    if (text != null)
                    {
                        text = h1.outerText;
                        if (text.Contains("\n"))
                        {
                            text = text.Replace("\n", "");
                        }
                        if (text.Contains("\r"))
                        {
                            text = text.Replace("\r", "");
                        }
                        return text;
                    }
                    else
                    {
                        string value = Convert.ToString(h1.getAttribute("value"));
                        value = object.ReferenceEquals(value, null) ? string.Empty : value;
                        return value;
                    }
                }
            }
            string type = "";
            try
            {
                type = Convert.ToString(h1.getAttribute("type"));
            }
            catch(Exception e)
            {
                type = "";
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
            }
            type = object.ReferenceEquals(type, null) ? string.Empty : type;

            if (tagName == "input" && type == "checkbox")
            {
                return "set to " + text;
            }
            else
            {
                string value = Convert.ToString(h1.getAttribute("value"));
                value = object.ReferenceEquals(value, null) ? string.Empty : value;
                if (value == null || tagName == "button")
                    ElementValue = text;
                else
                    ElementValue = value;
            }
            return ElementValue;
        }

        public static eElementType GetElementTypeEnum(string elemType)
        {
            eElementType elementType = eElementType.Unknown;                     

            if (elemType.ToUpper() == "INPUT.TEXT" || elemType.ToUpper() == "TEXTAREA" || elemType.ToUpper() == "INPUT.UNDEFINED" 
                || elemType.ToUpper() == "INPUT.PASSWORD" || elemType.ToUpper() == "INPUT.EMAIL")  // HTML text 
            {
                elementType = eElementType.TextBox;
            }
            else if (elemType.ToUpper() == "INPUT.BUTTON" || elemType.ToUpper() == "BUTTON" || elemType.ToUpper() == "INPUT.IMAGE" || 
                elemType.ToUpper() == "LINK" || elemType.ToUpper() == "INPUT.SUBMIT")  // HTML Button
            {
                elementType = eElementType.Button;
            }
            else if (elemType.ToUpper() == "TD" || elemType.ToUpper() == "TH" || elemType.ToUpper() == "TR")
            {
                elementType = eElementType.TableItem;
            }
            else if (elemType.ToUpper() == "LINK" || elemType.ToUpper() == "A") // HTML Link
            {
                elementType = eElementType.HyperLink;
            }
            else if (elemType.ToUpper() == "LABEL" || elemType.ToUpper() == "TITLE")// HTML Label
            {
                elementType = eElementType.Label;
            }
            else if (elemType.ToUpper() == "SELECT" || elemType.ToUpper() == "SELECT-ONE") // HTML Select/ComboBox
            {
                elementType = eElementType.ComboBox;
            }
            else if (elemType.ToUpper() == "TABLE" || elemType.ToUpper() == "CAPTION")// HTML Table
            {
                elementType = eElementType.Table;
            }
            else if (elemType.ToUpper() == "JEDITOR.TABLE")
            {
                elementType = eElementType.EditorPane;
            }
            else if (elemType.ToUpper() == "DIV") // DIV Element
            {
                elementType = eElementType.Div;
            }
            else if (elemType.ToUpper() == "SPAN")// SPAN Element
            {
                elementType = eElementType.Span;
            }
            else if (elemType.ToUpper() == "IMG" || elemType.ToUpper() == "MAP")// IMG Element
            {
                elementType = eElementType.Image;
            }
            else if (elemType.ToUpper() == "INPUT.CHECKBOX") // Check Box Element
            {
                elementType = eElementType.CheckBox;
            }
            else if (elemType.ToUpper() == "OPTGROUP" || elemType.ToUpper() == "OPTION")// HTML Radio
            {
                return eElementType.ComboBoxOption;
            }
            else if (elemType.ToUpper() == "INPUT.RADIO")// HTML Radio
            {
                elementType = eElementType.RadioButton;
            }
            else if (elemType.ToUpper() == "IFRAME")// HTML IFRAME
            {
                elementType = eElementType.Iframe;
            }
            else if (elemType.ToUpper() == "CANVAS")
            {
                elementType = eElementType.Canvas;
            }
            else if (elemType.ToUpper() == "FORM")
            {
                elementType = eElementType.Form;
            }
            else if (elemType.ToUpper() == "UL" || elemType.ToUpper() == "OL" || elemType.ToUpper() == "DL")
            {
                elementType = eElementType.List;
            }
            else if (elemType.ToUpper() == "LI" || elemType.ToUpper() == "DT" || elemType.ToUpper() == "DD")
            {
                elementType = eElementType.ListItem;
            }
            else if (elemType.ToUpper() == "MENU")
            {
                elementType = eElementType.MenuBar;
            }
            else if (elemType.ToUpper() == "H1" || elemType.ToUpper() == "H2" || elemType.ToUpper() == "H3" || elemType.ToUpper() == "H4" || elemType.ToUpper() == "H5" || elemType.ToUpper() == "H6" || elemType.ToUpper() == "P")
            {
                elementType = eElementType.Text;
            }
            else
                elementType = eElementType.Unknown;

            return elementType;
        }

        public string getElementType(IHTMLElement h1)
        {
            string elementType = string.Empty;
            string tagName = h1.tagName;
            string type = "";
            try
            {
                type = Convert.ToString(h1.getAttribute("type", 0));
            }
            catch (Exception e1)
            {
                type = "";
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e1.Message}", e1);
            }
            type = object.ReferenceEquals(type, null) ? string.Empty : type;

            if (tagName.ToLower() == "input")
                elementType = tagName + "." + type;
            else if (tagName == "a" || tagName == "A" || tagName == "li" || tagName=="LI")
                elementType = "link";
            else
                elementType = tagName;
            return elementType.ToUpper();
        }

        public string getElementName(IHTMLElement h1)
        {
            string name = Convert.ToString(h1.getAttribute("name"));
            name = object.ReferenceEquals(name, null) ? string.Empty : name;

            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }
            else
            {
                name = h1.tagName;
                return name;
            }
        }

        public string getElementTitle(IHTMLElement h1)
        {
            string tagName = h1.tagName;

            if (tagName == "TABLE")
                return "Table";
            string id = h1.id;
            if (!string.IsNullOrEmpty(id))
                return tagName + " ID=" + id;

            string name = "";
            name = Convert.ToString(h1.getAttribute("name"));
            name = object.ReferenceEquals(name, null) ? string.Empty : name;
            if (!string.IsNullOrEmpty(name))
                return tagName + " Name=" + name;
            string value = Convert.ToString(h1.getAttribute("value"));
            value = object.ReferenceEquals(value, null) ? string.Empty : value;
            if (!string.IsNullOrEmpty(value))
                return tagName;
            return tagName;
        }

        internal string GetPageInfo(string val)
        {
            string result = "";
            switch (val)
            {
                case "PageSource":
                    if (currentFrameDocument != null)
                    {
                        result = (dynamic)currentFrameDocument.documentElement.getAttribute("OuterHTML");
                    }
                    else
                    {
                        result = (dynamic)mHtmlDocument.documentElement.getAttribute("OuterHtml");
                    }                   
                    break;
                case "PageURL":
                    result = mHtmlDocument.url;
                    break;
            }
            return result;
        }

        public string GetValue(object HElem, string Attribute="value")
        {
            try
            {
                IHTMLElement elem = null;
                string res=string.Empty;
                if (HElem.GetType().ToString().Contains("mshtml"))
                {
                    elem = ((IHTMLElement)HElem);
                }
                else
                {
                    IHTMLDOMNode2 dom = HElem as IHTMLDOMNode2;
                    elem=((IHTMLDocument2)(dom.ownerDocument)).activeElement;
                }
                res = Convert.ToString(elem.getAttribute(Attribute));
                res = object.ReferenceEquals(res, null) ? string.Empty : res;

                if (res.Equals(""))
                {
                    res = elem.outerText;
                    if(res==null)
                    {
                        res = "";
                    }
                    if (res.Contains("\n"))
                    {
                        res = res.Replace("\n", "");
                    }
                    if (res.Contains("\r"))
                    {
                        res = res.Replace("\r", "");
                    }
                    return res;
                }

                return res;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Exception in GetValue::" + ex.Message, ex);
                return null;
            }
        }

        public string GetNodeAttributeValue(object HElem, string Attribute = "value")
        {
            try
            {
                string res = string.Empty;
                IHTMLDOMNode d1;
                if (HElem.GetType().ToString().Contains("mshtml"))
                {
                    IHTMLElement h1 = (IHTMLElement)HElem;
                    d1 = h1 as IHTMLDOMNode;
                }
                else
                {
                    d1 = (IHTMLDOMNode)HElem;
                }                
                IHTMLAttributeCollection HAttributes = d1.attributes;
                if (HAttributes != null)
                {
                    foreach (IHTMLDOMAttribute d in HAttributes)
                    {
                        if(d.nodeName.ToLower() == Attribute.ToLower())
                        {
                            res = Convert.ToString(d.nodeValue);
                            break;
                        }                        
                        
                    }
                }
                return res;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Exception in GetNodeAttributeValue::" + ex.Message, ex);
                return "";
            }
        }

        public string GetStyle(object HElem)
        {
            try
            {
                IHTMLElement elem = null;
                string res = string.Empty;
                if (HElem.GetType().ToString().Contains("mshtml"))
                {
                    elem = ((IHTMLElement)HElem);
                }
                else
                {
                    IHTMLDOMNode2 dom = HElem as IHTMLDOMNode2;
                    elem = ((IHTMLDocument2)(dom.ownerDocument)).activeElement;
                }
                res = Convert.ToString(elem.style.cssText);
                res = object.ReferenceEquals(res, null) ? string.Empty : res;
                
                return res;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Exception in GetStyle::" + ex.Message,ex);
                return "";
            }
        }

        public string FireSpecialEvent(object HElem, string value)
        {
            if (value == string.Empty)
            {
                return "Error - no value was sent.";
            }
            value = value.Replace(" ", string.Empty);
            value = value.Replace('"'.ToString(), string.Empty);
            value = "\"" + value + "\"";
            try
            {
                IHTMLElement elem = null;
                if (HElem.GetType().ToString().Contains("mshtml"))
                {
                    elem = ((IHTMLElement)HElem);
                }
                else
                {
                    IHTMLDOMNode2 dom = HElem as IHTMLDOMNode2;
                    elem = ((IHTMLDocument2)(dom.ownerDocument)).activeElement;
                }
                string elementId = getElementId(elem);
                if (elementId == string.Empty)
                {
                    return "Error - Element's id doesn't exsits.";
                }
                ((IHTMLElement2)elem).focus();
                string specialEventScript = GetFireSpecialEventScript("document.getElementById", value, elementId);
                if (currentFrameDocument != null)
                {
                    injectScriptCode(currentFrameDocument, specialEventScript);
                }
                else
                {
                    injectScriptCode(browserObject.Document, specialEventScript);
                }
                return value;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return "Error - " + ex;
            }
        }

        private string GetFireSpecialEventScript(string locatorBy, string value, string id)
        {            
            string specialEvent = @" (function fireSpecialEvent()
                                        {                                            
                                            el = " + locatorBy + "('" + id + "');" + @"                                            
                                            var res = " + value + ".split(',');" + @"
                                            var listCreateEvent = res;
                                            if ('createEvent' in document)
                                            {
                                                for (i = 0; i < listCreateEvent.length; i++) {
                                                    try {
                                                        var evt = document.createEvent('HTMLEvents');
                                                        evt.initEvent(listCreateEvent[i].toString(), false, true);
                                                        el.dispatchEvent(evt);
                                                    }
                                                    catch (err) { }
                                                }
                                            }
                                            else
                                            {
                                                for (i = 0; i < listCreateEvent.length; i++) {
                                                    try {
                                                        el.fireEvent('on' + listCreateEvent[i].toString());
                                                    }
                                                    catch (err) { }
                                                }
                                            }
                                        })();";
            return specialEvent;
        }

        public bool SetValue(object HElem, string value, string Attribute="value")
        {
            try
            {
                IHTMLElement elem = null;
                if (HElem.GetType().ToString().Contains("mshtml"))
                {
                    elem=((IHTMLElement)HElem);
                }
                else
                {
                    IHTMLDOMNode2 dom = HElem as IHTMLDOMNode2;
                    elem=((IHTMLDocument2)(dom.ownerDocument)).activeElement;
                }
                elem.setAttribute(Attribute, value);
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return false;
            }
        }
        public bool SendKeys(object HElem, string value)
        {
            try
            {
                IHTMLElement elem = null;
                if (HElem.GetType().ToString().Contains("mshtml"))
                {
                    elem = ((IHTMLElement)HElem);
                    ((HTMLDocument)browserObject.Document).focus();
                }
                else
                {
                    IHTMLDOMNode2 dom = HElem as IHTMLDOMNode2;
                    elem = ((IHTMLDocument2)(dom.ownerDocument)).activeElement;
                    ((HTMLDocument)dom.ownerDocument).focus();


                }

                ((IHTMLElement2)elem).focus();
                //elem.getAttribute()                
                System.Windows.Forms.SendKeys.SendWait(value + "{TAB}");
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return false;
            }
        }

        public void SwitchToDefaultFrame()
        {
            currentFrame = null;
            currentFrameDocument = null;
        }

        public string SwitchFrame(eLocateBy typ,string valueToFind)
        {
            try
            {                
                IHTMLElement frame = FindElementByLocator(typ, valueToFind);
                if (frame == null)
                    return "false";

                if (InitFrame(frame) == "false")
                    return "false";
                return "true";
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return "false";
            }
        }

        public string InitFrame(IHTMLElement node) 
        {
            try
            {                
                HTMLFrameElement frmElement = (HTMLFrameElement)node;
                DispHTMLDocument dispHtmlDoc = (DispHTMLDocument)((SHDocVw.IWebBrowser2)frmElement).Document;

                currentFrameDocument = (HTMLDocument)dispHtmlDoc;
                currentFrame = node;                

                return "true";
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.INFO, "Exception in init::" + ex.Message);
                return "false";
            }
        }
        public string getElementId(IHTMLElement h1)
        {
            string id = h1.id;
            if (!String.IsNullOrEmpty(id))
            {
                return id;
            }
            else
            {
                return "";
            }
        }

        public ObservableList<ControlProperty> GetHTMLElementProperties(ElementInfo EI)
        {
            ObservableList<ControlProperty> list = new ObservableList<ControlProperty>();
            IHTMLDOMNode d1;
            if (EI.ElementObject.GetType().ToString().Contains("mshtml"))
            {
                IHTMLElement h1 = (IHTMLElement)EI.ElementObject;
                d1 = h1 as IHTMLDOMNode;
            }
            else
                d1 =(IHTMLDOMNode) EI.ElementObject;
            string val = "";
            IHTMLAttributeCollection HAttributes = d1.attributes;
            if (HAttributes != null)
            {
                foreach (IHTMLDOMAttribute d in HAttributes)
                {
                    val = "";
                    ControlProperty CP = new ControlProperty();
                    CP.Name = d.nodeName;

                    val = Convert.ToString(d.nodeValue);
                    CP.Value = object.ReferenceEquals(val, null) ? string.Empty : val;
                    if(CP.Value!="")
                    {
                        list.Add(CP);
                    }
                }
            }
            try
            {
                if(((IHTMLElement)EI.ElementObject).style.cssText != null )
                {
                    ControlProperty CP = new ControlProperty();
                    CP.Name = "Style";
                    CP.Value = ((IHTMLElement)EI.ElementObject).style.cssText;
                    if (CP.Value != "")
                    {
                        list.Add(CP);
                    }
                }
            }
            catch(Exception e)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Exception while getting csstext in GetHTMLElementProperties::" + e.Message,e);
            }
            return list;
        }

        #region HTML Element Information Section

        public IHTMLElement GetActElement(Actions.Act act)
        {
            IHTMLElement CurAE = null;
            try
            {
                string LocValueCalculated = act.LocateValueCalculated;
                CurAE = FindElementByLocator(act.LocateBy, LocValueCalculated);
            }
            catch (ContextMarshalException e)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
                act.ExInfo += "Invalid Current Window. Please do switch do the correct window using Switch window before running the action";
            }

            return CurAE;
        }

        private IHTMLElement FindElementByLocator(eLocateBy locateBy,string LocValueCalculated)
        {
            IHTMLElement HEle = null;
            
            switch (locateBy)
            {
                case eLocateBy.ByID:
                    if (currentFrame == null)
                        HEle = mHtmlDocument.getElementById(LocValueCalculated);
                    else
                        HEle= currentFrameDocument.getElementById(LocValueCalculated);

                    break;
                case eLocateBy.ByName:
                    if (currentFrame == null)
                        HEle = (mshtml.IHTMLElement)mHtmlDocument.getElementsByName(LocValueCalculated).item(0);
                    else
                        HEle = (mshtml.IHTMLElement)currentFrameDocument.getElementsByName(LocValueCalculated).item(0);
                    
                    break;
                case eLocateBy.ByXPath:
                case eLocateBy.ByRelXPath:
                    HEle = GetElementByXPath(LocValueCalculated);
                    break;
                default:
                    throw new Exception("Locator not implement - " + locateBy.ToString());
            }

            return HEle;
        }

        public ObservableList<ElementInfo> GetElements(ElementLocator EL)
        {
            ObservableList<ElementInfo> list = new ObservableList<ElementInfo>();
            //temp for test
            List<IHTMLElement> AEList = FindElementsByLocator(EL);
            if (AEList != null)
            {
                foreach (IHTMLElement AE in AEList)
                {
                    HTMLElementInfo a = GetHtmlElementInfo(AE);
                    list.Add(a);
                }
            }

            return list;
        }

        private List<IHTMLElement> FindElementsByLocator(ElementLocator EL)
        {
            eLocateBy eLocatorType = EL.LocateBy;
            string LocValueCalculated = EL.LocateValue;
            
            switch (eLocatorType)
            {
                case eLocateBy.ByXPath:
                    return GetElementsByXpath(LocValueCalculated);
                default:
                    throw new Exception("Locator not implement - " + eLocatorType.ToString());
            }
        }

        public string SelectFromDropDown(IHTMLElement element, string value, string attribute = "value")
        {
            var currentAttribute = "";
            string temp;
            try
            {
                element.setAttribute(attribute, value);
                try
                {
                    currentAttribute = element.getAttribute(attribute).ToString();
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                }
                if (!currentAttribute.Equals(value))
                {
                    currentAttribute = SelectFromDropDownByChild(element, value, attribute);
                    if(currentAttribute.Equals("NoSelection"))
                    {
                        int idex = -1, j = -1;
                        IEnumerator enm = ((HTMLSelectElement)element).GetEnumerator();
                        while (enm.MoveNext())
                        {
                            j++;
                            if (((IHTMLElement)(enm.Current)).innerText.Equals(value))
                            {
                                idex = j;
                                temp = value;
                                break;
                            }
                        }

                    ((HTMLSelectElement)element).selectedIndex = idex;
                        if (idex == -1)
                        {
                            temp = (dynamic)element.getAttribute("innerText");
                            List<string> lst = temp.Split(' ').ToList<string>();
                            int idx = lst.IndexOf(value);
                            if (idx < 10)
                                element.setAttribute(attribute, "0" + idx.ToString());
                            else
                                element.setAttribute(attribute, idx.ToString());
                            temp = element.getAttribute(attribute);
                            currentAttribute = temp;
                        }
                    }
                }
                return currentAttribute;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return currentAttribute;
            }
        }

        public string SelectFromDropDownByChild(IHTMLElement element,string value,string attribute)
        {
            string currentAttribute = "NoSelection";
            IHTMLElementCollection coll = element.all;
            string name = "";
            foreach (IHTMLElement item in coll)
            {
                name = object.ReferenceEquals(item.innerHTML, null) ? "" : item.innerHTML;
                if(name.Contains("\n")||name.StartsWith(" ")||name.EndsWith(" "))
                {
                    name = name.Trim();
                }
                if (value.Equals(name))
                {
                    item.setAttribute("selected", "selected");
                    currentAttribute = element.getAttribute(attribute).ToString();
                }
            }
            return currentAttribute;
        }

        public bool Click(IHTMLElement element)
        {
            try
            {
                element.click();
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return false;
            }
        }

        public bool ClickAt(IHTMLElement element,string val="")
        {            
            int x = 0;
            int y = 0;
            if(!String.IsNullOrEmpty(val) && val.IndexOf(",")>=0)
            {
                int res;
                if (int.TryParse(val.Split(',')[0], out res) == true)
                    x = int.Parse(val.Split(',')[0]);
                if (int.TryParse(val.Split(',')[1], out res) == true)
                    y = int.Parse(val.Split(',')[1]);
            }
            WinAPIAutomation winAPI = new WinAPIAutomation();
            try
            {
                element.scrollIntoView();
                int elemX = getelementXCordinate(element);
                int elemY = getelementYCordinate(element);                
                winAPI.SendClickOnWinXYPoint(AEBrowser, elemX + x, elemY + y);
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return false;
            }
        }
        public bool MouseHover(IHTMLElement element, string val = "")
        {            
            int x = 0;
            int y = 0;
            if (!String.IsNullOrEmpty(val) && val.IndexOf(",") >= 0)
            {
                int res;
                if (int.TryParse(val.Split(',')[0], out res) == true)
                    x = int.Parse(val.Split(',')[0]);
                if (int.TryParse(val.Split(',')[1], out res) == true)
                    y = int.Parse(val.Split(',')[1]);
            }
            WinAPIAutomation winAPI = new WinAPIAutomation();
            try
            {
                element.scrollIntoView();
                winAPI.MoveMousetoXYPoint(AEBrowser, getelementXCordinate(element) + x, getelementYCordinate(element) + y);
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return false;
            }
        }
        public bool scrolltoElement(IHTMLElement element)
        {           
            try
            {
                element.scrollIntoView();               
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return false;
            }
        }
        public bool RightClick(IHTMLElement element, string val = "")
        {            
            int x = 10;
            int y = 10;
            if (!String.IsNullOrEmpty(val) && val.IndexOf(",") >= 0)
            {
                int res;
                if (int.TryParse(val.Split(',')[0], out res) == true)
                    x = int.Parse(val.Split(',')[0]);
                if (int.TryParse(val.Split(',')[1], out res) == true)
                    y = int.Parse(val.Split(',')[1]);
            }
            WinAPIAutomation winAPI = new WinAPIAutomation();
            try
            {
                element.scrollIntoView();
                x = getelementXCordinate(element) + x;
                Reporter.ToLogAndConsole(eAppReporterLogLevel.INFO, "elementX::" + x);
                Reporter.ToConsole("elementX::" + x);
                y = getelementYCordinate(element) + y;
                Reporter.ToLogAndConsole(eAppReporterLogLevel.INFO, "elementy::" + y);
                Reporter.ToConsole("elementY::" + y);
                winAPI.SendRightClick(AEBrowser, x +"," + y );                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return false;
            }
        }

        public ObservableList<ElementLocator> GetHTMLElementLocators(HTMLElementInfo EI)
        {
            ObservableList<ElementLocator> list = new ObservableList<ElementLocator>();

            if (!string.IsNullOrEmpty(EI.ID))
            {
                list.Add(new ElementLocator() { LocateBy = eLocateBy.ByID, LocateValue = EI.ID, Help = "Very Recommended - ID is Very good locater and probably unique" });
            }
            if (!string.IsNullOrEmpty(EI.Name))
            {
                list.Add(new ElementLocator() { LocateBy = eLocateBy.ByName, LocateValue = EI.Name, Help = "Very Recommended - Name is Very good locater and probably unique" });
            }
            if (string.IsNullOrEmpty(EI.XPath))
            {
                EI.XPath = GetElementAbsoluteXPath((IHTMLElement)EI.ElementObject);
            }
            if (EI.XPath != "")
                list.Add(new ElementLocator() { LocateBy = eLocateBy.ByXPath, LocateValue = EI.XPath, Help = "Very Recommended - Xpath is Very good locater and probably unique" });

            if (string.IsNullOrEmpty(EI.RelXpath))
            {
                if (EI.XPath != "")
                    EI.RelXpath = GetElementRelXPath(EI);
            }
            if (EI.RelXpath != "")
                list.Add(new ElementLocator() { LocateBy = eLocateBy.ByRelXPath, LocateValue = EI.RelXpath, Help = "Very Recommended - Xpath is Very good locater and probably unique" });
            
            return list;
        }

        public string GetElementRelXPath(HTMLElementInfo elemInfo)
        {
            var relxpath = "";
            string xpath = elemInfo.XPath;
            IHTMLElement elem = (IHTMLElement)elemInfo.ElementObject;
            try
            {
                while (relxpath.IndexOf("//") == -1 && elem != null)
                {
                    string id = elem.id;
                    if (!string.IsNullOrEmpty(id))
                    {
                        relxpath = xpath.Replace(getXPath(elem), "//" + elem.tagName.ToLower() + "[@id='" + id + "']");
                        continue;
                    }
                    string name = Convert.ToString(elem.getAttribute("name"));
                    if (!string.IsNullOrEmpty(name))
                    {
                        relxpath = xpath.Replace(getXPath(elem), "//" + elem.tagName.ToLower() + "[@name='" + name + "']");
                        continue;
                    }
                    elem = elem.parentElement;
                }
            }
            catch (Exception e)
            {
                relxpath = xpath;
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
            }
            if (relxpath == "")
                relxpath = xpath;
            return relxpath;
        }

        public string HighLightElement(object elem)
        {
            //TODO:Handle mshtml-element & HtmlAgilityPack-node generically
            {
                mshtml.IHTMLElement htmlEle = (mshtml.IHTMLElement)elem;

                UnLightCurrentHighlightedElement(htmlEle, "mshtml");
                CurrentHighlightedElement = htmlEle;
                if (elem != null)
                {
                    htmlEle.style.setAttribute("border", "solid 1px #ff0000");
                    return "true";
                }
            }
            return "false";
        }

        private void UnLightCurrentHighlightedElement(object olem, string objType)
        {
            {
                if (CurrentHighlightedElement != null)
                {
                    if (!String.IsNullOrEmpty(currHighlightedAttr))
                    {
                        CurrentHighlightedElement.style.setAttribute("border", currHighlightedAttr);
                    }
                    else
                    {
                        CurrentHighlightedElement.style.setAttribute("border", "1px solid DarkGray");
                    }
                }
                else
                {
                    currHighlightedAttr = (string)((IHTMLElement)olem).style.getAttribute("border");
                }
            }
        }

        public IHTMLElement GetHTMLElementFromPoint(int x, int y)
        {
            Reporter.ToLog(eAppReporterLogLevel.INFO, "GetHTMLElementFromPoint::" + x + "::" + y);
            Reporter.ToConsole("GetHTMLElementFromPoint::" + x + "::" + y);
            IHTMLElement Elem = mHtmlDocument.elementFromPoint(x, y);
            if (Elem.tagName.ToLower() == "iframe")
            {
                InitFrame(Elem);
                if (currentFrameDocument != null)
                {                    
                    x = x - getelementXCordinate(Elem);
                    y = y - getelementYCordinate(Elem);

                    Elem = currentFrameDocument.elementFromPoint(x, y);
                }                    
            }
            Reporter.ToConsole("GetHTMLElementFromPoint::" + Elem.className);
            Reporter.ToLog(eAppReporterLogLevel.INFO, "GetHTMLElementFromPoint::" + Elem.className);
            return Elem;
        }

        public int getelementXCordinate(IHTMLElement h1,bool frame =false)
        {
            if (object.ReferenceEquals(h1.offsetParent, null)) return 0;
            Reporter.ToConsole("getelementXCordinate-Parent is not null");
            if (h1.offsetLeft >= 0 && h1.offsetParent.offsetLeft >= 0)
            {
                Reporter.ToConsole("getelementXCordinate-"+ h1.offsetLeft);
                IHTMLElement h1Par = h1.offsetParent;
                int xPos = h1.offsetLeft;
                Reporter.ToConsole("getelementXCordinate-parLeft" + xPos);
                while (h1Par != null)
                {
                    xPos += h1Par.offsetLeft;
                    Reporter.ToConsole("getelementXCordinate-parLeft" + xPos);
                    h1Par = h1Par.offsetParent;
                }
                Reporter.ToConsole("getelementXCordinate-parLeft out " + xPos);
                int scrollLeft;
                if (currentFrameDocument != null && frame == false)
                {
                    xPos += getelementXCordinate(currentFrame, true);                   
                }  
                scrollLeft = getscrollLeft(h1);
                Reporter.ToConsole("getelementXCordinate-scrollLeft out2 " + scrollLeft);
                return xPos- scrollLeft;
            }
            return -1;
        }
        public int getscrollLeft(IHTMLElement h1)
        {
            int scrollLeft=0;
            IHTMLElement h1Par = h1.parentElement;
            while (h1Par != null)
            {
                scrollLeft += ((IHTMLElement2)h1Par).scrollLeft;
                Reporter.ToConsole("getelementXCordinate-scrollLeft" + scrollLeft);
                h1Par = h1Par.parentElement;
            }
            return scrollLeft;
        }

        public int getscrollTop(IHTMLElement h1)
        {
            int scrollTop = 0;
            IHTMLElement h1Par = h1.parentElement;
            while (h1Par != null)
            {
                scrollTop += ((IHTMLElement2)h1Par).scrollTop;
                Reporter.ToConsole("getelementYCordinate-getscrollTop" + scrollTop);
                h1Par = h1Par.parentElement;
            }
            return scrollTop;
        }

        public int getelementYCordinate(IHTMLElement h1,bool frame=false)
        {
            if (object.ReferenceEquals(h1.offsetParent, null)) return 0;
            Reporter.ToConsole("getelementYCordinate-Parent is not null");
            if (h1.offsetTop >= 0 && h1.offsetParent.offsetTop >= 0)
            {
                Reporter.ToConsole("getelementYCordinate-" + h1.offsetTop);
                IHTMLElement h1Par = h1.offsetParent;
                int yPos = h1.offsetTop;
                Reporter.ToConsole("getelementYCordinate-parTop" + yPos);
                while (h1Par != null)
                {
                    yPos += h1Par.offsetTop;
                    Reporter.ToConsole("getelementYCordinate-parTop" + yPos);
                    h1Par = h1Par.offsetParent;                    
                }
                Reporter.ToConsole("getelementYCordinate-parTop out" + yPos);
                int scrollTop;
                if (currentFrameDocument != null && frame == false)
                {
                    yPos += getelementYCordinate(currentFrame, true);                   
                }               
                scrollTop = getscrollTop(h1);
                Reporter.ToConsole("getelementYCordinate-scrollTop out2 " + scrollTop);
                return yPos - scrollTop;
            }
            return -1;

        }
        #endregion

        #region XPath
        public List<IHTMLElement> GetElementsByXpath(string XPath)
        {
            List<ElementInfo> elems = mXPathHelper.GetElementsByXpath(XPath);
            List<IHTMLElement> list = new List<IHTMLElement>();
            foreach (ElementInfo EI in elems)
            {
                list.Add((IHTMLElement)(((HTMLElementInfo)EI).ElementObject));
            }
            return list;
        }

        private void InitXpathHelper()
        {
            mXPathHelper = new XPathHelper(this, ImportantProperties);
            ImportantProperties.Add("Name");
            ImportantProperties.Add("Id");
            mXPathHelper = new XPathHelper(this, ImportantProperties);
        }

        XPathHelper IXPath.GetXPathHelper(ElementInfo info)
        {
            return mXPathHelper;
        }

        ElementInfo IXPath.GetRootElement()
        {
            return CurrentWindowRootElement;
        }

        ElementInfo IXPath.UseRootElement()
        {
            HTMLElementInfo root = new HTMLElementInfo();
            root.ElementObject = CurrentWindowRootElement.ElementObject;
            return root;
        }

        ElementInfo IXPath.GetElementParent(ElementInfo ElementInfo)
        {
            HTMLElementInfo elem = (HTMLElementInfo)ElementInfo;
            if (object.ReferenceEquals(((IHTMLElement)elem.ElementObject).parentElement, null)) return null;
            IHTMLElement parentElem = ((IHTMLElement)elem.ElementObject).parentElement;
            return GetHtmlElementInfo(parentElem);
        }

        string IXPath.GetElementProperty(ElementInfo ElementInfo, string PropertyName)
        {
            HTMLElementInfo EI = (HTMLElementInfo)ElementInfo;
            if (EI.ElementObject == null)
            {
                throw new Exception("Error: GetElementProperty received ElementInfo with HTMLElement = null");
            }
            if (EI.ElementObject.GetType().ToString().Contains("mshtml"))
                return GetValue((IHTMLElement)EI.ElementObject, PropertyName);
            else
                return GetValue((IHTMLDOMNode)EI.ElementObject, PropertyName);
        }

        ElementInfo IXPath.FindFirst(ElementInfo ElementInfo, List<XpathPropertyCondition> conditions)
        {
            HTMLElementInfo EI = (HTMLElementInfo)ElementInfo;
            if (EI.ElementObject == null)
            {
                throw new Exception("Error: GetElementProperty received ElementInfo with HTMLElement = null");
            }
            DispHTMLDocument dispHtmlDoc = null;
            IHTMLDOMNode child = null;
            if (EI.ElementObject == null)
            {
                return new ElementInfo();
            }
            else
            {
                IHTMLElement obj = (IHTMLElement)EI.ElementObject;
                dispHtmlDoc = (DispHTMLDocument)obj.document;
                child = dispHtmlDoc.firstChild;
                if (ReferenceEquals(child, null))
                {
                    IHTMLDOMNode domNode = obj as IHTMLDOMNode;
                    IHTMLFrameBase2 iframeBase = domNode as IHTMLFrameBase2;
                    IHTMLWindow2 domNode2 = iframeBase.contentWindow;
                    IHTMLDocument2 frameContent = (IHTMLDocument2)domNode2.document;
                    
                    IHTMLElementCollection elmPrevSibling = frameContent.all;
                    foreach (IHTMLElement f12 in elmPrevSibling)
                    {
                        if (f12.Equals(obj))
                        {
                            DispHTMLDocument doc = (DispHTMLDocument)f12.document;
                            child = doc.firstChild;
                        }
                    }
                }
            }
            if (child.GetType().ToString().Contains("mshtml"))
                return GetHtmlElementInfo((IHTMLElement)child);
            else
                return GetHtmlElementInfo((IHTMLDOMNode)child);
        }

        List<ElementInfo> IXPath.FindAll(ElementInfo ElementInfo, List<XpathPropertyCondition> conditions)
        {
            List<ElementInfo> listEI = new List<ElementInfo>();
            HTMLElementInfo EI = (HTMLElementInfo)ElementInfo;
            if (EI.ElementObject == null)
            {
                throw new Exception("Error: GetElementProperty received ElementInfo with HTMLElement = null");
            }
            HTMLElementInfo hInfo;
            IHTMLElement HtmlEle = (IHTMLElement)EI.ElementObject;
            DispHTMLDocument doc = (DispHTMLDocument)HtmlEle.document;
            IHTMLElementCollection child = doc.all;
            foreach (IHTMLElement item in child)
            {
                hInfo = GetHtmlElementInfo(item);
                listEI.Add(hInfo);
            }
            return listEI;
        }

        ElementInfo IXPath.GetPreviousSibling(ElementInfo ElementInfo)
        {
            HTMLElementInfo EI = (HTMLElementInfo)ElementInfo;
            if (EI.ElementObject == null)
            {
                throw new Exception("Error: GetElementProperty received ElementInfo with HTMLElement = null");
            }
            IHTMLDOMNode childNode = null;
            IHTMLElement obj;
            if (EI.ElementObject == null)
                return new ElementInfo();
            else
            {
                if (EI.ElementObject.GetType().ToString().Contains("mshtml"))
                {
                    obj = (IHTMLElement)EI.ElementObject;
                    childNode = (IHTMLDOMNode)obj;
                    if (object.ReferenceEquals(childNode.previousSibling, null)) return null;
                    childNode = childNode.previousSibling;
                    #region Refering to IFrame Elements
                    if (ReferenceEquals(childNode, null))
                        return null;
                    #endregion
                    if (childNode.GetType().ToString().Contains("mshtml"))
                        return GetHtmlElementInfo((IHTMLElement)childNode);
                    else
                    {
                        return GetHtmlElementInfo((IHTMLDOMNode)childNode);
                    }
                }
                else
                {
                    childNode = ((IHTMLDOMNode)((dynamic)(EI.ElementObject))).previousSibling;
                    #region Refering to IFrame Elements
                    if (ReferenceEquals(childNode, null))
                        return null;
                    #endregion
                    if (childNode.GetType().ToString().Contains("mshtml"))
                        return GetHtmlElementInfo((IHTMLElement)childNode);
                    else
                    {
                        return GetHtmlElementInfo((IHTMLDOMNode)childNode);
                    }
                }
            }
        }
        
        ElementInfo IXPath.GetNextSibling(ElementInfo ElementInfo)
        {
            HTMLElementInfo EI = (HTMLElementInfo)ElementInfo;
            if (EI.ElementObject == null)
            {
                throw new Exception("Error: GetElementProperty received ElementInfo with HTMLElement = null");
            }
            if (EI.ElementObject == null)
                return new ElementInfo();
            else
            {
                IHTMLElement obj;
                IHTMLDOMNode elem23;
                if (EI.ElementObject.GetType().ToString().Contains("mshtml"))
                {
                    obj = (IHTMLElement)EI.ElementObject;
                    elem23 = (IHTMLDOMNode)obj;
                    if (object.ReferenceEquals(elem23.nextSibling, null)) return null;
                    elem23 = elem23.nextSibling;
                    #region Refering to IFrame Elements
                    if (object.ReferenceEquals(elem23, null))
                        return null;
                    #endregion
                    if (elem23.GetType().ToString().Contains("mshtml"))
                        return GetHtmlElementInfo((IHTMLElement)elem23);
                    else
                        return GetHtmlElementInfo((IHTMLDOMNode)elem23);
                }
                else
                {
                    Type objType = EI.ElementObject.GetType();
                    elem23 = ((IHTMLDOMNode)((dynamic)(EI.ElementObject))).nextSibling;
                    # region Refering to IFrame Elements
                    if (object.ReferenceEquals(elem23, null))
                        return null; 
                    #endregion
                    if (elem23.GetType().ToString().Contains("mshtml"))
                        return GetHtmlElementInfo((IHTMLElement)elem23);
                    else
                        return GetHtmlElementInfo((IHTMLDOMNode)elem23);
                }
            }
        }
        
        public string GetElementAbsoluteXPath(IHTMLElement HAE)
        {
            HTMLElementInfo EI = new HTMLElementInfo(); //Create small simple EI
            EI = GetHtmlElementInfo(HAE);
            EI.ElementObject = HAE;
            string XPath = getXPath(HAE);
            return XPath;
        }

        public string GetElementXPath(HTMLElementInfo HTMLEI)
        {
            mHtmlDocument = browserObject.Document;
            HtmlNode HNode;
            string xpath = "";
            string id = HTMLEI.ID;
            id = object.ReferenceEquals(id, "") ? "NotAvailable" : id;
            string name = HTMLEI.Name;
            name = object.ReferenceEquals(name, "") ? "NotAvailable" : name;
            string title = HTMLEI.ElementTitle;
            title = object.ReferenceEquals(title, "") ? "NotAvailable" : title;
            string value = HTMLEI.Value;
            value = object.ReferenceEquals(value, "") ? "NotAvailable" : value;
            string type = HTMLEI.ElementType;
            type = object.ReferenceEquals(type, "") ? "NotAvailable" : type;

            IHTMLElement h1 = (IHTMLElement)HTMLEI.ElementObject;
            int x = -1;

            if (!object.ReferenceEquals(h1.offsetParent, null))
            {
                if (h1.offsetLeft >= 0 && h1.offsetParent.offsetLeft >= 0)
                {
                    x = h1.offsetLeft + h1.offsetParent.offsetLeft;
                }
            }
            int y = -1;
            if (!object.ReferenceEquals(h1.offsetParent, null))
            {
                if (h1.offsetTop >= 0 && h1.offsetParent.offsetTop >= 0)
                {
                    y = h1.offsetTop + h1.offsetParent.offsetTop;
                }
            }

            string outHtml = h1.outerHTML;
            outHtml = object.ReferenceEquals(outHtml, "") ? "NotAvailable" : outHtml;

            string inHtml = h1.innerHTML;

            inHtml = object.ReferenceEquals(inHtml, "") ? "NotAvailable" : inHtml;

            name = getElementTitle(h1);

            if (id != "NotAvailable")
            {
                HNode = HAPDocument.GetElementbyId(id);
                xpath = "";
                if (HNode != null)
                {
                    xpath = HNode.XPath;
                }
                return xpath;
            }
            else
            {
                sourceDoc = (mshtml.IHTMLDocument3)browserObject.Document;
                documentContents = sourceDoc.documentElement.outerHTML;
                HAPDocument = new HtmlAgilityPack.HtmlDocument();
                HAPDocument.LoadHtml(documentContents);
                IEnumerable<HtmlNode> tocChildren = HAPDocument.DocumentNode.Descendants();
                foreach (HtmlNode node in tocChildren)
                {

                    if (outHtml != "NotAvailable" && inHtml != "NotAvailable")
                    {
                        if (node.OuterHtml.Equals(outHtml) && node.InnerHtml.Equals(inHtml))
                        {
                            xpath = node.XPath;
                            return xpath;
                        }
                    }
                    if (inHtml != "NotAvailable")
                    {
                        if (node.InnerHtml.Equals(inHtml))
                        {
                            xpath = node.XPath;
                            return xpath;
                        }
                    }

                    if (outHtml != "NotAvailable")
                    {
                        if (node.OuterHtml.Equals(outHtml))
                        {
                            xpath = node.XPath;
                            return xpath;
                        }
                    }
                }
            }
            return xpath;
        }

        public IHTMLElement GetElementByXPath(string xpath)
        {
            if(currentFrameDocument != null)
                sourceDoc = (mshtml.IHTMLDocument3)currentFrameDocument;
            else            
                sourceDoc = (mshtml.IHTMLDocument3)browserObject.Document;
            documentContents = sourceDoc.documentElement.outerHTML;
           // Reporter.ToLog(eLogLevel.INFO, "documentContents::" + documentContents);
            HAPDocument = new HtmlAgilityPack.HtmlDocument();
            HAPDocument.LoadHtml(documentContents);
            mHtmlDocument = browserObject.Document;
            IHTMLElement h1 = null;
            HtmlNode node = null;
            try
            {
                node = HAPDocument.DocumentNode.SelectSingleNode(xpath);
            }
            catch
            {
            }
            if (node != null)
            {
                Reporter.ToLog(eAppReporterLogLevel.INFO, "nodenotnull::" + node.XPath);
                if (currentFrame != null)
                    h1 = GetHTMLElementFromXPath(node.XPath, currentFrameDocument);
                else
                    h1 = GetHTMLElementFromXPath(node.XPath, mHtmlDocument);
                if (h1 != null)
                    return h1;
            }            
            Reporter.ToLog(eAppReporterLogLevel.INFO, "xpath::" + xpath);

            if (currentFrame != null)                           
                h1 = GetHTMLElementFromXPath(xpath, currentFrameDocument);                                         
            else
                h1 = GetHTMLElementFromXPath(xpath, mHtmlDocument);
            if (h1 != null)
                return h1;

            if (node == null)
                return null;

            IHTMLElement elem = GetHTMLElementfromNode(node);

            return elem;
        }

        public IHTMLElement GetHTMLElementfromNode(HtmlNode node)
        {

            IHTMLElementCollection elemColl;
            if (currentFrame != null)
                elemColl = currentFrameDocument.getElementsByTagName(node.OriginalName);
            else
                elemColl = mHtmlDocument.getElementsByTagName(node.OriginalName);


            IHTMLElement htmlElem = null;
            foreach (IHTMLElement ele in elemColl)
            {
                if (checkNodeandHTml(node, ele) == true)
                {
                    htmlElem = ele;
                    IHTMLElement parEle = ele.parentElement;
                    HtmlNode parNode = node.ParentNode;
                    bool match = true;
                    int iCount = 0;
                    while (parNode != null && parEle != null && match == true && iCount < 6)
                    {
                        if (checkNodeandHTml(parNode, parEle) == true)
                        {
                            parNode = parNode.ParentNode;
                            parEle = parEle.parentElement;
                        }
                        else
                        {
                            match = false;
                        }
                        iCount++;
                    }
                    if (match == true)
                    {
                        return ele;
                    }
                }
            }
            if (htmlElem != null)
                return htmlElem;
            return null;
        }

        public bool checkNodeandHTml(HtmlNode node, IHTMLElement ele)
        {
            if (node.Id == ele.id)
                return true;

            string inHtml = node.InnerHtml.Replace("\"", string.Empty);

            string outHtml = node.OuterHtml.Replace("\"", string.Empty);
            string innerTextNode = node.InnerText.Replace("\"", string.Empty);
            if (!String.IsNullOrEmpty(ele.innerText))
            {
                string elemInnerText = ele.innerText.Replace("\"", string.Empty);
                if (elemInnerText.Equals(innerTextNode))
                {
                    return true;
                }
            }

            string elemInnerHtml = string.Empty;
            string elemOuterHtml = string.Empty;
            if (!String.IsNullOrEmpty(inHtml) && !String.IsNullOrEmpty(outHtml) &&
                !String.IsNullOrEmpty(ele.innerHTML) && !String.IsNullOrEmpty(ele.outerHTML))
            {
                elemInnerHtml = ele.innerHTML.Replace("\"", string.Empty);
                elemOuterHtml = ele.outerHTML.Replace("\"", string.Empty);
                if (elemInnerHtml.Replace("\"", string.Empty).Equals(inHtml) &&
                    elemOuterHtml.Replace("\"", string.Empty).Equals(outHtml))
                {
                    return true;
                }
            }

            if (!String.IsNullOrEmpty(inHtml) && !String.IsNullOrEmpty(ele.innerHTML))
            {
                elemInnerHtml = ele.innerHTML.Replace("\"", string.Empty);
                if (elemInnerHtml.Equals(inHtml))
                {
                    return true;
                }
            }

            if (!String.IsNullOrEmpty(outHtml) && !String.IsNullOrEmpty(ele.outerHTML))
            {
                elemOuterHtml = ele.outerHTML.Replace("\"", string.Empty);
                if (elemOuterHtml.Equals(outHtml))
                {
                    return true;
                }
            }
            return false;
        }

        public struct DocNode
        {
            public string Name;
            public string Pos;
        }

        public IHTMLElement GetHTMLElementFromXPath(string xpath, DispHTMLDocument doc)
        {
            try
            {
                var pattern = @"/(.*?)\[(.*?)\]"; // like div[1]
                                                  // Parse the XPath to extract the nodes on the path
                var matches = Regex.Matches(xpath, pattern);
                List<DocNode> PathToNode = new List<DocNode>();
                foreach (Match m in matches) // Make a path of nodes
                {
                    DocNode n = new DocNode();
                    n.Name = n.Name = m.Groups[1].Value;
                    int pos = 0;
                    if(int.TryParse(m.Groups[2].Value,out pos))
                        n.Pos = (pos - 1).ToString();
                    else
                        n.Pos = m.Groups[2].Value;
                    PathToNode.Add(n); // add the node to path 
                }

                IHTMLElement elem = null; //Traverse to the element using the path
                if (PathToNode.Count > 0)
                {
                    //begin from the body
                    foreach (DocNode n in PathToNode)
                    {
                        if (elem == null && n.Name.StartsWith("/"))
                            elem = doc.documentElement;
                        if (elem == null)
                            elem = doc.documentElement;
                        else
                            //Find the corresponding child by its name and position
                            elem = GetChild(elem, n);
                        if (elem == null)
                            return null;
                    }
                }
                return elem;
            }
            catch(Exception e)
            {
                Reporter.ToLog(eAppReporterLogLevel.INFO, "exception in GetHTMLElementFromXPath::" + e.Message);
                return null;
            }
        }

        public IHTMLElement GetChild(IHTMLElement el, DocNode node)
        {
            // Find corresponding child of the elemnt 
            // based on the name and position of the node
            int childPos = 0;
            int pos=0;
            var elChilds= el.children;
            if (node.Name.StartsWith(".."))
            {
                el = el.parentElement;
                node.Name = node.Name.Substring(2);
            }
            if (node.Name.StartsWith("/"))
            {
                elChilds = el.all;
                node.Name = node.Name.Substring(1);
            }
           
            foreach (IHTMLElement child in elChilds)
            {
                if (child.tagName.Equals(node.Name, StringComparison.OrdinalIgnoreCase) || node.Name == "*")
                {
                    if(int.TryParse(node.Pos,out pos))
                    {
                        if (childPos == pos)
                        {
                            return child;
                        }
                        childPos++;
                    }
                    else if(node.Pos.Split('=').Length>1)
                    {
                        string propName = node.Pos.Split('=')[0];
                        if(propName == "@class")
                            propName= "@className";
                        string propVal = node.Pos.Split('=')[1].Replace("'","").Replace("\"","");                            
                        if (propName.StartsWith("@") && Convert.ToString(child.getAttribute(propName.Substring(1))) == propVal)
                            return child;
                        else if (propName.StartsWith("text()") && child.innerText == propVal)
                            return child;                            
                    }
                    else if(node.Pos.StartsWith("contains") && node.Pos.Split(',').Length >1)
                    {
                        var propPattern = @"contains\((.*?),(.*?)\)";
                        var propmatches = Regex.Matches(node.Pos, propPattern);
                        if(propmatches.Count >0)
                        {
                            string propName = propmatches[0].Groups[1].Value;
                            if (propName == "@class")
                                propName = "@className";
                            string propVal = propmatches[0].Groups[2].Value.Replace("'", "").Replace("\"", "");
                            if ((propName == "text()" || propName == ".") && child.innerText.Contains(propVal))
                                return child;
                            else if(propName.StartsWith("@") && Convert.ToString(child.getAttribute(propName.Substring(1))).Contains(propVal) )
                                return child;
                        }
                    }
                }
            }
            IHTMLDOMNode domNode = null;
            if (el.tagName.ToLower().Equals("iframe"))
            {
                domNode = el as IHTMLDOMNode;
                IHTMLElement elemFst = null;
                IHTMLFrameBase2 iframeBase = domNode as IHTMLFrameBase2;
                IHTMLWindow2 domNode2 = iframeBase.contentWindow;
                frameContent = (IHTMLDocument2)domNode2.document;
                frameDocument = (DispHTMLDocument)frameContent;
                if (frameDocument.firstChild != null) 
                {
                    domNode = frameDocument.firstChild;
                    if (domNode.nodeName.Equals("HTML") || domNode.nodeName.Equals("Html") || domNode.nodeName.Equals("html"))
                    {
                        domNode = domNode.firstChild;
                    }
                    elemFst = domNode as IHTMLElement;
                    while (domNode.nextSibling != null)
                    {
                        domNode = domNode.nextSibling;
                        if (!domNode.nodeName.Equals("#text"))
                        {
                            elemFst = domNode as IHTMLElement;
                            if (elemFst.tagName.Equals(node.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                if (childPos == Convert.ToInt32(node.Pos))
                                {
                                    return elemFst;
                                }
                                childPos++;
                            }
                        }
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
