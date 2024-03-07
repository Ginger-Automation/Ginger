using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.Repository;
using GingerCore;
using System.Xml;
using Amdocs.Ginger.Common.Repository.Serialization.Exceptions;
using MethodTimer;

#nullable enable
namespace Amdocs.Ginger.Common.Repository.Serialization
{
    public sealed class BetterRepositorySerializer
    {
        private const string GingerRepositoryItemXMLElementName = "GingerRepositoryItem";

        public string Serialize(RepositoryItemBase item)
        {
            using MemoryStream stream = new();
            using XmlWriter writer = XmlWriter.Create(stream);
            WriteGingerRepositoryItem(writer, item);
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        private void WriteGingerRepositoryItem(XmlWriter writer, RepositoryItemBase item)
        {
            writer.WriteStartDocument();
            writer.WriteStartElement(GingerRepositoryItemXMLElementName);
            WriteRepositoryItemHeader(writer, item.RepositoryItemHeader);
            WriteRepositoryItemBase(writer, item);
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
        }

        private void WriteRepositoryItemHeader(XmlWriter writer, RepositoryItemHeader header)
        {
            SerializedSnapshot snapshot = header.CreateSnapshot();
            snapshot.Write(writer);
            writer.Flush();
        }

        private void WriteRepositoryItemBase(XmlWriter writer, RepositoryItemBase item)
        {
            SerializedSnapshot snapshot = item.CreateSnapshot();
            snapshot.Write(writer);
            writer.Flush();
        }

        public T Deserialize<T>(string xml) where T : RepositoryItemBase
        {
            using StringReader stringReader = new(xml);
            using XmlReader reader = XmlReader.Create(stringReader);
            return ReadGingerRepositoryItem<T>(reader);
        }

        private T ReadGingerRepositoryItem<T>(XmlReader reader) where T : RepositoryItemBase
        {
            reader.MoveToContent();

            reader.ValidateNodeType(XmlNodeType.Element);
            reader.ValidateNodeName(GingerRepositoryItemXMLElementName);

            RepositoryItemHeader? header = null;
            RepositoryItemBase? item = null;

            reader.ReadChildElements(childReader =>
            {
                if (childReader.HasName(RepositoryItemHeader.XmlElementName))
                    header = ReadRepositoryItemHeader(childReader);
                else
                    item = ReadRepositoryItemBase(childReader);
            });

            if (header == null)
                throw new MissingMandatoryNodeException($"No child element found in {GingerRepositoryItemXMLElementName} with name '{RepositoryItemHeader.XmlElementName}'.");

            if (item == null)
                throw new MissingMandatoryNodeException($"No child element found in {GingerRepositoryItemXMLElementName} which is a {nameof(RepositoryItemBase)} sub-type.");

            item.RepositoryItemHeader = header;

            return (T)item;
        }

        private RepositoryItemHeader ReadRepositoryItemHeader(XmlReader reader)
        {
            reader.ValidateNodeType(XmlNodeType.Element);
            reader.ValidateNodeName(RepositoryItemHeader.XmlElementName);

            DeserializedSnapshot snapshot = DeserializedSnapshot.Load(reader);
            return new RepositoryItemHeader(snapshot);
            
        }

        private RepositoryItemBase ReadRepositoryItemBase(XmlReader reader)
        {
            DeserializedSnapshot snapshot = DeserializedSnapshot.Load(reader);
            return RepositoryItemBaseFactory.Create(snapshot.Name, snapshot);
        }
    }
}
