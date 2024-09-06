using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            StringBuilder json = new();

            json.Append('{');
            string[] keys = _dict.Keys.ToArray();
            for (int keyIndex = 0; keyIndex < keys.Length; keyIndex++)
            {
                string key = keys[keyIndex];
                json.Append($"\"{key}\":");
                json.Append('[');
                string[] values = _dict[key].ToArray();
                for (int valueIndex = 0; valueIndex < values.Length; valueIndex++)
                {
                    string value = values[valueIndex];
                    json.Append($"\"{value}\"");
                    if (valueIndex < values.Length - 1)
                    {
                        json.Append(',');
                    }
                }
                json.Append(']');
                if (keyIndex < keys.Length - 1)
                {
                    json.Append(',');
                }
            }
            json.Append('}');

            return json.ToString();
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
