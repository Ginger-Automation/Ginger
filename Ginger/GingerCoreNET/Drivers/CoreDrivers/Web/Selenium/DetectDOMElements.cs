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
        bool isShadowRootDetected = false;
        public DetectDOMElements(SeleniumDriver seleniumDriver) { 
            this.seleniumDriver = seleniumDriver;
        }



        public ObservableList<ElementInfo> GetAllElementsFromPage(string path, PomSetting pomSetting, ObservableList<ElementInfo> foundElementsList = null, ObservableList<POMPageMetaData> PomMetaData = null)
        {
            return GetAllElementsFromPage(path, pomSetting, seleniumDriver.mDriver,foundElementsList, PomMetaData);
        }
        
        private ObservableList<ElementInfo> GetAllElementsFromPage(string path, PomSetting pomSetting, ISearchContext parentContext, ObservableList<ElementInfo> foundElementsList = null, ObservableList<POMPageMetaData> PomMetaData = null , string pageSource = null)
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
                            var xpath = ShadowDOM.ChangeXPathIfShadowDomExists(htmlElemNode.XPath , isShadowRootDetected);
                            if (htmlElemNode.Name.ToLower().Equals(eElementType.Svg.ToString().ToLower()))
                            {
                                xpath = ShadowDOM.ChangeXPathIfShadowDomExists(string.Concat(htmlElemNode.ParentNode.XPath, "//*[local-name()=\'svg\']") , isShadowRootDetected);
                            }

                            webElement = parentContext.FindElement(By.XPath(xpath));
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

                            HTMLElementInfo foundElemntInfo = new HTMLElementInfo();
                            foundElemntInfo.ElementType = elementTypeEnum.Item1;
                            foundElemntInfo.ElementTypeEnum = elementTypeEnum.Item2;
                            foundElemntInfo.ElementObject = webElement;
                            foundElemntInfo.Path = path;
                            foundElemntInfo.XPath = xpath;
                            foundElemntInfo.HTMLElementObject = htmlElemNode;
                            foundElemntInfo.ParentContext = parentContext;

                            ((IWindowExplorer)seleniumDriver).LearnElementInfoDetails(foundElemntInfo, pomSetting);
                            foundElemntInfo.Properties.Add(new ControlProperty() { Name = ElementProperty.Sequence, Value = foundElementsList.Count.ToString(), ShowOnUI = false });
                            if (seleniumDriver.ExtraLocatorsRequired)
                            {
                                seleniumDriver.GetRelativeXpathElementLocators(foundElemntInfo);

                                if (pomSetting != null && pomSetting.relativeXpathTemplateList != null && pomSetting.relativeXpathTemplateList.Count > 0)
                                {
                                    foreach (var template in pomSetting.relativeXpathTemplateList)
                                    {
                                        seleniumDriver.CreateXpathFromUserTemplate(template, foundElemntInfo);
                                    }
                                }
                            }
                            //Element Screenshot
                            if (pomSetting.LearnScreenshotsOfElements)
                            {
                                foundElemntInfo.ScreenShotImage = seleniumDriver.TakeElementScreenShot(webElement);
                            }

                            foundElemntInfo.IsAutoLearned = true;
                            foundElementsList.Add(foundElemntInfo);

                            seleniumDriver.allReadElem.Add(foundElemntInfo);
                            ISearchContext ShadowRoot = ShadowDOM.GetShadowRootIfExists(webElement);
                            if (ShadowRoot == null) continue;
                            isShadowRootDetected = true;
                            ReadOnlyCollection<IWebElement> ChildNodes = ShadowDOM.GetAllChildNodesFromShadow(ShadowRoot, seleniumDriver.mDriver);

                            foreach (IWebElement ChildNode in ChildNodes)
                            {
                                string InnerHTML = ShadowDOM.GetInnerHTML(ChildNode , seleniumDriver.mDriver);
                                if (!string.IsNullOrEmpty(InnerHTML))
                                {
                                    GetAllElementsFromPage(path, pomSetting, ChildNode, foundElementsList, PomMetaData, InnerHTML);
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
                            GetAllElementsFromPage(newPath, pomSetting, parentContext,foundElementsList, PomMetaData);
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
