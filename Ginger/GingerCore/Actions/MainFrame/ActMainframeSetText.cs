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
using Amdocs.Ginger.Repository;
using GingerCore.Helpers;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Amdocs.Ginger.Common.InterfacesLib;
namespace GingerCore.Actions.MainFrame
{
    public class ActMainframeSetText : Act
    {
        public override string ActionDescription { get { return "Set Text Main Frame"; } }
        public override string ActionUserDescription { get { return "Set Text Main Frame"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action to Set text at a particular location in Mainframe Terminal");
        }

        public override string ActionEditPage { get { return "Mainframe.ActMainFrameSetTextEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return true; } }
        public override bool ValueConfigsNeeded { get { return true; } }

        public new static partial class Fields
        {
            public static string SendAfterSettingText = "SendAfterSettingText";
            public static string SetTextMode = "SetTextMode";
            public static string ReloadValue = "ReloadValue";
        }

        public enum eSetTextMode
        {
            SetSingleField,
            SetMultipleFields,
        }
        private eSetTextMode mSetTextMode = eSetTextMode.SetSingleField;

        public bool ReloadValue
        {
            get
            {
                bool value = true;
                bool.TryParse(GetOrCreateInputParam(nameof(ReloadValue), value.ToString()).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ReloadValue), value.ToString());
            }
        }

        public eSetTextMode SetTextMode
        {
            get
            {
                return (eSetTextMode)GetOrCreateInputParam<eSetTextMode>(nameof(SetTextMode), eSetTextMode.SetSingleField);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(SetTextMode), value.ToString());
            }
        }

        public bool SendAfterSettingText
        {
            get
            {
                bool value = true;
                bool.TryParse(GetOrCreateInputParam(nameof(SendAfterSettingText)).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(SendAfterSettingText), value.ToString());
            }
        }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                List<ePlatformType> mPf = new List<ePlatformType>();

                mPf.Add(ePlatformType.MainFrame);
                return mPf;
            }
        }
        public override String ActionType
        {
            get
            {
                return "Set Text Main Frame";
            }
        }


        public void LoadCaretValueList()
        {
            ObservableList<ActInputValue> mCaretValueList = new ObservableList<ActInputValue>();
            String LoadText = ValueForDriver;
            if (String.IsNullOrWhiteSpace(LoadText))
                return;
            XmlDocument XD = new XmlDocument();
            XD.LoadXml(LoadText);
            foreach (XmlNode xn in XD.ChildNodes)
            {
                if (xn.Name == "EditableFields")
                {
                    foreach (XmlNode xns in xn.ChildNodes)
                    {
                        ActInputValue aiv = new ActInputValue();
                        foreach (XmlAttribute XA in xns.Attributes)
                        {
                            if (XA.Name == "Caret")
                            {
                                aiv.Param = XA.Value;
                            }
                            else if (XA.Name == "Text")
                            {
                                aiv.Value = XA.Value;
                            }
                        }

                        if (CaretValueList.Any(av => av.Param == aiv.Param) || CaretValueList.Count() == 0)
                        {
                            mCaretValueList.Add(aiv);
                        }
                    }
                }
            }
            CaretValueList = mCaretValueList;
        }

        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> CaretValueList = new ObservableList<ActInputValue>();
    }
}
