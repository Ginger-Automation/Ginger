#region License
/*
Copyright © 2014-2018 European Support Limited

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

using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Ginger.UserControlsLib.TextEditor.Common;
using ICSharpCode.AvalonEdit.CodeCompletion;
using GingerPlugIns.TextEditorLib;
using Amdocs.Ginger.Plugin.Core;

namespace Ginger.UserControlsLib.TextEditor.Office
{
    public class OfficeDcoumentEditor : TextEditorBase
    {
        public override string Descritpion { get { return "Office Document"; } }
        public override Image Icon { get
            {
                return null;
            }
        }

        public override List<string> Extensions
        {
            get
            {
                if (mExtensions.Count == 0)
                {
                    mExtensions.Add(".doc");
                    mExtensions.Add(".docx");
                    mExtensions.Add(".xls");
                    mExtensions.Add(".xlsx");
                }
                return mExtensions;
            }
        }


        public override ITextEditorPage EditorPage { get {
                return new OfficeDocumentPage();                
            } }

        public override IHighlightingDefinition HighlightingDefinition
        {
            get
            {
                return null;
            }
        }

        public override IFoldingStrategy FoldingStrategy
        {
            get
            {
                return null;

            }
        }

        public override List<ICompletionData> GetCompletionData(string txt, SelectedContentArgs SelectedContentArgs)
        {
            return null;
        }

        public override Page GetSelectedContentPageEditor(SelectedContentArgs SelectedContentArgs)
        {
            throw new NotImplementedException();
        }

        public override void UpdateSelectedContent()
        {
        }

        // if we want to add tool bar item and handler this is the place
        public override List<ITextEditorToolBarItem> Tools
        {
            get { return null; }
        }
    }
}