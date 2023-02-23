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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using GingerCore.Actions;
using GingerCore.Actions.Common;

namespace Ginger.Actions
{
    public partial class UIAutomation 
    {
        public ObservableList<Act> ElementLocators = new ObservableList<Act>();
        public ObservableList<Act> ParentLocators = new ObservableList<Act>();
        public static ObservableList<Act> PriorParent = new ObservableList<Act>();
        public static ObservableList<Act> CurrentParent = new ObservableList<Act>();

        public void CreateLocatorList(ucGrid LocatorsGrid, ObservableList<Act> Locators)
        {
            string[] lstMultiLocVals;
                lstMultiLocVals = Ginger.BusinessFlowWindows.EditLocatorsWindow.sMultiLocatorVals.Split('|'); 
            
            if (lstMultiLocVals.Length >= 1)
            {
                if (lstMultiLocVals.Length == 1 && lstMultiLocVals[0].ToString() == "") { Locators.Add(new ActDummy() { LocateBy = eLocateBy.ByLocalizedControlType, LocateValue = "edit" }); }
                else 
                {
                    foreach (string s in lstMultiLocVals)
                    {
                        string[] ls = s.Split(':');
                        if (ls.Length > 0)
                        {
                            Locators.Add(new ActDummy() { LocateBy = getLocatorTypeByString(ls[0].ToString()), LocateValue = ls[1].ToString() });
                        }
                    } 
                }
                
            }
            if (Locators.Count == 0) Locators.Add(new ActDummy() { LocateBy = eLocateBy.ByLocalizedControlType, LocateValue = "edit" });
            LocatorsGrid.DataSourceList = Locators;
        }
        public string getSelectedLocators(ObservableList<Act> Locators)
        {
            string strLoc = "";
            foreach (ActDummy a in Locators)
            {
                if (strLoc == "") { strLoc = strLoc + a.LocateBy.ToString() + ":" + a.LocateValue.ToString(); }
                else { strLoc = strLoc + "|" + a.LocateBy.ToString() + ":" + a.LocateValue.ToString(); }
            }
            return strLoc;       
        }
        
        public ObservableList<Act> CreateParentLocators(Act a)
        {
            ObservableList<Act> olParLoc = new ObservableList<Act>();
            //TODO make multi properties object oriented
            if (a.LocateValue.IndexOf('#') >= 0)
            {
                string[] lstMultiLocVals;
                string[] lstParentLocator = a.LocateValue.Split('#');
                if (lstParentLocator.Length > 1)
                {
                    lstMultiLocVals = lstParentLocator[1].Split('|');
                    if (lstMultiLocVals.Length >= 1)
                    {
                        foreach (string s in lstMultiLocVals)
                        {
                            string[] ls = s.Split(':');
                            if (ls.Length > 0)
                            {
                                olParLoc.Add(new ActDummy() { LocateBy = getLocatorTypeByString(ls[0].ToString()), LocateValue = ls[1].ToString() });
                            }
                        }
                    }
                }
                else
                { olParLoc.Add(new ActDummy() { LocateBy = eLocateBy.NA, LocateValue = "RootWindow" }); }
            }
            else
            { olParLoc.Add(new ActDummy() { LocateBy = eLocateBy.NA, LocateValue = "RootWindow" }); }
            ParentLocators = olParLoc;
            return olParLoc;
        }

        public bool getActParentLevel(Act a)
        {
            if (a.LocateValue == null) return false;
            if (a.LocateValue.ToString().IndexOf("#RootWindow") >= 0) return true;
            else return false;
        }

        public eLocateBy getLocatorTypeByString(string sLocType)
        {
            eLocateBy eL = eLocateBy.NA;
            switch (sLocType)
            {
                case "NA":
                    eL = eLocateBy.NA;
                    break;
                case "ByID":
                    eL = eLocateBy.ByID;
                    break;
                case "ByName":
                    eL = eLocateBy.ByName;
                    break;
                case "ByCSS":
                    eL = eLocateBy.ByCSS;
                    break;
                case "ByXPath":
                    eL = eLocateBy.ByXPath;
                    break;
                case "ByXY":
                    eL = eLocateBy.ByXY;
                    break;
                case "ByHref":
                    eL = eLocateBy.ByHref;
                    break;
                case "ByLinkText":
                    eL = eLocateBy.ByLinkText;
                    break;
                case "ByValue":
                    eL = eLocateBy.ByValue;
                    break;
                case "ByIndex":
                    eL = eLocateBy.ByIndex;
                    break;
                case "ByClassName":
                    eL = eLocateBy.ByClassName;
                    break;
                case "ByAutomationID":
                    eL = eLocateBy.ByAutomationID;
                    break;
                case "ByLocalizedControlType":
                    eL = eLocateBy.ByLocalizedControlType;
                    break;
                case "ByBoundingRectangle":
                    eL = eLocateBy.ByBoundingRectangle;
                    break;

            }
            return eL;
        }
    }
}
