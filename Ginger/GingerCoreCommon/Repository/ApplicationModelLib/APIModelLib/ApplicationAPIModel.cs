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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib;
using System;
using System.Collections.Generic;

namespace Amdocs.Ginger.Repository
{
    public class ApplicationAPIModel : ApplicationModelBase
    {

        //[IsSerializedForLocalRepository]
        /* public string GroupName { get; set; } *///Need for Reflecting the Name, Name field is not enough it needs to come with GroupName

        ApplicationAPIUtils.eWebApiType mAPIType = ApplicationAPIUtils.eWebApiType.REST;
        [IsSerializedForLocalRepository]
        public ApplicationAPIUtils.eWebApiType APIType { get { return mAPIType; } set { if (mAPIType != value) { mAPIType = value; OnPropertyChanged(nameof(APIType)); } } }

        private bool mIsSelected = true;
        // [IsSerializedForLocalRepository]
        public bool IsSelected { get { return mIsSelected; } set { if (mIsSelected != value) { mIsSelected = value; OnPropertyChanged(nameof(IsSelected)); } } }

        //[IsSerializedForLocalRepository]
        public Guid ApplicationGuid { get; set; }//Do not use, for backward support

        string mRequestBody = string.Empty;
        [IsSerializedForLocalRepository]
        public string RequestBody { get { return mRequestBody; } set { if (mRequestBody != value) { mRequestBody = value; OnPropertyChanged(nameof(RequestBody)); } } }

        string mEndpointURL;
        [IsSerializedForLocalRepository]
        public string EndpointURL { get { return mEndpointURL; } set { if (mEndpointURL != value) { mEndpointURL = value; OnPropertyChanged(nameof(EndpointURL)); } } }

        ApplicationAPIUtils.eHttpVersion mReqHttpVersion = ApplicationAPIUtils.eHttpVersion.HTTPV10;
        [IsSerializedForLocalRepository]
        public ApplicationAPIUtils.eHttpVersion ReqHttpVersion { get { return mReqHttpVersion; } set { if (mReqHttpVersion != value) { mReqHttpVersion = value; OnPropertyChanged(nameof(ReqHttpVersion)); } } }

        [IsSerializedForLocalRepository]
        public ObservableList<APIModelKeyValue> HttpHeaders = new ObservableList<APIModelKeyValue>();

        [IsSerializedForLocalRepository]
        public ObservableList<APIModelBodyKeyValue> APIModelBodyKeyValueHeaders = new ObservableList<APIModelBodyKeyValue>();

        [IsSerializedForLocalRepository]
        public ApplicationAPIUtils.eNetworkCredentials NetworkCredentials { get; set; }

        string mURLUser;
        [IsSerializedForLocalRepository]
        public string URLUser { get { return mURLUser; } set { if (mURLUser != value) { mURLUser = value; OnPropertyChanged(nameof(URLUser)); } } }

        string mURLDomain;
        [IsSerializedForLocalRepository]
        public string URLDomain { get { return mURLDomain; } set { if (mURLDomain != value) { mURLDomain = value; OnPropertyChanged(nameof(URLDomain)); } } }

        string mURLPass;
        [IsSerializedForLocalRepository]
        public string URLPass { get { return mURLPass; } set { if (mURLPass != value) { mURLPass = value; OnPropertyChanged(nameof(URLPass)); } } }

        bool mDoNotFailActionOnBadRespose = false;
        [IsSerializedForLocalRepository]
        public bool DoNotFailActionOnBadRespose { get { return mDoNotFailActionOnBadRespose; } set { if (mDoNotFailActionOnBadRespose != value) { mDoNotFailActionOnBadRespose = value; OnPropertyChanged(nameof(DoNotFailActionOnBadRespose)); } } }

        [IsSerializedForLocalRepository]
        public ApplicationAPIUtils.eRequestBodyType RequestBodyType { get; set; }

        [IsSerializedForLocalRepository]
        public ApplicationAPIUtils.eCretificateType CertificateType { get; set; }

        string mCertificatePath;
        [IsSerializedForLocalRepository]
        public string CertificatePath { get { return mCertificatePath; } set { if (mCertificatePath != value) { mCertificatePath = value; OnPropertyChanged(nameof(CertificatePath)); } } }

        bool mImportCetificateFile;
        [IsSerializedForLocalRepository]
        public bool ImportCetificateFile { get { return mImportCetificateFile; } set { if (mImportCetificateFile != value) { mImportCetificateFile = value; OnPropertyChanged(nameof(ImportCetificateFile)); } } }

