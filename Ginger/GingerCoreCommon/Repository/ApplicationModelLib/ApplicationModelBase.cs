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
using Amdocs.Ginger.Common.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Amdocs.Ginger.Repository
{
    public abstract class ApplicationModelBase : RepositoryItemBase
    {


        private string mName = string.Empty;

        [IsSerializedForLocalRepository]
        public string Name //OperationName 
        {
            get
            {
                return mName;
            }
            set
            {
                if (mName != value)
                {
                    mName = value;
                    OnPropertyChanged(nameof(this.Name));
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }

        private string mDescription = string.Empty;
        [IsSerializedForLocalRepository]
        public string Description
        {
            get
            {
                return mDescription;
            }
            set
            {
                if (mDescription != value)
                {
                    mDescription = value;
                    OnPropertyChanged(nameof(this.Description));
                }
            }
        }

        public override string ItemName { get { return this.Name; } set { this.Name = value; } }

        private bool mSelected = false;

        public bool Selected { get { return mSelected; } set { if (mSelected != value) { mSelected = value; OnPropertyChanged(nameof(Selected)); } } }

        public override string GetNameForFileName()
        {
            return this.Name;
        }

        [IsSerializedForLocalRepository]
        public ObservableList<AppModelParameter> AppModelParameters = [];

        [IsSerializedForLocalRepository]
        public ObservableList<GlobalAppModelParameter> GlobalAppModelParameters = [];

        [IsSerializedForLocalRepository]
        public ObservableList<RepositoryItemKey> TagsKeys = [];

        public override bool FilterBy(eFilterBy filterType, object obj)
        {
            switch (filterType)
            {
                case eFilterBy.Tags:
                    foreach (RepositoryItemKey tagKey in TagsKeys)
                    {
                        Guid guid = ((List<Guid>)obj).FirstOrDefault(x => tagKey.Guid.Equals(x) == true);
                        if (!guid.Equals(Guid.Empty))
                            return true;
                    }
                    break;
            }
            return false;
        }

        RepositoryItemKey mTargetApplicationKey;
        [IsSerializedForLocalRepository]
        public RepositoryItemKey TargetApplicationKey
        {
            get
            {
                return mTargetApplicationKey;
            }
            set
            {
                RepositoryItemKey previousKey = mTargetApplicationKey;
                mTargetApplicationKey = value;
                if ((previousKey == null && value != null)
                    || ((previousKey != null && value != null) && (previousKey.Guid != value.Guid || previousKey.ItemName != value.ItemName)))//workaround to make show as modified only when really needed
                {
                    OnPropertyChanged(nameof(this.TargetApplicationKey));
                }
            }
        }

        #region Output Template
        [IsSerializedForLocalRepository]
        public ObservableList<ActReturnValue> ReturnValues = [];

        private bool mSupportSimulation;

        [IsSerializedForLocalRepository]
        public bool SupportSimulation
        {
            get
            { return mSupportSimulation; }
            set
            {
                if (mSupportSimulation != value)
                {
                    mSupportSimulation = value;
                    OnPropertyChanged(nameof(SupportSimulation));
                }
            }
        }
        #endregion

        public virtual List<string> GetModelListsToConfigsWithExecutionData()
        {
            return [];
        }

        public void SetModelConfigsWithExecutionData(object obj = null)
        {
            if (obj == null)
            {
                obj = this;
            }
            var properties = obj.GetType().GetMembers().Where(x => x.MemberType is MemberTypes.Property or MemberTypes.Field);
            foreach (MemberInfo mi in properties)
            {
                try
                {
                    if (mi.Name is "mName" or "Name" or "ItemName" or "RelativeFilePath" or "FilePath" or "ObjFileExt" or "ObjFolderName" or "ItemNameField") continue;

                    PropertyInfo PI = obj.GetType().GetProperty(mi.Name);
                    dynamic value = null;
                    if (mi.MemberType == MemberTypes.Property)
                        value = PI.GetValue(obj);
                    else if (mi.MemberType == MemberTypes.Field)
                        value = obj.GetType().GetField(mi.Name).GetValue(obj);

                    if (value != null)
                    {
                        if (value is string)
                        {
                            string valueString = (string)PI.GetValue(obj);
                            foreach (AppModelParameter AMP in AppModelParameters)
                            {
                                valueString = valueString.Replace(AMP.PlaceHolder, AMP.ExecutionValue);
                                PI.SetValue(obj, valueString);
                            }
                            foreach (GlobalAppModelParameter GAMP in GlobalAppModelParameters)
                            {
                                valueString = valueString.Replace(GAMP.PlaceHolder, GAMP.ExecutionValue);
                                PI.SetValue(obj, valueString);
                            }
                        }
                        else if (value is IObservableList && GetModelListsToConfigsWithExecutionData().Contains(mi.Name))
                        {
                            foreach (object o in value)
                            {
                                SetModelConfigsWithExecutionData(o);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"error in model execution data :{ex.Message} ", ex);
                }
            }
        }

        public void UpdateParamsPlaceholder(object item, List<string> placeHoldersToReplace, string newVarName)
        {
            var properties = item.GetType().GetMembers().Where(x => x.MemberType is MemberTypes.Property or MemberTypes.Field);
            foreach (MemberInfo mi in properties)
            {
                if (mi.Name is "ItemImageType" or "OptionalValuesString" or "Path" or "PlaceHolder" or "FileName" or "TagsKeys" or "AppModelParameters" or "ContainingFolderFullPath" or "mName" or "Name" or "ItemName" or "RelativeFilePath" or "FilePath" or "ObjFileExt" or "ObjFolderName" or "ItemNameField")
                {
                    continue;
                }

                PropertyInfo PI = item.GetType().GetProperty(mi.Name);
                dynamic value = null;
                if (mi.MemberType == MemberTypes.Property)
                    value = PI.GetValue(item);
                else if (mi.MemberType == MemberTypes.Field)
                    value = item.GetType().GetField(mi.Name).GetValue(item);

                if (value is not null and IObservableList)
                {
                    foreach (object o in value)
                        UpdateParamsPlaceholder(o, placeHoldersToReplace, newVarName);
                }
                else if (value is not null and string)
                {
                    try
                    {
                        string valueString = (string)PI.GetValue(item);
                        foreach (string palceHolder in placeHoldersToReplace)
                        {
                            bool notifyPropertyChanged = false;
                            if (valueString.Contains(palceHolder))
                                notifyPropertyChanged = true;
                            valueString = valueString.Replace(palceHolder, newVarName);
                            PI.SetValue(item, valueString);

                            if (notifyPropertyChanged)
                                ((dynamic)item).OnPropertyChanged(mi.Name);
                        }
                    }
                    catch (Exception) { }
                }
            }
        }

        public ObservableList<AppModelParameter> MergedParamsList
        {
            get
            {
                ObservableList<AppModelParameter> mergedList = [.. AppModelParameters, .. GlobalAppModelParameters];
                return mergedList;
            }
        }


        public enum eModelUsageUpdateType
        {
            SinglePart,
            MultiParts
        }

        public enum eModelParts
        {
            All,
            ReturnValues,
            Parameters
        }


        // !!!!!!!!!!!!!!!!!!!!!!!!!
        public static List<string> GetSoapSecurityHeaderContent(ref string txtBoxBodyContent)
        {
            List<string> SecuritryContent = [];
            StringBuilder soapHeaderContent = new StringBuilder();

            soapHeaderContent.Append("\t<soapenv:Header>\n");
            soapHeaderContent.Append("\t\t<wsse:Security xmlns:wsse=");
            soapHeaderContent.Append("\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"");
            soapHeaderContent.Append("\n\t\t xmlns:wsu=");
            soapHeaderContent.Append("\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\"");
            soapHeaderContent.Append(">\n");
            soapHeaderContent.Append("\t\t\t\t<wsse:UsernameToken wsu:Id=");
            soapHeaderContent.Append("\"UsernameToken-{GET_HASH_CODE}");
            soapHeaderContent.Append("\">\n\t\t\t\t<wsse:Username>");
            soapHeaderContent.Append("{WSSECUSERNAME}");
            soapHeaderContent.Append("</wsse:Username>\n\t\t\t\t<wsse:Password Type=");
            soapHeaderContent.Append("\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText\"");
            soapHeaderContent.Append(">");
            soapHeaderContent.Append("{WSSECPASSWORD}");
            soapHeaderContent.Append("</wsse:Password>\n\t\t\t\t<wsse:Nonce EncodingType=");
            soapHeaderContent.Append("\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary\"");
            soapHeaderContent.Append(">" + "{GET_HASHED_WSSECPASSWORD}" + "=");
            soapHeaderContent.Append("</wsse:Nonce>\n\t\t\t\t<wsu:Created>");
            soapHeaderContent.Append("{GETUTCTIMESTAMP}");
            soapHeaderContent.Append("</wsu:Created>\n\t\t\t</wsse:UsernameToken>\n\t\t</wsse:Security>\n\t</soapenv:Header>\n");
            SecuritryContent.Add(soapHeaderContent.ToString());
            SecuritryContent.Add("{WSSECUSERNAME}");
            SecuritryContent.Add("{WSSECPASSWORD}");
            SecuritryContent.Add("{GETUTCTIMESTAMP}");
            SecuritryContent.Add("{GET_HASHED_WSSECPASSWORD}");
            SecuritryContent.Add("{GET_HASH_CODE}");

            string wsSecuritySettings = SecuritryContent.ElementAt(0);
            string pattern1 = "<soapenv:Header>(.*?)</soapenv:Header>|<soapenv:Header/>|<(\\s)soapenv:Header(\\s)/>";
            string pattern2 = "<soapenv:Body>|<soapenv:Body/>";
            string pattern3 = "\t<soapenv:Header>(.*?)\t</soapenv:Header>|\t<soapenv:Header/>|\t<(\\s)soapenv:Header(\\s)/>|\t<soapenv:Header>";
            bool isPatrn1Exists = Regex.IsMatch(txtBoxBodyContent, pattern1);
            if (!isPatrn1Exists)
            {
                bool isPattrn3Exists = Regex.IsMatch(txtBoxBodyContent, pattern3);
                if (!isPattrn3Exists)
                {
                    bool isPatrn2Exists = Regex.IsMatch(txtBoxBodyContent, pattern2);
                    if (isPatrn2Exists)
                    {
                        txtBoxBodyContent = Regex.Replace(txtBoxBodyContent, pattern2, wsSecuritySettings + "<soapenv:Body>");
                    }
                }
            }
            else
            {
                txtBoxBodyContent = Regex.Replace(txtBoxBodyContent, pattern1, wsSecuritySettings);
            }
            return SecuritryContent;
        }
    }
}
