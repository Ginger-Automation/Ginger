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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Plugin.Core;
using Ginger.UserControlsLib.TextEditor.Common;
using GingerCore.GeneralLib;
using GingerPlugIns.TextEditorLib;
using GingerWPF.DragDropLib;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;

namespace Ginger.UserControlsLib.TextEditor
{
    /// <summary>
    /// Interaction logic for UCTextEditor.xaml
    /// </summary>
    public partial class UCTextEditor : UserControl, IDragDrop, ITextHandler
    {
        TextEditorBase mTextEditor = null;
        GridLength mLastEditPageRowHeight = new GridLength(200);
        CompletionWindow completionWindow;

        public bool AllowWordWrapping
        {
            get
            {
                return this.textEditor.WordWrap;
            }
            set
            {
                this.textEditor.WordWrap = value;
            }
        }

        public BackgroundRenderer BackgroundRenderer { get; set; }

        // Create a new dependency proeprty so we can bind the control
        public static readonly DependencyProperty EditorTextProperty = DependencyProperty.Register("EditorText", typeof(string), typeof(UCTextEditor),
            new FrameworkPropertyMetadata()
            {
                BindsTwoWayByDefault = true
                ,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                PropertyChangedCallback = TextChanged
            }
        );

        private static void TextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((UCTextEditor)d).textEditor.Text = (string)e.NewValue;
        }

        public string FileName { get; set; }
        FoldingManager mFoldingManager;
        IFoldingStrategy mFoldingStrategy;

        public UCTextEditor()
        {
            InitializeComponent();

            EditPageRow.Height = new GridLength(0);
            textEditor.TextArea.TextEntered += TextArea_TextEntered;
            textEditor.TextArea.TextEntering += TextArea_TextEntering;
            BackgroundRenderer = new BackgroundRenderer(textEditor);
            textEditor.TextArea.TextView.BackgroundRenderers.Add(BackgroundRenderer);
        }

