using System;
using System.Collections.Generic;
using Amdocs.Ginger.Common.Repository.Serialization.Exceptions;
using Amdocs.Ginger.Repository;
using System.Xml;
using MethodTimer;
using System.Xml.Linq;

#nullable enable
namespace Amdocs.Ginger.Common.Repository.Serialization
{
    public sealed class DeserializedSnapshot
    {
        private readonly XmlReader _reader;
        private readonly string _name;

        public string Name => _name;

        public DeserializedSnapshot(XmlReader reader)
        {
            _reader = reader;
            _name = _reader.Name;
        }

        public void ReadProperties(Action<Property> propertyProcessor)
        {
            //MutableProperty property = new(_reader);
            _reader.ReadAttributes((name, value) =>
            {
                propertyProcessor(new Property(_reader));
                //propertyProcessor(property);
            });

            _reader.ReadChildElements(childReader =>
            {
                propertyProcessor(new Property(childReader));
                //property.Reader = childReader;
                //propertyProcessor(property);
            });
        }

        public class MutableProperty : Property
        {
            public XmlReader Reader
            {
                get => _reader;
                set => _reader = value;
            }

            public MutableProperty(XmlReader reader) : base(reader) { }
        }

        public class Property
        {
            private protected XmlReader _reader;

            public string Name => _reader.Name;

            private string Value => _reader.Value;

            public Property(XmlReader reader)
            {
                _reader = reader;
            }

            public bool HasName(string expected)
            {
                return string.Equals(Name, expected);
            }

            public string GetValue()
            {
                if (Value == null)
                    throw UnexpectedDeserializedPropertyType.WithDefaultMsg(propertyName: Name, expectedType: "string");
                return Value;
            }

            public Guid GetValueAsGuid()
            {
                return Guid.Parse(GetValue());
            }

            public int GetValueAsInt()
            {
                return int.Parse(GetValue());
            }

            public bool GetValueAsBool()
            {
                return bool.Parse(GetValue());
            }

            public T GetValueAsEnum<T>() where T : struct 
            {
                return Enum.Parse<T>(GetValue());
            }

            public DateTime GetValueAsDateTime(string format, IFormatProvider? provider = null)
            {
                return DateTime.ParseExact(GetValue(), format, provider);
            }

            public T GetValue<T>() where T : RepositoryItemBase
            {
                if (_reader == null)
                    throw UnexpectedDeserializedPropertyType.WithDefaultMsg(propertyName: Name, expectedType: typeof(T).Name);
                DeserializedSnapshot snapshot = new(_reader);
                return RepositoryItemBaseFactory.Create<T>(snapshot);
            }

            public IEnumerable<T> GetValues<T>() where T : RepositoryItemBase
            {
                if (_reader == null)
                    throw UnexpectedDeserializedPropertyType.WithDefaultMsg(propertyName: Name, expectedType: typeof(IEnumerable<T>).Name);
                List<T> values = [];
                _reader.ReadChildElements(childReader =>
                {
                    DeserializedSnapshot snapshot = new(childReader);
                    T item = (T)RepositoryItemBaseFactory.Create(snapshot.Name, snapshot);
                    values.Add(item);
                });
                return values;
            }

            public LazyLoadListDetails GetValuesLazy()
            {
                LazyLoadListDetails details = new()
                {
                    Config = new()
                    {
                        ListName = _reader.Name,
                        LazyLoadType = LazyLoadListConfig.eLazyLoadType.StringData
                    },
                    DataAsString = _reader.ReadOuterXml()
                };
                return details;
            }
        }
    }
}
