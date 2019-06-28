using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Ginger.Plugin.Platform.Web.Elements
{
    //TODO: move to GingerWebElement base class and make abstract
   public interface IGingerWebElement
    {
        object Element { get; set; }
        void DragAndDrop(string DragDropType,IGingerWebElement Element);
        string GetAttribute(string attributeName);
        int GetHeight();        
        //TODO:Enable Item count 
        //int GetItemCount();
        Size GetSize();
        string GetStyle();
        int GetWidth();
        void Hover();
        bool IsEnabled();
        bool IsVisible();
        void RightClick();

        // ?????????????? !!!!!!!!!!!!!!!!!!!
        string RunJavascript(string Script);
        void ScrollToElement();
        void SetFocus();
     

    }
}
