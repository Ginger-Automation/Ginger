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

using System.Collections.Generic;
using System.Xml.Linq;

namespace GingerCore.XPathParser
{
    public class XPathTreeBuilder : IXPathBuilder<XElement>
    {
        public void StartBuild() { }

        public XElement EndBuild(XElement result)
        {
            return result;
        }

        public XElement String(string value)
        {
            return new XElement("string", new XAttribute("value", value));
        }

        public XElement Number(string value)
        {
            return new XElement("number", new XAttribute("value", value));
        }

        public XElement Operator(XPathOperator op, XElement left, XElement right)
        {
            if (op == XPathOperator.UnaryMinus)
            {
                return new XElement("negate", left);
            }
            return new XElement(op.ToString(), left, right);
        }

        public XElement Axis(XPathAxis xpathAxis, System.Xml.XPath.XPathNodeType nodeType, string prefix, string name)
        {
            return new XElement(xpathAxis.ToString(),
                new XAttribute("nodeType", nodeType.ToString()),
                new XAttribute("prefix", prefix ?? "(null)"),
                new XAttribute("name", name ?? "(null)")
            );
        }

        public XElement JoinStep(XElement left, XElement right)
        {
            return new XElement("step", left, right);
        }

        public XElement Predicate(XElement node, XElement condition, bool reverseStep)
        {
            return new XElement("predicate", new XAttribute("reverse", reverseStep),
                node, condition
            );
        }

        public XElement Variable(string prefix, string name)
        {
            return new XElement("variable",
                new XAttribute("prefix", prefix ?? "(null)"),
                new XAttribute("name", name ?? "(null)")
            );
        }

        public XElement Function(string prefix, string name, IList<XElement> args)
        {
            XElement xe = new XElement("variable",
                new XAttribute("prefix", prefix ?? "(null)"),
                new XAttribute("name", name ?? "(null)")
            );
            foreach (XElement e in args)
            {
                xe.Add(e);
            }
            return xe;
        }
    }
}