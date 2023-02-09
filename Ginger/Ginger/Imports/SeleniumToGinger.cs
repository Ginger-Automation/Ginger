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

using System;
using System.Collections.Generic;
using System.Linq;
using Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using HtmlAgilityPack;
using GingerCore.GeneralLib;
using GingerCore.Actions.Common;
using Amdocs.Ginger.Common.UIElement;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;

namespace Ginger.Import
{
    class SeleniumToGinger
    {
        public static BusinessFlow ConvertSeleniumScript(string FileName)
        {
            //TODO: move code from here to converter/import class
            var doc = new HtmlDocument();
            Activity result = new Activity() { Active = true };
            
            BusinessFlow bf = new BusinessFlow("");
            ePlatformType actionsPlatform = ePlatformType.Web;
            try
            {
                //get the required platform for actions                
                string selectedPlatform = "";
                if (InputBoxWindow.OpenDialog("Required Platform", "Required platform (set 'Web' or 'Mobile'):", ref selectedPlatform))
                {
                    if (selectedPlatform.Trim().ToUpper() == "MOBILE")
                        actionsPlatform = ePlatformType.Mobile;
                }

                doc.Load(FileName);

                result.ActivityName = doc.DocumentNode.Descendants("title").FirstOrDefault().InnerText;
                bf.Name = doc.DocumentNode.Descendants("title").FirstOrDefault().InnerText;

                List<HtmlNode> rows = doc.DocumentNode.Descendants("tbody").FirstOrDefault().Descendants()
                    .Where(o => o.Name.StartsWith("tr")).ToList();
                bf.Activities.Add(result);
                string action = "";
                string locatevalue = "";
                string value = "";
                // string locby = "";
                ActGenElement.eGenElementAction GenAction;
                eLocateBy locType = eLocateBy.ByXPath;

                foreach (HtmlNode row in rows)
                {
                    if (row.Descendants("title").Count() > 0)
                    {
                        if (bf != null)
                        {
                            WorkSpace.Instance.SolutionRepository.AddRepositoryItem(bf);                            
                        }
                        bf = new BusinessFlow(row.Descendants("title").FirstOrDefault().InnerText);
                        result = new Activity() { Active = true };
                        result.ActivityName = row.Descendants("title").FirstOrDefault().InnerText;
                        bf.Activities.Add(result);
                    }
                    else
                    {
                        action = row.Descendants("td").ToList()[0].InnerText;
                        locatevalue = row.Descendants("td").ToList()[1].InnerText.Replace("&amp;", "&").Replace("&gt;", ">");
                        value = row.Descendants("td").ToList()[2].InnerText;

                        if (locatevalue.Trim().IndexOf("id=", StringComparison.CurrentCultureIgnoreCase) == 0
                            || locatevalue.Trim().IndexOf("id:=", StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            locType = eLocateBy.ByID;
                            locatevalue = locatevalue.Replace("id=", "").Replace("id:=", ""); ;
                        }

                        if (locatevalue.Trim().IndexOf("link=", StringComparison.CurrentCultureIgnoreCase) == 0
                            || locatevalue.Trim().IndexOf("LinkText:=", StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            locType = eLocateBy.ByLinkText;
                            locatevalue = locatevalue.Replace("link=", "").Replace("LinkText:=", "");
                        }
                        if (locatevalue.Trim().IndexOf("css=", StringComparison.CurrentCultureIgnoreCase) == 0
                            || locatevalue.Trim().IndexOf("cssselector:=", StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            locType = eLocateBy.ByCSS;
                            locatevalue = locatevalue.Replace("css=", "").Replace("cssselector:=", "");
                        }
                        if (locatevalue.Trim().IndexOf("/") == 0 || locatevalue.Trim().IndexOf("xpath", StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            locType = eLocateBy.ByXPath;
                            locatevalue = locatevalue.Replace("xpath=", "").Replace("xpath:=", "").Replace("xpath:", "").Trim();
                        }

                        switch (action.ToUpper().Trim())
                        {
                            case "OPEN":
                                GenAction = ActGenElement.eGenElementAction.GotoURL;
                                locType = eLocateBy.NA;
                                locatevalue = "";
                                break;
                            case "FCOMMONLAUNCHENVIRONMENT":
                                GenAction = ActGenElement.eGenElementAction.GotoURL;
                                locType = eLocateBy.NA;
                                value = locatevalue;
                                locatevalue = "";
                                break;
                            case "TYPE":
                            case "FCOMMONSETVALUEEDITBOX":
                                GenAction = ActGenElement.eGenElementAction.SetValue;
                                break;
                            case "SELECT":
                            case "FCOMMONSELECTIONOPTIONFROMLIST":
                                GenAction = ActGenElement.eGenElementAction.SelectFromDropDown;
                                break;
                            case "FCOMMONJAVASCRIPTCLICK":
                                GenAction = ActGenElement.eGenElementAction.Click;
                                break;
                            case "CLICKANDWAIT":
                            case "CLICK":
                            default:
                                GenAction = ActGenElement.eGenElementAction.Click;
                                break;
                        }

                        result.Acts.Add(new ActGenElement() { Active = true, Description = GenAction.ToString(), LocateBy = locType, LocateValue = locatevalue, GenElementAction = GenAction, Value = value, Platform = actionsPlatform });
                    }
                }
                if (bf != null)
                {
                    return bf;                                       
                }
            }
            catch (Exception)
            {
                Reporter.ToUser(eUserMsgKey.ImportSeleniumScriptError);
            }
            return null;
        }
    }
}