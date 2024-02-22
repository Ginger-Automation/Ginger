#region License
/*
Copyright © 2014-2023 European Support Limited

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
using System.Xml;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.Repository.Serialization;
using Amdocs.Ginger.Common.SourceControlLib;
using GingerCore;
using Microsoft.CodeAnalysis.Operations;
using Newtonsoft.Json.Linq;

namespace Amdocs.Ginger.Repository
{

    public class RepositoryItemHeader
    {
        public const string XmlElementName = "Header";

        // Keep it first attr since it will help find object based on Guid without reading other attr, write it first!
        public Guid ItemGuid { get; set; }
        // We keep the class type of the content obj RI in the header so when we want to read only headers first we will know the type of the RI from header        
        public string ItemType { get; set; }

        /// <summary>
        /// Ginger version which this item was used to save
        /// </summary>
        public string GingerVersion { get; set; }


        // Version of this object, each time a save to file is done and the saved file is different/dirty then it increase +1
        // First version is 1
        public int Version { get; set; }


        // UserID create this RI
        public string CreatedBy { get; set; }


        // Created time in UTC
        public DateTime Created { get; set; }

        // Last user updated this RI
        public string LastUpdateBy { get; set; }

        // Last time this file was save when update was done
        public DateTime LastUpdate { get; set; }



        //TODO: External ID - for example to QC - need to be only in class itself not here, BF only?
        // public string ExternalID { get; set; }

        public RepositoryItemHeader() { }

        //public RepositoryItemHeader(RIBXmlReader reader)
        //{
        //    //Load(reader);
        //    reader.Load(DeserializeProperty);
        //}

        public RepositoryItemHeader(DeserializedSnapshot snapshot)
        {
            snapshot.ReadProperties(ReadSnapshotProperties);
        }

        public RepositoryItemHeader(DeserializedSnapshot2 snapshot)
        {
            ItemGuid = snapshot.GetValueAsGuid(nameof(ItemGuid));
            ItemType = snapshot.GetValue(nameof(ItemType));
            CreatedBy = snapshot.GetValue(nameof(CreatedBy));
            Created = snapshot.GetValueAsDateTime(nameof(Created), "yyyyMMddHHmm");
            GingerVersion = snapshot.GetValue(nameof(GingerVersion));
            Version = snapshot.GetValueAsInt(nameof(Version));
            LastUpdateBy = snapshot.GetValue(nameof(LastUpdateBy));
            LastUpdate = snapshot.GetValueAsDateTime(nameof(LastUpdate), "yyyyMMddHHmm");
        }

        public virtual SerializedSnapshot CreateSnapshot()
        {
            SerializedSnapshot.Builder builder = new();
            builder.SetName("Header");
            WriteSnapshotProperties(builder);
            return builder.Build();
        }

        protected virtual SerializedSnapshot.Builder WriteSnapshotProperties(SerializedSnapshot.Builder builder)
        {
            return builder
                .WithValue(nameof(ItemGuid), ItemGuid.ToString())
                .WithValue(nameof(ItemType), ItemType)
                .WithValue(nameof(CreatedBy), CreatedBy)
                .WithValue(nameof(Created), Created.ToString("yyyyMMddHHmm"))
                .WithValue(nameof(GingerVersion), GingerVersion)
                .WithValue(nameof(Version), Version.ToString())
                .WithValue(nameof(LastUpdateBy), LastUpdateBy)
                .WithValue(nameof(LastUpdate), LastUpdate.ToString("yyyyMMddHHmm"));
        }

        protected virtual void ReadSnapshotProperties(DeserializedSnapshot.Property property)
        {
            if (property.HasName(nameof(ItemGuid)))
                ItemGuid = property.GetValueAsGuid();
            else if (property.HasName(nameof(ItemType)))
                ItemType = property.GetValue();
            else if (property.HasName(nameof(CreatedBy)))
                CreatedBy = property.GetValue();
            else if (property.HasName(nameof(Created)))
                Created = property.GetValueAsDateTime("yyyyMMddHHmm");
            else if (property.HasName(nameof(GingerVersion)))
                GingerVersion = property.GetValue();
            else if (property.HasName(nameof(Version)))
                Version = property.GetValueAsInt();
            else if (property.HasName(nameof(LastUpdateBy)))
                LastUpdateBy = property.GetValue();
            else if (property.HasName(nameof(LastUpdate)))
                LastUpdate = property.GetValueAsDateTime("yyyyMMddHHmm");
        }

        //private void Load2(RIBXmlReader reader)
        //{
        //    XmlReader xmlReader = reader.XmlReader;
        //    if (xmlReader.NodeType != XmlNodeType.Element)
        //        throw new Exception($"Expected a element node type but found {xmlReader.NodeType}.");

        //    for (int attrIndex = 0; attrIndex < xmlReader.AttributeCount; attrIndex++)
        //    {
        //        xmlReader.MoveToAttribute(attrIndex);
        //        ParseAttribute(attributeName: xmlReader.Name, attributeValue: xmlReader.Value);
        //    }
        //    xmlReader.MoveToElement();

        //    int startDepth = xmlReader.Depth;
        //    while (xmlReader.Read())
        //    {
        //        bool reachedEndOfFile = xmlReader.EOF;
        //        bool reachedSibling = xmlReader.Depth == startDepth && xmlReader.NodeType == XmlNodeType.Element;
        //        bool reachedParent = xmlReader.Depth < startDepth;
        //        if (reachedEndOfFile || reachedSibling || reachedParent)
        //            break;

        //        if (xmlReader.NodeType != XmlNodeType.Element)
        //            continue;

        //        if (xmlReader.Depth != startDepth + 1)
        //            continue;

        //        ParseElement(elementName: xmlReader.Name, reader);
        //    }
        //}

        //private void Load(RIBXmlReader reader)
        //{
        //    if (!reader.XmlReader.IsStartElement())
        //        throw new Exception($"Expected a start element.");

        //    ReadAttributes(reader);
        //    ReadChildElements(reader);
        //}
        //public static bool UsePropertyParsers { get; set; }

        //private void ReadAttributes(RIBXmlReader reader)
        //{
        //    if (!reader.XmlReader.HasAttributes)
        //        return;

        //    while (reader.XmlReader.MoveToNextAttribute())
        //    {
        //        if (!UsePropertyParsers)
        //        {
        //            //ParseAttribute(reader.XmlReader.Name, reader.XmlReader.Value);
        //            DeserializeProperty(reader);
        //        }
        //        else
        //        {
        //            //foreach (PropertyParser<string> attributeParser in AttributeParsers())
        //            foreach (PropertyParser<RepositoryItemHeader, string> attributeParser in AttributeParsers())
        //            {
        //                if (string.Equals(attributeParser.Name, reader.Name))
        //                {
        //                    attributeParser.Parser.Invoke(this, reader.XmlReader.Value);
        //                    break;
        //                }
        //            }
        //        }
        //    }

        //    reader.XmlReader.MoveToElement();
        //}

        //private void ReadChildElements(RIBXmlReader reader)
        //{
        //    if (reader.XmlReader.IsEmptyElement)
        //        return;

        //    int startDepth = reader.XmlReader.Depth;
        //    while (reader.XmlReader.Read())
        //    {
        //        reader.XmlReader.MoveToContent();

        //        bool reachedEndOfElement = reader.XmlReader.Depth == startDepth && reader.XmlReader.NodeType == XmlNodeType.EndElement;
        //        if (reachedEndOfElement)
        //            break;

        //        if (!reader.XmlReader.IsStartElement())
        //            continue;

        //        bool isGrandChild = reader.XmlReader.Depth > startDepth + 1;
        //        if (isGrandChild)
        //        {
        //            //continue;
        //            reader.XmlReader.Skip();
        //        }

        //        if (!UsePropertyParsers)
        //        {
        //            ParseElement(reader.Name, reader);
        //            DeserializeProperty(reader);
        //        }
        //        else
        //        {
        //            //foreach (PropertyParser<RIBXmlReader> elementParser in ElementParsers())
        //            foreach (PropertyParser<RepositoryItemHeader,RIBXmlReader> elementParser in ElementParsers())
        //            {
        //                if (string.Equals(elementParser.Name, reader.Name))
        //                {
        //                    elementParser.Parser.Invoke(this, reader);
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //}

        //private IEnumerable<PropertyParser<RepositoryItemHeader,string>> AttributeParsers()
        //{
        //    return _attributeParsers;
        //    //return new List<PropertyParser<string>>()
        //    //{
        //    //    new(nameof(ItemGuid), value => ItemGuid = Guid.Parse(value)),
        //    //    new(nameof(ItemType), value => ItemType = value),
        //    //    new(nameof(CreatedBy), value => CreatedBy = value),
        //    //    new(nameof(Created), value => Created = DateTime.ParseExact(value, "yyyyMMddHHmm", provider: null)),
        //    //    new(nameof(GingerVersion), value => GingerVersion = value),
        //    //    new(nameof(Version), value => Version = int.Parse(value)),
        //    //    new(nameof(LastUpdateBy), value => LastUpdateBy = value),
        //    //    new(nameof(LastUpdate), value => LastUpdate = DateTime.ParseExact(value, "yyyyMMddHHmm", provider: null))
        //    //};
        //}

        //protected static readonly IEnumerable<PropertyParser<RepositoryItemHeader,string>> _attributeParsers =
        //    new List<PropertyParser<RepositoryItemHeader,string>>()
        //    {
        //        new(nameof(ItemGuid), (rih,value) => rih.ItemGuid = Guid.Parse(value)),
        //        new(nameof(ItemType), (rih,value) => rih.ItemType = value),
        //        new(nameof(CreatedBy), (rih,value) => rih.CreatedBy = value),
        //        new(nameof(Created), (rih,value) => rih.Created = DateTime.ParseExact(value, "yyyyMMddHHmm", provider: null)),
        //        new(nameof(GingerVersion), (rih,value) => rih.GingerVersion = value),
        //        new(nameof(Version), (rih,value) => rih.Version = int.Parse(value)),
        //        new(nameof(LastUpdateBy), (rih,value) => rih.LastUpdateBy = value),
        //        new(nameof(LastUpdate), (rih,value) => rih.LastUpdate = DateTime.ParseExact(value, "yyyyMMddHHmm", provider: null))
        //    };

        //private IEnumerable<PropertyParser<RepositoryItemHeader,RIBXmlReader>> ElementParsers()
        //{
        //    return Array.Empty<PropertyParser<RepositoryItemHeader,RIBXmlReader>>();
        //}

        //private void ParseAttribute(string attributeName, string attributeValue)
        //{
        //    if (string.Equals(attributeName, nameof(ItemGuid)))
        //        ItemGuid = Guid.Parse(attributeValue);
        //    else if (string.Equals(attributeName, nameof(ItemType)))
        //        ItemType = attributeValue;
        //    else if (string.Equals(attributeName, nameof(CreatedBy)))
        //        CreatedBy = attributeValue;
        //    else if (string.Equals(attributeName, nameof(Created)))
        //        Created = DateTime.ParseExact(attributeValue, "yyyyMMddHHmm", provider: null);
        //    else if (string.Equals(attributeName, nameof(GingerVersion)))
        //        GingerVersion = attributeValue;
        //    else if (string.Equals(attributeName, nameof(Version)))
        //        Version = int.Parse(attributeValue);
        //    else if (string.Equals(attributeName, nameof(LastUpdateBy)))
        //        LastUpdateBy = attributeValue;
        //    else if (string.Equals(attributeName, nameof(LastUpdate)))
        //        LastUpdate = DateTime.ParseExact(attributeValue, "yyyyMMddHHmm", provider: null);
        //}

        //private void ParseElement(string elementName, RIBXmlReader reader)
        //{

        //}

        //private void DeserializeProperty(DeserializePropertyInfo prop)
        //{
        //    if (prop.HasName(nameof(ItemGuid)))
        //        ItemGuid = Guid.Parse(prop.Value);
        //    else if (prop.HasName(nameof(ItemType)))
        //        ItemType = prop.Value;
        //    else if (prop.HasName(nameof(CreatedBy)))
        //        CreatedBy = prop.Value;
        //    else if (prop.HasName(nameof(Created)))
        //        Created = DateTime.ParseExact(prop.Value, "yyyyMMddHHmm", provider: null);
        //    else if (prop.HasName(nameof(GingerVersion)))
        //        GingerVersion = prop.Value;
        //    else if (prop.HasName(nameof(Version)))
        //        Version = int.Parse(prop.Value);
        //    else if (prop.HasName(nameof(LastUpdateBy)))
        //        LastUpdateBy = prop.Value;
        //    else if (prop.HasName(nameof(LastUpdate)))
        //        LastUpdate = DateTime.ParseExact(prop.Value, "yyyyMMddHHmm", provider: null);
        //}
    }
}
