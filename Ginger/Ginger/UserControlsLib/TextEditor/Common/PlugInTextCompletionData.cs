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
using System.Collections.Generic;

namespace Ginger.UserControlsLib.TextEditor.Common
{
    internal class PlugInTextCompletionData : ICompletionData
    {
        public PlugInTextCompletionData(string keyWord, List<string> keyWords)
        {
            this.Text = keyWord;
        }

        public string Text { get; private set; }
        public ImageSource Image { get; set; }

        public object Content
        {
            get { return this.Text; }
        }

        public object Description { get; set; }

        public double Priority { get { return 0; } }

        void ICompletionData.Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            string line = textArea.Document.GetText(textArea.Document.GetLineByOffset(completionSegment.EndOffset));
            textArea.Document.Replace(completionSegment.Offset - 1, 1, this.Text);
        }
    }
}