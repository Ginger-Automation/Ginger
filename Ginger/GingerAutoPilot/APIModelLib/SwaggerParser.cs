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

using Amdocs.Ginger.Repository;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NSwag;

using System;
using System.Collections.Generic;
using System.Linq;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.APIModelLib;
using YamlDotNet;
using System.IO;
using YamlDotNet.Serialization;
using Newtonsoft.Json;

namespace Amdocs.Ginger.Common.Repository.ApplicationModelLib.APIModelLib
{
    public class SwaggerParser : APIConfigurationsDocumentParserBase
    {
        string ResolvedJson = "";
        SwaggerDocument Swaggerdoc = null;

        public override ObservableList<ApplicationAPIModel> ParseDocument(string FileName, bool avoidDuplicatesNodes = false)
        {
            string FinalFileName = "";
            Uri url = new Uri(FileName);
            
            ObservableList<ApplicationAPIModel> SwaggerModels = new ObservableList<ApplicationAPIModel>();
            string orignaljson = "";
            if (url.IsFile)
            {
             orignaljson=   System.IO.File.ReadAllText(FileName);
            }
            {
                orignaljson = GeneralLib.HttpUtilities.Download(url);

            }
           try
            {

                JToken.Parse(orignaljson);
                FinalFileName = FileName;
            }
            catch
            {
                var r = new StringReader(orignaljson);
                var deserializer = new Deserializer();
                var yamlObject = deserializer.Deserialize(r);
                StringWriter tw = new StringWriter();
                var serializer = new Newtonsoft.Json.JsonSerializer();
                serializer.Serialize(tw, yamlObject);
                orignaljson = tw.ToString();
                string tempfile = System.IO.Path.GetTempFileName();
      
                System.IO.File.WriteAllText(tempfile, orignaljson);
                FinalFileName = tempfile;
            }


            Swaggerdoc = SwaggerDocument.FromJsonAsync(orignaljson).Result;
           foreach (var paths in Swaggerdoc.Paths)
            {
                SwaggerPathItem SPi = paths.Value;
                foreach (KeyValuePair<SwaggerOperationMethod, SwaggerOperation> so in SPi.AsEnumerable())
                {
                    SwaggerOperation Operation = so.Value;


                    bool supportBody = true;
                    if (Operation.RequestBody == null && Operation.ActualConsumes.Count() == 0)
                    {

                        ApplicationAPIModel basicModal = GenerateBasicModel(Operation, so.Key, ref supportBody, paths.Key);
                        SwaggerModels.Add(basicModal);
                        GenerateResponse(Operation,basicModal);
                    }

        
                    else if(Operation.ActualConsumes.Count() == 0 && Operation.RequestBody.Content.Count() != 0)
                    {

                        foreach (var body in Operation.RequestBody.Content)
                        {

                            ApplicationAPIModel AAM = GenerateBasicModel(Operation, so.Key, ref supportBody, paths.Key);
                       


                            if (supportBody)
                            {
                                switch (body.Key)
                                {
                                    case "application/x-www-form-urlencoded":
                                        GenerateFormParameters(AAM, Operation);
                                        break;
                                    case "multipart/form-data":
                                        GenerateFormParameters(AAM, Operation,true);
                                        break;
                                    case "application/json":
                                        AAM.ContentType = ApplicationAPIUtils.eContentType.JSon;
                                        AAM.ResponseContentType = ApplicationAPIUtils.eContentType.JSon;
                                        if (Operation.RequestBody!= null)
                                        {
                                            AAM.AppModelParameters.Append(GenerateJsonBody(AAM, Operation.RequestBody.Content.ElementAt(0).Value.Schema));
                                        }
                                        if (Operation.ActualConsumes.Count() > 1)
                                        {
                                            AAM.Name += "-JSON"; AAM.Description = "Body Type is JSON";
                                        }
          
                                        break;
                                    case "application/xml":
                                        AAM.ContentType = ApplicationAPIUtils.eContentType.XML;
                                        AAM.ResponseContentType = ApplicationAPIUtils.eContentType.XML;
                                        if (Operation.RequestBody != null)
                                        {
                                            AAM.AppModelParameters.Append(GenerateXMLBody(AAM, Operation.RequestBody.Content.ElementAt(0).Value.Schema));
                                        }
                                        if (Operation.ActualConsumes.Count() > 1)
                                        {
                                            AAM.Name += "-XML";
                                            AAM.Description = "Body Type is XML";
                                        }
                              
                                        break;

                                }
                            }
                            GenerateResponse(Operation, AAM);
                            SwaggerModels.Add(AAM);
                        }

                    }
                  
                    foreach (var body in    Operation.ActualConsumes)
                    {
                    
                        ApplicationAPIModel AAM= GenerateBasicModel(Operation,so.Key,ref supportBody,paths.Key);
                        
         
                    
                 
                        if (supportBody)
                        {
                            switch (body)
                            {
                                case "application/x-www-form-urlencoded":
                                    GenerateFormParameters(AAM, Operation);
                                    break;
                                case "multipart/form-data":
                                    GenerateFormParameters(AAM, Operation,true);
                                    break;
                                case "application/json":
                                    AAM.ContentType = ApplicationAPIUtils.eContentType.JSon;
                                    AAM.ResponseContentType = ApplicationAPIUtils.eContentType.JSon;
                                    if (Operation.RequestBody!= null)
                                    {
                                        AAM.AppModelParameters.Append(GenerateJsonBody(AAM, Operation.RequestBody.Content.ElementAt(0).Value.Schema));
                                    }
                                    if (Operation.ActualConsumes.Count()>1)
                                    {
                                        AAM.Name += "-JSON";
                                        AAM.Description = "Body Type is JSON";
                                    }
                               
                                    break;
                                case "application/xml":
                                    AAM.ContentType = ApplicationAPIUtils.eContentType.XML;
                                    AAM.ResponseContentType = ApplicationAPIUtils.eContentType.XML;
                                    if (Operation.RequestBody!= null)
                                    {
                                        AAM.AppModelParameters.Append(GenerateXMLBody(AAM, Operation.RequestBody.Content.ElementAt(0).Value.Schema));
                                    }
                                        if (Operation.ActualConsumes.Count() > 1)
                                    {
                                        AAM.Name += "-XML";
                                        AAM.Description = "Body Type is XML";
                                    }
                                    break;

                            }
                        }
                        GenerateResponse(Operation, AAM);
                        
                        SwaggerModels.Add(AAM);
                    }


         

                   
                }
            }
            return SwaggerModels;
        }

