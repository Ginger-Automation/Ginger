using Ginger.Plugin.Platform.Web.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web.Elements
{
   public interface ITextBox:IGingerWebElement,IGetValue
    {

        void ClearValue();
        string GetFont();
        string GetText();
        int GetTextLength();

        bool IsValuePopulated();
   
        //TODO: Enable multisetvalue
      //  void SetMultiValue(string[] values);

        void SendKeys(string keys);
        void SetText(string Text);

        void SetValue(string Text);
    }
}
