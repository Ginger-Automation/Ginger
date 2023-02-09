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

using Gherkin;
using Ginger.Properties;

namespace Ginger.UserControlsLib.TextEditor.Gherkin
{
    public class GherkinParserException
    {
        public enum eErrorType
        {
            Error,
            Warning
        }
        public eErrorType ErrorType { get; set; }
        public int Line { get; set; }

        public int Column { get; set; }

        public string Error { get; set; }

        public static class Fields
        {
            public static string Line = "Line";
            public static string Column = "Column";
            public static string Error = "Error";
            public static string ErrorType = "ErrorType";
            public static string ErrorImage = "ErrorImage";
        }

        public System.Drawing.Image ErrorImage
        {
            get
            {
                if (ErrorType == eErrorType.Error)
                    return Resources._ErrorRed_16x16;
                else
                    return Resources._ErrorYellow_16x16;
            }
        }

        private ParserException mParserException;

        public GherkinParserException(ParserException PE)
        {
            ErrorType = eErrorType.Error;
            this.mParserException = PE;
            Line = mParserException.Location.Line;
            Column = mParserException.Location.Column;
            Error = mParserException.Message;
        }
 
        public GherkinParserException(int Line, int Column, string WarningMessage)
        {
            ErrorType = eErrorType.Warning;
            this.Line = Line;
            this.Column = Column;
            this.Error = WarningMessage;
        }
    }
}
