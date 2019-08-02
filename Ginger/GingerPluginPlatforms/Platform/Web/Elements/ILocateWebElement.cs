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

using Amdocs.Ginger.Plugin.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web.Elements
{
    public enum eElementType
    {
        Unknown,
        WebElement,
        Button,
        Canvas,
        CheckBox,
        ComboBox,
        Div,
        Image,
        Label,
        List,
        RadioButton,
        Span,
        Table,
        TextBox,
        HyperLink
    }

    public interface ILocateWebElement
    {        
        IGingerWebElement LocateElementByID(eElementType elementType, string id);

        IGingerWebElement LocateElementByXPath(eElementType elementType, string xpath);
     

        // TODO: make all below same like above
       IGingerWebElement LocateElementByCss(eElementType elementType, string LocateValue);

       IGingerWebElement LocateElementByLinkTest(eElementType elementType,string LocateValue);

       IGingerWebElement LocateElementByPartiallinkText(eElementType elementType,string LocateValue);
       IGingerWebElement LocateElementByTag(eElementType elementType,string LocateValue);
        IGingerWebElement LocateElementByName(eElementType elementType, string locateByValue);


        /*
        List<IGingerWebElement> LocateElementsbyCSS(string Css);

        List<IGingerWebElement> LocateElementsByClassName(string ClassName);
        List<IGingerWebElement> LocateElementsByTagName(string tag);
        */
    }
}
