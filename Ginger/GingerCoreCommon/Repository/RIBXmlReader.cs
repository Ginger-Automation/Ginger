using GingerCore;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Amdocs.Ginger.Repository
{
    public sealed class RIBXmlReader
    {
        public string Name => XmlReader.Name;

        public XmlReader XmlReader { get; }

        public RIBXmlReader(XmlReader xmlReader)
        {
            XmlReader = xmlReader;
        }

        public IEnumerable<T> ForEachChild<T>(Func<RIBXmlReader,T> childParser)
        {
            if (XmlReader.NodeType != XmlNodeType.Element)
                throw new Exception($"Expected a element node type but found {XmlReader.NodeType}.");

            List<T> children = [];

            int startDepth = XmlReader.Depth;
            while (XmlReader.Read())
            {
                XmlReader.MoveToContent();

                bool reachedEndOfElement = XmlReader.Depth == startDepth && XmlReader.NodeType == XmlNodeType.EndElement;
                if (reachedEndOfElement)
                    break;

                if (!XmlReader.IsStartElement())
                    continue;

                bool isGrandChild = XmlReader.Depth > startDepth + 1;
                if (isGrandChild)
                    continue;

                children.Add(childParser(this));
            }

            return children;
        }
    }
}
