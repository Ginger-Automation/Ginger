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


        /*
        List<IGingerWebElement> LocateElementsbyCSS(string Css);

        List<IGingerWebElement> LocateElementsByClassName(string ClassName);
        List<IGingerWebElement> LocateElementsByTagName(string tag);
        */
    }
}
