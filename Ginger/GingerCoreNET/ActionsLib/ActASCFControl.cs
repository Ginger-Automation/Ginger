#region License
/*
Copyright © 2014-2026 European Support Limited

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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET;
using GingerCore.Actions.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GingerCore.Actions
{
    // Action class for ASCF control
    public class ActASCFControl : Act, IObsoleteAction
    {
        public override string ActionDescription { get { return "ASCF Control Action"; } }
        public override string ActionUserDescription { get { return "ASCF Control Action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
        }

        public override string ActionEditPage { get { return "ActASCFControlEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return true; } }
        public override bool ValueConfigsNeeded { get { return true; } }

        bool IObsoleteAction.IsObsoleteForPlatform(ePlatformType platform)
        {
            if (platform == ePlatformType.ASCF)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        Type IObsoleteAction.TargetAction()
        {
            return typeof(ActUIElement);
        }

        String IObsoleteAction.TargetActionTypeName()
        {
            ActUIElement actUIElement = new ActUIElement();
            return actUIElement.ActionDescription;
        }

        ePlatformType IObsoleteAction.GetTargetPlatform()
        {
            return ePlatformType.Java;
        }

        Act IObsoleteAction.GetNewAction()
        {
            AutoMapper.MapperConfiguration mapConfig = new AutoMapper.MapperConfiguration(cfg => { cfg.CreateMap<Act, ActUIElement>(); });
            ActUIElement newAct = mapConfig.CreateMapper().Map<Act, ActUIElement>(this);
            newAct.ElementType = eElementType.Unknown;

            newAct.ElementAction = this.ControlAction switch
            {
                eControlAction.Click => ActUIElement.eElementAction.Click,
                eControlAction.GetValue => ActUIElement.eElementAction.GetValue,
                eControlAction.SetValue => ActUIElement.eElementAction.SetValue,
                eControlAction.Hover => ActUIElement.eElementAction.Hover,
                eControlAction.IsVisible => ActUIElement.eElementAction.IsVisible,
                eControlAction.IsEnabled => ActUIElement.eElementAction.IsEnabled,
                eControlAction.Collapse => ActUIElement.eElementAction.Collapse,
                eControlAction.Expand => ActUIElement.eElementAction.Expand,
                eControlAction.SetFocus => ActUIElement.eElementAction.SetFocus,
                _ => ActUIElement.eElementAction.Unknown,
            };
            MapASCFActionItems(this.LocateValue, newAct);

            return newAct;
        }

        public bool checkWhetherTreeElement(String locateValue, ActUIElement newAct)
        {
            int braceA = locateValue.IndexOf("[");
            int braceB = locateValue.IndexOf("]");

            if ((braceA != -1 && braceB != -1))
            {
                string path = locateValue.Substring(braceA, braceB - braceA + 1);
                if (!path.Contains(':'))
                {
                    if (path.IndexOf("/") != -1)
                    {
                        locateValue = locateValue.Replace(path, string.Empty)[(locateValue.LastIndexOf(":") + 1)..];
                        newAct.ElementLocateValue = locateValue;
                        newAct.Value = this.Value;
                        newAct.ElementType = eElementType.Unknown;

                        foreach (ActUIElement.eElementAction x in Enum.GetValues(typeof(ActUIElement.eElementAction)))
                        {
                            if (this.ControlAction.ToString().Equals(x.ToString()))
                            {
                                newAct.ElementAction = x;
                            }
                            else
                            {
                                // if old action is not supported by ACTUIElement, for example, GetControlProperty
                                newAct.ElementAction = ActUIElement.eElementAction.Unknown;
                            }
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        public void MapASCFActionItems(String locateValue, ActUIElement newAct)
        {
            newAct.ElementLocateBy = LocateBy switch
            {
                eLocateBy.ByXPath => eLocateBy.ByXPath,
                eLocateBy.ByName => eLocateBy.ByName,
                eLocateBy.ByValue => eLocateBy.ByValue,
                eLocateBy.ByText => eLocateBy.ByText,
                eLocateBy.ByTitle => eLocateBy.ByTitle,
                _ => eLocateBy.Unknown,
            };
            if (!String.IsNullOrEmpty(locateValue))
            {
                // check if the current action is a garbage action or not
                if (!string.IsNullOrEmpty(this.LocateValue) && this.LocateValue.Contains("com.amdocs.crm.workspace.OpenWindows:OpenedWindowsTree"))
                {
                    newAct.Active = false;
                    newAct.Description = "Please remove this action - " + this.Description;
                    return;
                }

                int slash = locateValue.IndexOf("/");
                int colon = locateValue.IndexOf(":");
                if (slash == -1)
                {

                    newAct.Value = this.Value;
                    if (locateValue.Count(x => x == ':') > 1)
                    {
                        locateValue = locateValue[(locateValue.LastIndexOf(':') + 1)..];
                    }
                    else
                    {
                        locateValue = locateValue[(colon + 1)..];
                    }

                    newAct.ElementLocateValue = locateValue;

                    newAct.ElementType = eElementType.Unknown;

                    foreach (ActUIElement.eElementAction x in Enum.GetValues(typeof(ActUIElement.eElementAction)))
                    {
                        if (this.ControlAction.ToString().Equals(x.ToString()))
                        {
                            newAct.ElementAction = x;
                            return;
                        }
                        else
                        {
                            // if old action is not supported by ACTUIElement, for example, GetControlProperty
                            newAct.ElementAction = ActUIElement.eElementAction.Unknown;
                        }
                    }
                }
                else
                {
                    if (checkWhetherTreeElement(locateValue, newAct))
                    {
                        return;
                    }
                    newAct.ElementType = eElementType.Table;
                    newAct.ElementAction = ActUIElement.eElementAction.TableCellAction;
                    newAct.ElementLocateBy = eLocateBy.ByContainerName;
                    // Parse the Column Selector 
                    int col = locateValue.IndexOf(":", slash);

                    string colSelector = (slash == locateValue.Length - 1) ? "" : locateValue[(slash + 1)..^1].Replace(@":", string.Empty);
                    colSelector = colSelector.Trim();
                    int colNum = 0;
                    if (colSelector.All(char.IsDigit))
                    {
                        colNum = Convert.ToInt32(colSelector);
                        newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.ColSelectorValue, ActUIElement.eTableElementRunColSelectorValue.ColNum.ToString());
                        newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateColTitle, colNum.ToString());
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(colSelector))
                        {
                            if (colSelector.IndexOf("[") == -1)
                            {
                                newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.ColSelectorValue, ActUIElement.eTableElementRunColSelectorValue.ColTitle.ToString());
                                newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateColTitle, colSelector);
                            }
                            else
                            {
                                if (colSelector.StartsWith("[name="))
                                {
                                    newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.ColSelectorValue, ActUIElement.eTableElementRunColSelectorValue.ColName.ToString());
                                }
                                else if (colSelector.StartsWith("[title="))
                                {
                                    newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.ColSelectorValue, ActUIElement.eTableElementRunColSelectorValue.ColTitle.ToString());
                                }
                                else if (colSelector.StartsWith("[number="))
                                {
                                    newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.ColSelectorValue, ActUIElement.eTableElementRunColSelectorValue.ColNum.ToString());
                                }
                                newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateColTitle, colSelector.Split('\"')[1]);
                            }
                        }
                    }

                    // Parse the Row Selector
                    string rowSelector = locateValue[(col + 1)..];
                    if (!String.IsNullOrEmpty(rowSelector))
                    {
                        if (rowSelector.StartsWith("[number="))
                        {
                            string row = rowSelector.Split('\"')[1];
                            if ("random".Equals(row))
                            {
                                newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowType, "Any Row");
                            }
                            else
                            {
                                newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowType, "Row Number");
                                newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowValue, row);
                            }
                        }
                        else if (rowSelector.All(char.IsDigit))
                        {
                            int rowNum = Convert.ToInt32(rowSelector);
                            newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowType, "Row Number");
                            newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowValue, rowNum.ToString());
                        }
                        else if (rowSelector.Equals("random"))
                        {
                            newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowType, "Any Row");
                        }
                        else
                        {
                            if (rowSelector == string.Empty)
                            {
                                throw new Exception("Malformed row selector");
                            }
                            string[] conditions = rowSelector.Split(new string[] { "]]" }, StringSplitOptions.None);
                            foreach (string condition in conditions)
                            {
                                string cond = !String.IsNullOrEmpty(condition) ? condition.Trim() : "";
                                if (String.IsNullOrEmpty(cond.ToString()) || cond[0] == ']')
                                {
                                    break;
                                }
                                if (cond.IndexOf(":") != -1)
                                {
                                    colon = cond.IndexOf(":", 1);
                                    string rowColSelector = cond.Substring(cond.LastIndexOf("[", colon) + 1, colon);
                                    string rowColSelectorValue = rowColSelector.Split('\"')[1];
                                    if (rowColSelector.Contains("\""))
                                    {
                                        rowColSelector = rowColSelector[..rowColSelector.IndexOf("=")];
                                        if (rowColSelector.Equals("name"))
                                        {
                                            newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.WhereColSelector, ActUIElement.eTableElementRunColSelectorValue.ColName.ToString());
                                        }
                                        else if (rowColSelector.Equals("title"))
                                        {
                                            newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.WhereColSelector, ActUIElement.eTableElementRunColSelectorValue.ColTitle.ToString());
                                        }
                                        else if (rowColSelector.Equals("number"))
                                        {
                                            newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.WhereColSelector, ActUIElement.eTableElementRunColSelectorValue.ColNum.ToString());
                                        }
                                    }
                                    else
                                    {
                                        newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.WhereColSelector, ActUIElement.eTableElementRunColSelectorValue.ColTitle.ToString());
                                    }
                                    newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.WhereColumnTitle, rowColSelectorValue);

                                    // Determine the Property Selector
                                    string rowColProperty = cond[(colon + 1)..];
                                    string[] properties = rowColProperty.Split(new string[] { "\\[" }, StringSplitOptions.None);
                                    for (int i = 0; i < properties.Length; i++)
                                    {
                                        if (i > 1)
                                        {

                                        }
                                        string p = new string(properties[i].Split('\"')[0].Where(c => char.IsLetter(c)).ToArray());
                                        string pv = properties[i].Split('\"')[1];
                                        if (pv.Equals("<empty>"))
                                        {
                                            pv = "";
                                        }

                                        string o = properties[i].Split('\"')[0][(p.Length + 1)..];

                                        newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.WhereProperty, p);
                                        newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.WhereOperator,
                                            "=".Equals(o) ? ActUIElement.eTableElementRunColOperator.Equals.ToString() :
                                            "!=".Equals(o) ? ActUIElement.eTableElementRunColOperator.NotEquals.ToString() :
                                            "=~".Equals(o) ? ActUIElement.eTableElementRunColOperator.Contains.ToString() :
                                            "!~".Equals(o) ? ActUIElement.eTableElementRunColOperator.NotContains.ToString() :
                                            "=^".Equals(o) ? ActUIElement.eTableElementRunColOperator.StartsWith.ToString() :
                                            "!^".Equals(o) ? ActUIElement.eTableElementRunColOperator.NotStartsWith.ToString() :
                                            "=$".Equals(o) ? ActUIElement.eTableElementRunColOperator.EndsWith.ToString() :
                                            "!$".Equals(o) ? ActUIElement.eTableElementRunColOperator.NotEndsWith.ToString() : "");
                                        newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.WhereColumnValue, pv);
                                    }
                                }
                            }
                            newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowType, "Where");
                        }
                    }
                    foreach (ActUIElement.eElementAction x in Enum.GetValues(typeof(ActUIElement.eElementAction)))
                    {
                        if (this.ControlAction.ToString().Equals(x.ToString()))
                        {
                            newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.ControlAction, x.ToString());
                            newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.ControlActionValue, this.Value);
                        }
                    }
                    newAct.ElementLocateValue = locateValue.Split('/')[0][(locateValue.Split('/')[0].LastIndexOf(":") + 1)..];
                }
            }
        }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.ASCF);
                }
                return mPlatforms;
            }
        }

        public enum eControlAction
        {
            [EnumValueDescription("Set Value")]
            SetValue = 1,
            [EnumValueDescription("Get Value")]
            GetValue = 2, // TODO: not supported remove
            Click = 3,
            Hover = 4,
            [EnumValueDescription("Is Visible")]
            IsVisible = 5,
            [EnumValueDescription("Is Enabled")]
            IsEnabled = 6,
            [EnumValueDescription("Get Width")]
            GetWidth = 7,
            [EnumValueDescription("Get Height")]
            GetHeight = 8,
            [EnumValueDescription("Get Style")]
            GetStyle = 9,
            Collapse = 10,
            Expand = 11,
            [EnumValueDescription("Set Focus")]
            SetFocus = 12,
            [EnumValueDescription("Set Visible")]
            SetVisible = 13,
            [EnumValueDescription("Set Window State")]
            SetWindowState = 15,
            [EnumValueDescription("Get Control Property")]
            GetControlProperty = 16,
            [EnumValueDescription("Invoke Script")]
            InvokeScript = 17,
            [EnumValueDescription("Send Keystrokes")]
            KeyType = 18
        }

        public enum eControlProperty
        {
            NA = 0,
            Value = 1,
            Text = 2,
            Type = 3,
            Enabled = 4,
            Visible = 5,
            List = 6,
            HTML = 7,
            DateTimeValue = 8,
            ToolTip = 9
        }

        public eControlAction ControlAction
        {
            get
            {
                return GetOrCreateInputParam<eControlAction>(nameof(ControlAction), eControlAction.SetValue);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ControlAction), value.ToString());
            }
        }

        public eControlProperty ControlProperty
        {
            get
            {
                return GetOrCreateInputParam<eControlProperty>(nameof(ControlProperty), eControlProperty.NA);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ControlProperty), value.ToString());
            }
        }

        public bool WaitForIdle { get; set; }


        public override String ToString()
        {
            return "ASCFControl - " + ControlAction;
        }

        public override String ActionType
        {
            get
            {
                return "ASCFControl: " + ControlAction.ToString();
            }
        }

        public override eImageType Image { get { return eImageType.Java; } }
    }
}
