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
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore.Drivers.Common;
using GingerCore.Platforms.PlatformsInfo;
using HtmlAgilityPack;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.POM
{
    internal sealed class POMLearner
    {
        internal interface IBrowserElementProvider
        {
            public Task<IBrowserElement?> GetElementAsync(eLocateBy locateBy, string locateValue);
            public Task OnFrameEnterAsync(HTMLElementInfo frameElement);
            public Task OnFrameExitAsync(HTMLElementInfo frameElement);
            public Task OnShadowDOMEnterAsync(HTMLElementInfo shadowHostElement);
            public Task OnShadowDOMExitAsync(HTMLElementInfo shadowHostElement);
        }

        private static readonly Regex UserTemplateRegex = new("@[a-zA-Z]*");

        private readonly HtmlDocument _htmlDocument;

        //why PomSetting needs to be null in some cases? I have no idea
        private readonly PomSetting? _pomSetting;
        private readonly IXPath _xpathImpl;
        private readonly IBrowserElementProvider _browserElementProvider;

        private POMLearner(HtmlDocument htmlDocument, IBrowserElementProvider browserElementProvider, PomSetting? pomSetting, IXPath xpathImpl)
        {
            _htmlDocument = htmlDocument;
            _browserElementProvider = browserElementProvider;
            _pomSetting = pomSetting;
            _xpathImpl = xpathImpl;
        }

        private static XPathHelper NewXPathHelper(IXPath xpathImpl)
        {
            List<string> importantProperties = ["POMLearner", "Web"];
            return new XPathHelper(xpathImpl, importantProperties);
        }

        internal static POMLearner Create(string html, IBrowserElementProvider browserElementProvider, PomSetting? pomSetting, IXPath xpathImpl)
        {
            HtmlDocument htmlDocument = new();
            htmlDocument.LoadHtml(html);
            return new POMLearner(htmlDocument, browserElementProvider, pomSetting, xpathImpl);
        }

        internal Task LearnElementsAsync(IList<ElementInfo> learnedElements, CancellationToken cancellationToken = default, Bitmap ScreenShot = null)
        {
            return LearnDocumentElementsAsync(_htmlDocument, parentPath: string.Empty, Guid.Empty, learnedElements, cancellationToken, ScreenShot: ScreenShot);
        }

        private async Task LearnDocumentElementsAsync(HtmlDocument htmlDocument, string parentPath, Guid parentElementId, IList<ElementInfo> learnedElements, CancellationToken cancellationToken, Bitmap ScreenShot = null)
        {
            await LearnHtmlNodeChildElements(htmlDocument.DocumentNode, parentPath, parentElementId, learnedElements, cancellationToken, ScreenShot: ScreenShot);
        }

        private async Task LearnHtmlNodeChildElements(HtmlNode htmlNode, string parentPath, Guid parentElementId, IList<ElementInfo> learnedElements, CancellationToken cancellationToken, IList<ElementInfo>? childElements = null, Bitmap ScreenShot = null)
        {
            foreach (HtmlNode childNode in htmlNode.ChildNodes)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                eElementType childNodeElementType = GetElementType(childNode);
                IBrowserElement? browserElement = null;
                HTMLElementInfo? childElement = null;

                if (IsNodeLearnable(childNode))
                {
                    bool shouldLearnThisNode = _pomSetting.FilteredElementType != null && _pomSetting.FilteredElementType.Any(x => x.ElementType.Equals(childNodeElementType));
                    browserElement = await _browserElementProvider.GetElementAsync(eLocateBy.ByXPath, childNode.XPath);
                    if (browserElement != null && await IsBrowserElementVisibleAsync(browserElement))
                    {
                        childElement = await CreateHTMLElementInfoAsync(childNode, parentPath, parentElementId, browserElement, captureScreenshot: shouldLearnThisNode, ScreenShot: ScreenShot);
                    }

                    if (childElement != null && shouldLearnThisNode)
                    {
                        learnedElements.Add(childElement);
                        if (childElements != null)
                        {
                            childElements.Add(childElement);
                        }
                    }
                }

                IList<ElementInfo> grandChildElements = [];
                if (!string.Equals(childNode.Name, "head", StringComparison.OrdinalIgnoreCase))
                {
                    await LearnHtmlNodeChildElements(childNode, parentPath, parentElementId, learnedElements, cancellationToken, grandChildElements, ScreenShot: ScreenShot);
                }

                if (childElement != null)
                {
                    foreach (ElementInfo grandChildElement in grandChildElements)
                    {
                        grandChildElement.ParentElement = childElement;
                    }
                    childElement.ChildElements.Clear();
                    childElement.ChildElements.AddRange(grandChildElements);

                    await LearnShadowDOMElementsAsync(childElement, parentPath, learnedElements, cancellationToken, ScreenShot: ScreenShot);
                    await LearnFrameElementsAsync(childElement, parentPath, learnedElements, cancellationToken, ScreenShot: ScreenShot);
                }
            }
        }

        private async Task<bool> IsBrowserElementVisibleAsync(IBrowserElement browserElement)
        {
            Size size = await browserElement.SizeAsync();
            bool isVisible = await browserElement.IsVisibleAsync();

            if (isVisible && size.Width > 0 && size.Height > 0)
            {
                return true;
            }

            string script = @"element => {
                let computedStyle = window.getComputedStyle(element);
                if (!computedStyle) {
                    return true;
                }
                let displayValue = computedStyle.getPropertyValue('display');
                if (displayValue && displayValue.toLowerCase() === 'none') {
                    return false;
                }
                let widthValue = computedStyle.getPropertyValue('width');
                if (widthValue && widthValue.toLowerCase() === 'auto') {
                    return false;
                }
                let heightValue = computedStyle.getPropertyValue('height');
                if (heightValue && heightValue.toLowerCase() === 'auto') {
                    return false;
                }
                return true;
            }";

            string scriptResult = await browserElement.ExecuteJavascriptAsync(script);

            if (!bool.TryParse(scriptResult, out bool scriptResultBool))
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"error while checking computed styles of element, expected boolean result but found {scriptResult}");
                return false;
            }

            return scriptResultBool;
        }

        private static bool IsNodeLearnable(HtmlNode htmlNode)
        {
            if (htmlNode.Name.StartsWith("#"))
            {
                return false;
            }

            if (htmlNode.XPath.Contains("/noscript", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            IEnumerable<string> learningExcludedItems = ["noscript", "script", "style", "meta", "head", "link", "html", "body"];
            if (learningExcludedItems.Any(x => string.Equals(x, htmlNode.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            return true;
        }

        private async Task<HTMLElementInfo> CreateHTMLElementInfoAsync(HtmlNode htmlNode, string parentPath, Guid parentElementId, IBrowserElement browserElement, bool captureScreenshot = true, Bitmap ScreenShot = null)
        {
            Size size = await browserElement.SizeAsync();
            Point position = await browserElement.PositionAsync();
            HTMLElementInfo htmlElementInfo = new()
            {
                ElementName = GenerateElementName(htmlNode),
                ElementType = htmlNode.Name,
                ElementTypeEnum = GetElementType(htmlNode),
                ElementObject = browserElement,
                Path = parentPath,
                Width = size.Width,
                Height = size.Height,
                X = position.X,
                Y = position.Y,
                HTMLElementObject = htmlNode,
                XPath = htmlNode.XPath,
                IsAutoLearned = true,
                Locators = [],
                Properties = [
                    new()
                    {
                        Name = ElementProperty.ParentPOMGUID,
                        Value = parentElementId.ToString(),
                        ShowOnUI = false,
                    },
                    new()
                    {
                        Name = ElementProperty.Sequence,
                        Value = string.Empty,
                        ShowOnUI = false,
                    }],
            };
            //LearnElementInfoDetails(htmlElementInfo); //check what properties of HTMLElementInfo are set in this method
            htmlElementInfo.XPath = GenerateXPathFromHtmlElementInfo(htmlElementInfo);
            htmlElementInfo.RelXpath = GenerateRelativeXPathFromHTMLElementInfo(htmlElementInfo, _xpathImpl, _pomSetting);
            htmlElementInfo.Locators.AddRange(await GenerateLocatorsAsync(htmlElementInfo, _pomSetting));
            htmlElementInfo.Locators.AddRange(await GenerateRelativeXPathLocatorsAsync(htmlElementInfo));
            htmlElementInfo.Locators.AddRange(GenerateXPathLocatorsFromUserTemplates(htmlElementInfo.HTMLElementObject.Attributes));
            htmlElementInfo.OptionalValuesObjectsList.AddRange(await GetOptionalValuesAsync(htmlElementInfo));
            htmlElementInfo.Properties.AddRange(await GetPropertiesAsync(htmlElementInfo));
            htmlElementInfo.SetLocatorsAndPropertiesCategory(ePomElementCategory.Web);
            if (captureScreenshot)
            {
                htmlElementInfo.ScreenShotImage = GingerCoreNET.GeneralLib.General.TakeElementScreenShot(htmlElementInfo, ScreenShot);
            }

            return htmlElementInfo;
        }

        private string GenerateElementName(HtmlNode htmlNode)
        {
            string elementName = string.Empty;

            string tag = htmlNode.Name;
            if (!string.IsNullOrEmpty(tag) && !elementName.Contains(tag))
            {
                elementName += $" {tag}";
            }

            string type = htmlNode.GetAttributeValue("type", def: string.Empty);
            if (!string.IsNullOrEmpty(type) && !elementName.Contains(type))
            {
                elementName += $" {type}";
            }

            string name = htmlNode.GetAttributeValue("name", def: string.Empty);
            if (!string.IsNullOrEmpty(name) && !elementName.Contains(name))
            {
                elementName += $" {name}";
            }

            string title = htmlNode.GetAttributeValue("title", def: string.Empty);
            if (!string.IsNullOrEmpty(title) && !elementName.Contains(title))
            {
                elementName += $" {title}";
            }

            string id = htmlNode.GetAttributeValue("id", def: string.Empty);
            if (!string.IsNullOrEmpty(id) && !elementName.Contains(id))
            {
                elementName += $" {id}";
            }

            string value = htmlNode.GetAttributeValue("value", def: string.Empty);
            if (!string.IsNullOrEmpty(value) && !elementName.Contains(value))
            {
                elementName += $" {value}";
            }

            string text = htmlNode.InnerText;
            if (!string.IsNullOrEmpty(text) && text.Length <= 15 && !elementName.Contains(text))
            {
                elementName += $" {text}";
            }

            return elementName;
        }

        private eElementType GetElementType(HtmlNode htmlNode)
        {
            string tag = htmlNode.Name;
            string type = htmlNode.GetAttributeValue(name: "type", def: "");
            return GetElementType(tag, type);
        }

        internal static eElementType GetElementType(string tag, string type)
        {
            return tag.ToUpper() switch
            {
                "INPUT" => type.ToUpper() switch
                {
                    "UNDEFINED" or "TEXT" or "PASSWORD" or "EMAIL" or "TEL" or "SEARCH" or "NUMBER" or "URL" or "DATE" => eElementType.TextBox,
                    "IMAGE" or "SUBMIT" or "BUTTON" => eElementType.Button,
                    "CHECKBOX" => eElementType.CheckBox,
                    "RADIO" => eElementType.RadioButton,
                    _ => eElementType.Unknown,
                },
                "TEXTAREA" or "TEXT" => eElementType.TextBox,
                "RESET" or "SUBMIT" or "BUTTON" => eElementType.Button,
                "TD" or "TH" or "TR" => eElementType.TableItem,
                "LINK" or "A" or "LI" => eElementType.HyperLink,
                "LABEL" or "TITLE" => eElementType.Label,
                "SELECT" or "SELECT-ONE" => eElementType.ComboBox,
                "TABLE" or "CAPTION" => eElementType.Table,
                "JEDITOR.TABLE" => eElementType.EditorPane,
                "DIV" => eElementType.Div,
                "SPAN" => eElementType.Span,
                "IMG" or "MAP" => eElementType.Image,
                "CHECKBOX" => eElementType.CheckBox,
                "OPTGROUP" or "OPTION" => eElementType.ComboBoxOption,
                "RADIO" => eElementType.RadioButton,
                "IFRAME" or "FRAME" or "FRAMESET" => eElementType.Iframe,
                "CANVAS" => eElementType.Canvas,
                "FORM" => eElementType.Form,
                "UL" or "OL" or "DL" => eElementType.List,
                "LI" or "DT" or "DD" => eElementType.ListItem,
                "MENU" => eElementType.MenuBar,
                "H1" or "H2" or "H3" or "H4" or "H5" or "H6" or "P" => eElementType.Text,
                "SVG" => eElementType.Svg,
                _ => eElementType.Unknown,
            };

        }

        internal static async Task<IEnumerable<ElementLocator>> GenerateLocatorsAsync(HTMLElementInfo htmlElementInfo, PomSetting? pomSetting)
        {
            ElementLocator? getPOMSettingElementLocator(eLocateBy locateBy)
            {
                if (pomSetting == null || pomSetting.ElementLocatorsSettingsList == null)
                {
                    return null;
                }
                return pomSetting.ElementLocatorsSettingsList.FirstOrDefault(l => l.LocateBy == locateBy);
            }

            List<ElementLocator> locators = new(new WebPlatform().GetLearningLocators());
            foreach (ElementLocator locator in locators)
            {
                switch (locator.LocateBy)
                {
                    case eLocateBy.ByID:
                        string id = string.Empty;
                        if (htmlElementInfo.HTMLElementObject != null)
                        {
                            id = htmlElementInfo.HTMLElementObject.GetAttributeValue("id", def: string.Empty);
                        }
                        else if (htmlElementInfo.ElementObject is IBrowserElement browserElement)
                        {
                            id = await browserElement.AttributeValueAsync("id");
                        }
                        if (string.IsNullOrEmpty(id))
                        {
                            continue;
                        }
                        locator.LocateValue = id;
                        locator.IsAutoLearned = true;
                        locator.EnableFriendlyLocator = getPOMSettingElementLocator(eLocateBy.ByID)?.EnableFriendlyLocator ?? false;
                        break;
                    case eLocateBy.ByName:
                        string name = string.Empty;
                        if (htmlElementInfo.HTMLElementObject != null)
                        {
                            name = htmlElementInfo.HTMLElementObject.GetAttributeValue("name", def: string.Empty);
                        }
                        else if (htmlElementInfo.ElementObject is IBrowserElement browserElement)
                        {
                            name = await browserElement.AttributeValueAsync("name");
                        }
                        if (string.IsNullOrEmpty(name))
                        {
                            continue;
                        }
                        locator.LocateValue = name;
                        locator.IsAutoLearned = true;
                        locator.EnableFriendlyLocator = getPOMSettingElementLocator(eLocateBy.ByName)?.EnableFriendlyLocator ?? false;
                        break;
                    case eLocateBy.ByXPath:
                        string xpath = htmlElementInfo.XPath;
                        if (string.IsNullOrEmpty(xpath))
                        {
                            continue;
                        }
                        locator.LocateValue = xpath;
                        locator.IsAutoLearned = true;
                        locator.EnableFriendlyLocator = getPOMSettingElementLocator(eLocateBy.ByID)?.EnableFriendlyLocator ?? false;
                        break;
                    case eLocateBy.ByRelXPath:
                        string relXPath = htmlElementInfo.RelXpath;
                        if (string.IsNullOrEmpty(relXPath))
                        {
                            continue;
                        }
                        locator.LocateValue = relXPath;
                        locator.IsAutoLearned = true;
                        locator.EnableFriendlyLocator = getPOMSettingElementLocator(eLocateBy.ByRelXPath)?.EnableFriendlyLocator ?? false;
                        break;
                    case eLocateBy.ByTagName:
                        string tagName = htmlElementInfo.ElementType;
                        if (htmlElementInfo.HTMLElementObject != null)
                        {
                            tagName = htmlElementInfo.HTMLElementObject.Name;
                        }
                        else if (htmlElementInfo.ElementObject is IBrowserElement browserElement)
                        {
                            tagName = await browserElement.TagNameAsync();
                        }
                        if (string.IsNullOrEmpty(tagName))
                        {
                            continue;
                        }
                        locator.LocateValue = tagName;
                        locator.IsAutoLearned = true;
                        locator.EnableFriendlyLocator = getPOMSettingElementLocator(eLocateBy.ByTagName)?.EnableFriendlyLocator ?? false;
                        break;
                }
            }
            return locators.Where(l => l.IsAutoLearned);
        }

        internal static Task<IEnumerable<OptionalValue>> GetOptionalValuesAsync(HTMLElementInfo htmlElementInfo)
        {
            if (!ElementInfo.IsElementTypeSupportingOptionalValues(htmlElementInfo.ElementTypeEnum))
            {
                return Task.FromResult<IEnumerable<OptionalValue>>([]);
            }

            List<OptionalValue> optionalValues = [];
            if (htmlElementInfo.HTMLElementObject != null)
            {
                foreach (HtmlNode childNode in htmlElementInfo.HTMLElementObject.ChildNodes)
                {
                    if (!childNode.Name.StartsWith("#") && !string.IsNullOrEmpty(childNode.InnerText))
                    {
                        string[] innerTextValues = childNode
                            .InnerText
                            .Split('\n')
                            .Where(s => !string.IsNullOrEmpty(s.Trim()))
                            .Where(s => !string.Equals(s.Trim(), "\r"))
                            .Select(s => s.Replace("\r", ""))
                            .ToArray();
                        foreach (string innerTextValue in innerTextValues)
                        {
                            optionalValues.Add(new OptionalValue() { Value = innerTextValue, IsDefault = false });
                        }
                    }
                }
            }
            else if (htmlElementInfo.ElementObject is IBrowserElement browserElement)
            {

            }
            return Task.FromResult<IEnumerable<OptionalValue>>(optionalValues);
        }

        internal static async Task<IEnumerable<ControlProperty>> GetPropertiesAsync(HTMLElementInfo htmlElementInfo)
        {
            List<ControlProperty> properties = [];

            if (!string.IsNullOrEmpty(htmlElementInfo.ElementType))
            {
                properties.Add(new()
                {
                    Name = ElementProperty.PlatformElementType,
                    Value = htmlElementInfo.ElementType,
                });
            }
            properties.Add(new()
            {
                Name = ElementProperty.ElementType,
                Value = htmlElementInfo.ElementTypeEnum.ToString(),
            });
            if (!string.IsNullOrEmpty(htmlElementInfo.Path))
            {
                properties.Add(new()
                {
                    Name = ElementProperty.ParentIFrame,
                    Value = htmlElementInfo.Path,
                });
            }
            if (!string.IsNullOrEmpty(htmlElementInfo.XPath))
            {
                properties.Add(new()
                {
                    Name = ElementProperty.XPath,
                    Value = htmlElementInfo.XPath,
                });
            }
            if (!string.IsNullOrEmpty(htmlElementInfo.RelXpath))
            {
                properties.Add(new()
                {
                    Name = ElementProperty.RelativeXPath,
                    Value = htmlElementInfo.RelXpath,
                });
            }
            if (!string.IsNullOrEmpty(htmlElementInfo.Value))
            {
                properties.Add(new()
                {
                    Name = ElementProperty.Value,
                    Value = htmlElementInfo.Value,
                });
            }
            Size size = new(htmlElementInfo.Width, htmlElementInfo.Height);
            if (htmlElementInfo.ElementObject is IBrowserElement)
            {
                Size browserElementSize = await ((IBrowserElement)htmlElementInfo.ElementObject).SizeAsync();
                size.Width = browserElementSize.Width;
                size.Height = browserElementSize.Height;
            }
            properties.Add(new()
            {
                Name = ElementProperty.Width,
                Value = size.Width.ToString(),
            });
            properties.Add(new()
            {
                Name = ElementProperty.Height,
                Value = size.Height.ToString(),
            });
            Point position = new(htmlElementInfo.X, htmlElementInfo.Y);
            if (htmlElementInfo.ElementObject is IBrowserElement)
            {
                Point browserElementPosition = await ((IBrowserElement)htmlElementInfo.ElementObject).PositionAsync();
                position.X = browserElementPosition.X;
                position.Y = browserElementPosition.Y;
            }
            properties.Add(new()
            {
                Name = ElementProperty.X,
                Value = position.X.ToString(),
            });
            properties.Add(new()
            {
                Name = ElementProperty.Y,
                Value = position.Y.ToString(),
            });
            if (htmlElementInfo.OptionalValuesObjectsList.Count > 0)
            {
                htmlElementInfo.OptionalValuesObjectsList[0].IsDefault = true;
                properties.Add(new()
                {
                    Name = ElementProperty.OptionalValues,
                    Value = htmlElementInfo.OptionalValuesObjectsListAsString.Replace("*", "")
                });
            }
            IEnumerable<KeyValuePair<string, string>> htmlAttributes = [];
            if (htmlElementInfo.HTMLElementObject != null)
            {
                htmlAttributes = htmlElementInfo.HTMLElementObject.Attributes.Select(a => new KeyValuePair<string, string>(a.Name, a.Value));
            }
            else if (htmlElementInfo.ElementObject is IBrowserElement)
            {
                htmlAttributes = await ((IBrowserElement)htmlElementInfo.ElementObject).AttributesAsync();
            }
            foreach (KeyValuePair<string, string> htmlAttribute in htmlAttributes)
            {
                if (properties.Any(p => string.Equals(p.Name, htmlAttribute.Key) && string.Equals(p.Value, htmlAttribute.Value)))
                {
                    continue;
                }
                if (string.Equals(htmlAttribute.Key, "style") ||
                    string.Equals(htmlAttribute.Value, "border: 3px dashed red;") ||
                    string.Equals(htmlAttribute.Value, "outline: 3px dashed red;"))
                {
                    continue;
                }

                properties.Add(new()
                {
                    Name = htmlAttribute.Key,
                    Value = htmlAttribute.Value,
                });
            }
            if (htmlElementInfo.HTMLElementObject != null &&
                !string.IsNullOrEmpty(htmlElementInfo.HTMLElementObject.InnerText) &&
                htmlElementInfo.OptionalValues.Count == 0 &&
                htmlElementInfo.HTMLElementObject.ChildNodes.Count == 0)
            {
                properties.Add(new()
                {
                    Name = ElementProperty.InnerText,
                    Value = htmlElementInfo.HTMLElementObject.InnerText
                });
            }

            return properties;
        }

        private async Task<IEnumerable<ElementLocator>> GenerateRelativeXPathLocatorsAsync(HTMLElementInfo htmlElementInfo)
        {
            if (htmlElementInfo.ElementTypeEnum == eElementType.Svg)
            {
                return [];
            }

            List<ElementLocator> locators = [];
            XPathHelper xpathHelper = NewXPathHelper(_xpathImpl);

            string relXPathWithMultipleAtrrs = xpathHelper.CreateRelativeXpathWithTagNameAndAttributes(htmlElementInfo);
            if (!string.IsNullOrEmpty(relXPathWithMultipleAtrrs) &&
                _browserElementProvider.GetElementAsync(eLocateBy.ByRelXPath, relXPathWithMultipleAtrrs) != null)
            {
                locators.Add(new()
                {
                    LocateBy = eLocateBy.ByRelXPath,
                    LocateValue = relXPathWithMultipleAtrrs,
                    IsAutoLearned = true,
                });
            }

            string innerText = htmlElementInfo.HTMLElementObject.InnerText;
            if (!string.IsNullOrEmpty(innerText))
            {
                var relXPathWithExactTextMatch = xpathHelper.CreateRelativeXpathWithTextMatch(htmlElementInfo, isExactMatch: true);
                if (!string.IsNullOrEmpty(relXPathWithExactTextMatch) &&
                    (await _browserElementProvider.GetElementAsync(eLocateBy.ByRelXPath, relXPathWithExactTextMatch)) != null)
                {
                    locators.Add(new()
                    {
                        LocateBy = eLocateBy.ByRelXPath,
                        LocateValue = relXPathWithExactTextMatch,
                        IsAutoLearned = true
                    });

                    var relXPathWithContainsText = xpathHelper.CreateRelativeXpathWithTextMatch(htmlElementInfo, isExactMatch: false);
                    if (!string.IsNullOrEmpty(relXPathWithContainsText))
                    {
                        locators.Add(new()
                        {

                            LocateBy = eLocateBy.ByRelXPath,
                            LocateValue = relXPathWithContainsText,
                            IsAutoLearned = true
                        });
                    }
                }
            }

            var relXPathWithSiblingText = xpathHelper.CreateRelativeXpathWithSibling(htmlElementInfo);
            if (!string.IsNullOrEmpty(relXPathWithSiblingText) &&
                _browserElementProvider.GetElementAsync(eLocateBy.ByRelXPath, relXPathWithSiblingText) != null)
            {
                var elementLocator = new ElementLocator() { LocateBy = eLocateBy.ByRelXPath, LocateValue = relXPathWithSiblingText, IsAutoLearned = true };
                locators.Add(elementLocator);
            }

            return locators;
        }

        private IEnumerable<ElementLocator> GenerateXPathLocatorsFromUserTemplates(IEnumerable<HtmlAttribute> htmlAttributes)
        {
            if (_pomSetting == null ||
                _pomSetting.RelativeXpathTemplateList == null ||
                _pomSetting.RelativeXpathTemplateList.Count <= 0)
            {
                return [];
            }

            List<ElementLocator> locators = [];
            foreach (var template in _pomSetting.RelativeXpathTemplateList)
            {
                var relXpath = string.Empty;

                var attributeCount = 0;

                MatchCollection attList = UserTemplateRegex.Matches(template.Value);
                var strList = new List<string>();
                foreach (Match item in attList)
                {
                    if (item == null)
                    {
                        continue;
                    }
                    strList.Add(item.Value.Remove(0, 1));
                }

                foreach (var item in htmlAttributes)
                {
                    if (strList.Contains(item.Name))
                    {
                        relXpath = template.Value.Replace(item.Name + "=\'\'", item.Name + "=\'" + item.Value + "\'");

                        attributeCount++;
                    }
                }

                if (relXpath != string.Empty &&
                    attributeCount == attList.Count &&
                    _browserElementProvider.GetElementAsync(eLocateBy.ByRelXPath, relXpath) != null)
                {
                    locators.Add(new()
                    {
                        LocateBy = eLocateBy.ByRelXPath,
                        LocateValue = relXpath,
                        IsAutoLearned = true
                    });
                }
            }
            return locators;
        }

        internal static string GenerateXPathFromHtmlElementInfo(HTMLElementInfo htmlElementInfo)
        {
            if (!string.IsNullOrEmpty(htmlElementInfo.XPath) && !string.Equals(htmlElementInfo.XPath, "/"))
            {
                return htmlElementInfo.XPath;
            }

            string lastXPathSegment = string.Empty;
            if (!string.IsNullOrEmpty(htmlElementInfo.Path))
            {
                string[] xpathSegments = htmlElementInfo.Path.Split('/');
                lastXPathSegment = xpathSegments[^1];
            }
            string xpath = string.Empty;
            if (!lastXPathSegment.Contains("frame"))
            {
                xpath = htmlElementInfo.Path;
            }

            Stack<HtmlNode> nodes = [];
            nodes.Push(htmlElementInfo.HTMLElementObject);

            while (nodes.Count > 0)
            {
                HtmlNode currentNode = nodes.Pop();
                string tag = currentNode.Name;

                if (string.Equals(tag, "html"))
                {
                    xpath = $"/html[1]{xpath}";
                    continue;
                }

                HtmlNode parentNode = currentNode.ParentNode;
                int count = 1;
                foreach (HtmlNode childNode in parentNode.ChildNodes)
                {
                    if (childNode != currentNode)
                    {
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }

                xpath = $"/{tag}[{count}]{xpath}";
                nodes.Push(parentNode);
            }

            return xpath;
        }

        internal static string GenerateRelativeXPathFromHTMLElementInfo(HTMLElementInfo htmlElementInfo, IXPath xpathImpl, PomSetting? pomSetting)
        {
            if (htmlElementInfo.ElementTypeEnum == eElementType.Svg)
            {
                return string.Empty;
            }

            XPathHelper xpathHelper = NewXPathHelper(xpathImpl);
            return xpathHelper.GetElementRelXPath(htmlElementInfo, pomSetting);
        }

        private async Task<string?> GetElementScreenshotAsync(IBrowserElement? browserElement)
        {
            if (_pomSetting == null ||
                !_pomSetting.LearnScreenshotsOfElements ||
                browserElement == null)
            {
                return null;
            }

            try
            {
                return Convert.ToBase64String(await browserElement.ScreenshotAsync());
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task LearnShadowDOMElementsAsync(HTMLElementInfo shadowHostElement, string parentPath, IList<ElementInfo> learnedElements, CancellationToken cancellationToken, Bitmap ScreenShot = null)
        {
            if (_pomSetting == null ||
                !_pomSetting.LearnShadowDomElements ||
                shadowHostElement.ElementTypeEnum == eElementType.Iframe ||
                shadowHostElement.ElementObject == null ||
                await ((IBrowserElement)shadowHostElement.ElementObject).ShadowRootAsync() == null)
            {
                return;
            }

            IBrowserElement browserElement = (IBrowserElement)shadowHostElement.ElementObject;

            await _browserElementProvider.OnShadowDOMEnterAsync(shadowHostElement);

            IBrowserShadowRoot? browserShadowRoot = await browserElement.ShadowRootAsync();
            string? shadowRootHTML = browserShadowRoot != null ? await browserShadowRoot.HTML() : "";
            if (string.IsNullOrEmpty(shadowRootHTML))
            {
                return;
            }

            HtmlDocument shadowRootHtmlDocument = new();
            shadowRootHtmlDocument.LoadHtml(shadowRootHTML);

            await LearnDocumentElementsAsync(shadowRootHtmlDocument, parentPath, shadowHostElement.Guid, learnedElements, cancellationToken, ScreenShot: ScreenShot);

            await _browserElementProvider.OnShadowDOMExitAsync(shadowHostElement);
        }

        private async Task LearnFrameElementsAsync(HTMLElementInfo frameElement, string parentPath, IList<ElementInfo> learnedElements, CancellationToken cancellationToken, Bitmap ScreenShot = null)
        {
            if (frameElement.ElementTypeEnum != eElementType.Iframe)
            {
                return;
            }

            IBrowserElement? browserElement = (IBrowserElement?)frameElement.ElementObject;
            if (browserElement == null)
            {
                return;
            }

            string iframePageSource = "";
            try
            {
                iframePageSource = await browserElement.ExecuteJavascriptAsync("element => element.contentDocument.documentElement.outerHTML");
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error while getting IFrame page source", ex);
            }
            if (string.IsNullOrEmpty(iframePageSource))
            {
                return;
            }

            HtmlDocument frameHtmlDocument = new();
            frameHtmlDocument.LoadHtml(iframePageSource);

            await _browserElementProvider.OnFrameEnterAsync(frameElement);

            string newParentPath = string.IsNullOrEmpty(parentPath) ? frameElement.XPath : $"{parentPath},{frameElement.XPath}";

            await LearnDocumentElementsAsync(frameHtmlDocument, newParentPath, Guid.Empty, learnedElements, cancellationToken, ScreenShot: ScreenShot);

            await _browserElementProvider.OnFrameExitAsync(frameElement);
        }
    }
}
