#region License
/*
Copyright © 2014-2019 European Support Limited

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
