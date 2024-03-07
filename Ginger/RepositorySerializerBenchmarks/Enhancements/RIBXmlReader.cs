using Amdocs.Ginger.Common.Repository;
using GingerCore;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Amdocs.Ginger.Common.Repository.Serialization
{
    public sealed class RIBXmlReader
    {
        private readonly DeserializePropertyInfo _deserializePropertyInfo;

        public string Name => XmlReader.Name;

        public string Value => XmlReader.Value;

        public XmlReader XmlReader { get; }

        public RIBXmlReader(XmlReader xmlReader)
        {
            _deserializePropertyInfo = new(this);
            XmlReader = xmlReader;
        }

        public IEnumerable<T> ForEachChild<T>(Func<RIBXmlReader, T> childParser)
        {
            if (XmlReader.NodeType != XmlNodeType.Element)
                throw new Exception($"Expected a element node type but found {XmlReader.NodeType}.");

            List<T> children = [];

            var startDepth = XmlReader.Depth;
            while (XmlReader.Read())
            {
                XmlReader.MoveToContent();

                var reachedEndOfElement = XmlReader.Depth == startDepth && XmlReader.NodeType == XmlNodeType.EndElement;
                if (reachedEndOfElement)
                    break;

                if (!XmlReader.IsStartElement())
                    continue;

                var isGrandChild = XmlReader.Depth > startDepth + 1;
                if (isGrandChild)
                    continue;

                children.Add(childParser(this));
            }

            return children;
        }

        public bool IsName(string name)
        {
            return string.Equals(name, Name);
        }

        public void Load(Action<DeserializePropertyInfo> deserializeProperty)
        {
            if (!XmlReader.IsStartElement())
                throw new Exception($"Expected a start element.");

            ReadAttributes(deserializeProperty);
            ReadChildElements(deserializeProperty);
        }

        private void ReadAttributes(Action<DeserializePropertyInfo> deserializeProperty)
        {
            if (!XmlReader.HasAttributes)
                return;

            while (XmlReader.MoveToNextAttribute())
            {
                deserializeProperty.Invoke(_deserializePropertyInfo);
            }

            XmlReader.MoveToElement();
        }

        private void ReadChildElements(Action<DeserializePropertyInfo> deserializeProperty)
        {
            if (XmlReader.IsEmptyElement)
                return;

            var startDepth = XmlReader.Depth;
            while (XmlReader.Read())
            {
                XmlReader.MoveToContent();

                var reachedEndOfElement = XmlReader.Depth == startDepth && XmlReader.NodeType == XmlNodeType.EndElement;
                if (reachedEndOfElement)
                    break;

                if (!XmlReader.IsStartElement())
                    continue;

                var isGrandChild = XmlReader.Depth > startDepth + 1;
                if (isGrandChild)
                {
                    XmlReader.Skip();
                    continue;
                }

                 deserializeProperty.Invoke(_deserializePropertyInfo);
            }
        }
    }
}
