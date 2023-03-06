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

using System.Text;
using System;

namespace GingerCore.XPathParser {
    public class XPathParserException : System.Exception {
        public string queryString;
        public int    startChar;
        public int    endChar;

        public XPathParserException(string queryString, int startChar, int endChar, string message) : base(message) {
            this.queryString = queryString;
            this.startChar = startChar;
            this.endChar = endChar;
        }

        private enum TrimType {
            Left,
            Right,
            Middle,
        }

        // This function is used to prevent long quotations in error messages
        private static void AppendTrimmed(StringBuilder sb, string value, int startIndex, int count, TrimType trimType) {
            const int    TrimSize   = 32;
            const string TrimMarker = "...";

            if (count <= TrimSize) {
                sb.Append(value, startIndex, count);
            } else {
                switch (trimType) {
                case TrimType.Left:
                    sb.Append(TrimMarker);
                    sb.Append(value, startIndex + count - TrimSize, TrimSize);
                    break;
                case TrimType.Right:
                    sb.Append(value, startIndex, TrimSize);
                    sb.Append(TrimMarker);
                    break;
                case TrimType.Middle:
                    sb.Append(value, startIndex, TrimSize / 2);
                    sb.Append(TrimMarker);
                    sb.Append(value, startIndex + count - TrimSize / 2, TrimSize / 2);
                    break;
                }
            }
        }

        internal string MarkOutError() {
            if (queryString == null || queryString.Trim(' ').Length == 0) {
                return null;
            }

            int len = endChar - startChar;
            StringBuilder sb = new StringBuilder();

            AppendTrimmed(sb, queryString, 0, startChar, TrimType.Left);
            if (len > 0) {
                sb.Append(" -->");
                AppendTrimmed(sb, queryString, startChar, len, TrimType.Middle);
            }

            sb.Append("<-- ");
            AppendTrimmed(sb, queryString, endChar, queryString.Length - endChar, TrimType.Right);

            return sb.ToString();
        }

        private string FormatDetailedMessage() {
            string message = Message;
            string error = MarkOutError();

            if (error != null && error.Length > 0) {
                if (message.Length > 0) {
                    message += Environment.NewLine;
                }
                message += error;
            }
            return message;
        }

        public override string ToString() {
            string result = this.GetType().FullName;
            string info = FormatDetailedMessage();
            if (info != null && info.Length > 0) {
                result += ": " + info;
            }
            if (StackTrace != null) {
                result += Environment.NewLine + StackTrace;
            }
            return result;
        }
    }
}