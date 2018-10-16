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

using Amdocs.Ginger.Common;
using Ginger.GherkinLib;
using Ginger.UserControlsLib.TextEditor.Common;
using GingerCore;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GingerPlugIns.TextEditorLib;
using Ginger.TagsLib;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Plugin.Core;

namespace Ginger.UserControlsLib.TextEditor.Gherkin
{
    public class GherkinDcoumentEditor : TextEditorBase
    {
        public override string Descritpion { get { return "Gherkin Feature File"; } }
        public override Image Icon { get { throw new NotImplementedException(); } }

        public override List<string> Extensions
        {
            get
            {
                if (mExtensions.Count == 0)
                {
                    mExtensions.Add(".feature");
                }
                return mExtensions;
            }
        }

        public override ITextEditorPage EditorPage { get {
                return new GherkinPage();                
            } }

        public override IHighlightingDefinition HighlightingDefinition
        {
            get
            {
                return GetHighlightingDefinitionFromResource(Properties.Resources.GherkinHighlighting);                
            }
        }

        public override IFoldingStrategy FoldingStrategy
        {
            get
            {
                return new GherkinFoldingStrategy();

            }
        }

        public ObservableList<GherkinStep> OptimizedSteps { get; set; }
        public ObservableList<GherkinTag> OptimizedTags { get; set; }

        //Added By Nethan
        public static int CompletionWindowSize = 0;

        public override List<ICompletionData> GetCompletionData(string txt, SelectedContentArgs SelectedContentArgs)
        {
            List<ICompletionData> list = new List<ICompletionData>();
            
                //TODO: fix me - only when in the begining of line - allow lower case too
            string CurrentLine = SelectedContentArgs.CaretLineText();
            while (CurrentLine.StartsWith(" ") || CurrentLine.StartsWith("\t"))
                CurrentLine = CurrentLine.Substring(1);

            bool IsAtStartOfLine = CurrentLine.Length == 1;
            bool IsAfterKeyWord = CurrentLine.StartsWith("Given") || CurrentLine.StartsWith("When") || CurrentLine.StartsWith("Then") || CurrentLine.StartsWith("And");
            if (SelectedContentArgs.IsAtStartOfLine() || IsAtStartOfLine)
            {
                if (txt.ToUpper() == "G") list.Add(new GherkinTextCompletionData("Given"));
                if (txt.ToUpper() == "W") list.Add(new GherkinTextCompletionData("When"));
                if (txt.ToUpper() == "T") list.Add(new GherkinTextCompletionData("Then"));
                if (txt.ToUpper() == "A") list.Add(new GherkinTextCompletionData("And"));

                if (txt == " ")
                {
                    list.Add(new GherkinTextCompletionData("Given"));
                    list.Add(new GherkinTextCompletionData("When"));
                    list.Add(new GherkinTextCompletionData("Then"));
                    list.Add(new GherkinTextCompletionData("And"));
                }

                if (txt.ToUpper() == "S")
                {
                    list.Add(new GherkinTextCompletionData("Scenario:"));
                    list.Add(new GherkinTextCompletionData("Scenario Outline:"));
                }
            }
            else if (IsAfterKeyWord)
            {
                if (txt == " ")
                {
                    if (OptimizedSteps != null)
                    foreach (GherkinStep GH in OptimizedSteps)
                    {

                        if (CompletionWindowSize < GH.Text.Length)
                        {
                            CompletionWindowSize = GH.Text.Length;
                        }
                        list.Add(GETVTDM(GH));
                    }

                    return list;
                }

                while (CurrentLine.StartsWith(" ") || CurrentLine.StartsWith("\t"))
                    CurrentLine = CurrentLine.Substring(1);

                if (CurrentLine.StartsWith("Given"))
                    CurrentLine = CurrentLine.Substring(5);
                if (CurrentLine.StartsWith("When"))
                    CurrentLine = CurrentLine.Substring(4);
                if (CurrentLine.StartsWith("Then"))
                    CurrentLine = CurrentLine.Substring(4);
                if (CurrentLine.StartsWith("And"))
                    CurrentLine = CurrentLine.Substring(3);

                while (CurrentLine.StartsWith(" ") || CurrentLine.StartsWith("\t"))
                    CurrentLine = CurrentLine.Substring(1);

                if (CurrentLine != string.Empty && OptimizedSteps != null)
                    foreach (GherkinStep GH in OptimizedSteps)
                    {
                        if (GH.Text.ToUpper().Contains(CurrentLine.ToUpper()))
                        {
                            if (CompletionWindowSize < GH.Text.Length)
                            {
                                CompletionWindowSize = GH.Text.Length;
                            }
                            list.Add(GETVTDM(GH));
                        }
                    }
            }
            //TODO:: Need to check usage
            /*if (CurrentLine.Contains("@") && CurrentLine.LastIndexOf(" ") != -1)
                CurrentLine = CurrentLine.Substring(CurrentLine.LastIndexOf(" "));
            while (CurrentLine.StartsWith(" ") || CurrentLine.StartsWith("\t"))
                CurrentLine = CurrentLine.Substring(1);*/

            if (CurrentLine.Contains("@"))
            {
                foreach (GherkinTag GT in OptimizedTags)
                {
                    if (!CurrentLine.ToUpper().Contains(GT.Name.ToUpper()))
                    {
                        if (CompletionWindowSize < GT.Name.Length)
                        {
                            CompletionWindowSize = GT.Name.Length;
                        }

                        ICompletionData TCD = list.Where(x => x.Text == GT.Name).FirstOrDefault();
                        if (TCD == null)
                            list.Add(GetTagName(GT));
                    }

                }                
                foreach (RepositoryItemTag tag in App.UserProfile.Solution.Tags )
                {
                    string tagname = "@" + tag.Name;
                    if (!CurrentLine.ToUpper().Contains(tagname.ToUpper()))
                    {
                        if (CompletionWindowSize < tag.Name.Length)
                        {
                            CompletionWindowSize = tag.Name.Length;
                        }

                        ICompletionData TCD = list.Where(x => x.Text == tagname).FirstOrDefault();
                        if (TCD == null)
                            list.Add(GetTagName(tag));
                    }
                }
            }

            //TODO: fix me - show only in the start of line after Given/When then...
            //if (txt == "I")
            //{                
            //    foreach (GherkinStep GH in OptimizedSteps)
            //    {
            //        //if (CompletionWindowSize < GH.Text.Length) { CompletionWindowSize = GH.Text.Length; }
            //            list.Add(GETVTDM(GH));
            //    }                
            //}
            return list;
        }

