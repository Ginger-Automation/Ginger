using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Common.Telemetry
{
    public sealed class TelemetryMetadata
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

        public void Append(string key, string value)
        {
            if (!_dict.TryGetValue(key, out LinkedList<string> values) || values == null)
            {
                values = new();
                _dict[key] = values;
            }
            values.AddLast(value);
        }

        public void Add(string key, string value)
        {
            LinkedList<string> values = new();
            values.AddLast(value);
            _dict[key] = values;
        }

        public void SetTags(params string[] tags)
        {
            foreach (string tag in tags)
            {
                Append(TagsKey, tag);
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
    }
}
