﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Amdocs.Ginger.Common.InterfacesLib;

namespace Ginger.GeneralLib
{


    class TextBoxFormatter : ITextBoxFormatter
    {
        private TextBlock mTextBlock;
        public TextBoxFormatter(object ActionRecUseCaseTextBlock)
        {

            this.mTextBlock = (TextBlock)ActionRecUseCaseTextBlock;
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
            System.Windows.Documents.Run run = new System.Windows.Documents.Run(txt);
            run.FontWeight = FontWeights.Bold;
            mTextBlock.Inlines.Add(run);
        }

        public void AddUnderLineText(string txt)
        {
            System.Windows.Documents.Run run = new System.Windows.Documents.Run(txt);
            run.TextDecorations = TextDecorations.Underline;
            mTextBlock.Inlines.Add(run);
        }

        public void AddFormattedText(string txt, object txtColor, bool isBold = false, bool isUnderline = false) // System.Drawing.Brush
        {
            System.Windows.Documents.Run formattedTxt = new System.Windows.Documents.Run(txt);
            if (isBold)
                formattedTxt.FontWeight = FontWeights.Bold;
            if (isUnderline)
                formattedTxt.TextDecorations = TextDecorations.Underline;
            if (txtColor != null)
                formattedTxt.Foreground = (SolidColorBrush)txtColor;
            mTextBlock.Inlines.Add(formattedTxt);
        }
        public string GetText()
        {
            string text = "";
            foreach (System.Windows.Documents.Run run in mTextBlock.Inlines)
                text = text + run.Text;
            return text;
            return null;
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
