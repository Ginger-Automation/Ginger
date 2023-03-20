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


using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;

namespace GingerCore.Helpers
{
    public class TextBlockHelper
    {

        private ITextBoxFormatter TBF;
        public TextBlockHelper(object ActionRecUseCaseTextBlock) // System.Windows.Controls.TextBlock
        {
            TBF = TargetFrameworkHelper.Helper.CreateTextBoxFormatter(ActionRecUseCaseTextBlock);
        }

        public void AddText(string txt)
        {
            TBF.AddText(txt);
        }

        public void AddLineBreak()
        {
            TBF.AddLineBreak();
        }

        public void AddBoldText(string txt)
        {
            TBF.AddBoldText(txt);
        }

        public void AddUnderLineText(string txt)
        {
            TBF.AddUnderLineText(txt);
        }

        public void AddFormattedText(string txt, object txtColor, bool isBold = false, bool isUnderline = false) // System.Drawing.Brush
        {
            TBF.AddFormattedText(txt, txtColor, isBold, isUnderline);
        }
        public string GetText()
        {
            return TBF.GetText();
        }
        public void AddImage(string image, int width, int height)
        {
            TBF.AddImage(image, width, height);
        }
    }
}
