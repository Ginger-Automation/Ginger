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
using System.Xml.XPath;

namespace GingerCore.XPathParser {
    public interface IXPathBuilder<Node> {
        // Should be called once per build
        void StartBuild();                 

        // Should be called after build for result tree post-processing
        Node EndBuild(Node result);

        Node String(string value);

        Node Number(string value);

        Node Operator(XPathOperator op, Node left, Node right);

        Node Axis(XPathAxis xpathAxis, XPathNodeType nodeType, string prefix, string name);

        Node JoinStep(Node left, Node right);
        
        // reverseStep is how parser communicates to builder difference between "ansestor[1]" and "(ansestor)[1]" 
        Node Predicate(Node node, Node condition, bool reverseStep);

        Node Variable(string prefix, string name);

        Node Function(string prefix, string name, IList<Node> args);
    }
}
