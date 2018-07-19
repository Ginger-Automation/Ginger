#region License
/*
Copyright © 2014-2018 European Support Limited

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
using Amdocs.Ginger.Common.Repository.ApplicationModelLib;
using System;
using System.Collections.Generic;

namespace Amdocs.Ginger.Repository
{
    public class ApplicationAPIModel : ApplicationModelBase
    {

        //[IsSerializedForLocalRepository]
        /* public string GroupName { get; set; } *///Need for Reflecting the Name, Name field is not eanogh it needs to come with GroupName

        [IsSerializedForLocalRepository]
        public ApplicationAPIUtils.eWebApiType APIType { get; set; }

        private bool mIsSelected = true;
        // [IsSerializedForLocalRepository]
        public bool IsSelected { get { return mIsSelected; } set { mIsSelected = value; OnPropertyChanged(nameof(IsSelected)); } }

        //[IsSerializedForLocalRepository]
        public Guid ApplicationGuid { get; set; }//Do not use, for backward support

        [IsSerializedForLocalRepository]
        public string RequestBody { get; set; } = string.Empty;

        [IsSerializedForLocalRepository]
        public string EndpointURL { get; set; }

        [IsSerializedForLocalRepository]
        public ApplicationAPIUtils.eHttpVersion ReqHttpVersion { get; set; }

        [IsSerializedForLocalRepository]
        public ObservableList<APIModelKeyValue> HttpHeaders = new ObservableList<APIModelKeyValue>();

        [IsSerializedForLocalRepository]
        public ObservableList<APIModelBodyKeyValue> APIModelBodyKeyValueHeaders = new ObservableList<APIModelBodyKeyValue>();

        [IsSerializedForLocalRepository]
        public ApplicationAPIUtils.eNetworkCredentials NetworkCredentials { get; set; }

        [IsSerializedForLocalRepository]
        public string URLUser { get; set; }

        [IsSerializedForLocalRepository]
        public string URLDomain { get; set; }

        [IsSerializedForLocalRepository]
        public string URLPass { get; set; }

        [IsSerializedForLocalRepository]
        public bool DoNotFailActionOnBadRespose { get; set; }
        
        [IsSerializedForLocalRepository]
        public ApplicationAPIUtils.eRequestBodyType RequestBodyType { get; set; }

        [IsSerializedForLocalRepository]
        public ApplicationAPIUtils.eCretificateType CertificateType { get; set; }

        [IsSerializedForLocalRepository]
        public string CertificatePath { get; set; }

        [IsSerializedForLocalRepository]
        public bool ImportCetificateFile { get; set; }

        [IsSerializedForLocalRepository]
        public string CertificatePassword { get; set; }

        [IsSerializedForLocalRepository]
        public ApplicationAPIUtils.eSercurityType SecurityType { get; set; }

        [IsSerializedForLocalRepository]
        public ApplicationAPIUtils.eAuthType AuthorizationType { get; set; }

        [IsSerializedForLocalRepository]
        public string TemplateFileNameFileBrowser { get; set; } = string.Empty;

        [IsSerializedForLocalRepository]
        public string ImportRequestFile { get; set; }

        [IsSerializedForLocalRepository]
        public string AuthUsername { get; set; } = string.Empty;

        [IsSerializedForLocalRepository]
        public string AuthPassword { get; set; } = string.Empty;

        // We overide the file extension so all subclass of ApplicationAPIModelBase will have the same extension
        public override string ObjFileExt
        {
            get
            {                
                return "Ginger.ApplicationAPIModel";
            }
        }

        #region SOAP Action 

        [IsSerializedForLocalRepository]
        public string SOAPAction { get; set; }
        #endregion 

        #region REST Action 
        [IsSerializedForLocalRepository]
        public ApplicationAPIUtils.eRequestType RequestType { get; set; }

        [IsSerializedForLocalRepository]
        public ApplicationAPIUtils.eContentType ResponseContentType { get; set; }

        [IsSerializedForLocalRepository]
        public ApplicationAPIUtils.eContentType ContentType { get; set; }

        [IsSerializedForLocalRepository]
        public ApplicationAPIUtils.eCookieMode CookieMode { get; set; }
        #endregion

        //[IsSerializedForLocalRepository]
        //public OutputTemplateModel mOutputTemplateModel { get; set; }

        public ObservableList<TemplateFile> OptionalValuesTemplates = new ObservableList<TemplateFile>();// XML & JSON
       
    }


}
