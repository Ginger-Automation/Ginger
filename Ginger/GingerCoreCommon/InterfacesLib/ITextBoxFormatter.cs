using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Common.InterfacesLib
{
  public  interface ITextBoxFormatter
    {
      
         void AddText(string txt);


         void AddLineBreak();


         void AddHeader1(string txt);

         void AddBoldText(string txt);


         void AddUnderLineText(string txt);

         void AddFormattedText(string txt, object txtColor, bool isBold = false, bool isUnderline = false); 
       
         string GetText();

         void AddImage(string image, int width, int height);
       
    }
}
