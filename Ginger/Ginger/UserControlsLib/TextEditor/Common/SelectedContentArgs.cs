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

using ICSharpCode.AvalonEdit.Folding;
using System.Collections.ObjectModel;

namespace Ginger.UserControlsLib.TextEditor.Common
{
    public class SelectedContentArgs
    {
        private ICSharpCode.AvalonEdit.TextEditor mTextEditor;
        public ICSharpCode.AvalonEdit.TextEditor TextEditor { get { return mTextEditor; } set { mTextEditor = value; } }

        private FoldingManager mFoldingManager;
        public FoldingManager FoldingManager { get { return mFoldingManager; } set { mFoldingManager = value; } }

        public int CaretPosition 
        {
            get
            {
                return mTextEditor.CaretOffset;
            }
        }


        public ReadOnlyCollection<FoldingSection> FoldingsContaining
        {
            get
            {
                int pos = mTextEditor.CaretOffset;
                ReadOnlyCollection<FoldingSection> list = null;
                if (mFoldingManager != null)
                {
                    list = mFoldingManager.GetFoldingsContaining(pos);
                }
                return list;
            }
        }

        public int StartPos { get; set; }
        public int EndPos { get; set; }
        public int Length { get { return EndPos - StartPos + 1; }  }

        internal string GetFoldingTitle()
        {
            return mFoldingManager.GetFoldingsContaining(mTextEditor.CaretOffset)[0].Title;
        }

        internal int GetFoldingOffSet()
        {
            return ((ICSharpCode.AvalonEdit.Document.ISegment)mFoldingManager.GetFoldingsContaining(mTextEditor.CaretOffset)[0]).Offset;
        }



        internal ReadOnlyCollection<FoldingSection> GetFoldingsAtCaretPosition()
        {
            ReadOnlyCollection<FoldingSection> list = null;
            if (mFoldingManager!=null) list = mFoldingManager.GetFoldingsContaining(mTextEditor.CaretOffset);
            return list;
        }

        internal int GetCaretPosition()
        {
            return mTextEditor.CaretOffset;
        }

        public string CaretLineText()
        {            
            string txt = mTextEditor.Document.GetText(mTextEditor.Document.GetLineByOffset(mTextEditor.CaretOffset));
            return txt;
        }

        internal bool IsAtStartOfLine()
        {
            int i = mTextEditor.CaretOffset -1;
            
            while (i>0)
            {
                i--;
                char c = mTextEditor.Document.GetCharAt(i);
                if (c == ' ') continue;
                if (c == '\r') return true;
                if (c == '\n') return true;
                return false;
            }

            return false;
        }
    }
}
