#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using NSwag;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Amdocs.Ginger.Common.Repository.ApplicationModelLib.APIModelLib.SwaggerApi
{
    public class OpenApiVer3 : OpenApiBase
    {
        SwaggerDocument opendoc = null;
        public OpenApiComponents OpenApiComponents { get; set; }

        public ObservableList<ApplicationAPIModel> OpenApiThree(SwaggerDocument Swaggerdoc, ObservableList<ApplicationAPIModel> SwaggerModels)
        {

            opendoc = Swaggerdoc;

            var reqBodyNullExampleList = GetExamplesFromDefinitions(opendoc);
            foreach (var paths in opendoc.Paths)
            {
                SwaggerPathItem SPi = paths.Value;
                var enumValuesListAMP = SetEnumsValue(paths);
                foreach (KeyValuePair<SwaggerOperationMethod, SwaggerOperation> so in SPi.AsEnumerable())
                {
                    SwaggerOperation Operation = so.Value;

                    bool supportBody = true;
                    if (Operation.RequestBody == null)
                    {
                        ApplicationAPIModel basicModal = GenerateBasicModel(Operation, so.Key, ref supportBody, paths.Key, opendoc);
                        SetOptionalValue(basicModal.AppModelParameters, reqBodyNullExampleList, enumValuesListAMP);
                        SwaggerModels.Add(basicModal);
                        GenerateResponse(Operation, basicModal);
                    }

                    else if (Operation.RequestBody.Content.Any())
                    {
                        foreach (var body in Operation.RequestBody.Content)
                        {
                            ApplicationAPIModel AAM = GenerateBasicModel(Operation, so.Key, ref supportBody, paths.Key, opendoc);

                            if (supportBody)
                            {
                                switch (body.Key)
                                {
                                    case "application/x-www-form-urlencoded":
                                        GenerateFormParameters(AAM, Operation);
                                        if (Operation.RequestBody != null)
                                        {
                                            AAM.AppModelParameters.Append(GenerateXMLBody(AAM, Operation.RequestBody.Content.ElementAt(0).Value.Schema));
                                            AAM.Name += "-UrlEncoded";
                                            AAM.Description = "Body Type is UrlEncoded ";
                                        }
                                        break;
                                    case "application/json":
                                        AAM.RequestContentType = ApplicationAPIUtils.eRequestContentType.JSon;
                                        AAM.ResponseContentType = ApplicationAPIUtils.eResponseContentType.JSon;
                                        if (Operation.RequestBody != null)
                                        {
                                            AAM.AppModelParameters.Append(GenerateJsonBody(AAM, Operation.RequestBody.Content.ElementAt(0).Value.Schema));
                                            AAM.Name += "-JSON";
                                            AAM.Description = "Body Type is JSON";
                                        }

                                        break;
                                    case "application/xml":
                                        AAM.RequestContentType = ApplicationAPIUtils.eRequestContentType.XML;
                                        AAM.ResponseContentType = ApplicationAPIUtils.eResponseContentType.XML;
                                        if (Operation.RequestBody != null)
                                        {
                                            AAM.AppModelParameters.Append(GenerateXMLBody(AAM, Operation.RequestBody.Content.ElementAt(0).Value.Schema));
                                            AAM.Name += "-XML";
                                            AAM.Description = "Body Type is XML";
                                        }

                                        break;
                                    default:
                                        Console.WriteLine("Content Type not supported");
                                        break;

                                }

                                SetOptionalValue(AAM.AppModelParameters, ExampleValueDict(Operation), enumValuesListAMP);
                            }
                            GenerateResponse(Operation, AAM);
                            SwaggerModels.Add(AAM);
                        }

                    }

                }
            }


            return SwaggerModels;
        }

        public static Dictionary<string, string> GetExamplesFromOpenApiComponents(OpenApiComponents apiComponents)
        {

            Dictionary<string, string> exampleValues = new Dictionary<string, string>();
            try
            {
                if (apiComponents.Schemas != null && apiComponents.Schemas.Count != 0)
                {
                    foreach (var schemaEntry in apiComponents.Schemas)
                    {
                        string schemaName = schemaEntry.Key;
                        var schemaDefinition = schemaEntry.Value;

                        if (schemaDefinition.ActualProperties != null && schemaDefinition.ActualProperties.Count > 0)
                        {
                            foreach (var item in schemaDefinition.ActualProperties)
                            {
                                var actualName = item.Key;
                                var actualDefinition = item.Value.Example?.ToString();
                                if (actualDefinition != null && !exampleValues.ContainsKey(actualName.ToLower()))
                                {
                                    exampleValues.Add(actualName.ToLower(), actualDefinition);
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
