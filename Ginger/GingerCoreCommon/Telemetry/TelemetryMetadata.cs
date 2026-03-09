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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Amdocs.Ginger.Common.Telemetry
{
    public sealed class TelemetryMetadata : IEnumerable<KeyValuePair<string, IEnumerable<string>>>
    {
        public const string TagsKey = "Tags";

        private readonly Dictionary<string, LinkedList<string>> _dict;

        public TelemetryMetadata()
        {
            _dict = [];
        }

        public static TelemetryMetadata WithTags(params string[] tags)
        {
            TelemetryMetadata metadata = [];
            metadata.SetTags(tags);
            return metadata;
        }

        public void Add(string key, string value)
        {
            if (!_dict.TryGetValue(key, out LinkedList<string> values) || values == null)
            {
                values = new();
                _dict[key] = values;
            }
            values.AddLast(value);
        }

        public void SetTags(params string[] tags)
        {
            foreach (string tag in tags)
            {
                Add(TagsKey, tag);
            }
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(_dict, Formatting.None);
        }

        public IEnumerator<KeyValuePair<string, IEnumerable<string>>> GetEnumerator()
        {
            return _dict
                .Select(kv => new KeyValuePair<string, IEnumerable<string>>(kv.Key, kv.Value))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
