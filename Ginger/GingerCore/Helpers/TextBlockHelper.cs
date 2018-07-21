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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace GingerCore.Helpers
{
    public class TextBlockHelper
    {
        private System.Windows.Controls.TextBlock mTextBlock;

        public TextBlockHelper(System.Windows.Controls.TextBlock ActionRecUseCaseTextBlock)
        {
            // TODO: Complete member initialization
            this.mTextBlock = ActionRecUseCaseTextBlock;
        }

        public void AddText(string txt)
        {
            mTextBlock.Inlines.Add(txt);            
        }

        public void AddLineBreak()
        {
            mTextBlock.Inlines.Add(new LineBreak());
        }

        public void AddHeader1(string txt)
        {
            mTextBlock.Inlines.Add(new Bold(new System.Windows.Documents.Run(txt))); 
        }

        public void AddBoldText(string txt)
        {
            Run run = new Run(txt);
            run.FontWeight = FontWeights.Bold;
            mTextBlock.Inlines.Add(run);
        }

        public void AddUnderLineText(string txt)
        {
            Run run = new Run(txt);
            run.TextDecorations = TextDecorations.Underline;
            mTextBlock.Inlines.Add(run);
        }

        public void AddFormattedText(string txt, System.Windows.Media.Brush txtColor, bool isBold = false, bool isUnderline = false)
        {
            Run formattedTxt = new System.Windows.Documents.Run(txt);
            if (isBold)
                formattedTxt.FontWeight = FontWeights.Bold;
            if (isUnderline)
                formattedTxt.TextDecorations = TextDecorations.Underline;
            if (txtColor != null)
                formattedTxt.Foreground = txtColor;
            mTextBlock.Inlines.Add(formattedTxt);
        }
        public string GetText()
        {
            string text = "";
            foreach (Run run in mTextBlock.Inlines)
                text = text + run.Text;
            return text;
        }
        public void AddImage(string image, int width, int height)
        {
            Image img = new Image();
            img.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + image));
            img.Width = width;
            img.Height = height;
            mTextBlock.Inlines.Add(img);
        }
    }
}
