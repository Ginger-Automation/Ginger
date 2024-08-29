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
            foreach (string tag in tags)
            {
                metadata.Append(TagsKey, tag);
            }
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

        public string ToJSON()
        {
            StringBuilder json = new();

            json.Append('{');
            foreach (string key in _dict.Keys)
            {
                json.Append($"'{key}':");
                json.Append('[');
                foreach (string value in _dict[key])
                {
                    json.Append($"'{value}'");
                }
                json.Append(']');
            }
            json.Append('}');

            return json.ToString();
        }
    }
}
