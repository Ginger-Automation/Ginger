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
using Amdocs.Ginger.Common.InterfacesLib;
namespace GingerCore.Actions
{
    public class ActDemoPage : ActWithoutDriver
    {
        public override string ActionDescription { get { return "Demo Page Example Action22"; } }
        
       public override string ActionUserDescription { get { return "Code Example for Data structure and binding"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you need to create new Action with the common data and binding method.");
        }

        public override string ActionEditPage { get { return "ActDemoPageEditPage"; } }

        //Developer to change it to true if he wants to use this action.
        public override bool IsSelectableAction { get { return false; } }

        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

        public ActDemoPage()
        {
            GetOrCreateInputParam(ActDemoPage.Fields.CheckBoxParam, "false");
        }

        public override string ActionType
        {
            get
            {
                return "Data structure example";
            }
        }

        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    AddAllPlatforms();
                }
                return mPlatforms;
            }
        }

        public enum eComboBoxDataValueType
        {
            [EnumValueDescription("Value 1")]
            Value1,
            [EnumValueDescription("Value 2")]
            Value2,
            [EnumValueDescription("Value 3")]
            Value3,
        }


        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> ActionGrid = new ObservableList<ActInputValue>();

        public override List<ObservableList<ActInputValue>> GetInputValueListForVEProcessing()
        {
            List<ObservableList<ActInputValue>> list = new List<ObservableList<ActInputValue>>();
            list.Add(ActionGrid);
            return list;
        }

        public new static partial class Fields
        {
            public static string TextBoxParamFile = "TextBoxParamFile";
            public static string TextBoxParamFolder = "TextBoxParamFolder";
            public static string TextBoxParamNoVE = "TextBoxParamNoVE";
            public static string TextBoxParamNoBrowser = "TextBoxParamNoBrowser";
            public static string RegularTextBox = "RegularTextBox";
            public static string CheckBoxParam = "CheckBoxParam";
            public static string RadioParam = "RadioParam";
            public static string UCRadioParam = "UCRadioParam";
            public static string ComboBoxDataValueType = "ComboBoxDataValueType";
            public static string ComboBoxDataValueTypeWithVE = "ComboBoxDataValueTypeWithVE";
        }

        public enum eRadioButtonValueType
        {
            [EnumValueDescription("Yes value")]
            Yes,
            [EnumValueDescription("No Value")]
            No,
            [EnumValueDescription("Maybe Value")]
            Maybe
        }

        public eComboBoxDataValueType ComboBoxDataValueType_Value
        {
            get
            {
                eComboBoxDataValueType eVal = eComboBoxDataValueType.Value1;
                if (Enum.TryParse<eComboBoxDataValueType>(GetInputParamValue(Fields.ComboBoxDataValueType), out eVal))
                    return eVal;
                else
                    return eComboBoxDataValueType.Value1;  //default value          
            }
        }

        public bool CheckBoxParam_Value
        {
            get
            {
                bool returnValue = true;
                if (Boolean.TryParse((GetInputParamValue(ActDemoPage.Fields.CheckBoxParam)), out returnValue))
                {
                    return returnValue;
                }
                else
                    return false;
            }
        }

        public int RegularTextBoxParam_intValue
        {
            get
            {
                int returnValue = 0;
                if (int.TryParse((GetInputParamValue(ActDemoPage.Fields.RegularTextBox)), out returnValue))
                {
                    return returnValue;
                }
                else
                    return returnValue;
            }
        }

        public override void Execute()
        {
            //CheckBox Boolean Positive example
            AddOrUpdateInputParamValue(ActDemoPage.Fields.CheckBoxParam, "true");
            bool b1 = CheckBoxParam_Value;

            //CheckBox Boolean Negative example
            AddOrUpdateInputParamValue(ActDemoPage.Fields.CheckBoxParam, "true1");
            bool b2 = CheckBoxParam_Value;

            //UCcomboboxexample 
            string c1 = GetInputParamValue(ActDemoPage.Fields.ComboBoxDataValueType);

            //UCcomboboxexample 
            string c2 = GetInputParamValue(ActDemoPage.Fields.ComboBoxDataValueTypeWithVE);

            //TextBox int Positive example 
            AddOrUpdateInputParamValue(ActDemoPage.Fields.RegularTextBox, "11");
            int a1 = RegularTextBoxParam_intValue;

            //TextBox int Negative example
            AddOrUpdateInputParamValue(ActDemoPage.Fields.RegularTextBox, "aa11");
            int a2 = RegularTextBoxParam_intValue;
            
            if (ComboBoxDataValueType_Value == eComboBoxDataValueType.Value2)
            {
                string tem1 = ComboBoxDataValueType_Value.ToString();
            }

            if (GetInputParamValue(Fields.ComboBoxDataValueType) == (eComboBoxDataValueType.Value2.ToString()))
            {
                string tem2 = ComboBoxDataValueType_Value.ToString();
            }
        }
    }
}
