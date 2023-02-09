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
using System.Diagnostics;
using System.Xml.XPath;

namespace GingerCore.XPathParser {
    public class XPathStringBuilder : IXPathBuilder<string> {
        #region IXPathBuilder<string> Members

        public void StartBuild() { }

        public string EndBuild(string result) {
            return result;
        }

        public string String(string value) {
            return "'" + value + "'";
        }

        public string Number(string value) {
            return value;
        }

        public string Operator(XPathOperator op, string left, string right) {
            Debug.Assert(op != XPathOperator.Union);
            if (op == XPathOperator.UnaryMinus) {
                return "-" + left;
            }
            return left + opStrings[(int)op] + right;
        }

        public string Axis(XPathAxis xpathAxis, XPathNodeType nodeType, string prefix, string name) {
            string nodeTest;
            switch (nodeType) {
            case XPathNodeType.ProcessingInstruction:
                Debug.Assert(prefix == "");
                nodeTest = "processing-instruction(" + name + ")";
                break;
            case XPathNodeType.Text:
                Debug.Assert(prefix == null && name == null);
                nodeTest = "text()";
                break;
            case XPathNodeType.Comment:
                Debug.Assert(prefix == null && name == null);
                nodeTest = "comment()";
                break;
            case XPathNodeType.All:
                nodeTest = "node()";
                break;
            case XPathNodeType.Attribute:
            case XPathNodeType.Element:
            case XPathNodeType.Namespace:
                nodeTest = QNameOrWildcard(prefix, name);
                break;
            default:
                throw new ArgumentException("unexpected XPathNodeType", "XPathNodeType");
            }
            return axisStrings[(int)xpathAxis] + nodeTest;
        }

        public string JoinStep(string left, string right) {
            return left + '/' + right;
        }

        public string Predicate(string node, string condition, bool reverseStep) {
            if (!reverseStep) {
                // In this method we don't know how axis was represented in original XPath and the only 
                // difference between ancestor::*[2] and (ancestor::*)[2] is the reverseStep parameter.
                // to not store the axis from previous builder events we simply wrap node in the () here.
                node = '(' + node + ')';
            }
            return node + '[' + condition + ']';
        }

        public string Variable(string prefix, string name) {
            return '$' + QName(prefix, name);
        }

        public string Function(string prefix, string name, IList<string> args) {
            string result = QName(prefix, name) + '(';
            for (int i = 0; i < args.Count; i++) {
                if (i != 0) {
                    result += ',';
                }
                result += args[i];
            }
            result += ')';
            return result;
        }

        private static string QName(string prefix, string localName) {
            if (prefix == null) {
                throw new ArgumentNullException("prefix");
            }
            if (localName == null) {
                throw new ArgumentNullException("localName");
            }
            return prefix == "" ? localName : prefix + ':' + localName;
        }

        private static string QNameOrWildcard(string prefix, string localName) {
            if (prefix == null) {
                Debug.Assert(localName == null);
                return "*";
            }
            if (localName == null) {
                Debug.Assert(prefix != "");
                return prefix + ":*";
            }
            return prefix == "" ? localName : prefix + ':' + localName;
        }

        #endregion

        string[] opStrings = { 
            /* Unknown    */ " Unknown ",
            /* Or         */ " or " ,
            /* And        */ " and ",
            /* Eq         */ "="    ,
            /* Ne         */ "!="   ,
            /* Lt         */ "<"    ,
            /* Le         */ "<="   ,
            /* Gt         */ ">"    ,
            /* Ge         */ ">="   ,
            /* Plus       */ "+"    ,
            /* Minus      */ "-"    ,
            /* Multiply   */ "*"    ,
            /* Divide     */ " div ",
            /* Modulo     */ " mod ",
            /* UnaryMinus */ "-"    ,
            /* Union      */ "|"  
        };

        string[] axisStrings = {
            /*Unknown          */ "Unknown::"           ,
            /*Ancestor         */ "ancestor::"          ,
            /*AncestorOrSelf   */ "ancestor-or-self::"  ,
            /*Attribute        */ "attribute::"         ,
            /*Child            */ "child::"             ,
            /*Descendant       */ "descendant::"        ,
            /*DescendantOrSelf */ "descendant-or-self::",
            /*Following        */ "following::"         ,
            /*FollowingSibling */ "following-sibling::" ,
            /*Namespace        */ "namespace::"         ,
            /*Parent           */ "parent::"            ,
            /*Preceding        */ "preceding::"         ,
            /*PrecedingSibling */ "preceding-sibling::" ,
            /*Self             */ "self::"              ,
            /*Root             */ "root::"              ,
        };
    }
}