        private void GenerateResponse(SwaggerOperation operation, ApplicationAPIModel basicModal)
        {
            if (operation.Responses.Count > 1 && operation.Responses.Keys.Where(x => x.StartsWith("2")).Count() > 0)
            {
                string sucesskey = operation.Responses.Keys.Where(x => x.StartsWith("2")).ElementAt(0);
                SwaggerResponse response = null;
                operation.Responses.TryGetValue(sucesskey, out response);
                if (response.Schema != null)
                {


                    if (basicModal.ContentType == ApplicationAPIUtils.eContentType.XML)
                    {

                        ApplicationAPIModel JsonResponseModel = new ApplicationAPIModel();
                        var i = GenerateXMLBody(JsonResponseModel, response.Schema);

                        foreach (AppModelParameter currModel in GenerateJsonBody(JsonResponseModel, response.Schema))
                        {
                            ActReturnValue arv = new ActReturnValue();
                            arv.ItemName = currModel.ItemName;
                            arv.Path = currModel.XPath;
                            arv.DoNotConsiderAsTemp = true;
                            basicModal.ReturnValues.Add(arv);
                        }


                    }
                    else if (basicModal.ContentType == ApplicationAPIUtils.eContentType.JSon)
                    {

                        ApplicationAPIModel JsonResponseModel = new ApplicationAPIModel();
                        var i=GenerateJsonBody(JsonResponseModel, response.Schema);

                        foreach (AppModelParameter currModel in GenerateJsonBody(JsonResponseModel, response.Schema))
                        {
                            ActReturnValue arv = new ActReturnValue();
                            arv.ItemName = currModel.ItemName;
                            arv.Path = currModel.XPath;
                            arv.DoNotConsiderAsTemp = true;
                            basicModal.ReturnValues.Add(arv);
                        }

                    }
                }
                else
                {

                }
            }
        }

