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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Nodes;

#nullable enable
namespace Amdocs.Ginger.CoreNET.NewSelfHealing
{
    internal sealed class MobileElementPropertyMatcher : ElementPropertyMatcher
    {
        private const string WeightageFileName = "SelfHealingPropertyWeightage.json";

        private static readonly IReadOnlyDictionary<string, double> DefaultWeightage = new Dictionary<string, double>()
        {
            //High
            { "id", 1 }, { "name", 1 }, { "text", 1 }, 
            
            //Low
            { "element type", 0.7 }, { "label", 0.7 }, { "class", 0.7 }
        };
        private const double DefaultUnknownPropertyWeightage = 0.7;

        private static readonly double UnknownPropertyWeightage;

        private static readonly IReadOnlyDictionary<string, double> Weightage;

        static MobileElementPropertyMatcher()
        {
            LoadWeightageFromFile(out Dictionary<string, double>? weightageFromFile, out double? unknownPropertyWeightage);

            if (weightageFromFile != null)
            {
                Weightage = new Dictionary<string, double>(weightageFromFile);
            }
            else
            {
                Weightage = new Dictionary<string, double>(DefaultWeightage);
            }

            if (unknownPropertyWeightage != null)
            {
                UnknownPropertyWeightage = unknownPropertyWeightage.Value;
            }
            else
            {
                UnknownPropertyWeightage = DefaultUnknownPropertyWeightage;
            }
        }

        private static void LoadWeightageFromFile(out Dictionary<string, double>? weightageFromFile, out double? unknownPropertyWeightage)
        {
            weightageFromFile = null;
            unknownPropertyWeightage = null;
            try
            {
                Dictionary<string, double> tempWeightageFromFile = [];

                string weightageFilePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SelfHealingConfig", WeightageFileName));

                JsonNode json = JsonNode.Parse(File.OpenRead(weightageFilePath))!;
                JsonObject mobile = json["mobile"]!.AsObject();
                JsonArray high = mobile["high"]!.AsArray();
                foreach (JsonNode? item in high)
                {
                    tempWeightageFromFile.Add(item!.AsValue().GetValue<string>(), 1);
                }
                JsonArray low = mobile["low"]!.AsArray();
                foreach (JsonNode? item in low)
                {
                    tempWeightageFromFile.Add(item!.AsValue().GetValue<string>(), 0.7);
                }

                weightageFromFile = tempWeightageFromFile;
                unknownPropertyWeightage = mobile["unknown"]!.AsValue().GetValue<double>();
            }
            catch(Exception ex) 
            {
                Debug.WriteLine($"Failed to load weightage file,\n{ex}");
            }
        }

        protected override double GetPropertyWeightage(string? propertyName)
        {
            if (propertyName == null)
                return UnknownPropertyWeightage;

            if (Weightage.TryGetValue(propertyName.ToLower(), out double value))
            {
                return value;
            }
            return UnknownPropertyWeightage;
        }
    }
}
