using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            TelemetryMetadata metadata = new();
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
