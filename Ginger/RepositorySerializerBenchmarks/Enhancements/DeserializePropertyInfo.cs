using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Amdocs.Ginger.Common.Repository.Serialization
{
    public sealed class DeserializePropertyInfo
    {
        private readonly RIBXmlReader _ribXmlReader;

        internal DeserializePropertyInfo(RIBXmlReader ribXmlReader)
        {
            _ribXmlReader = ribXmlReader;
        }

        public string Name => _ribXmlReader.XmlReader.Name;

        public string Value => _ribXmlReader.XmlReader.Value;

        public IEnumerable<T> ForEachChild<T>(Func<RIBXmlReader, T> childParser)
        {
            if (_ribXmlReader.XmlReader.NodeType != XmlNodeType.Element)
                throw new Exception($"Expected a element node type but found {_ribXmlReader.XmlReader.NodeType}.");

            List<T> children = [];

            var startDepth = _ribXmlReader.XmlReader.Depth;
            while (_ribXmlReader.XmlReader.Read())
            {
                _ribXmlReader.XmlReader.MoveToContent();

                var reachedEndOfElement = _ribXmlReader.XmlReader.Depth == startDepth && _ribXmlReader.XmlReader.NodeType == XmlNodeType.EndElement;
                if (reachedEndOfElement)
                    break;

                if (!_ribXmlReader.XmlReader.IsStartElement())
                    continue;

                var isGrandChild = _ribXmlReader.XmlReader.Depth > startDepth + 1;
                if (isGrandChild)
                    continue;

                children.Add(childParser(_ribXmlReader));
            }

            return children;
        }

        public bool HasName(string name)
        {
            return string.Equals(name, Name);
        }
    }
}
