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

        public override string GetNameForFileName()
        {
            return this.Name;
        }

        [IsSerializedForLocalRepository]
        public ObservableList<AppModelParameter> AppModelParameters = new ObservableList<AppModelParameter>();

        [IsSerializedForLocalRepository]
        public ObservableList<GlobalAppModelParameter> GlobalAppModelParameters = new ObservableList<GlobalAppModelParameter>();

        [IsSerializedForLocalRepository]
        public ObservableList<RepositoryItemKey> TagsKeys = new ObservableList<RepositoryItemKey>();

        public override bool FilterBy(eFilterBy filterType, object obj)
        {
            switch (filterType)
            {
                case eFilterBy.Tags:
                    foreach (RepositoryItemKey tagKey in TagsKeys)
                    {
                        Guid guid = ((List<Guid>)obj).Where(x => tagKey.Guid.Equals(x) == true).FirstOrDefault();
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
        public ObservableList<ActReturnValue> ReturnValues = new ObservableList<ActReturnValue>();

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
            return new List<string>();
        }

        public void SetModelConfigsWithExecutionData(object obj=null)
        {
            if (obj == null)
            {
                obj = this;
            }
            var properties = obj.GetType().GetMembers().Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field);
            foreach (MemberInfo mi in properties)
            {
                try
                {
                    if (mi.Name == "mName" || mi.Name == "Name" || mi.Name == "ItemName" || mi.Name == "RelativeFilePath" || mi.Name == "FilePath" || mi.Name == "ObjFileExt" || mi.Name == "ObjFolderName" || mi.Name == "ItemNameField") continue;

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
                        else if(value is IObservableList && GetModelListsToConfigsWithExecutionData().Contains(mi.Name))
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
                }
            }
        }

        public void UpdateParamsPlaceholder(object item, List<string> placeHoldersToReplace, string newVarName)
        {
            var properties = item.GetType().GetMembers().Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field);
            foreach (MemberInfo mi in properties)
            {
                    if (mi.Name == "ItemImageType" || mi.Name == "OptionalValuesString" || mi.Name == "Path" || mi.Name == "PlaceHolder" || mi.Name == "FileName" || mi.Name == "TagsKeys" || mi.Name == "AppModelParameters" || mi.Name == "ContainingFolderFullPath" || mi.Name == "mName" || mi.Name == "Name" || mi.Name == "ItemName" || mi.Name == "RelativeFilePath" || mi.Name == "FilePath" || mi.Name == "ObjFileExt" || mi.Name == "ObjFolderName" || mi.Name == "ItemNameField")
                    {
                        continue;
                    }

                    PropertyInfo PI = item.GetType().GetProperty(mi.Name);
                    dynamic value = null;
                    if (mi.MemberType == MemberTypes.Property)
                        value = PI.GetValue(item);
                    else if (mi.MemberType == MemberTypes.Field)
                        value = item.GetType().GetField(mi.Name).GetValue(item);

                    if (value != null && value is IObservableList)
                    {
                        foreach (object o in value)
                            UpdateParamsPlaceholder(o, placeHoldersToReplace, newVarName);
                    }
                    else if (value != null && value is string)
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
                ObservableList<AppModelParameter> mergedList = new ObservableList<AppModelParameter>();
                foreach (AppModelParameter localParam in AppModelParameters)
                    mergedList.Add(localParam);
                foreach (GlobalAppModelParameter globalParam in GlobalAppModelParameters)
                    mergedList.Add(globalParam);
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
            List<string> SecuritryContent = new List<string>();
            StringBuilder soapHeaderContent = new StringBuilder();

            soapHeaderContent.Append("\t<soapenv:Header>\n");
            soapHeaderContent.Append("\t\t<wsse:Security xmlns:wsse=");
            soapHeaderContent.Append("\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"");
            soapHeaderContent.Append("\n\t\t xmlns:wsu=");
            soapHeaderContent.Append("\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\"");
            soapHeaderContent.Append(">\n");
            soapHeaderContent.Append("\t\t\t\t<wsse:UsernameToken wsu:Id=");
            soapHeaderContent.Append("\"UsernameToken-{Function Fun=GetHashCode({Function Fun=GetGUID()})}");
            soapHeaderContent.Append("\">\n\t\t\t\t<wsse:Username>");
            soapHeaderContent.Append("{WSSECUSERNAME}");
            soapHeaderContent.Append("</wsse:Username>\n\t\t\t\t<wsse:Password Type=");
            soapHeaderContent.Append("\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText\"");
            soapHeaderContent.Append(">");
            soapHeaderContent.Append("{WSSECPASSWORD}");
            soapHeaderContent.Append("</wsse:Password>\n\t\t\t\t<wsse:Nonce EncodingType=");
            soapHeaderContent.Append("\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary\"");
            soapHeaderContent.Append(">" + "{Function Fun=GenerateHashCode(\"{Function Fun=GetGUID()}wssecpassword\")}" + "=");
            soapHeaderContent.Append("</wsse:Nonce>\n\t\t\t\t<wsu:Created>");
            soapHeaderContent.Append("{Function Fun=GetUTCTimeStamp()}");
            soapHeaderContent.Append("</wsu:Created>\n\t\t\t</wsse:UsernameToken>\n\t\t</wsse:Security>\n\t</soapenv:Header>\n");
            SecuritryContent.Add(soapHeaderContent.ToString());
            SecuritryContent.Add("{WSSECUSERNAME}");
            SecuritryContent.Add("{WSSECPASSWORD}");


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
