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
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Ginger.UserControlsLib.TextEditor.Gherkin
{
    class GherkinTextCompletionData : ICompletionData
    {
        public GherkinTextCompletionData(string text)
        {
            this.Text = text;
        }
        
        public ImageSource Image { get; set; }

        public string Text { get; private set; }

        // Use this property if you want to show a fancy UIElement in the drop down list.
        public object Content
        {
            get { return this.Text; }
        }

        public object Description { get; set; }

        public double Priority { get { return 0; } }

        ImageSource ICompletionData.Image { get { throw new NotImplementedException(); } }

        void ICompletionData.Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            string line = textArea.Document.GetText(textArea.Document.GetLineByOffset(completionSegment.EndOffset));
            string lineNoSpace = RemoveAllLineStartSpaces(line);
            string lineAfterHeader = string.Empty;
            if (lineNoSpace.StartsWith("Given"))
                lineAfterHeader = lineNoSpace.Substring(5);
            else if (lineNoSpace.StartsWith("And"))
                lineAfterHeader = lineNoSpace.Substring(3);
            else if (lineNoSpace.StartsWith("When"))
                lineAfterHeader = lineNoSpace.Substring(4);
            else if (lineNoSpace.StartsWith("Then"))
                lineAfterHeader = lineNoSpace.Substring(4);
            else
                lineAfterHeader = lineNoSpace;

            if (lineAfterHeader == " ")
            {
                textArea.Document.Replace(completionSegment.Offset, 0, this.Text);
                return;
            }


            lineAfterHeader = RemoveAllLineStartSpaces(lineAfterHeader);

            if (lineAfterHeader.Length == 0)
                  textArea.Document.Replace(completionSegment.Offset - 1, 1, this.Text);
            else
                textArea.Document.Replace(completionSegment.Offset - lineAfterHeader.Length, lineAfterHeader.Length, this.Text);
        }

        private string RemoveAllLineStartSpaces(string line)
        {
            while (line.StartsWith(" ")|| line.StartsWith("\t"))
                line = line.Substring(1);
            return line;
        }
    }
}
