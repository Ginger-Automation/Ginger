using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.Repository.Serialization
{
    public sealed class SerializedSnapshot
    {
        private readonly string _name;
        private readonly IReadOnlyDictionary<string, object> _properties;

        public string Name => _name;

        private SerializedSnapshot(string name, IReadOnlyDictionary<string, object> properties)
        {
            _name = name;
            _properties = properties;
        }

        public void Write(XmlWriter writer)
        {
            writer.WriteStartElement(_name);

            foreach (KeyValuePair<string, string> stringValue in GetStringValueList())
            {
                writer.WriteAttributeString(stringValue.Key, stringValue.Value);
                writer.Flush();
            }

            foreach (KeyValuePair<string, RepositoryItemBase> ribValue in GetRIBValueList())
            {
                writer.WriteStartElement(ribValue.Key);
                ribValue.Value.CreateSnapshot().Write(writer);
                writer.WriteEndElement();
                writer.Flush();
            }

            foreach (KeyValuePair<string, IEnumerable<RepositoryItemBase>> ribCollectionValue in GetRIBCollectionValueList())
            {
                writer.WriteStartElement(ribCollectionValue.Key);
                foreach (RepositoryItemBase rib in ribCollectionValue.Value)
                    rib.CreateSnapshot().Write(writer);
                writer.WriteEndElement();
                writer.Flush();
            }

            writer.WriteEndElement();
            writer.Flush();
        }

        private IEnumerable<KeyValuePair<string, string>> GetStringValueList()
        {
            return
                _properties
                .Where(keyValue => keyValue.Value is string)
                .Select(keyValue => new KeyValuePair<string, string>(keyValue.Key, (string)keyValue.Value))
                .OrderBy(keyValue => keyValue.Key);
        }

        private IEnumerable<KeyValuePair<string, RepositoryItemBase>> GetRIBValueList()
        {
            return
                _properties
                .Where(keyValue => keyValue.Value is RepositoryItemBase)
                .Select(keyValue => new KeyValuePair<string, RepositoryItemBase>(keyValue.Key, (RepositoryItemBase)keyValue.Value));
        }

        private IEnumerable<KeyValuePair<string, IEnumerable<RepositoryItemBase>>> GetRIBCollectionValueList()
        {
            return
                _properties
                .Where(keyValue => keyValue.Value is IEnumerable<RepositoryItemBase>)
                .Select(keyValue => new KeyValuePair<string, IEnumerable<RepositoryItemBase>>(keyValue.Key, (IEnumerable<RepositoryItemBase>)keyValue.Value));
        }

        public sealed class Builder
        {
            private string _name;
            private readonly Dictionary<string, object> _properties;

            public Builder() 
            {
                _properties = [];
            }

            public Builder SetName(string name)
            {
                _name = name;
                return this;
            }

            public Builder WithValue(string name, string value)
            {
                _properties[name] = value;
                return this;
            }

            public Builder WithValue(string name, RepositoryItemBase item)
            {
                _properties[name] = item;
                return this;
            }

            public Builder WithValues(string name, IEnumerable<RepositoryItemBase> values)
            {
                _properties[name] = values;
                return this;
            }

            public SerializedSnapshot Build()
            {
                if (string.IsNullOrEmpty(_name))
                    throw new InvalidOperationException("Cannot create snapshot with null/empty name.");

                return new SerializedSnapshot(_name, _properties);
            }
        }
    }
}
