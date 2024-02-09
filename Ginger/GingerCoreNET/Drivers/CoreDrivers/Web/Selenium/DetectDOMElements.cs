using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Common;
using GingerCore.Drivers.Common;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using GingerCore.Drivers;
using System.Collections.ObjectModel;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Selenium
{
    public class DetectDOMElements
    {
        SeleniumDriver seleniumDriver;
        public DetectDOMElements(SeleniumDriver seleniumDriver) { 
            this.seleniumDriver = seleniumDriver;
        }
        public ObservableList<ElementInfo> FindAllElementsFromPOM(string path, PomSetting pomSetting, ObservableList<ElementInfo> foundElementsList = null, ObservableList<POMPageMetaData> PomMetaData = null)
        {
            return FindAllElementsFromPOM(path, pomSetting, seleniumDriver.mDriver,Guid.Empty,foundElementsList, PomMetaData);
        }
        
        private ObservableList<ElementInfo> FindAllElementsFromPOM(string path, PomSetting pomSetting, ISearchContext parentContext, Guid ParentGUID, ObservableList<ElementInfo> foundElementsList = null, ObservableList<POMPageMetaData> PomMetaData = null , bool isShadowRootDetected = false, string pageSource = null)
        {
            if (PomMetaData == null)
            {
                PomMetaData = new ObservableList<POMPageMetaData>();
            }
            if (foundElementsList == null)
            {
                foundElementsList = new ObservableList<ElementInfo>();

            }

            List<HtmlNode> formElementsList = new List<HtmlNode>();
            string documentContents = pageSource ?? seleniumDriver.mDriver.PageSource;
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(documentContents);
            IEnumerable<HtmlNode> htmlElements = htmlDoc.DocumentNode.Descendants().Where(x => !x.Name.StartsWith("#"));

            if (htmlElements.Any())
            {
                foreach (HtmlNode htmlElemNode in htmlElements)
                {
                    try
                    {
                        if (seleniumDriver.StopProcess)
                        {
                            return foundElementsList;
                        }
                        //The <noscript> tag defines an alternate content to be displayed to users that have disabled scripts in their browser or have a browser that doesn't support script.
                        //skip to learn to element which is inside noscript tag
                        if (htmlElemNode.Name.ToLower().Equals("noscript") || htmlElemNode.XPath.ToLower().Contains("/noscript"))
                        {
                            continue;
                        }
                        //get Element Type
                        Tuple<string, eElementType> elementTypeEnum = SeleniumDriver.GetElementTypeEnum(htmlNode: htmlElemNode);

                        // set the Flag in case you wish to learn the element or not
                        bool learnElement = true;

                        //filter element if needed, in case we need to learn only the MappedElements .i.e., LearnMappedElementsOnly is checked
                        if (pomSetting != null && pomSetting.filteredElementType != null)
                        {
                            //Case Learn Only Mapped Element : set learnElement to false in case element doesn't exist in the filteredElementType List AND element is not frame element
                            if (!pomSetting.filteredElementType.Contains(elementTypeEnum.Item2))
                            {
                                learnElement = false;
                            }
                        }

                        IWebElement webElement = null;
                        if (learnElement)
                        {
                            string xpath = RenderXPath.ChangeXPathIfShadowDomExists(htmlElemNode.XPath , isShadowRootDetected);
                            if (htmlElemNode.Name.ToLower().Equals(eElementType.Svg.ToString().ToLower()))
                            {
                                xpath = string.Concat(htmlElemNode.ParentNode.XPath, "//*[local-name()=\'svg\']");
                            }

                            if (string.IsNullOrEmpty(xpath))
                            {
                                webElement = (IWebElement)parentContext;
                            }
                            else
                            {
                                webElement = parentContext.FindElement(By.XPath(xpath));
                            }
                            if (webElement == null)
                            {
                                continue;
                            }
                            //filter none visible elements
                            if (!webElement.Displayed || webElement.Size.Width == 0 || webElement.Size.Height == 0)
                            {
                                //for some element like select tag el.Displayed is false but element is visible in page
                                if (webElement.GetCssValue("display").Equals("none", StringComparison.OrdinalIgnoreCase))
                                {
                                    continue;
                                }
                                else if (webElement.GetCssValue("width").Equals("auto") || webElement.GetCssValue("height").Equals("auto"))
                                {
                                    continue;
                                }
                            }

                            HTMLElementInfo foundElementInfo = new HTMLElementInfo();
                            foundElementInfo.ElementType = elementTypeEnum.Item1;
                            foundElementInfo.ElementTypeEnum = elementTypeEnum.Item2;
                            foundElementInfo.ElementObject = webElement;
                            foundElementInfo.Path = path;
                            // should we remove Xpath from HTMLElementInfo as we have a list for it now
                            foundElementInfo.HTMLElementObject = htmlElemNode;
                            foundElementInfo.XPath = xpath;
                            foundElementInfo.ParentPomElementGuid = (ParentGUID.Equals(Guid.Empty)) ? foundElementInfo.ParentPomElementGuid : ParentGUID;

                            ((IWindowExplorer)seleniumDriver).LearnElementInfoDetails(foundElementInfo, pomSetting);
                            foundElementInfo.Properties.Add(new ControlProperty() { Name = ElementProperty.Sequence, Value = foundElementsList.Count.ToString(), ShowOnUI = false });
                            if (seleniumDriver.ExtraLocatorsRequired)
                            {
                                seleniumDriver.GetRelativeXpathElementLocators(foundElementInfo);

                                if (pomSetting != null && pomSetting.relativeXpathTemplateList != null && pomSetting.relativeXpathTemplateList.Count > 0)
                                {
                                    foreach (var template in pomSetting.relativeXpathTemplateList)
                                    {
                                        seleniumDriver.CreateXpathFromUserTemplate(template, foundElementInfo);
                                    }
                                }
                            }
                            //Element Screenshot
                            if (pomSetting.LearnScreenshotsOfElements)
                            {
                                foundElementInfo.ScreenShotImage = seleniumDriver.TakeElementScreenShot(webElement);
                            }

                            foundElementInfo.IsAutoLearned = true;
                            foundElementsList.Add(foundElementInfo);

                            seleniumDriver.allReadElem.Add(foundElementInfo);
                            ISearchContext ShadowRoot = ShadowDOM.GetShadowRootIfExists(webElement);
                            if (ShadowRoot == null) continue;
                            ReadOnlyCollection<IWebElement> ChildNodes = SearchElementsFromDOM.GetAllChildNodes<IWebElement>(ShadowRoot, seleniumDriver.mDriver);

                            foreach (IWebElement ChildNode in ChildNodes)
                            {
                                string InnerHTML = ShadowDOM.GetHTML(ChildNode , seleniumDriver.mDriver);
                                if (!string.IsNullOrEmpty(InnerHTML))
                                {
                                    FindAllElementsFromPOM(path, pomSetting, ChildNode, foundElementInfo.Guid,foundElementsList, PomMetaData,true ,InnerHTML);
                                }
                            }
                        }

                        if (eElementType.Iframe == elementTypeEnum.Item2)
                        {
                            string xpath = htmlElemNode.XPath;
                            if (webElement == null)
                            {
                                webElement = parentContext.FindElement(By.XPath(xpath));
                            }
                            seleniumDriver.mDriver.SwitchTo().Frame(webElement);
                            string newPath = string.Empty;
                            if (path == string.Empty)
                            {
                                newPath = xpath;
                            }
                            else
                            {
                                newPath = path + "," + xpath;
                            }
                            FindAllElementsFromPOM(newPath, pomSetting, parentContext,ParentGUID,foundElementsList, PomMetaData,isShadowRootDetected ,pageSource);
                            seleniumDriver.mDriver.SwitchTo().ParentFrame();
                        }

                        if (eElementType.Form == elementTypeEnum.Item2)
                        {
                            formElementsList.Add(htmlElemNode);
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, string.Format("Failed to learn the Web Element '{0}'", htmlElemNode.Name), ex);
                    }
                }
            }

            int pomActivityIndex = 1;
            if (formElementsList.Any())
            {
                foreach (HtmlNode formElement in formElementsList)
                {
                    POMPageMetaData pomMetaData = new POMPageMetaData();
                    pomMetaData.Type = POMPageMetaData.MetaDataType.Form;
                    pomMetaData.Name = formElement.GetAttributeValue("name", "") != string.Empty ? formElement.GetAttributeValue("name", "") : formElement.GetAttributeValue("id", "");
                    if (string.IsNullOrEmpty(pomMetaData.Name))
                    {
                        pomMetaData.Name = "POM Activity - " + seleniumDriver.mDriver.Title + " " + pomActivityIndex;
                        pomActivityIndex++;
                    }
                    else
                    {
                        pomMetaData.Name += " " + seleniumDriver.mDriver.Title;
                    }

                    IEnumerable<HtmlNode> formInputElements = ((HtmlNode)formElement).Descendants().Where(x => x.Name.StartsWith("input"));
                    seleniumDriver.CreatePOMMetaData(foundElementsList, formInputElements.ToList(), pomMetaData, pomSetting);
                    IEnumerable<HtmlNode> formButtonElements = ((HtmlNode)formElement).Descendants().Where(x => x.Name.StartsWith("button"));
                    seleniumDriver.CreatePOMMetaData(foundElementsList, formButtonElements.ToList(), pomMetaData, pomSetting);

                    PomMetaData.Add(pomMetaData);

                }
            }
            return foundElementsList;
        }
    }
}