        private GherkinTextCompletionData GETVTDM(GherkinStep GS)
        {
            GherkinTextCompletionData TCD = new GherkinTextCompletionData(GS.Text);
            TCD.Description = "Step: " + GS.Text + " - " + GS.AutomationStatus;
            
            BitmapImage b = new BitmapImage();
            b.BeginInit();
            b.UriSource = new Uri("pack://application:,,,/Ginger;component/Images/@AddActivity_16x16.png"); 
            b.EndInit();
            TCD.Image = b;

            return TCD;
        }

        private TextCompletionData GetTagName(GherkinTag GT)
        {
            TextCompletionData TCD = new TextCompletionData(GT.Name);
            TCD.Description = "Step: " + GT.Name + " - " + GT.Line;

            BitmapImage b = new BitmapImage();
            b.BeginInit();
            b.UriSource = new Uri("pack://application:,,,/Ginger;component/Images/@AddActivity_16x16.png");
            b.EndInit();
            TCD.Image = b;

            return TCD;
        }

        private TextCompletionData GetTagName(RepositoryItemTag GT)
        {
            TextCompletionData TCD = new TextCompletionData("@" + GT.Name);
            TCD.Description = GT.Description;

            BitmapImage b = new BitmapImage();
            b.BeginInit();
            b.UriSource = new Uri("pack://application:,,,/Ginger;component/Images/@AddActivity_16x16.png");
            b.EndInit();
            TCD.Image = b;

            return TCD;
        }

        public override Page GetSelectedContentPageEditor(SelectedContentArgs SelectedContentArgs)
        {
            ReadOnlyCollection<FoldingSection> list = SelectedContentArgs.GetFoldingsAtCaretPosition();  
            if (list == null) return null;
            if (list.Count > 0)
            {
                string txt = list[0].TextContent;         
                if (txt.Contains("Examples:"))
                {
                    //TODO: check if 0 or something else
                    GherkinTableEditorPage p = new GherkinTableEditorPage(SelectedContentArgs);
                    return p;
                }
            }
            
            return null;
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