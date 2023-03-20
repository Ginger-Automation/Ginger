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

using Amdocs.Ginger.Common.APIModelLib;
using Amdocs.Ginger.Repository;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NSwag;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

namespace Amdocs.Ginger.Common.Repository.ApplicationModelLib.APIModelLib
{
    public class SwaggerParser : APIConfigurationsDocumentParserBase
    {
        SwaggerDocument Swaggerdoc = null;

        public override ObservableList<ApplicationAPIModel> ParseDocument(string FileName, ObservableList<ApplicationAPIModel> SwaggerModels, bool avoidDuplicatesNodes = false)
        {
            string FinalFileName = "";
            Uri url = new Uri(FileName);

            string orignaljson = "";
            if (url.IsFile)
            {
                orignaljson = System.IO.File.ReadAllText(FileName);
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
                        GenerateResponse(Operation, basicModal);
                    }


                    else if (Operation.ActualConsumes.Count() == 0 && Operation.RequestBody.Content.Count() != 0)
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
                                        GenerateFormParameters(AAM, Operation, true);
                                        break;
                                    case "application/json":
                                        AAM.ContentType = ApplicationAPIUtils.eContentType.JSon;
                                        AAM.ResponseContentType = ApplicationAPIUtils.eContentType.JSon;
                                        if (Operation.RequestBody != null)
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

                    foreach (var body in Operation.ActualConsumes)
                    {

                        ApplicationAPIModel AAM = GenerateBasicModel(Operation, so.Key, ref supportBody, paths.Key);




                        if (supportBody)
                        {
                            switch (body)
                            {
                                case "application/x-www-form-urlencoded":
                                    GenerateFormParameters(AAM, Operation);
                                    break;
                                case "multipart/form-data":
                                    GenerateFormParameters(AAM, Operation, true);
                                    break;
                                case "application/json":
                                    AAM.ContentType = ApplicationAPIUtils.eContentType.JSon;
                                    AAM.ResponseContentType = ApplicationAPIUtils.eContentType.JSon;
                                    if (Operation.RequestBody != null)
                                    {
                                        AAM.AppModelParameters.Append(GenerateJsonBody(AAM, Operation.RequestBody.Content.ElementAt(0).Value.Schema));
                                    }
                                    if (Operation.ActualConsumes.Count() > 1)
                                    {
                                        AAM.Name += "-JSON";
                                        AAM.Description = "Body Type is JSON";
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
            }
            return SwaggerModels;
        }

        private void GenerateResponse(SwaggerOperation operation, ApplicationAPIModel basicModal)
        {
            if (operation.Responses.Count > 0 && operation.Responses.Keys.Where(x => x.StartsWith("2")).Count() > 0)
            {
                //handling only the first sucess response code need to be improved
                //as discussed, for now handling response for only success
                string sucesskey = operation.Responses.Keys.Where(x => x.StartsWith("2")).ElementAt(0);
                SwaggerResponse response = null;
                operation.Responses.TryGetValue(sucesskey, out response);

                if (response != null && response.Schema == null)
                {
                    if (response.Reference != null && response.Reference is SwaggerResponse)
                    {
                        response = response.Reference;
                    }
                }

                if (response.Schema != null)
                {
                    var schemaObj = response.Schema;
                    if (response.Schema.HasReference && response.Schema.Reference != null)
                    {
                        schemaObj = response.Schema.Reference;
                    }
                    if (basicModal.ContentType == ApplicationAPIUtils.eContentType.XML)
                    {

                        ApplicationAPIModel JsonResponseModel = new ApplicationAPIModel();
                        var i = GenerateXMLBody(JsonResponseModel, schemaObj);

                        foreach (AppModelParameter currModel in GenerateJsonBody(JsonResponseModel, schemaObj))
                        {
                            ActReturnValue arv = new ActReturnValue();
                            arv.ItemName = currModel.ItemName;
                            arv.Path = currModel.XPath;
                            arv.DoNotConsiderAsTemp = true;
                            basicModal.ReturnValues.Add(arv);
                        }
                    }
                    else if (basicModal.ContentType == ApplicationAPIUtils.eContentType.JSon ||
                        basicModal.ContentType == ApplicationAPIUtils.eContentType.FormData)
                    {
                        ApplicationAPIModel JsonResponseModel = new ApplicationAPIModel();
                        var i = GenerateJsonBody(JsonResponseModel, schemaObj);

                        foreach (AppModelParameter currModel in GenerateJsonBody(JsonResponseModel, schemaObj))
                        {
                            ActReturnValue arv = new ActReturnValue();
                            arv.ItemName = currModel.ItemName;
                            arv.Path = currModel.XPath;
                            arv.DoNotConsiderAsTemp = true;
                            basicModal.ReturnValues.Add(arv);
                        }

                    }
                }
            }
        }

        private ApplicationAPIModel GenerateBasicModel(SwaggerOperation Operation, SwaggerOperationMethod method, ref bool supportBody, string path)
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
            if (string.IsNullOrWhiteSpace(AAM.Name))
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
                SwaggerParameter parameter = new SwaggerParameter();
                if (!string.IsNullOrEmpty(param.Name))
                {
                    parameter = param;
                }
                else
                {
                    if (param.HasReference && param.Reference is SwaggerParameter)
                    {
                        parameter = (SwaggerParameter)param.Reference;
                    }
                }
                if (parameter.Kind == SwaggerParameterKind.Header)
                {
                    string modelName = "<" + parameter.Name + ">";
                    APIModelKeyValue header = new APIModelKeyValue();
                    header.ItemName = parameter.Name;
                    header.Param = parameter.Name;
                    header.Value = modelName;
                    ObservableList<OptionalValue> listOptions = GetListOfParamEnums(parameter);
                    AAM.AppModelParameters.Add(new AppModelParameter(modelName, parameter.Name + " in headers", "", "", listOptions));
                    AAM.HttpHeaders.Add(header);
                }
                else if (parameter.Kind == SwaggerParameterKind.Query)
                {
                    string modelName = parameter.Name;
                    AAM.EndpointURL = !AAM.EndpointURL.Contains("?") ?
                        AAM.EndpointURL + "?" + parameter.Name + "=" + "[<" + parameter.Name + ">]" :
                        AAM.EndpointURL + "+" + parameter.Name + "=" + "[<" + parameter.Name + ">]";
                    ObservableList<OptionalValue> listOptions = GetListOfParamEnums(parameter);
                    AAM.AppModelParameters.Add(new AppModelParameter(string.Format("[<{0}>]", modelName), parameter.Name + " in query", "", "", listOptions));
                }
            }

            return AAM;
        }



        private ObservableList<AppModelParameter> GenerateXMLBody(ApplicationAPIModel aAM, JsonSchema4 operation)
        {

            string SampleBody = JsonSchemaTools.JsonSchemaFaker(operation, null, true);
            string XMlName = operation.HasReference ? XMlName = operation.Reference.Xml.Name : XMlName = operation.Xml.Name;




            SampleBody = "{\"" + XMlName + "\":" + SampleBody + "}";
            string s2 = SampleBody;
            string xmlbody = JsonConvert.DeserializeXmlNode(SampleBody).OuterXml;
            string temppath = System.IO.Path.GetTempFileName();
            File.WriteAllText(temppath, xmlbody);
            XMLTemplateParser XTp = new XMLTemplateParser();
            ApplicationAPIModel aam = XTp.ParseDocument(temppath, new ObservableList<ApplicationAPIModel>()).ElementAt(0);
            object[] BodyandModelParameters = JSONTemplateParser.GenerateBodyANdModelParameters(SampleBody);
            aAM.RequestBody = aam.RequestBody;
            aAM.RequestBodyType = ApplicationAPIUtils.eRequestBodyType.FreeText;
            aam.ContentType = ApplicationAPIUtils.eContentType.XML;
            return aam.AppModelParameters;
        }

        private ObservableList<AppModelParameter> GenerateJsonBody(ApplicationAPIModel aAM, JsonSchema4 operation)
        {
            string SampleBody = JsonSchemaTools.JsonSchemaFaker(operation, null);
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

                if (SP.Schema != null && SP.Schema.ActualProperties != null && SP.Schema.ActualProperties.Count > 0)
                {
                    foreach (KeyValuePair<string, JsonProperty> property in SP.Schema.ActualProperties)
                    {
                        string modelParameterName = "{" + SP.Name.ToUpper() + "." + property.Key + "}";
                        aAM.AppModelParameters.Add(new AppModelParameter()
                        {
                            ItemName = modelParameterName,
                        });

                        aAM.APIModelBodyKeyValueHeaders.Add(new APIModelBodyKeyValue()
                        {
                            Param = modelParameterName,
                            ValueType = SP.Type == JsonObjectType.File ? APIModelBodyKeyValue.eValueType.File : APIModelBodyKeyValue.eValueType.Text,
                            Value = modelParameterName,

                        });
                    }
                }
                else
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

        private ObservableList<OptionalValue> GetListOfParamEnums(SwaggerParameter swaggerParameter)
        {
            ObservableList<OptionalValue> lstOptions = new ObservableList<OptionalValue>();
            try
            {
                if (swaggerParameter.Item != null && swaggerParameter.Item.Enumeration != null && swaggerParameter.Item.Enumeration.Count != 0)
                {
                    foreach (object item in swaggerParameter.Item.Enumeration)
                    {
                        OptionalValue value = new OptionalValue()
                        {
                            Value = item.ToString(),
                            ItemName = item.ToString(),
                        };
                        lstOptions.Add(value);
                    }
                }
                if (swaggerParameter.Enumeration != null && swaggerParameter.Enumeration.Count > 0)
                {
                    foreach (object item in swaggerParameter.Enumeration)
                    {
                        OptionalValue value = new OptionalValue()
                        {
                            Value = item.ToString(),
                            ItemName = item.ToString(),
                        };
                        lstOptions.Add(value);
                    }

                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in getting optional values enum", ex);
            }
            return lstOptions;
        }

    }
}