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
using YamlDotNet;
using YamlDotNet.Serialization;
using YamlDotNet.RepresentationModel;
using NJsonSchema.Yaml;
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Serialization.NamingConventions;

namespace Amdocs.Ginger.Common.Repository.ApplicationModelLib.APIModelLib
{
    public class YamlParser : APIConfigurationsDocumentParserBase
    {
        YamlDocument ymldoc = null;

        public override ObservableList<ApplicationAPIModel> ParseDocument(string FileName, ObservableList<ApplicationAPIModel> SwaggerModels, bool avoidDuplicatesNodes = false)
        {
            string FinalFileName = "";
            Uri url = new Uri(FileName);

            string orignalyaml = "";
            if (url.IsFile)
            {
                orignalyaml = System.IO.File.ReadAllText(FileName);
            }
            else
            {
                orignalyaml = GeneralLib.HttpUtilities.Download(url);

            }
            try
            {

                JsonSchemaYaml.FromYamlAsync(orignalyaml);
                FinalFileName = FileName;
            }
            catch
            {
                var r = new StringReader(orignalyaml);
                var deserializer = new Deserializer();
                var yamlObject = deserializer.Deserialize(r);
                StringWriter tw = new StringWriter();
                var serializer = new Newtonsoft.Json.JsonSerializer();
                serializer.Serialize(tw, yamlObject);
                orignalyaml = tw.ToString();
                string tempfile = System.IO.Path.GetTempFileName();

                System.IO.File.WriteAllText(tempfile, orignalyaml);
                FinalFileName = tempfile;
            }

            ymldoc = ConvertYamlStringToYamlDocument(orignalyaml);   
            

            foreach (var item in ymldoc.AllNodes)
            {
                
            }
            return SwaggerModels;
        }

        public YamlDocument ConvertYamlStringToYamlDocument(string yamlString)
        {
            var deserializer = new DeserializerBuilder().Build();

            // Deserialize the YAML string into a C# object
            var yamlDocument = deserializer.Deserialize<YamlDocument>(yamlString);
            

            if (yamlDocument is YamlDocument)
            {
                return (YamlDocument)yamlDocument;
            }
            else
            {
                return null;
            }
        }



    }
}