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

namespace Amdocs.Ginger.Common.Repository.ApplicationModelLib.APIModelLib.SwaggerApi
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
            else
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
            if (Swaggerdoc.SchemaType.ToString() == "Swagger2")
            {
                SwaggerVer2 s2 = new SwaggerVer2();
                SwaggerModels = s2.SwaggerTwo(Swaggerdoc,SwaggerModels);
            }
            else
            {
                OpenApiVer3 s3 = new OpenApiVer3();
                SwaggerModels = s3.OpenApiThree(Swaggerdoc, SwaggerModels);
            }

            return SwaggerModels;
        }

    }
}