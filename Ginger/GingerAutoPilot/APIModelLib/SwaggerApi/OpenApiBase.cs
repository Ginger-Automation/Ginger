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

using Amdocs.Ginger.Common.APIModelLib;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.WorkSpaceLib;
using Amdocs.Ginger.Repository;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using NJsonSchema;
using NSwag;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Amdocs.Ginger.Common.Repository.ApplicationModelLib.APIModelLib.SwaggerApi
{
    public abstract class OpenApiBase
    {


        public void GenerateResponse(SwaggerOperation operation, ApplicationAPIModel basicModal)
        {

            if (operation.Responses.Count > 0 && operation.Responses.Keys.Any(x => x.StartsWith("2")))
            {
                //handling only the first sucess response code need to be improved
                //as discussed, for now handling response for only success
                string sucesskey = operation.Responses.Keys.Where(x => x.StartsWith("2")).ElementAt(0);
                operation.Responses.TryGetValue(sucesskey, out SwaggerResponse response);

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
                    if (basicModal.RequestContentType == ApplicationAPIUtils.eRequestContentType.XML)
                    {

                        ApplicationAPIModel JsonResponseModel = new ApplicationAPIModel();
                        var i = GenerateXMLBody(JsonResponseModel, schemaObj);

                        foreach (AppModelParameter currModel in GenerateJsonBody(JsonResponseModel, schemaObj))
                        {
                            ActReturnValue arv = new ActReturnValue
                            {
                                ItemName = currModel.ItemName,
                                Path = currModel.XPath,
                                DoNotConsiderAsTemp = true
                            };
                            basicModal.ReturnValues.Add(arv);
                        }
                    }
                    else if (basicModal.RequestContentType == ApplicationAPIUtils.eRequestContentType.JSon ||
                            basicModal.RequestContentType == ApplicationAPIUtils.eRequestContentType.FormData)
                    {
                        ApplicationAPIModel jsonResponseModel = new ApplicationAPIModel();
                        var generatedJsonBody = GenerateJsonBody(jsonResponseModel, schemaObj);

                        foreach (AppModelParameter currModel in generatedJsonBody)
                        {
                            ActReturnValue arv = new ActReturnValue
                            {
                                ItemName = currModel.ItemName,
                                Path = currModel.XPath,
                                DoNotConsiderAsTemp = true
                            };
                            basicModal.ReturnValues.Add(arv);
                        }
                    }

                }
            }
        }

        public ApplicationAPIModel GenerateBasicModel(SwaggerOperation Operation, SwaggerOperationMethod method, ref bool supportBody, string path, SwaggerDocument apidoc)
        {
            ApplicationAPIModel AAM = new ApplicationAPIModel();
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("{[a-zA-Z]*}");
            foreach (var Match in reg.Matches(path))
            {
                string modal = "<" + Match.ToString().ToUpper() + ">";
                path = path.Replace(Match.ToString(), modal);
                AAM.AppModelParameters.Add(new AppModelParameter(modal, $"{Match} in url. " +
                    $"{Operation.Parameters.FirstOrDefault(g => g.Name.Equals(Match.ToString().TrimStart('{').TrimEnd('}'), StringComparison.InvariantCultureIgnoreCase))?.Description}", "", "", new ObservableList<OptionalValue>()));
            }

            AAM.EndpointURL = path;
            AAM.APIType = ApplicationAPIUtils.eWebApiType.REST;
            string summaryText = Convert.ToString(Operation.Summary);
            AAM.Name = summaryText.Length > 50 ? summaryText.Substring(0, 50) : summaryText;

            if (string.IsNullOrWhiteSpace(AAM.Name))
            {
                AAM.Name = Operation.OperationId;
            }
            AAM.URLDomain = apidoc.BaseUrl;
            supportBody = true;
            if (Operation.Tags.Count > 0)
            {
                try
                {
                    foreach (var tag in Operation.Tags)
                    {
                        var existingTag = GingerCoreCommonWorkSpace.Instance.Solution.Tags.FirstOrDefault(t => t.Name.Equals(tag, StringComparison.InvariantCultureIgnoreCase));
                        if (existingTag != null)
                        {
                            AAM.TagsKeys.Add(existingTag.Key);
                        }
                        else
                        {
                            var newlyAddedTag = new RepositoryItemTag { Name = tag };
                            GingerCoreCommonWorkSpace.Instance.Solution.Tags.Add(newlyAddedTag);
                            AAM.TagsKeys.Add(newlyAddedTag.Key);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Error while adding tags to solution according as per API tags", ex);
                }
            }
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
                    APIModelKeyValue header = new APIModelKeyValue
                    {
                        ItemName = parameter.Name,
                        Param = parameter.Name,
                        Value = modelName
                    };
                    ObservableList<OptionalValue> listOptions = GetListOfParamEnums(parameter);
                    AAM.AppModelParameters.Add(new AppModelParameter(modelName, $"{parameter.Name} in headers. {parameter.Description}", "", "", listOptions));
                    AAM.HttpHeaders.Add(header);
                }
                else if (parameter.Kind == SwaggerParameterKind.Query)
                {
                    string modelName = parameter.Name;
                    AAM.EndpointURL = !AAM.EndpointURL.Contains('?') ?
                        AAM.EndpointURL + "?" + parameter.Name + "=" + "[<" + parameter.Name + ">]" :
                        AAM.EndpointURL + "+" + parameter.Name + "=" + "[<" + parameter.Name + ">]";
                    ObservableList<OptionalValue> listOptions = GetListOfParamEnums(parameter);
                    AAM.AppModelParameters.Add(new AppModelParameter(string.Format("[<{0}>]", modelName), $"{parameter.Name} in query. {parameter.Description}", "", "", listOptions));
                }
            }

            return AAM;
        }



        public ObservableList<AppModelParameter> GenerateXMLBody(ApplicationAPIModel aAM, JsonSchema4 operation)
        {

            string SampleBody = JsonSchemaTools.JsonSchemaFaker(operation, null, true);
            string XMlName = operation.HasReference ? XMlName = operation.Reference.Xml.Name : XMlName = operation.Xml?.Name;

            if (string.IsNullOrWhiteSpace(XMlName))
            {
                return [];
            }
            SampleBody = "{\"" + XMlName + "\":" + SampleBody + "}";
            string s2 = SampleBody;
            string xmlbody = JsonConvert.DeserializeXmlNode(SampleBody).OuterXml;
            string temppath = System.IO.Path.GetTempFileName();
            File.WriteAllText(temppath, xmlbody);
            XMLTemplateParser XTp = new XMLTemplateParser();
            ApplicationAPIModel aam = XTp.ParseDocument(temppath, new ObservableList<ApplicationAPIModel>()).ElementAt(0);
            aAM.RequestBody = aam.RequestBody;
            aAM.RequestBodyType = ApplicationAPIUtils.eRequestBodyType.FreeText;
            aam.RequestContentType = ApplicationAPIUtils.eRequestContentType.XML;
            return aam.AppModelParameters;

        }

        public static ObservableList<AppModelParameter> GenerateJsonBody(ApplicationAPIModel aAM, JsonSchema4 operation)
        {
            string SampleBody = JsonSchemaTools.JsonSchemaFaker(operation, null);
            object[] BodyandModelParameters = JSONTemplateParser.GenerateBodyANdModelParameters(SampleBody);
            aAM.RequestBody = (string)BodyandModelParameters[0];
            return (ObservableList<AppModelParameter>)BodyandModelParameters[1];
        }

        public void GenerateFormParameters(ApplicationAPIModel aAM, SwaggerOperation operation, bool isMultiPartFormdata = false)
        {
            if (isMultiPartFormdata)
            {
                aAM.RequestContentType = ApplicationAPIUtils.eRequestContentType.FormData;
            }
            else
            {
                aAM.RequestContentType = ApplicationAPIUtils.eRequestContentType.XwwwFormUrlEncoded;
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

        public ObservableList<OptionalValue> GetListOfParamEnums(SwaggerParameter swaggerParameter)
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
                else if (swaggerParameter.Enumeration != null && swaggerParameter.Enumeration.Count > 0)
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
                else if (swaggerParameter.ActualSchema.ActualProperties.Count > 0)
                {
                    foreach (var cnt in swaggerParameter.ActualSchema.ActualProperties)
                    {
                        if (cnt.Value.Enumeration.Count > 0)
                        {
                            foreach (var item in cnt.Value.Enumeration)
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
                }
                else if (swaggerParameter.ActualSchema.Enumeration.Count > 0)
                {
                    foreach (var item in swaggerParameter.ActualSchema.Enumeration)
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

        public static Dictionary<string, string> ExampleValueDict(SwaggerOperation operations)
        {

            Dictionary<string, string> exampleValues = new Dictionary<string, string>();

            try
            {
                if (operations.RequestBody != null)
                {
                    if (operations.RequestBody.Content.ElementAt(0).Value.Examples != null)
                    {
                        foreach (var schemaEntry in operations.RequestBody.Content.ElementAt(0).Value.Examples.Values)
                        {
                            var jsonValue = JsonConvert.SerializeObject(schemaEntry.Value);
                            JsonExtended je4Jn = new JsonExtended(jsonValue);
                            IEnumerable<JsonExtended> EEEL = je4Jn.GetEndingNodes();
                            if (EEEL.FirstOrDefault() != null)
                            {
                                foreach (var cnt in EEEL)
                                {
                                    if (!exampleValues.ContainsKey(cnt.Name.ToLower()))
                                    {
                                        exampleValues.Add(cnt.Name.ToLower(), cnt.JsonString);
                                    }
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Example values could not be fetched, please check the API", ex);
            }

            return exampleValues;

        }

        public void SetOptionalValue(ObservableList<AppModelParameter> AppModelParameters, Dictionary<string, string> listExampleValues, Dictionary<string, HashSet<string>> enumExampleList)
        {
            bool IsDefaultValueCheck = true;
            if (AppModelParameters.Count > 0)
            {
                foreach (var item in AppModelParameters)
                {
                    string parameterName = (item.ElementName.TrimStart('<', '{', '[').TrimEnd('>', '}', ']')).ToLower();

                    if (!string.IsNullOrEmpty(parameterName) && listExampleValues.TryGetValue(parameterName, out string exampleValue))
                    {
                        ObservableList<OptionalValue> tempList = new ObservableList<OptionalValue>
                        {
                            new OptionalValue()
                            {
                                Value = exampleValue,
                                IsDefault = true
                            }
                        };

                        item.OptionalValuesList = tempList;
                    }

                    if (!string.IsNullOrEmpty(parameterName) && enumExampleList.TryGetValue(parameterName, out HashSet<string> enumExampleValue))
                    {
                        ObservableList<OptionalValue> tempList = new ObservableList<OptionalValue>();

                        foreach (var value in enumExampleValue)
                        {
                            tempList.Add(new OptionalValue
                            {
                                Value = value,
                                IsDefault = IsDefaultValueCheck
                            });
                            IsDefaultValueCheck = false;
                            item.OptionalValuesList = tempList;
                        }


                    }


                }
            }

        }

        public Dictionary<string, HashSet<string>> SetEnumsValue(dynamic sd)
        {

            Dictionary<string, HashSet<string>> exampleValuesEnums = new Dictionary<string, HashSet<string>>();

            foreach (var item in sd.Value)
            {
                if (item.Value.Parameters.Count > 0)
                {


                    foreach (var definition in item.Value.Parameters)
                    {
                        if (definition.ActualSchema.ActualProperties.Count > 0)
                        {
                            foreach (var cnt in definition.ActualSchema.ActualProperties)
                            {
                                if (cnt.Value.Enumeration.Count > 0)
                                {
                                    foreach (var ent in cnt.Value.Enumeration)
                                    {
                                        AddValueForKey(exampleValuesEnums, cnt.Key, ent.ToString());
                                    }
                                }

                            }
                        }
                    }
                }
            }
            return exampleValuesEnums;

        }

        public void AddValueForKey(Dictionary<string, HashSet<string>> dictionary, string key, string value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary[key] = new HashSet<string> { value };
            }
            else
            {
                dictionary[key].Add(value);
            }
        }

        /// <summary>
        /// For Request Body null apis
        /// </summary>
        /// <param name="apidoc"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetExamplesFromDefinitions(SwaggerDocument apidoc)
        {

            Dictionary<string, string> exampleValues = new Dictionary<string, string>();
            try
            {
                if (apidoc?.Definitions != null && apidoc.Definitions.Count != 0)
                {
                    foreach (var schemaEntry in apidoc.Definitions)
                    {
                        string schemaName = schemaEntry.Key;
                        var schemaDefinition = schemaEntry.Value;

                        if (schemaDefinition.ActualProperties != null && schemaDefinition.ActualProperties.Count > 0)
                        {
                            foreach (var item in schemaDefinition.ActualProperties)
                            {
                                var actualName = item.Key.ToLower();
                                var actualDefinition = item.Value.Example?.ToString();
                                if (actualDefinition != null && !exampleValues.ContainsKey(actualName.ToLower()))
                                {
                                    exampleValues.Add(actualName, actualDefinition.ToString());
                                }
                            }
                        }
                        else if (schemaDefinition.Example != null)
                        {
                            if (!exampleValues.ContainsKey(schemaName.ToLower()))
                            {
                                exampleValues.Add(schemaName.ToLower(), schemaDefinition.Example.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Example values could not be fetched, please check the API", ex);
            }

            return exampleValues;
        }
    }
}