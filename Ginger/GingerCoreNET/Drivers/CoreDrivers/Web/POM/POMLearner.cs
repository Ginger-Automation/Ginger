using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.IO;
using Amdocs.Ginger.Repository;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using GingerCore.Drivers.Common;
using GingerCore.Platforms.PlatformsInfo;
using HtmlAgilityPack;
using Microsoft.Graph;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        internal Task<IEnumerable<HTMLElementInfo>> LearnElementsAsync()
        {
            return LearnDocumentElementsAsync(_htmlDocument);
        }

        private Task<IEnumerable<HTMLElementInfo>> LearnDocumentElementsAsync(HtmlDocument htmlDocument)
        {
            return LearnHtmlNodeChildElements(htmlDocument.DocumentNode);
        }

        private async Task<IEnumerable<HTMLElementInfo>> LearnHtmlNodeChildElements(HtmlNode htmlNode)
        {
            List<HTMLElementInfo> htmlElements = [];

            foreach (HtmlNode childNode in htmlNode.ChildNodes)
            {
                if (!ShouldIncludeHtmlNode(childNode))
                {
                    continue;
                }

                IBrowserElement? browserElement = await _browserElementProvider.GetElementAsync(eLocateBy.ByXPath, childNode.XPath);
                if (browserElement == null)
                {
                    continue;
                }

                if (!await browserElement.IsVisibleAsync())
                {
                    continue;
                }

                HTMLElementInfo childElement = await CreateHTMLElementInfoAsync(childNode, browserElement);

                htmlElements.Add(childElement);

                htmlElements.AddRange(await LearnShadowDOMElementsAsync(childElement));
                htmlElements.AddRange(await LearnFrameElementsAsync(childElement));
                //TODO: create suggested activities

                IEnumerable<HTMLElementInfo> grandChildElements = await LearnHtmlNodeChildElements(childNode);
                foreach (HTMLElementInfo grandChildElement in grandChildElements)
                {
                    grandChildElement.ParentElement = childElement;
                }
                childElement.ChildElements.Clear();
                childElement.ChildElements.AddRange(grandChildElements.Cast<ElementInfo>());

                htmlElements.AddRange(grandChildElements);
            }

            return htmlElements;
        }

        private bool ShouldIncludeHtmlNode(HtmlNode htmlNode)
        {
            if (_pomSetting == null || _pomSetting.filteredElementType == null)
            {
                return false;
            }

            eElementType type = GetElementType(htmlNode);
            return _pomSetting.filteredElementType.Contains(type);
        }

        private async Task<HTMLElementInfo> CreateHTMLElementInfoAsync(HtmlNode htmlNode, IBrowserElement browserElement)
        {
            Size size = await browserElement.SizeAsync();
            Point position = await browserElement.PositionAsync();
            HTMLElementInfo htmlElementInfo = new()
            {
                ElementType = htmlNode.Name,
                ElementTypeEnum = GetElementType(htmlNode),
                ElementObject = browserElement,
                Path = string.Empty,
                Width = size.Width,
                Height = size.Height,
                X = position.X,
                Y = position.Y,
                HTMLElementObject = htmlNode,
                XPath = htmlNode.XPath,
                IsAutoLearned = true,
                ScreenShotImage = await GetElementScreenshotAsync(browserElement),
                Locators = [],
                Properties = [
                    new()
                    {
                        Name = ElementProperty.ParentPOMGUID,
                        Value = Guid.Empty.ToString(),
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
            htmlElementInfo.Locators.AddRange(GenerateLocators(htmlElementInfo, _pomSetting));
            htmlElementInfo.Locators.AddRange(GenerateRelativeXPathLocators(htmlElementInfo));
            htmlElementInfo.Locators.AddRange(GenerateXPathLocatorsFromUserTemplates(htmlElementInfo.HTMLElementObject.Attributes));
            htmlElementInfo.OptionalValuesObjectsList.AddRange(GetOptionValues(htmlElementInfo));
            htmlElementInfo.Properties.AddRange(GetProperties(htmlElementInfo));
            htmlElementInfo.SetLocatorsAndPropertiesCategory(ePomElementCategory.Web);

            return htmlElementInfo;
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

        internal static IEnumerable<ElementLocator> GenerateLocators(HTMLElementInfo htmlElementInfo, PomSetting? pomSetting)
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
                        string id = htmlElementInfo.HTMLElementObject.GetAttributeValue("id", def: string.Empty);
                        if (string.IsNullOrEmpty(id))
                        {
                            continue;
                        }
                        locator.LocateValue = id;
                        locator.IsAutoLearned = true;
                        locator.EnableFriendlyLocator = getPOMSettingElementLocator(eLocateBy.ByID)?.EnableFriendlyLocator ?? false;
                        break;
                    case eLocateBy.ByName:
                        string name = htmlElementInfo.HTMLElementObject.GetAttributeValue("name", def: string.Empty);
                        if (string.IsNullOrEmpty(name))
                        {
                            continue;
                        }
                        locator.LocateValue = name;
                        locator.IsAutoLearned = true;
                        locator.EnableFriendlyLocator = getPOMSettingElementLocator(eLocateBy.ByName)?.EnableFriendlyLocator ?? false;
                        break;
                    case eLocateBy.ByRelXPath:
                        string relXPath = htmlElementInfo.XPath;
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

        internal static  IEnumerable<OptionalValue> GetOptionValues(HTMLElementInfo htmlElementInfo)
        {
            if (!ElementInfo.IsElementTypeSupportingOptionalValues(htmlElementInfo.ElementTypeEnum))
            {
                return [];
            }
            
            List<OptionalValue> optionalValues = [];
            foreach (HtmlNode childNode in htmlElementInfo.HTMLElementObject.ChildNodes)
            {
                if (!childNode.Name.StartsWith("#") && !string.IsNullOrEmpty(childNode.InnerText))
                {
                    string[] tempOpVals = childNode.InnerText.Split('\n');
                    foreach (string cuVal in tempOpVals)
                    {
                        optionalValues.Add(new OptionalValue() { Value = cuVal, IsDefault = false });
                    }
                }
            }
            return optionalValues;
        }

        internal static IEnumerable<ControlProperty> GetProperties(HTMLElementInfo htmlElementInfo)
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
            properties.Add(new()
            {
                Name = ElementProperty.Width,
                Value = htmlElementInfo.Width.ToString(),
            });
            properties.Add(new()
            {
                Name = ElementProperty.Height,
                Value = htmlElementInfo.Height.ToString(),
            });
            properties.Add(new()
            {
                Name = ElementProperty.X,
                Value = htmlElementInfo.X.ToString(),
            });
            properties.Add(new()
            {
                Name = ElementProperty.Y,
                Value = htmlElementInfo.Y.ToString(),
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
            foreach (HtmlAttribute htmlAttribute in htmlElementInfo.HTMLElementObject.Attributes)
            {
                if (!properties.Any(p => string.Equals(p.Name, htmlAttribute.Name) && string.Equals(p.Value, htmlAttribute.Value)))
                {
                    properties.Add(new()
                    {
                        Name = htmlAttribute.Name,
                        Value = htmlAttribute.Value,
                    });
                }
            }
            if (!string.IsNullOrEmpty(htmlElementInfo.HTMLElementObject.InnerText) &&
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

        private IEnumerable<ElementLocator> GenerateRelativeXPathLocators(HTMLElementInfo htmlElementInfo)
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
                    _browserElementProvider.GetElementAsync(eLocateBy.ByRelXPath, relXPathWithExactTextMatch) != null)
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
                _pomSetting.relativeXpathTemplateList == null ||
                _pomSetting.relativeXpathTemplateList.Count <= 0)
            {
                return [];
            }

            List<ElementLocator> locators = [];
            foreach (string template in _pomSetting.relativeXpathTemplateList)
            {
                var relXpath = string.Empty;

                var attributeCount = 0;

                var attList = UserTemplateRegex.Matches(template);
                var strList = new List<string>();
                foreach (string? item in attList)
                {
                    if (item == null)
                    {
                        continue;
                    }
                    strList.Add(item.ToString().Remove(0, 1));
                }

                foreach (var item in htmlAttributes)
                {
                    if (strList.Contains(item.Name))
                    {
                        relXpath = template.Replace(item.Name + "=\'\'", item.Name + "=\'" + item.Value + "\'");

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
                lastXPathSegment = xpathSegments[xpathSegments.Length - 1];
            }
            string xpath = string.Empty;
            if (!lastXPathSegment.Contains("frame"))
            {
                xpath = htmlElementInfo.Path;
            }

            Stack<HtmlNode> nodes = [];
            nodes.Push(htmlElementInfo.HTMLElementObject);

            while(nodes.Count > 0)
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
                foreach(HtmlNode childNode in parentNode.ChildNodes)
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

        private async Task<string> GetElementScreenshotAsync(IBrowserElement? browserElement)
        {
            if (_pomSetting == null ||
                !_pomSetting.LearnScreenshotsOfElements || 
                browserElement == null)
            {
                return string.Empty;
            }

            return Convert.ToBase64String(await browserElement.ScreenshotAsync());
        }

        private async Task<IEnumerable<HTMLElementInfo>> LearnShadowDOMElementsAsync(HTMLElementInfo shadowHostElement)
        {
            if (_pomSetting == null ||
                !_pomSetting.LearnShadowDomElements ||
                shadowHostElement.ElementTypeEnum == eElementType.Iframe ||
                shadowHostElement.ElementObject == null ||
                await ((IBrowserElement)shadowHostElement.ElementObject).ShadowRootAsync() != null)
            {
                return [];
            }

            IBrowserElement browserElement = (IBrowserElement)shadowHostElement.ElementObject;

            await _browserElementProvider.OnShadowDOMEnterAsync(shadowHostElement);

            IBrowserShadowRoot? browserShadowRoot = await browserElement.ShadowRootAsync();
            string? shadowRootHTML = browserShadowRoot != null ? await browserShadowRoot.HTML() : "";
            if (string.IsNullOrEmpty(shadowRootHTML))
            {
                return [];
            }

            HtmlDocument shadowRootHtmlDocument = new();
            shadowRootHtmlDocument.LoadHtml(shadowRootHTML);

            IEnumerable<HTMLElementInfo> htmlElements = await LearnDocumentElementsAsync(shadowRootHtmlDocument);

            await _browserElementProvider.OnShadowDOMExitAsync(shadowHostElement);

            return htmlElements;
        }

        private async Task<IEnumerable<HTMLElementInfo>> LearnFrameElementsAsync(HTMLElementInfo frameElement)
        {
            if (frameElement.ElementTypeEnum != eElementType.Iframe)
            {
                return [];
            }

            IBrowserElement? browserElement = (IBrowserElement?)frameElement.ElementObject;
            if (browserElement == null)
            {
                return [];
            }

            string iframePageSource = await browserElement.ExecuteJavascriptAsync("element => element.contentDocument.documentElement.outerHTML");
            if (string.IsNullOrEmpty(iframePageSource))
            {
                return [];
            }

            HtmlDocument frameHtmlDocument = new();
            frameHtmlDocument.LoadHtml(iframePageSource);

            await _browserElementProvider.OnFrameEnterAsync(frameElement);
            
            IEnumerable<HTMLElementInfo> htmlElements = await LearnDocumentElementsAsync(frameHtmlDocument);

            await _browserElementProvider.OnFrameExitAsync(frameElement);

            return htmlElements;
        }
    }
}
