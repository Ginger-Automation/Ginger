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

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Windows;
using System.Windows.Media;

namespace Ginger.UserControlsLib.TextEditor.Common
{
    public class ColorizeAvalonEdit : DocumentColorizingTransformer
    {
        //TODO: add me to textEditor
        // enable to select the word to highlight - removed hard coded abc
        protected override void ColorizeLine(DocumentLine line)
        {
            int lineStartOffset = line.Offset;
            string text = CurrentContext.Document.GetText(line);
            int start = 0;
            int index;
            while ((index = text.IndexOf("abc", start)) >= 0)
            {
                base.ChangeLinePart(
                    lineStartOffset + index, // startOffset
                    lineStartOffset + index + 10, // endOffset
                    (VisualLineElement element) =>
                    {
                        // This lambda gets called once for every VisualLineElement
                        // between the specified offsets.
                        System.Windows.Media.Typeface tf = element.TextRunProperties.Typeface;
                        // Replace the typeface with a modified version of
                        // the same typeface
                        element.TextRunProperties.SetTypeface(new Typeface(
                                    tf.FontFamily,
                                    FontStyles.Italic,
                                    FontWeights.Bold,
                                    tf.Stretch
                                ));
                    });
                start = index + 1; // search for next occurrence
            }
        }
    }
}
