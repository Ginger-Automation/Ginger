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
using NJsonSchema;
using NSwag;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Reflection;

namespace Amdocs.Ginger.Common.Repository.ApplicationModelLib.APIModelLib.SwaggerApi
{
    public class OpenApiVer3 : OpenApiBase
    {
        SwaggerDocument opendoc = null;
        public OpenApiComponents OpenApiComponents { get; set; }
        
        public ObservableList<ApplicationAPIModel> OpenApiThree(SwaggerDocument Swaggerdoc, ObservableList<ApplicationAPIModel> SwaggerModels)
        {
            
            opendoc = Swaggerdoc;
            
            

            foreach (var paths in opendoc.Paths)
            {
                SwaggerPathItem SPi = paths.Value;
                foreach (KeyValuePair<SwaggerOperationMethod, SwaggerOperation> so in SPi.AsEnumerable())
                {
                    SwaggerOperation Operation = so.Value;


                    bool supportBody = true;
                    if (Operation.RequestBody == null)
                    {

                        ApplicationAPIModel basicModal = GenerateBasicModel(Operation, so.Key, ref supportBody, paths.Key,opendoc);
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
                                        AAM.ContentType = ApplicationAPIUtils.eContentType.JSon;
                                        AAM.ResponseContentType = ApplicationAPIUtils.eContentType.JSon;
                                        if (Operation.RequestBody != null)
                                        {
                                            AAM.AppModelParameters.Append(GenerateJsonBody(AAM, Operation.RequestBody.Content.ElementAt(0).Value.Schema));
                                            AAM.Name += "-JSON"; AAM.Description = "Body Type is JSON";
                                        }

                                        break;
                                    case "application/xml":
                                        AAM.ContentType = ApplicationAPIUtils.eContentType.XML;
                                        AAM.ResponseContentType = ApplicationAPIUtils.eContentType.XML;
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
                            }
                            GenerateResponse(Operation, AAM);
                            SwaggerModels.Add(AAM);
                        }

                    }

                }
            }

            
            return SwaggerModels;
        }

        

    }
}