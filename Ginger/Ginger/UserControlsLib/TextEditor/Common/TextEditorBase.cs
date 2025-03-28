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

using Amdocs.Ginger.Plugin.Core;
using Ginger.UserControlsLib.TextEditor.Common;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Xml;

namespace Ginger.UserControlsLib.TextEditor
{
    public abstract class TextEditorBase
    {
        // TODO: clean or virtual        

        internal List<string> mExtensions = [];
        public abstract List<string> Extensions { get; }
        public abstract string Descritpion { get; }
        public abstract Image Icon { get; }
        public virtual ITextEditorPage EditorPage
        {
            get
            {
                // we return null so it will use the default text editor, however editor can override and return it's own page editor like Gherkin does                
                return null;
            }
        }
        public abstract IHighlightingDefinition HighlightingDefinition { get; }
        public abstract IFoldingStrategy FoldingStrategy { get; }

        public abstract Page GetSelectedContentPageEditor(SelectedContentArgs SelectedContentArgs);

        public abstract void UpdateSelectedContent();
        public abstract List<ICompletionData> GetCompletionData(string txt, SelectedContentArgs SelectedContentArgs);

        internal IHighlightingDefinition GetHighlightingDefinitionFromResource(byte[] xshd)
        {
            MemoryStream ms = new MemoryStream(xshd);
            using (XmlReader reader = XmlReader.Create(ms))
            {
                IHighlightingDefinition HighlightingDefinition = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
                return HighlightingDefinition;
            }
        }

        public abstract List<ITextEditorToolBarItem> Tools { get; }

        public virtual string Title()
        {
            return null;
        }
    }
}