        private ApplicationAPIModel GenerateBasicModel(SwaggerOperation Operation, SwaggerOperationMethod method,ref bool supportBody, string path)
        {
            ApplicationAPIModel AAM = new ApplicationAPIModel();
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("{[a-zA-Z]*}");
            foreach (var Match in reg.Matches(path))
            {
                string modal = "<" + Match.ToString().ToUpper() + ">";
                path = path.Replace(Match.ToString(), modal);
                AAM.AppModelParameters.Add(new AppModelParameter(modal, Match.ToString() + "in url", "", "", new ObservableList<OptionalValue>()));
            }
            AAM.EndpointURL = Swaggerdoc.BaseUrl + path;
            AAM.APIType = ApplicationAPIUtils.eWebApiType.REST;
            AAM.Name = Operation.Summary;
            if(string.IsNullOrWhiteSpace(AAM.Name))
            {
                AAM.Name = Operation.OperationId;
            }
            AAM.URLDomain = Swaggerdoc.BaseUrl;
             supportBody = true;
            switch (method)
            {
                case SwaggerOperationMethod.Get:
                    AAM.RequestType = ApplicationAPIUtils.eRequestType.GET;
                    supportBody = false;
                    break;
                case SwaggerOperationMethod.Delete:
                    AAM.RequestType = ApplicationAPIUtils.eRequestType.DELETE;
                    break;
                case SwaggerOperationMethod.Head:
                    AAM.RequestType = ApplicationAPIUtils.eRequestType.Head;
                    break;
                case SwaggerOperationMethod.Options:
                    AAM.RequestType = ApplicationAPIUtils.eRequestType.Options;
                    break;
                case SwaggerOperationMethod.Patch:
                    AAM.RequestType = ApplicationAPIUtils.eRequestType.PATCH;
                    break;
                case SwaggerOperationMethod.Post:
                    AAM.RequestType = ApplicationAPIUtils.eRequestType.POST;
                    break;
                case SwaggerOperationMethod.Put:
                    AAM.RequestType = ApplicationAPIUtils.eRequestType.PUT;
                    break;
                case SwaggerOperationMethod.Trace:
                    AAM.RequestType = ApplicationAPIUtils.eRequestType.POST;
                    break;
                case SwaggerOperationMethod.Undefined:
                    AAM.RequestType = ApplicationAPIUtils.eRequestType.POST;
                    break;
            }


            foreach (SwaggerParameter param in Operation.Parameters)
            {
                if (param.Kind == SwaggerParameterKind.Header)
                {
                    string modelName = "<" + param.Name + ">";
                    APIModelKeyValue header = new APIModelKeyValue();
                    header.ItemName = param.Name;
                    header.Param = param.Name;
                    header.Value = modelName;
                    AAM.AppModelParameters.Add(new AppModelParameter(modelName, param.Name+ " in headers", "", "", new ObservableList<OptionalValue>()));
                    AAM.HttpHeaders.Add(header);
                }
            }

            return AAM;
        }



        private ObservableList<AppModelParameter> GenerateXMLBody(ApplicationAPIModel aAM, JsonSchema4 operation)
        {

            string SampleBody = JsonSchemaTools.JsonSchemaFaker(operation, true);
            string XMlName = operation.HasReference? XMlName = operation.Reference.Xml.Name: XMlName = operation.Xml.Name;

        


            SampleBody = "{\"" + XMlName + "\":" + SampleBody + "}";
            string s2 = SampleBody;
            string xmlbody = JsonConvert.DeserializeXmlNode(SampleBody).OuterXml;
            string temppath = System.IO.Path.GetTempFileName();
            File.WriteAllText(temppath, xmlbody);
            XMLTemplateParser XTp = new XMLTemplateParser();
            ApplicationAPIModel aam = XTp.ParseDocument(temppath).ElementAt(0);
            object[] BodyandModelParameters = JSONTemplateParser.GenerateBodyANdModelParameters(SampleBody);
            aAM.RequestBody = aam.RequestBody;
            aAM.RequestBodyType = ApplicationAPIUtils.eRequestBodyType.FreeText;
            aam.ContentType = ApplicationAPIUtils.eContentType.XML;
            return aam.AppModelParameters;
        }

        private ObservableList<AppModelParameter> GenerateJsonBody(ApplicationAPIModel aAM, JsonSchema4 operation)
        {

            string SampleBody = JsonSchemaTools.JsonSchemaFaker(operation);

            object[] BodyandModelParameters = JSONTemplateParser.GenerateBodyANdModelParameters(SampleBody);
            aAM.RequestBody = (string)BodyandModelParameters[0];
          return (ObservableList<AppModelParameter>)BodyandModelParameters[1];
        }

        private void GenerateFormParameters(ApplicationAPIModel aAM, SwaggerOperation operation, bool isMultiPartFormdata = false)
        {
            if (isMultiPartFormdata)
            {
                aAM.ContentType = ApplicationAPIUtils.eContentType.FormData;
            }
            else
            {
                aAM.ContentType = ApplicationAPIUtils.eContentType.XwwwFormUrlEncoded;
            }
            foreach (SwaggerParameter SP in operation.ActualParameters)
            {
                string ModelParameterName = "{" + SP.Name.ToUpper() + "}";

                aAM.AppModelParameters.Add(new AppModelParameter()
                {
                    ItemName = ModelParameterName,
                });

                aAM.APIModelBodyKeyValueHeaders.Add(new APIModelBodyKeyValue()
                {
                    Param = SP.Name,
                    ValueType = SP.Type == JsonObjectType.File ? APIModelBodyKeyValue.eValueType.File : APIModelBodyKeyValue.eValueType.Text,
                    Value = ModelParameterName,

                });
            }


        }

    }
}