        string mCertificatePassword;
        [IsSerializedForLocalRepository]
        public string CertificatePassword { get { return mCertificatePassword; } set { if (mCertificatePassword != value) { mCertificatePassword = value; OnPropertyChanged(nameof(CertificatePassword)); } } }

        [IsSerializedForLocalRepository]
        public ApplicationAPIUtils.eSercurityType SecurityType { get; set; }

        [IsSerializedForLocalRepository]
        public ApplicationAPIUtils.eAuthType AuthorizationType { get; set; }

        string mTemplateFileNameFileBrowser = string.Empty;
        [IsSerializedForLocalRepository]
        public string TemplateFileNameFileBrowser { get { return mTemplateFileNameFileBrowser; } set { if (mTemplateFileNameFileBrowser != value) { mTemplateFileNameFileBrowser = value; OnPropertyChanged(nameof(TemplateFileNameFileBrowser)); } } }

        string mImportRequestFile;
        [IsSerializedForLocalRepository]
        public string ImportRequestFile { get { return mImportRequestFile; } set { if (mImportRequestFile != value) { mImportRequestFile = value; OnPropertyChanged(nameof(ImportRequestFile)); } } }

        string mAuthUsername = string.Empty;
        [IsSerializedForLocalRepository]
        public string AuthUsername { get { return mAuthUsername; } set { if (mAuthUsername != value) { mAuthUsername = value; OnPropertyChanged(nameof(AuthUsername)); } } }

        string mAuthPassword = string.Empty;
        [IsSerializedForLocalRepository]
        public string AuthPassword { get { return mAuthPassword; } set { if (mAuthPassword != value) { mAuthPassword = value; OnPropertyChanged(nameof(AuthPassword)); } } }

        // We override the file extension so all subclass of ApplicationAPIModelBase will have the same extension
        public override string ObjFileExt
        {
            get
            {
                return "Ginger.ApplicationAPIModel";
            }
        }

        #region SOAP Action 

        string mSOAPAction;
        [IsSerializedForLocalRepository]
        public string SOAPAction { get { return mSOAPAction; } set { if (mSOAPAction != value) { mSOAPAction = value; OnPropertyChanged(nameof(SOAPAction)); } } }
        #endregion 

        #region REST Action 

        ApplicationAPIUtils.eRequestType mRequestType = ApplicationAPIUtils.eRequestType.GET;
        [IsSerializedForLocalRepository]
        public ApplicationAPIUtils.eRequestType RequestType { get { return mRequestType; } set { if (mRequestType != value) { mRequestType = value; OnPropertyChanged(nameof(RequestType)); } } }

        ApplicationAPIUtils.eContentType mResponseContentType = ApplicationAPIUtils.eContentType.JSon;
        [IsSerializedForLocalRepository]
        public ApplicationAPIUtils.eContentType ResponseContentType { get { return mResponseContentType; } set { if (mResponseContentType != value) { mResponseContentType = value; OnPropertyChanged(nameof(ResponseContentType)); } } }

        ApplicationAPIUtils.eContentType mContentType = ApplicationAPIUtils.eContentType.JSon;
        [IsSerializedForLocalRepository]
        public ApplicationAPIUtils.eContentType ContentType { get { return mContentType; } set { if (mContentType != value) { mContentType = value; OnPropertyChanged(nameof(ContentType)); } } }

        ApplicationAPIUtils.eCookieMode mCookieMode = ApplicationAPIUtils.eCookieMode.Session;
        [IsSerializedForLocalRepository]
        public ApplicationAPIUtils.eCookieMode CookieMode { get { return mCookieMode; } set { if (mCookieMode != value) { mCookieMode = value; OnPropertyChanged(nameof(CookieMode)); } } }
        #endregion

        //[IsSerializedForLocalRepository]
        //public OutputTemplateModel mOutputTemplateModel { get; set; }

        public ObservableList<TemplateFile> OptionalValuesTemplates = new ObservableList<TemplateFile>();// XML & JSON

        public override eImageType ItemImageType
        {
            get
            {
                return eImageType.APIModel;
            }
        }

        public override string ItemNameField
        {
            get
            {
                return nameof(this.Name);
            }
        }


        public override List<string> GetModelListsToConfigsWithExecutionData()
        {
            List<string> list = new List<string>();
            list.Add(nameof(this.HttpHeaders));
            list.Add(nameof(this.APIModelBodyKeyValueHeaders));
            return list;
        }
    }
}
