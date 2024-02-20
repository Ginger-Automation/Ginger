using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using GingerCore.Drivers;
using GingerCoreNET.Drivers.CommonLib;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Selenium
{
    public class LocateWebElement
    {
        private readonly SeleniumDriver seleniumDriver;
        private readonly ShadowDOM shadowDOM = new();
        public LocateWebElement(SeleniumDriver seleniumDriver)
        {
            this.seleniumDriver = seleniumDriver;
        }
        public IWebElement LocateElementByLocator(ElementLocator locator, ISearchContext searchContext, List<FriendlyLocatorElement> friendlyLocatorElements = null, bool AlwaysReturn = true)
        {
            IWebElement elem = null;
            locator.StatusError = "";
            locator.LocateStatus = ElementLocator.eLocateStatus.Pending;
            try
            {
                try
                {
                    Protractor.NgWebDriver ngDriver = null;
                    if (locator.LocateBy == eLocateBy.ByngRepeat || locator.LocateBy == eLocateBy.ByngSelectedOption || locator.LocateBy == eLocateBy.ByngBind || locator.LocateBy == eLocateBy.ByngModel)
                    {
                        ngDriver = new Protractor.NgWebDriver(seleniumDriver.mDriver);
                        ngDriver.WaitForAngular();
                    }
                    if (locator.LocateBy == eLocateBy.ByngRepeat)
                    {
                        elem = ngDriver.FindElement(Protractor.NgBy.Repeater(locator.LocateValue));
                    }
                    if (locator.LocateBy == eLocateBy.ByngSelectedOption)
                    {
                        elem = ngDriver.FindElement(Protractor.NgBy.SelectedOption(locator.LocateValue));
                    }
                    if (locator.LocateBy == eLocateBy.ByngBind)
                    {
                        elem = ngDriver.FindElement(Protractor.NgBy.Binding(locator.LocateValue));
                    }
                    if (locator.LocateBy == eLocateBy.ByngModel)
                    {
                        elem = ngDriver.FindElement(Protractor.NgBy.Model(locator.LocateValue));
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Exception occurred when LocateElementByLocator", ex);
                    if (AlwaysReturn)
                    {
                        elem = null;
                        locator.StatusError = ex.Message;
                        locator.LocateStatus = ElementLocator.eLocateStatus.Failed;
                        return elem;
                    }
                    else
                    {
                        throw;
                    }
                }


                if (locator.LocateBy == eLocateBy.ByID)
                {
                    if (locator.LocateValue.IndexOf("{RE:") >= 0)
                    {
                        elem = seleniumDriver.FindElementReg(locator.LocateBy, locator.LocateValue);
                    }
                    else
                    {
                        if (locator.EnableFriendlyLocator && FriendlyLocatorElement.DoesFriendlyLocatorListExist(friendlyLocatorElements))
                        {
                            By by = By.Id(locator.LocateValue) as By;
                            elem = seleniumDriver.GetElementByFriendlyLocatorlist(friendlyLocatorElements, by);
                        }
                        elem ??= searchContext.FindElement(By.Id(locator.LocateValue));
                    }

                }

                if (locator.LocateBy == eLocateBy.ByName)
                {
                    if (locator.LocateValue.IndexOf("{RE:") >= 0)
                    {
                        elem = seleniumDriver.FindElementReg(locator.LocateBy, locator.LocateValue);
                    }
                    else
                    {
                        if (locator.EnableFriendlyLocator && FriendlyLocatorElement.DoesFriendlyLocatorListExist(friendlyLocatorElements))
                        {
                            By by = By.Name(locator.LocateValue) as By;
                            elem = seleniumDriver.GetElementByFriendlyLocatorlist(friendlyLocatorElements, by);
                        }
                        elem ??= searchContext.FindElement(By.Name(locator.LocateValue));
                    }

                }

                if (locator.LocateBy == eLocateBy.ByHref)
                {
                    string pattern = @".+:\/\/[^\/]+";
                    string sel = "//a[contains(@href, '@RREEPP')]";
                    sel = sel.Replace("@RREEPP", new Regex(pattern).Replace(locator.LocateValue, ""));
                    try
                    {
                        if (locator.LocateValue.IndexOf("{RE:") >= 0)
                        {
                            elem = seleniumDriver.FindElementReg(locator.LocateBy, locator.LocateValue);
                        }
                        else
                        {
                            if (locator.EnableFriendlyLocator && FriendlyLocatorElement.DoesFriendlyLocatorListExist(friendlyLocatorElements))
                            {
                                By by = By.XPath(sel) as By;
                                elem = seleniumDriver.GetElementByFriendlyLocatorlist(friendlyLocatorElements, by);
                            }

                            if (searchContext is ShadowRoot)
                            {
                                elem ??= searchContext.FindElement(By.CssSelector($"a[href='{sel}']"));
                            }
                            else
                            {
                                elem ??= searchContext.FindElement(By.XPath(sel));
                            }

                        }

                    }
                    catch (NoSuchElementException ex)
                    {
                        try
                        {
                            sel = "//a[href='@']";
                            sel = sel.Replace("@", locator.LocateValue);
                            elem = searchContext.FindElement(By.XPath(sel));
                            locator.StatusError = ex.Message;
                        }
                        catch (Exception)
                        { }
                    }
                    catch (Exception)
                    { }
                }

                // need to check if this works with Shadow Root
                if (locator.LocateBy == eLocateBy.ByLinkText)
                {
                    locator.LocateValue = locator.LocateValue.Trim();
                    try
                    {
                        if (locator.LocateValue.IndexOf("{RE:") >= 0)
                        {
                            elem = seleniumDriver.FindElementReg(locator.LocateBy, locator.LocateValue);
                        }
                        else
                        {
                            if (locator.EnableFriendlyLocator && FriendlyLocatorElement.DoesFriendlyLocatorListExist(friendlyLocatorElements))
                            {
                                By by = By.LinkText(locator.LocateValue) as By;
                                elem = seleniumDriver.GetElementByFriendlyLocatorlist(friendlyLocatorElements, by);
                            }
                            elem ??= searchContext.FindElement(By.LinkText(locator.LocateValue));
                            if (elem == null)
                            {
                                if (locator.EnableFriendlyLocator && FriendlyLocatorElement.DoesFriendlyLocatorListExist(friendlyLocatorElements))
                                {
                                    By by = By.XPath("//*[text()='" + locator.LocateValue + "']") as By;
                                    elem = seleniumDriver.GetElementByFriendlyLocatorlist(friendlyLocatorElements, by);
                                }
                                elem ??= searchContext.FindElement(By.XPath("//*[text()='" + locator.LocateValue + "']"));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            if (ex.GetType() == typeof(NoSuchElementException))
                            {
                                elem = searchContext.FindElement(By.XPath("//*[text()='" + locator.LocateValue + "']"));

                            }
                        }
                        catch (Exception ex2)
                        {
                            locator.StatusError = ex2.Message;
                        }
                    }

                }
                if (locator.LocateBy == eLocateBy.ByXPath || locator.LocateBy == eLocateBy.ByRelXPath)
                {

                    if (locator.EnableFriendlyLocator && FriendlyLocatorElement.DoesFriendlyLocatorListExist(friendlyLocatorElements))
                    {
                        By by = By.XPath(locator.LocateValue) as By;
                        elem = seleniumDriver.GetElementByFriendlyLocatorlist(friendlyLocatorElements, by);
                    }
                    if (searchContext is ShadowRoot)
                    {
                        if (locator.LocateBy.Equals(eLocateBy.ByXPath))
                        {

                            string cssSelector = shadowDOM.ConvertXPathToCssSelector(locator.LocateValue);
                            elem ??= searchContext.FindElement(By.CssSelector(cssSelector));
                        }
                    }

                    else
                    {
                        elem ??= searchContext.FindElement(By.XPath(locator.LocateValue));
                    }
                }

                if (locator.LocateBy == eLocateBy.ByValue)
                {
                    if (locator.LocateValue.IndexOf("{RE:") >= 0)
                    {
                        elem = seleniumDriver.FindElementReg(locator.LocateBy, locator.LocateValue);
                    }
                    else
                    {
                        if (locator.EnableFriendlyLocator && FriendlyLocatorElement.DoesFriendlyLocatorListExist(friendlyLocatorElements))
                        {
                            By by = By.XPath("//*[@value=\"" + locator.LocateValue + "\"]") as By;
                            elem = seleniumDriver.GetElementByFriendlyLocatorlist(friendlyLocatorElements, by);
                        }
                        if (searchContext is ShadowRoot)
                        {
                            elem ??= searchContext.FindElement(By.CssSelector($"[value='{locator.LocateValue}']"));
                        }
                        else
                        {
                            elem ??= searchContext.FindElement(By.XPath("//*[@value=\"" + locator.LocateValue + "\"]"));
                        }

                    }
                }

                if (locator.LocateBy == eLocateBy.ByAutomationID)
                {
                    if (locator.EnableFriendlyLocator && FriendlyLocatorElement.DoesFriendlyLocatorListExist(friendlyLocatorElements))
                    {
                        By by = By.XPath("//*[@data-automation-id=\"" + locator.LocateValue + "\"]") as By;
                        elem = seleniumDriver.GetElementByFriendlyLocatorlist(friendlyLocatorElements, by);
                    }

                    if (searchContext is ShadowRoot)
                    {
                        elem ??= searchContext.FindElement(By.CssSelector($"[data-automation-id={locator.LocateValue}]"));
                    }
                    else
                    {
                        elem ??= searchContext.FindElement(By.XPath("//*[@data-automation-id=\"" + locator.LocateValue + "\"]"));
                    }


                }

                if (locator.LocateBy == eLocateBy.ByCSS)
                {
                    if (locator.EnableFriendlyLocator && FriendlyLocatorElement.DoesFriendlyLocatorListExist(friendlyLocatorElements))
                    {
                        By by = By.CssSelector(locator.LocateValue) as By;
                        elem = seleniumDriver.GetElementByFriendlyLocatorlist(friendlyLocatorElements, by);
                    }
                    elem ??= searchContext.FindElement(By.CssSelector(locator.LocateValue));
                }

                // need to check with shadow dom
                if (locator.LocateBy == eLocateBy.ByClassName)
                {
                    if (locator.EnableFriendlyLocator && FriendlyLocatorElement.DoesFriendlyLocatorListExist(friendlyLocatorElements))
                    {
                        By by = By.ClassName(locator.LocateValue) as By;
                        elem = seleniumDriver.GetElementByFriendlyLocatorlist(friendlyLocatorElements, by);
                    }

                    elem ??= searchContext.FindElement(By.ClassName(locator.LocateValue));

                }

                if (locator.LocateBy == eLocateBy.ByMulitpleProperties)
                {
                    elem = seleniumDriver.GetElementByMutlipleAttributes(locator.LocateValue, searchContext);
                }

                if (locator.LocateBy == eLocateBy.ByTagName)
                {
                    if (locator.EnableFriendlyLocator && FriendlyLocatorElement.DoesFriendlyLocatorListExist(friendlyLocatorElements))
                    {
                        By by = By.TagName(locator.LocateValue) as By;
                        elem = seleniumDriver.GetElementByFriendlyLocatorlist(friendlyLocatorElements, by);

                    }
                    if (searchContext is ShadowRoot)
                    {
                        elem = searchContext.FindElement(By.CssSelector(locator.LocateValue));
                    }

                    else
                    {
                        elem = searchContext.FindElement(By.TagName(locator.LocateValue));
                    }
                }
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                if (AlwaysReturn == true)
                {
                    elem = null;
                    locator.StatusError = ex.Message;
                    locator.LocateStatus = ElementLocator.eLocateStatus.Failed;
                    return elem;
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (AlwaysReturn == true)
                {
                    elem = null;
                    locator.StatusError = ex.Message;
                    locator.LocateStatus = ElementLocator.eLocateStatus.Failed;
                    return elem;
                }
                else
                {
                    throw ex;
                }
            }

            if (elem != null)
            {
                locator.LocateStatus = ElementLocator.eLocateStatus.Passed;
            }

            return elem;
        }

    }
}
