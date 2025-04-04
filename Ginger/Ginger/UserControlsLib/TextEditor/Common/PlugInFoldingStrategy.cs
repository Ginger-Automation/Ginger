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

using GingerPlugIns.TextEditorLib.Common;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using System.Collections.Generic;

namespace Ginger.UserControlsLib.TextEditor.Common
{
    class PlugInFoldingStrategy : IFoldingStrategy
    {
        private string Tag = string.Empty;
        private List<string> FoldingTitles;

        public PlugInFoldingStrategy(PlugInEditorFoldingStrategy foldingStrategy)
        {
            Tag = foldingStrategy.Tag;
            FoldingTitles = foldingStrategy.FoldingTitles;
        }

        public void UpdateFolding(FoldingManager manager, TextDocument document)
        {
            int firstErrorOffset;
            IEnumerable<NewFolding> newFoldings = CreateNewFoldings(document, out firstErrorOffset);
            manager.UpdateFoldings(newFoldings, firstErrorOffset);
        }

        public IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
        {
            firstErrorOffset = -1;
            return CreateNewFoldings(document);
        }

        public IEnumerable<NewFolding> CreateNewFoldings(ITextSource document)
        {
            List<NewFolding> newFoldings = [];

            //TODO: string Examples = "Examples:";   // will be nested handle like braces

            string line = "";
            int linestart = 0;

            for (int i = 0; i < document.TextLength; i++)
            {
                char c = document.GetCharAt(i);
                line += c;
                // we check when we find new line
                if (c is '\n' or '\r')
                {
                    string lt = line.Trim();
                    foreach (string title in FoldingTitles)
                    {
                        if (lt.StartsWith(title) || lt.StartsWith(Tag))
                        {
                            //Create folding for previous block
                            if (linestart > 0)
                            {
                                int ScenarioEnd = i - line.Length - 1;
                                if (ScenarioEnd > linestart)
                                {
                                    newFoldings.Add(new NewFolding(linestart, ScenarioEnd));
                                }
                            }

                            linestart = i;
                            if (lt.StartsWith(Tag))
                            {
                                linestart++;  // if this is tag line start the folding from next line
                            }

                            line = "";
                        }
                        line = "";
                    }
                }
            }

            // for the last scenario
            if (linestart > 0)
            {
                newFoldings.Add(new NewFolding(linestart, document.TextLength));
            }

            newFoldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
            return newFoldings;
        }
    }
}