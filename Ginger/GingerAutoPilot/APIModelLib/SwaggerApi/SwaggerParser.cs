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

using Amdocs.Ginger.Repository;
using Newtonsoft.Json.Linq;
using NSwag;
using System;

namespace Amdocs.Ginger.Common.Repository.ApplicationModelLib.APIModelLib.SwaggerApi
{
    public class SwaggerParser : APIConfigurationsDocumentParserBase
    {
        SwaggerDocument Swaggerdoc = null;

        public override ObservableList<ApplicationAPIModel> ParseDocument(string FileName, ObservableList<ApplicationAPIModel> SwaggerModels, bool avoidDuplicatesNodes = false)
        {
            if (IsValidYaml(FileName))
            {
                try
                {
                    string fileContent = FileContentProvider(FileName);
                    string fileConverted = ConvertYamlToJson(fileContent);
                    JToken.Parse(fileConverted); // doing the Jtoken to validate the json file
                    Swaggerdoc = SwaggerDocument.FromJsonAsync(fileConverted).Result;
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to read provided yaml document ", ex);
                    Reporter.ToUser(eUserMsgKey.InvalidYAML);
                }
            }
            else
            {
                try
                {
                    string fileContent = FileContentProvider(FileName);
                    JToken.Parse(fileContent);  // doing the Jtoken to validate the json file
                    Swaggerdoc = SwaggerDocument.FromJsonAsync(fileContent).Result;
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to read provided json document ", ex);
                    Reporter.ToUser(eUserMsgKey.InvalidJSON);
                }
            }

            if (Swaggerdoc.SchemaType.ToString() == "Swagger2")
            {
                SwaggerVer2 s2 = new SwaggerVer2();
                SwaggerModels = s2.SwaggerTwo(Swaggerdoc, SwaggerModels);
            }
            else
            {
                OpenApiVer3 s3 = new OpenApiVer3();
                SwaggerModels = s3.OpenApiThree(Swaggerdoc, SwaggerModels);
            }

            return SwaggerModels;
        }

        public string getInfoTitle()
        {
            return Swaggerdoc.Info.Title;
        }

    }
}