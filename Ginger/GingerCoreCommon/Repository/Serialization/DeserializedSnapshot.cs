using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Amdocs.Ginger.Common.Repository.Serialization.Exceptions;
using Amdocs.Ginger.Common.SourceControlLib;
using Amdocs.Ginger.Repository;
using System.Xml.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#nullable enable
namespace Amdocs.Ginger.Common.Repository.Serialization
{
    public sealed class DeserializedSnapshot
    {
        public string Name => _rootElement.Name;

        private readonly LiteXmlElement _rootElement;

        private DeserializedSnapshot(LiteXmlElement rootElement)
        {
            _rootElement = rootElement;
        }

        public static DeserializedSnapshot Load(XmlReader reader)
        {
            DeserializedSnapshot snapshot = new(LiteXmlElement.Load(reader));
            return snapshot;
        }

        public string GetValue(string propertyName)
        {
            Span<LiteXmlAttribute> attributes = _rootElement.Attributes;
            for (int attributeIndex = 0; attributeIndex < attributes.Length; attributeIndex++)
            {
                if (string.Equals(attributes[attributeIndex].Name, propertyName))
                    return attributes[attributeIndex].Value;
            }

            throw DeserializedPropertyNotFoundException.WithDefaultMessage(propertyName);
        }

        public string GetValue(string propertyName, string defaultValue)
        {
            try
            {
                return GetValue(propertyName);
            }
            catch (DeserializedPropertyNotFoundException)
            {
                return defaultValue;
            }
        }

        public Guid GetValueAsGuid(string propertyName)
        {
            return Guid.Parse(GetValue(propertyName));
        }

        public Guid GetValueAsGuid(string propertyName, Guid defaultValue)
        {
            try
            {
                return GetValueAsGuid(propertyName);
            }
            catch (DeserializedPropertyNotFoundException)
            {
                return defaultValue;
            }
        }

        public int GetValueAsInt(string propertyName)
        {
            return int.Parse(GetValue(propertyName));
        }

        public int GetValueAsInt(string propertyName, int defaultValue)
        {
            try
            {
                return GetValueAsInt(propertyName);
            }
            catch (DeserializedPropertyNotFoundException)
            {
                return defaultValue;
            }
        }

        public bool GetValueAsBool(string propertyName)
        {
            return bool.Parse(GetValue(propertyName));
        }

        public bool GetValueAsBool(string propertyName, bool defaultValue)
        {
            try
            {
                return GetValueAsBool(propertyName);
            }
            catch (DeserializedPropertyNotFoundException)
            {
                return defaultValue;
            }
        }

        public T GetValueAsEnum<T>(string propertyName) where T : struct
        {
            return Enum.Parse<T>(GetValue(propertyName));
        }

        public T GetValueAsEnum<T>(string propertyName, T defaultValue) where T : struct
        {
            try
            {
                return GetValueAsEnum<T>(propertyName);
            }
            catch (DeserializedPropertyNotFoundException)
            {
                return defaultValue;
            }
        }

        public DateTime GetValueAsDateTime(string propertyName, string format, IFormatProvider? provider = null)
        {
            return DateTime.ParseExact(GetValue(propertyName), format, provider);
        }

        public DateTime GetValueAsDateTime(string propertyName, DateTime defaultValue, string format, IFormatProvider? provider = null)
        {
            try
            {
                return GetValueAsDateTime(propertyName, format, provider);
            }
            catch (DeserializedPropertyNotFoundException)
            {
                return defaultValue;
            }
        }

        public TRepositoryItemBase GetValue<TRepositoryItemBase>(string propertyName) where TRepositoryItemBase : RepositoryItemBase
        {
            Span<LiteXmlElement> childElements = _rootElement.ChildElements;
            for (int index = 0; index < childElements.Length; index++)
            {
                LiteXmlElement childElement = childElements[index];
                if (string.Equals(childElement.Name, propertyName))
                    return (TRepositoryItemBase)RepositoryItemBaseFactory.Create(childElement.Name, new DeserializedSnapshot(childElement));
            }

            throw DeserializedPropertyNotFoundException.WithDefaultMessage(propertyName);
        }

        public TRepositoryItemBase GetValue<TRepositoryItemBase>(string propertyName, TRepositoryItemBase defaultValue) where TRepositoryItemBase : RepositoryItemBase
        {
            try
            {
                return GetValue<TRepositoryItemBase>(propertyName);
            }
            catch (DeserializedPropertyNotFoundException)
            {
                return defaultValue;
            }
        }

        public IEnumerable<TRepositoryItemBase> GetValues<TRepositoryItemBase>(string propertyName) where TRepositoryItemBase : RepositoryItemBase
        {
            Span<LiteXmlElement> childElements = _rootElement.ChildElements;
            for (int index = 0; index < childElements.Length; index++)
            {
                LiteXmlElement childElement = childElements[index];
                if (string.Equals(childElement.Name, propertyName))
                {
                    Span<LiteXmlElement> valuesXml = childElement.ChildElements;
                    TRepositoryItemBase[] values = new TRepositoryItemBase[valuesXml.Length];
                    for (int valueIndex = 0; valueIndex < valuesXml.Length; valueIndex++)
                    {
                        LiteXmlElement value = valuesXml[valueIndex];
                        values[valueIndex] = (TRepositoryItemBase)RepositoryItemBaseFactory.Create(value.Name, new DeserializedSnapshot(value));
                    }
                    return values;
                }
            }

            return Array.Empty<TRepositoryItemBase>();
        }

        public LiteXmlElement GetValuesLite(string propertyName)
        {
            Span<LiteXmlElement> childElements = _rootElement.ChildElements;
            for (int index = 0; index < childElements.Length; index++)
            {
                LiteXmlElement childElement = childElements[index];
                if (Equals(childElement.Name, propertyName))
                {
                    return childElement;
                }
            }
            
            throw DeserializedPropertyNotFoundException.WithDefaultMessage(propertyName);
        }

        public IEnumerable<TRepositoryItemBase> GetValues<TRepositoryItemBase>(string propertyName, IEnumerable<TRepositoryItemBase> defaultValue) where TRepositoryItemBase : RepositoryItemBase
        {
            try
            {
                return GetValues<TRepositoryItemBase>(propertyName);
            }
            catch (DeserializedPropertyNotFoundException)
            {
                return defaultValue;
            }
        }
    }
}
