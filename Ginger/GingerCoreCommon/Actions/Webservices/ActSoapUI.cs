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
using GingerCore.Actions.WebServices;

using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Enums;

namespace GingerCore.Actions
{
    public class ActSoapUI : Act
    {
        public override string ActionDescription { get { return "SoapUI Wrapper Action"; } }
        public override string ActionUserDescription { get { return "Run SoapUI commands on Dos/Unix system"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you need to run SoapUI Project XML using the SoapUI test runner." + Environment.NewLine + Environment.NewLine + "This action contains list of options which will allow you to run simple or complicated test using the generated soapUI project XML." + Environment.NewLine + Environment.NewLine + "Prerequisite:" + Environment.NewLine + "1.) you should have SoapUI 5.0.0 or above installation folder on your Machine." + Environment.NewLine + "2.) SoapUI ProjectXML." + Environment.NewLine + "3.) Values/Properties in case you need to add some on the top of the existing data under the project XML." + Environment.NewLine + "4.) String for validation if needs to verify the output." + Environment.NewLine + Environment.NewLine + "Validation: The action will pass if :" + Environment.NewLine + "1.)ignore validation checkbox is unchecked and No step came back with Failed/unknown status." + Environment.NewLine + "2.)The Ignore Validation checkbox is checked and The entered Validation string for output has been found in the response" + Environment.NewLine + Environment.NewLine + "Validation: The action will be failed if :" + Environment.NewLine + "1.)Ignore Validation is unchecked and at lease one step came back with Failed status." + Environment.NewLine + "2.)Ignore Validation checkbox is checked and The entered Validation string has not been found in the response for at lease one step");
        }

        public override string ActionEditPage { get { return "WebServices.ActSoapUIEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

        public override eImageType Image { get { return eImageType.Exchange; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.WebServices);
                }
                return mPlatforms;
            }
        }

        public new static partial class Fields
        {
            public static string XMLFile = "XMLFile";
            public static string ImportFile = "ImportFile";
            public static string IgnoreValidation = "IgnoreValidation";
            public static string TestCase = "TestCase";
            public static string TestSuite = "TestSuite";
            public static string TestCasePropertiesRequiered = "TestCasePropertiesRequiered";
            public static string TestCasePropertiesRequieredControlEnabled = "TestCasePropertiesRequieredControlEnabled";
            public static string PropertiesOrPlaceHolders = "PropertiesOrPlaceHolders";
            public static string Domain = "Domain";
            public static string EndPoint = "EndPoint";
            public static string HostPort = "HostPort";
            public static string Username = "Username";
            public static string Password = "Password";
            public static string PasswordWSS = "PasswordWSS";
            public static string PasswordWSSType = "PasswordWSSType";
            public static string UIrelated = "UIrelated";
            public static string isActionExecuted = "isActionExecuted";
            public static string AddXMLResponse = "AddXMLResponse";
        }

        public bool PropertiesRequieredControlEnabled_Value
        {
            get
            {
                bool returnValue = true;
                if (Boolean.TryParse((GetInputParamValue(ActSoapUI.Fields.TestCasePropertiesRequieredControlEnabled)), out returnValue))
                {
                    return returnValue;
                }
                else
                    return false;
            }
        }

        public bool AddXMLResponse_Value
        {
            get
            {
                bool returnValue = true;
                if (Boolean.TryParse((GetInputParamValue(ActSoapUI.Fields.AddXMLResponse)), out returnValue))
                {
                    return returnValue;
                }
                else
                    return false;
            }
        }

        [IsSerializedForLocalRepository]
        public ObservableList<ActSoapUiInputValue> AllProperties = new ObservableList<ActSoapUiInputValue>();

        [IsSerializedForLocalRepository]
        public ObservableList<ActSoapUiInputValue> TempProperties = new ObservableList<ActSoapUiInputValue>();

        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> SystemProperties = new ObservableList<ActInputValue>();
        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> GlobalProperties = new ObservableList<ActInputValue>();
        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> ProjectProperties = new ObservableList<ActInputValue>();

        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> TestCaseProperties = new ObservableList<ActInputValue>();

        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> ProjectInnerProperties = new ObservableList<ActInputValue>();

        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> TestSuiteProperties = new ObservableList<ActInputValue>();

        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> TestStepProperties = new ObservableList<ActInputValue>();

        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> TestSuitePlaceHolder = new ObservableList<ActInputValue>();
        public override List<ObservableList<ActInputValue>> GetInputValueListForVEProcessing()
        {
            List<ObservableList<ActInputValue>> list = new List<ObservableList<ActInputValue>>();
            list.Add(SystemProperties);
            list.Add(GlobalProperties);
            list.Add(ProjectProperties);
            list.Add(TestCaseProperties);
            list.Add(TestSuiteProperties);
            list.Add(TestStepProperties);
            list.Add(TestSuitePlaceHolder);
            list.Add(ProjectInnerProperties);
            // convert ObservableList<ActSoapUiInputValue> to ObservableList<ActInputValue>
            list.Add(ConvertActSoapUiToActInputValue(AllProperties));
            return list;
        }

        private ObservableList<ActInputValue> ConvertActSoapUiToActInputValue(ObservableList<ActSoapUiInputValue> list)
        {
            ObservableList<ActInputValue> obList = new ObservableList<ActInputValue>();
            foreach (var item in list)
            {
                obList.Add(item);
            }
            return obList;
        }
        public enum ePasswordWSSType
        {
            [EnumValueDescription("Text")]
            Text,
            [EnumValueDescription("Digest")]
            Digest
        }

        public override String ActionType
        {
            get
            {
                return "SoapUI Test Runner";
            }
        }

        private string mLastExecutionFolderPath = string.Empty;
        public string LastExecutionFolderPath
        {
            get
            {
                return mLastExecutionFolderPath;
            }
            set
            {
                mLastExecutionFolderPath = value;
                OnPropertyChanged(Fields.isActionExecuted);
            }
        }

        public bool isActionExecuted
        {
            get
            {
                return (System.IO.Directory.Exists(LastExecutionFolderPath));
            }
        }
    }
}
