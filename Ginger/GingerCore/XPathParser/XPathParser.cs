#region License
/*
Copyright © 2014-2025 European Support Limited

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
using System.Diagnostics;

namespace GingerCore.XPathParser
{
    using System.Globalization;
    using XPathNodeType = System.Xml.XPath.XPathNodeType;

    public class XPathParser<Node>
    {
        private XPathScanner scanner;
        private IXPathBuilder<Node> builder;
        private Stack<int> posInfo = new Stack<int>();

        // Six possible causes of exceptions in the builder:
        // 1. Undefined prefix in a node test.
        // 2. Undefined prefix in a variable reference, or unknown variable.
        // 3. Undefined prefix in a function call, or unknown function, or wrong number/types of arguments.
        // 4. Argument of Union operator is not a node-set.
        // 5. First argument of Predicate is not a node-set.
        // 6. Argument of Axis is not a node-set.

        public Node Parse(string xpathExpr, IXPathBuilder<Node> builder)
        {
            Debug.Assert(this.scanner == null && this.builder == null);
            Debug.Assert(builder != null);

            Node result = default(Node);
            this.scanner = new XPathScanner(xpathExpr);
            this.builder = builder;
            this.posInfo.Clear();

            try
            {
                builder.StartBuild();
                result = ParseExpr();
                scanner.CheckToken(LexKind.Eof);
            }
            catch (XPathParserException e)
            {
                if (e.queryString == null)
                {
                    e.queryString = scanner.Source;
                    PopPosInfo(out e.startChar, out e.endChar);
                }
                throw;
            }
            finally
            {
                result = builder.EndBuild(result);
#if DEBUG
                this.builder = null;
                this.scanner = null;
#endif
            }
            Debug.Assert(posInfo.Count == 0, "PushPosInfo() and PopPosInfo() calls have been unbalanced");
            return result;
        }

        #region Location paths and node tests
        /**************************************************************************************************/
        /*  Location paths and node tests                                                                 */
        /**************************************************************************************************/

        private static bool IsStep(LexKind lexKind)
        {
            return (
                lexKind is LexKind.Dot or
                LexKind.DotDot or
                LexKind.At or
                LexKind.Axis or
                LexKind.Star or
                LexKind.Name   // NodeTest is also Name
            );
        }

        /*
        *   LocationPath ::= RelativeLocationPath | '/' RelativeLocationPath? | '//' RelativeLocationPath
        */
        private Node ParseLocationPath()
        {
            if (scanner.Kind == LexKind.Slash)
            {
                scanner.NextLex();
                Node opnd = builder.Axis(XPathAxis.Root, XPathNodeType.All, null, null);

                if (IsStep(scanner.Kind))
                {
                    opnd = builder.JoinStep(opnd, ParseRelativeLocationPath());
                }
                return opnd;
            }
            else if (scanner.Kind == LexKind.SlashSlash)
            {
                scanner.NextLex();
                return builder.JoinStep(
                    builder.Axis(XPathAxis.Root, XPathNodeType.All, null, null),
                    builder.JoinStep(
                        builder.Axis(XPathAxis.DescendantOrSelf, XPathNodeType.All, null, null),
                        ParseRelativeLocationPath()
                    )
                );
            }
            else
            {
                return ParseRelativeLocationPath();
            }
        }

        /*
        *   RelativeLocationPath ::= Step (('/' | '//') Step)*
        */
        private Node ParseRelativeLocationPath()
        {
            Node opnd = ParseStep();
            if (scanner.Kind == LexKind.Slash)
            {
                scanner.NextLex();
                opnd = builder.JoinStep(opnd, ParseRelativeLocationPath());
            }
            else if (scanner.Kind == LexKind.SlashSlash)
            {
                scanner.NextLex();
                opnd = builder.JoinStep(opnd,
                    builder.JoinStep(
                        builder.Axis(XPathAxis.DescendantOrSelf, XPathNodeType.All, null, null),
                        ParseRelativeLocationPath()
                    )
                );
            }
            return opnd;
        }

        /*
        *   Step ::= '.' | '..' | (AxisName '::' | '@')? NodeTest Predicate*
        */
        private Node ParseStep()
        {
            Node opnd;
            if (LexKind.Dot == scanner.Kind)
            {                  // '.'
                scanner.NextLex();
                opnd = builder.Axis(XPathAxis.Self, XPathNodeType.All, null, null);
                if (LexKind.LBracket == scanner.Kind)
                {
                    throw scanner.PredicateAfterDotException();
                }
            }
            else if (LexKind.DotDot == scanner.Kind)
            {        // '..'
                scanner.NextLex();
                opnd = builder.Axis(XPathAxis.Parent, XPathNodeType.All, null, null);
                if (LexKind.LBracket == scanner.Kind)
                {
                    throw scanner.PredicateAfterDotDotException();
                }
            }
            else
            {                                            // (AxisName '::' | '@')? NodeTest Predicate*
                XPathAxis axis;
                switch (scanner.Kind)
                {
                    case LexKind.Axis:                              // AxisName '::'
                        axis = scanner.Axis;
                        scanner.NextLex();
                        scanner.NextLex();
                        break;
                    case LexKind.At:                                // '@'
                        axis = XPathAxis.Attribute;
                        scanner.NextLex();
                        break;
                    case LexKind.Name:
                    case LexKind.Star:
                        // NodeTest must start with Name or '*'
                        axis = XPathAxis.Child;
                        break;
                    default:
                        throw scanner.UnexpectedTokenException(scanner.RawValue);
                }

                opnd = ParseNodeTest(axis);

                while (LexKind.LBracket == scanner.Kind)
                {
                    opnd = builder.Predicate(opnd, ParsePredicate(), IsReverseAxis(axis));
                }
            }
            return opnd;
        }

        private static bool IsReverseAxis(XPathAxis axis)
        {
            return (
                axis is XPathAxis.Ancestor or XPathAxis.Preceding or
                XPathAxis.AncestorOrSelf or XPathAxis.PrecedingSibling
            );
        }

        /*
        *   NodeTest ::= NameTest | ('comment' | 'text' | 'node') '(' ')' | 'processing-instruction' '('  Literal? ')'
        *   NameTest ::= '*' | NCName ':' '*' | QName
        */
        private Node ParseNodeTest(XPathAxis axis)
        {
            XPathNodeType nodeType;
            string nodePrefix, nodeName;

            int startChar = scanner.LexStart;
            InternalParseNodeTest(scanner, axis, out nodeType, out nodePrefix, out nodeName);
            PushPosInfo(startChar, scanner.PrevLexEnd);
            Node result = builder.Axis(axis, nodeType, nodePrefix, nodeName);
            PopPosInfo();
            return result;
        }

        private static bool IsNodeType(XPathScanner scanner)
        {
            return scanner.Prefix.Length == 0 && (
                scanner.Name == "node" ||
                scanner.Name == "text" ||
                scanner.Name == "processing-instruction" ||
                scanner.Name == "comment"
            );
        }

        private static XPathNodeType PrincipalNodeType(XPathAxis axis)
        {
            return (
                axis == XPathAxis.Attribute ? XPathNodeType.Attribute :
                axis == XPathAxis.Namespace ? XPathNodeType.Namespace :
                /*else*/                      XPathNodeType.Element
            );
        }

        private static void InternalParseNodeTest(XPathScanner scanner, XPathAxis axis, out XPathNodeType nodeType, out string nodePrefix, out string nodeName)
        {
            switch (scanner.Kind)
            {
                case LexKind.Name:
                    if (scanner.CanBeFunction && IsNodeType(scanner))
                    {
                        nodePrefix = null;
                        nodeName = null;
                        switch (scanner.Name)
                        {
                            case "comment": nodeType = XPathNodeType.Comment; break;
                            case "text": nodeType = XPathNodeType.Text; break;
                            case "node": nodeType = XPathNodeType.All; break;
                            default:
                                Debug.Assert(scanner.Name == "processing-instruction");
                                nodeType = XPathNodeType.ProcessingInstruction;
                                break;
                        }

                        scanner.NextLex();
                        scanner.PassToken(LexKind.LParens);

                        if (nodeType == XPathNodeType.ProcessingInstruction)
                        {
                            if (scanner.Kind != LexKind.RParens)
                            {  // 'processing-instruction' '(' Literal ')'
                                scanner.CheckToken(LexKind.String);
                                // It is not needed to set nodePrefix here, but for our current implementation
                                // comparing whole QNames is faster than comparing just local names
                                nodePrefix = string.Empty;
                                nodeName = scanner.StringValue;
                                scanner.NextLex();
                            }
                        }

                        scanner.PassToken(LexKind.RParens);
                    }
                    else
                    {
                        nodePrefix = scanner.Prefix;
                        nodeName = scanner.Name;
                        nodeType = PrincipalNodeType(axis);
                        scanner.NextLex();
                        if (nodeName == "*")
                        {
                            nodeName = null;
                        }
                    }
                    break;
                case LexKind.Star:
                    nodePrefix = null;
                    nodeName = null;
                    nodeType = PrincipalNodeType(axis);
                    scanner.NextLex();
                    break;
                default:
                    throw scanner.NodeTestExpectedException(scanner.RawValue);
            }
        }

        /*
        *   Predicate ::= '[' Expr ']'
        */
        private Node ParsePredicate()
        {
            scanner.PassToken(LexKind.LBracket);
            Node opnd = ParseExpr();
            scanner.PassToken(LexKind.RBracket);
            return opnd;
        }
        #endregion

        #region Expressions
        /**************************************************************************************************/
        /*  Expressions                                                                                   */
        /**************************************************************************************************/

        /*
        *   Expr   ::= OrExpr
        *   OrExpr ::= AndExpr ('or' AndExpr)*
        *   AndExpr ::= EqualityExpr ('and' EqualityExpr)*
        *   EqualityExpr ::= RelationalExpr (('=' | '!=') RelationalExpr)*
        *   RelationalExpr ::= AdditiveExpr (('<' | '>' | '<=' | '>=') AdditiveExpr)*
        *   AdditiveExpr ::= MultiplicativeExpr (('+' | '-') MultiplicativeExpr)*
        *   MultiplicativeExpr ::= UnaryExpr (('*' | 'div' | 'mod') UnaryExpr)*
        *   UnaryExpr ::= ('-')* UnionExpr
        */
        private Node ParseExpr()
        {
            return ParseSubExpr(/*callerPrec:*/0);
        }

        private Node ParseSubExpr(int callerPrec)
        {
            XPathOperator op;
            Node opnd;

            // Check for unary operators
            if (scanner.Kind == LexKind.Minus)
            {
                op = XPathOperator.UnaryMinus;
                int opPrec = XPathOperatorPrecedence[(int)op];
                scanner.NextLex();
                opnd = builder.Operator(op, ParseSubExpr(opPrec), default(Node));
            }
            else
            {
                opnd = ParseUnionExpr();
            }

            // Process binary operators
            while (true)
            {
                op = (scanner.Kind <= LexKind.LastOperator) ? (XPathOperator)scanner.Kind : XPathOperator.Unknown;
                int opPrec = XPathOperatorPrecedence[(int)op];
                if (opPrec <= callerPrec)
                {
                    return opnd;
                }

                // Operator's precedence is greater than the one of our caller, so process it here
                scanner.NextLex();
                opnd = builder.Operator(op, opnd, ParseSubExpr(/*callerPrec:*/opPrec));
            }
        }

        private static int[] XPathOperatorPrecedence = {
            /*Unknown    */ 0,
            /*Or         */ 1,
            /*And        */ 2,
            /*Eq         */ 3,
            /*Ne         */ 3,
            /*Lt         */ 4,
            /*Le         */ 4,
            /*Gt         */ 4,
            /*Ge         */ 4,
            /*Plus       */ 5,
            /*Minus      */ 5,
            /*Multiply   */ 6,
            /*Divide     */ 6,
            /*Modulo     */ 6,
            /*UnaryMinus */ 7,
            /*Union      */ 8,  // Not used
        };

        /*
        *   UnionExpr ::= PathExpr ('|' PathExpr)*
        */
        private Node ParseUnionExpr()
        {
            int startChar = scanner.LexStart;
            Node opnd1 = ParsePathExpr();

            if (scanner.Kind == LexKind.Union)
            {
                PushPosInfo(startChar, scanner.PrevLexEnd);
                opnd1 = builder.Operator(XPathOperator.Union, default(Node), opnd1);
                PopPosInfo();

                while (scanner.Kind == LexKind.Union)
                {
                    scanner.NextLex();
                    startChar = scanner.LexStart;
                    Node opnd2 = ParsePathExpr();
                    PushPosInfo(startChar, scanner.PrevLexEnd);
                    opnd1 = builder.Operator(XPathOperator.Union, opnd1, opnd2);
                    PopPosInfo();
                }
            }
            return opnd1;
        }

        /*
        *   PathExpr ::= LocationPath | FilterExpr (('/' | '//') RelativeLocationPath )?
        */
        private Node ParsePathExpr()
        {
            // Here we distinguish FilterExpr from LocationPath - the former starts with PrimaryExpr
            if (IsPrimaryExpr())
            {
                int startChar = scanner.LexStart;
                Node opnd = ParseFilterExpr();
                int endChar = scanner.PrevLexEnd;

                if (scanner.Kind == LexKind.Slash)
                {
                    scanner.NextLex();
                    PushPosInfo(startChar, endChar);
                    opnd = builder.JoinStep(opnd, ParseRelativeLocationPath());
                    PopPosInfo();
                }
                else if (scanner.Kind == LexKind.SlashSlash)
                {
                    scanner.NextLex();
                    PushPosInfo(startChar, endChar);
                    opnd = builder.JoinStep(opnd,
                        builder.JoinStep(
                            builder.Axis(XPathAxis.DescendantOrSelf, XPathNodeType.All, null, null),
                            ParseRelativeLocationPath()
                        )
                    );
                    PopPosInfo();
                }
                return opnd;
            }
            else
            {
                return ParseLocationPath();
            }
        }

        /*
        *   FilterExpr ::= PrimaryExpr Predicate*
        */
        private Node ParseFilterExpr()
        {
            int startChar = scanner.LexStart;
            Node opnd = ParsePrimaryExpr();
            int endChar = scanner.PrevLexEnd;

            while (scanner.Kind == LexKind.LBracket)
            {
                PushPosInfo(startChar, endChar);
                opnd = builder.Predicate(opnd, ParsePredicate(), /*reverseStep:*/false);
                PopPosInfo();
            }
            return opnd;
        }

        private bool IsPrimaryExpr()
        {
            return (
                scanner.Kind == LexKind.String ||
                scanner.Kind == LexKind.Number ||
                scanner.Kind == LexKind.Dollar ||
                scanner.Kind == LexKind.LParens ||
                scanner.Kind == LexKind.Name && scanner.CanBeFunction && !IsNodeType(scanner)
            );
        }

        /*
        *   PrimaryExpr ::= Literal | Number | VariableReference | '(' Expr ')' | FunctionCall
        */
        private Node ParsePrimaryExpr()
        {
            Debug.Assert(IsPrimaryExpr());
            Node opnd;
            switch (scanner.Kind)
            {
                case LexKind.String:
                    opnd = builder.String(scanner.StringValue);
                    scanner.NextLex();
                    break;
                case LexKind.Number:
                    opnd = builder.Number(scanner.RawValue);
                    scanner.NextLex();
                    break;
                case LexKind.Dollar:
                    int startChar = scanner.LexStart;
                    scanner.NextLex();
                    scanner.CheckToken(LexKind.Name);
                    PushPosInfo(startChar, scanner.LexStart + scanner.LexSize);
                    opnd = builder.Variable(scanner.Prefix, scanner.Name);
                    PopPosInfo();
                    scanner.NextLex();
                    break;
                case LexKind.LParens:
                    scanner.NextLex();
                    opnd = ParseExpr();
                    scanner.PassToken(LexKind.RParens);
                    break;
                default:
                    Debug.Assert(
                        scanner.Kind == LexKind.Name && scanner.CanBeFunction && !IsNodeType(scanner),
                        "IsPrimaryExpr() returned true, but the lexeme is not recognized"
                    );
                    opnd = ParseFunctionCall();
                    break;
            }
            return opnd;
        }

        /*
        *   FunctionCall ::= FunctionName '(' (Expr (',' Expr)* )? ')'
        */
        private Node ParseFunctionCall()
        {
            List<Node> argList = [];
            string name = scanner.Name;
            string prefix = scanner.Prefix;
            int startChar = scanner.LexStart;

            scanner.PassToken(LexKind.Name);
            scanner.PassToken(LexKind.LParens);

            if (scanner.Kind != LexKind.RParens)
            {
                while (true)
                {
                    argList.Add(ParseExpr());
                    if (scanner.Kind != LexKind.Comma)
                    {
                        scanner.CheckToken(LexKind.RParens);
                        break;
                    }
                    scanner.NextLex();  // move off the ','
                }
            }

            scanner.NextLex();          // move off the ')'
            PushPosInfo(startChar, scanner.PrevLexEnd);
            Node result = builder.Function(prefix, name, argList);
            PopPosInfo();
            return result;
        }
        #endregion

        /**************************************************************************************************/
        /*  Helper methods                                                                                */
        /**************************************************************************************************/

        private void PushPosInfo(int startChar, int endChar)
        {
            posInfo.Push(startChar);
            posInfo.Push(endChar);
        }

        private void PopPosInfo()
        {
            posInfo.Pop();
            posInfo.Pop();
        }

        private void PopPosInfo(out int startChar, out int endChar)
        {
            endChar = posInfo.Pop();
            startChar = posInfo.Pop();
        }

        private static double ToDouble(string str)
        {
            double d;
            if (double.TryParse(str, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingWhite, NumberFormatInfo.InvariantInfo, out d))
            {
                return d;
            }
            return double.NaN;
        }

        public void GetOKPath()
        {
            //TODO: return what part was OK so can display to the user
            throw new System.NotImplementedException();
        }
    }
}