        private void UCTextEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                e.Handled = true;
                FindReplacePage FRP = new FindReplacePage(this.textEditor);
                FRP.ShowAsWindow();
            }
        }

        private void TextArea_TextEntering(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }

        private void TextArea_TextEntered(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            completionWindow = new CompletionWindow(textEditor.TextArea);
            SelectedContentArgs args = new SelectedContentArgs
            {
                TextEditor = textEditor
            };
            //TODO: fill the rest of the args

            List<ICompletionData> data = mTextEditor.GetCompletionData(e.Text, args);
            if (data != null && data.Any())
            {
                IList<ICompletionData> cdata = completionWindow.CompletionList.CompletionData;

                foreach (ICompletionData TCD in data)
                {
                    cdata.Add(TCD);
                }

                completionWindow.Width = completionWindow.Width + (Gherkin.GherkinDcoumentEditor.CompletionWindowSize * 2.5);
                completionWindow.CloseAutomatically = true;
                completionWindow.Show();
                completionWindow.Closed += delegate
                {
                    ICompletionData ICD = (ICompletionData)completionWindow.CompletionList.ListBox.SelectedItem;
                    completionWindow = null;
                };
            }
        }

        internal void HideToolBar()
        {
            toolbar.Visibility = Visibility.Collapsed;
            ToolBarRow.Height = new GridLength(0);
        }

        internal void Bind(object obj, string attrName)
        {
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(this, EditorTextProperty, obj, attrName);
        }

        public void Save()
        {
            Reporter.ToStatus(eStatusMsgKey.SaveItem, null, Path.GetFileName(FileName), "file");
            textEditor.Save(FileName);
            Reporter.HideStatusMessage();
        }

        /// <summary>
        /// Initialize the text Editor
        /// </summary>
        /// <param name="FileName">The full path of the file to edit/view</param>
        /// <param name="EnableEdit">enable changing the document, if disable some toolbar buttons will not be visible</param>
        /// <param name="TextEditor">Text Editor to use, if null then text editor will be selected based on the extension of the file</param>
        public void Init(string FileName, TextEditorBase TextEditor, bool EnableEdit = true, bool RemoveToolBar = false)
        {
            mTextEditor = TextEditor;

            if (mTextEditor is ITextEditor)
            {
                ((ITextEditor)mTextEditor).TextHandler = this;
            }

            //TODO: put it in general func
            string SolutionPath = FileName.Replace(WorkSpace.Instance.Solution.Folder, "~");
            lblTitle.Content = SolutionPath;

            toolbar.IsEnabled = true;
            if (EnableEdit)
            {
                SaveButton.Visibility = Visibility.Visible;
                UndoButton.Visibility = Visibility.Visible;
                DeleteButton.Visibility = Visibility.Visible;
                toolbar.IsEnabled = true;
                textEditor.IsReadOnly = false;
            }
            else
            {
                textEditor.IsReadOnly = true;
                SaveButton.Visibility = Visibility.Collapsed;
            }

            this.FileName = FileName;
            if (!string.IsNullOrEmpty(this.FileName) && File.Exists(this.FileName))
            {
                textEditor.Load(this.FileName);
            }
            else
            {
                textEditor.Clear();
            }

            textEditor.ShowLineNumbers = true;

            //TODO: highlight current line;

            SetDocumentEditor(TextEditor);
            if (RemoveToolBar)
            {
                HideToolBar();
            }
        }

        public void SetDocumentEditor(TextEditorBase TE)
        {
            mTextEditor = TE;
            IHighlightingDefinition HD = TE.HighlightingDefinition;
            if (HD != null)
            {
                textEditor.SyntaxHighlighting = HD;
            }
            else
            {
                // try to use Avalon highlighting built in to find the correct highlight by file extension
                string ext = Path.GetExtension(FileName).ToLower();
                textEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinitionByExtension(ext);
            }

            IFoldingStrategy FG = TE.FoldingStrategy;
            if (FG != null)
            {
                try
                {
                    mFoldingManager = FoldingManager.Install(textEditor.TextArea);
                }
                catch
                { }
                mFoldingStrategy = FG;
                InitFoldings();
            }

            textEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;

            //TODO: set indentation manager   

            // Add tools in toolbar
            if (TE.Tools != null)
            {
                foreach (ITextEditorToolBarItem t in TE.Tools)
                {
                    if (t is TextEditorToolBarItem textEditorToolBarItem)
                    {
                        AddToolbarTool(textEditorToolBarItem.Image, textEditorToolBarItem.clickHandler, textEditorToolBarItem.toolTip, textEditorToolBarItem.toolVisibility);
                    }
                    else
                    {
                        // Plugin text editor
                        AddPluginToolbarTool(t);
                    }

                }
            }
        }

        public void AddPluginToolbarTool(ITextEditorToolBarItem t)
        {
            Button tool = new Button
            {
                ToolTip = t.ToolTip,
                Content = t.ToolText,
                Tag = t
            };
            tool.Click += ToolBarItemClick;


            //To keep the tools before the search control we do remove and then add
            //DO NOT Delete
            // toolbar.Items.Remove(lblSearch);
            //  toolbar.Items.Remove(txtSearch);
            //  toolbar.Items.Remove(btnClearSearch);
            toolbar.Items.Remove(lblView);
            toolbar.Items.Remove(comboView);
            toolbar.Items.Add(tool);
            //   toolbar.Items.Add(lblSearch);
            //   toolbar.Items.Add(txtSearch);
            //   toolbar.Items.Add(btnClearSearch);
            toolbar.Items.Add(lblView);
            toolbar.Items.Add(comboView);
        }

        private void ToolBarItemClick(object sender, RoutedEventArgs e)
        {
            ITextEditorToolBarItem tool = (ITextEditorToolBarItem)((Button)sender).Tag;
            tool.Execute((ITextEditor)mTextEditor);
        }

        //TODO: looks liek too many calls, even the the caret didn't move, can first check if pos changed otherwise return - keep last
        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            SelectedContentArgs args = new SelectedContentArgs
            {
                TextEditor = this.textEditor,
                FoldingManager = mFoldingManager
            };
            if (mTextEditor == null)
            {
                return;
            }

            Page p = mTextEditor.GetSelectedContentPageEditor(args);           
            if (p != null)
            {
                SelectionEditorFrame.ClearAndSetContent(p);
                SelectionEditorFrame.Visibility = Visibility.Visible;

                if (mLastEditPageRowHeight.Value == 0) { mLastEditPageRowHeight = new GridLength(200); }

                EditPageRow.Height = mLastEditPageRowHeight;
                SelectionEditorFrameSplitter.Visibility = Visibility.Visible;
            }
            else
            {
                if (SelectionEditorFrame.Visibility == Visibility.Visible)
                {
                    // Save the last width so we can restore to same size                
                    if (EditPageRow.Height.Value > 0)
                    {
                        mLastEditPageRowHeight = new GridLength(EditPageRow.ActualHeight);
                        SelectionEditorFrameSplitter.Visibility = Visibility.Visible;
                    }

                    SelectionEditorFrame.Visibility = Visibility.Collapsed;
                    SelectionEditorFrameSplitter.Visibility = Visibility.Collapsed;
                    EditPageRow.Height = new GridLength(0);
                }
            }
        }

        private void InitFoldings()
        {
            // start a timer to set folding every 2 sec
            DispatcherTimer foldingUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            foldingUpdateTimer.Tick += delegate { UpdateFoldings(); };
            foldingUpdateTimer.Start();
        }

        void UpdateFoldings()
        {
            mFoldingStrategy.UpdateFolding(mFoldingManager, textEditor.Document);
        }

        public string GetText()
        {
            return textEditor.Text;
        }

        //TODO: see how we can reuse with AvalonEdit
        // function to return all words in doc which match the regex pattern, then we send it to the formater
        public static IEnumerable<TextRange> GetAllWordRanges(FlowDocument document, string pattern)
        {
            TextPointer pointer = document.ContentStart;
            while (pointer != null)
            {
                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = pointer.GetTextInRun(LogicalDirection.Forward);
                    MatchCollection matches = Regex.Matches(textRun, pattern);
                    foreach (Match match in matches)
                    {
                        int startIndex = match.Index;
                        int length = match.Length;
                        TextPointer start = pointer.GetPositionAtOffset(startIndex);
                        TextPointer end = start.GetPositionAtOffset(length);
                        yield return new TextRange(start, end);
                    }
                }
                pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
            }
        }

        private Image CreateCopyImage(Image image)
        {
            Image NewImage = new Image
            {
                Source = image.Source
            };
            return NewImage;
        }

        public void AddToolbarTool(Image image, ToolClickRoutedEventHandler clickHandler, string toolTip = "", Visibility toolVisibility = Visibility.Visible)
        {
            Button tool = new Button
            {
                Visibility = toolVisibility,
                ToolTip = toolTip,
                Content = CreateCopyImage(image)
            };
            tool.Click += ToolBarButtonClick;
            tool.Tag = clickHandler;

            //To keep the tools before the search control we do remove and then add
            //DO NOT Delete
            // toolbar.Items.Remove(lblSearch);
            //  toolbar.Items.Remove(txtSearch);
            //  toolbar.Items.Remove(btnClearSearch);
            toolbar.Items.Remove(lblView);
            toolbar.Items.Remove(comboView);
            toolbar.Items.Add(tool);
            //   toolbar.Items.Add(lblSearch);
            //   toolbar.Items.Add(txtSearch);
            //   toolbar.Items.Add(btnClearSearch);
            toolbar.Items.Add(lblView);
            toolbar.Items.Add(comboView);
        }

        private void ToolBarButtonClick(object sender, RoutedEventArgs e)
        {
            // Call the text editor event which added this button, giving him all the data in args
            // since it can be implemented also as Plugin we need to avoid passing Avalon objects

            ToolClickRoutedEventHandler clickHandler = (ToolClickRoutedEventHandler)((Button)sender).Tag;
            TextEditorToolRoutedEventArgs args = new TextEditorToolRoutedEventArgs
            {
                CaretLocation = textEditor.CaretOffset,
                txt = textEditor.Text
            };
            clickHandler.Invoke(args);

            BackgroundRenderer.Segments.Clear();
            if (!string.IsNullOrEmpty(args.ErrorMessage))
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, args.ErrorMessage);

                if (args.ErrorLines != null)
                {
                    AddSegments(args.ErrorLines);
                }
            }
            else if (!string.IsNullOrEmpty(args.SuccessMessage))//succ
            {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, args.SuccessMessage);
            }
            else if (!string.IsNullOrEmpty(args.WarnMessage))//warn
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, args.WarnMessage);
            }
        }

        private void AddSegments(List<int> linesNumbers)
        {
            foreach (int lineNumber in linesNumbers)
            {
                var line = textEditor.Document.GetLineByNumber(lineNumber);
                BackgroundRenderer.Segments.Add(line);
            }
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            this.textEditor.Undo();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            this.textEditor.SelectedText = "";
        }

        private void FindReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            FindReplacePage FRP = new FindReplacePage(this.textEditor);
            FRP.ShowAsWindow();
        }

        void IDragDrop.Drop(DragInfo Info)
        {
            //this is a simple drop to take the text dropped
            // if we need more complex to get the object handle it in the containing page not here
            textEditor.SelectedText = Info.Header;
        }

        void IDragDrop.DragEnter(DragInfo Info)
        {
            Info.DragTarget = this;
            Info.DragIcon = DragInfo.eDragIcon.Add;
        }

        public void StartDrag(DragInfo Info)
        {
        }

        void IDragDrop.DragOver(DragInfo Info)
        {
        }

        public void HighlightLine(int LineNumber)
        {
            if (LineNumber > textEditor.Document.LineCount)
            {
                return;
            }
            var line = textEditor.Document.GetLineByNumber(LineNumber);
            BackgroundRenderer.HighLightLine = line;
            textEditor.Focus();  // need to focus to get the redraw to happen, //TODO: find alternative so user can stay in grid or wherever
            textEditor.ScrollToLine(LineNumber);
        }
        public void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            mTextEditor.UpdateSelectedContent();
        }
        public Visibility ShowUpdateContent
        {
            get { return UpdateButton.Visibility; }
            set { UpdateButton.Visibility = value; }
        }

        public string SetUpdateLabel
        {
            get { return UpdateButton.Content.ToString(); }
            set { UpdateButton.Content = value; }
        }

        private void UpdateButton_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (UpdateButton.Visibility == Visibility.Visible)
            {
                UpdateRow.Height = new GridLength(27);
            }
            else
            {
                UpdateRow.Height = new GridLength(0);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        public string Text { get { return textEditor.Text; } set { textEditor.Text = value; } }

        public int CaretLocation { get { return textEditor.CaretOffset; } set { textEditor.CaretOffset = value; } }

        public void AppendText(string text)
        {
            textEditor.AppendText(text);
        }

        public void InsertText(string text)
        {
            throw new NotImplementedException();
        }

        public void ShowMessage(MessageType messageType, string text)
        {
            throw new NotImplementedException();
        }

        public void SetContentEditorTitleLabel(string titleContent, Style titleStyle = null)
        {
            lblTitle.Visibility = Visibility.Collapsed;
            ContentEditorTitleLabel.Visibility = Visibility.Visible;
            ContentEditorTitleLabel.Content = titleContent;
            if (titleContent == string.Empty)
            {
                xFirstGrid.RowDefinitions[0].Height = new GridLength(0);
            }
            if (titleStyle != null)
            {
                ContentEditorTitleLabel.Style = titleStyle;
            }
        }
    }
}