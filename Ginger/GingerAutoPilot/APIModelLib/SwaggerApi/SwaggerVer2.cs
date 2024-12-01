#region License
/*
Copyright © 2014-2024 European Support Limited

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
    public class SwaggerVer2 : OpenApiBase
    {
        SwaggerDocument swagTwo = null;

        public ObservableList<ApplicationAPIModel> SwaggerTwo(SwaggerDocument Swaggerdoc, ObservableList<ApplicationAPIModel> SwaggerModels)
        {
            swagTwo = Swaggerdoc;

            var reqBodyNullExampleList = GetExamplesFromDefinitions(swagTwo);

            foreach (var paths in swagTwo.Paths)
            {
                SwaggerPathItem SPi = paths.Value;
                var enumExampleList = SetEnumsValue(paths);

                foreach (KeyValuePair<SwaggerOperationMethod, SwaggerOperation> so in SPi.AsEnumerable())
                {
                    SwaggerOperation Operation = so.Value;


                    bool supportBody = true;
                    if (Operation.RequestBody == null && !Operation.ActualConsumes.Any())
                    {

                        ApplicationAPIModel basicModal = GenerateBasicModel(Operation, so.Key, ref supportBody, paths.Key, swagTwo);
                        SetOptionalValue(basicModal.AppModelParameters, reqBodyNullExampleList, enumExampleList);
                        SwaggerModels.Add(basicModal);
                        GenerateResponse(Operation, basicModal);
                    }


                    else if (!Operation.ActualConsumes.Any() && Operation.RequestBody.Content.Any())
                    {

                        foreach (var body in Operation.RequestBody.Content)
                        {

                            ApplicationAPIModel AAM = GenerateBasicModel(Operation, so.Key, ref supportBody, paths.Key, swagTwo);



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
                                    default:
                                        Console.WriteLine("Content Type not supported");
                                        break;

                                }
                                SetOptionalValue(AAM.AppModelParameters, ExampleValueDict(Operation), enumExampleList);

                            }
                            GenerateResponse(Operation, AAM);
                            SwaggerModels.Add(AAM);
                        }

                    }

                    foreach (var body in Operation.ActualConsumes)
                    {

                        ApplicationAPIModel AAM = GenerateBasicModel(Operation, so.Key, ref supportBody, paths.Key, swagTwo);




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
                                default:
                                    break;

                            }
                            SetOptionalValue(AAM.AppModelParameters, ExampleValueDict(Operation), enumExampleList);

                        }
                        GenerateResponse(Operation, AAM);

                        SwaggerModels.Add(AAM);
                    }





                }
            }
            return SwaggerModels;
        }
    }
}