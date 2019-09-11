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

// This JS will be injected into Browser active page, for use in Selenium driver and Java driver for Widgets 

GingerLib = {};

// Define the GingerLib of function we can use, in this way it is kind of JS OO, will not overlap with other JS scripts running on the page

'use strict';
function define_GingerLib() {
    GingerLib = {};

    //----------------------------------------------------------------------------------------------------------------------
    // To Highlight element we keep the current element and add/remove the GingerHighlight style to the element
    //----------------------------------------------------------------------------------------------------------------------
    var CurrentHighlightElement;
    var CurrentX;
    var CurrentY;
    var currentframe; 
    var ImplicitWait;
    var docState;
	var ErrorMessage;
    //public functions

    //----------------------------------------------------------------------------------------------------------------------
    // Add Java Script to page
    //----------------------------------------------------------------------------------------------------------------------
    GingerLib.AddScript = function(JavaScriptCode) {
        var script = document.createElement('script');
        script.type = "text/javascript";
        script.text = JavaScriptCode;
        var head = document.getElementsByTagName('head')[0];
        head.appendChild(script);
        return "OK";
    }


    //----------------------------------------------------------------------------------------------------------------------
    // Hello
    // Simple hello world to check the script is working
    //----------------------------------------------------------------------------------------------------------------------
    GingerLib.Hello = function () {
        //alert("Hello from the GingerLib Java Script HTML Helper library.");
        alert("document.doctype::" + document.doctype);
        if (document.doctype == null) {
            alert(document.head.innerText);
            alert(document.head.outerHTML);
            alert(document.body.outerHTML);
        }
    }
    //----------------------------------------------------------------------------------------------------------------------
    // Process Hex Input String --> Used from Java Driver For Widgets Solution
    //----------------------------------------------------------------------------------------------------------------------
    GingerLib.ProcessHexInputString = function (InputHexString) {
		
		try
		{
			 var RequestPL = GingerPayLoadFromHexString(InputHexString);    
			var pl = GingerLib.ProcessCommand(RequestPL);
			return pl.GetHexString();
		}
		catch (error) 
		{			
			var pl = new GingerPayLoad("Error");
			pl.AddValueString("Failed to execute command. Error :"+error.message);
			pl.ClosePackage();
			return pl;
        }	
      
    }

    //----------------------------------------------------------------------------------------------------------------------
    // Process PayLoad
    //----------------------------------------------------------------------------------------------------------------------
    GingerLib.ProcessPayLoad = function (PayLoadByes) {
        // TODO: only once + handle err
        var RequestPL = GingerPayLoadFromBytes(PayLoadByes);
        var pl = GingerLib.ProcessCommand(RequestPL);
        return pl.GetPackage();
    }

    //----------------------------------------------------------------------------------------------------------------------
    // CheckJSSupport
    //----------------------------------------------------------------------------------------------------------------------
    GingerLib.CheckJSSupport = function () {
        //TODO: confirm working OK on several browsers and do the check
        // ArrayBuffer exist only from IE 10, Chrome 7, FF 4, Opera 11.6, Safari 5.1 - we need to handle in case old browser...
        alert("CheckJSSupport");
        if (ArrayBuffer != undefined) {
            alert("ArrayBuffer - OK");
        }
        else {
            alert("ArrayBuffer - not defined");
        }
    }


    //----------------------------------------------------------------------------------------------------------------------
    // Process Command
    //----------------------------------------------------------------------------------------------------------------------
    GingerLib.ProcessCommand = function (PayLoad) {
        var Command = PayLoad.mName;
        if (Command == "Echo") {
            var txt = PayLoad.GetValueString();
            return GingerLib.Echo(txt);
        }
        else if (Command === "WidgetsUIElementAction")
        {
           var list = PayLoad.GetListPayLoad();
           var inputValues = PayLoad.GetInputValues(list);

           var ElementAction = inputValues["ElementAction"];
           var ElementLocateBy = inputValues["ElementLocateBy"];
           var ElementLocateValue = inputValues["ElementLocateValue"];
           var Value = inputValues["Value"];

            if (ElementAction === "TriggerJavaScriptEvent")
            {
                var IsMouseEvent = inputValues["IsMouseEvent"];

                var element = "undefined";

                element = GingerLib.GetElementWithImplicitSync(ElementLocateBy, ElementLocateValue);

                if (element === undefined) {

                    if (ErrorMessage == "")
                        ErrorMessage = "ERROR|Web Element not Found: " + LocateBy + " " + LocateValue;

                    var payload = new GingerPayLoad("ERROR");
                    payload.AddValueString(ErrorMessage);
                    payload.ClosePackage();
                    return payload;
                }

                if (IsMouseEvent.toLowerCase() == "true")
                {
                    return GingerLib.fireMouseEvent(element, Value);               
                }
                else
                {
                    return GingerLib.fireSpecialEvent(element, Value);
                }
            }

            if (ElementAction === "IsVisible")
            {
                ElementAction = "Visible";
            }
            if (ElementAction === "IsEnabled")
            {
                ElementAction = "Enabled";
            }
            if (ElementAction === "Select") {
                ElementAction = "SelectFromDropDown";
            }
            else if (ElementAction === "SelectByIndex") {
                ElementAction = "SelectFromDropDownByIndex";
            }

           return GingerLib.HandleHTMLControlAction(ElementAction, ElementLocateBy, ElementLocateValue, Value);
        }
        else if (Command == "HTMLElementAction") {
            var ControlAction = PayLoad.GetValueString();
            var LocateBy = PayLoad.GetValueString();
            var LocateValue = PayLoad.GetValueString();
            var Value = PayLoad.GetValueString();           
            return GingerLib.HandleHTMLControlAction(ControlAction,LocateBy,LocateValue,Value);
        } else if (Command == "GetPageSource") {
            return GingerLib.GetPageSource();
        } else if (Command == "GetPageURL") {
            return GingerLib.GetPageURL();
        } else if (Command == "GetPageTitle") {
            return GingerLib.GetPageTitle();
        } else if (Command == "SwitchToDefaultFrame") {
            return GingerLib.SwitchToDefaultFrame();
        } else if (Command == "JscriptInjected") {
            return GingerLib.JscriptInjected();
        }
        else if (Command == "GetVisibleElements") {
            return GingerLib.GetVisibleElements();
        }
        else if (Command == "GetComponentFromCursor") {
            return GingerLib.GetComponentFromCursor();
        }
        else if (Command == "AddScript") {
            var script = PayLoad.GetValueString();
            GingerLib.AddScript(script);
            var PLResp = new GingerPayLoad("Ok");
            PLResp.AddValueString("Script Added successfully");
            PLResp.ClosePackage();
            return PLResp;
        }
        else if (Command == "GingerAgentConfig") {
            ImplicitWait = parseInt(PayLoad.GetValueString());
          
            var PLResp = new GingerPayLoad("Ok");
            PLResp.AddValueString("Implicit wait set successfully");
            PLResp.ClosePackage();
            return PLResp;
        }
        else if (Command == "Screenshot") {
            var pl;
            var img = GingerLib.GetScreenShot();

			if(img!=undefined)
			{
				if (img.indexOf("ERROR") == -1)
					pl = new GingerPayLoad("HTMLScreenShot");
				else
					pl = new GingerPayLoad("Error");

				pl.AddValueString(img);
				pl.AddValueString("ScreenShot successfully");
				pl.ClosePackage();
				return pl;			
			}
			else
			{
				pl= new GingerPayLoad("Error:Failed to capture screenshot, img is undefined");
				pl.ClosePackage();
				return pl;		
			}
 
        }
        else if (Command == "HighLightElement") {
            var Path = PayLoad.GetValueString();
            var XPath = PayLoad.GetValueString();
            GingerLib.HighLightElement(XPath,Path);
            var PLResp = new GingerPayLoad("Ok");
            PLResp.AddValueString("Element Highlighted successfully");
            PLResp.ClosePackage()
            return PLResp;
        }
        else if (Command == "GetElementChildren") {
            var Path = PayLoad.GetValueString();
            var XPath = PayLoad.GetValueString();
            return GingerLib.GetElementChildren(XPath, Path);
        }
        else if (Command == "GetElementProperties") {
            var Path = PayLoad.GetValueString();
            var XPath = PayLoad.GetValueString();
            return GingerLib.GetElementProperties(XPath,Path);        
        } else if (Command == "StartRecording") {
            var PLResp = new GingerPayLoad("Ok");
            PLResp.ClosePackage();
            GingerRecorderLib.StartRecording();
            return PLResp;
        } else if (Command == "GetRecording") {
            return GingerRecorderLib.GetRecording();
        } else if (Command == "StopRecording") {
            var PLResp = new GingerPayLoad("Ok");
            PLResp.ClosePackage();
            GingerRecorderLib.StopRecording();
            return PLResp;
        }
        else {
            var pl = new GingerPayLoad("ERROR");
            pl.AddValueString("Unknown Command - " + Command);
            pl.ClosePackage();
            return pl;
        }
    }

    //----------------------------------------------------------------------------------------------------------------------
    // Echo - Return Payload with the txt - used for testing of passing String
    //----------------------------------------------------------------------------------------------------------------------
    GingerLib.Echo = function (txt) {
        var pl = new GingerPayLoad("Echo Response");
        pl.AddValueString(txt);
        pl.ClosePackage();
        return pl;
    }

    //----------------------------------------------------------------------------------------------------------------------
    // Echo - Return Payload with the Page Source
    //----------------------------------------------------------------------------------------------------------------------
    GingerLib.GetPageSource = function () {
        var pl = new GingerPayLoad("Page Source");
        var pageSource = document.all[0].outerHTML;
        if (pageSource == "" )
            pageSource = document.documentElement.outerHTML;

        pl.AddValueString(pageSource);
        pl.ClosePackage();
        return pl;
    }

    //----------------------------------------------------------------------------------------------------------------------
    // Echo - Return Payload with the Page URL
    //----------------------------------------------------------------------------------------------------------------------
    GingerLib.GetPageURL = function () {
        var pl = new GingerPayLoad("Page URL");
        pl.AddValueString(document.URL);
        pl.ClosePackage();
        return pl;
    }
    GingerLib.GetPageTitle = function () {
        var pl = new GingerPayLoad("Page Title");
        pl.AddValueString(document.title);
        pl.ClosePackage();
        return pl;
    }
    GingerLib.SwitchToDefaultFrame = function () {
        currentframe = undefined;
        var pl = new GingerPayLoad("Switch To Default Frame");
        pl.AddValueString(document.URL);
        pl.ClosePackage();
        return pl;
    }

    //----------------------------------------------------------------------------------------------------------------------
    // Echo - Return Payload with the detailed
    //----------------------------------------------------------------------------------------------------------------------
    GingerLib.JscriptInjected = function () {
        var RC = "";
        var pl = new GingerPayLoad("JScriptExist");
        try{
            if(typeof jQuery != 'undefined' && window.jQuery)
            {
                if ($("body").prop("tagName") != undefined)
                    RC = "JQuery Found";
            }
            else {
                RC = "JQuery not Found";
            }
        } catch (error) {
            RC = "JQuery not Found";
        }
        pl.AddValueString(RC);
        pl.ClosePackage();
        return pl;
    }

    //----------------------------------------------------------------------------------------------------------------------
    // HandleHTMLControlAction
    //
    //---------
        
    GingerLib.HandleHTMLControlAction = function (ControlAction, LocateBy, LocateValue, Value) {

        var el = undefined;
        if (ControlAction == "Visible" || ControlAction == "Enabled")
        {          
            el = GingerLib.GetElement(LocateBy, LocateValue);
        }
        else
        {
            el = GingerLib.GetElementWithImplicitSync(LocateBy, LocateValue);
        }
          
        if (el == undefined) {    

            if (ErrorMessage == "")
               ErrorMessage = "ERROR|Web Element not Found: " + LocateBy + " " + LocateValue;

            var pl = new GingerPayLoad("ERROR");
            pl.AddValueString(ErrorMessage);
            pl.ClosePackage();
            return pl;
        }
        else {

            if (ControlAction == "SetValue") {
                return GingerLib.SetElementValue(el, Value);
            } 
            if (ControlAction == "SelectFromDropDown") {
                return GingerLib.SelectFromDropDown(el, Value);
            }
            if (ControlAction == "SelectFromDropDownByIndex") {
                return GingerLib.SelectFromDropDownByIndex(el, Value);
            }
            else if (ControlAction=="GetValue") {
                return GingerLib.GetElementValue(el);
            }
            else if (ControlAction == "Click") {
                return GingerLib.ClickElement(el,Value);
            }
            else if (ControlAction == "AsnycClick") {
                return GingerLib.AsnycClickElement(el);
            }
            else if (ControlAction == "Visible") {
                return GingerLib.IsVisible(el);
            }
            else if (ControlAction == "SwitchFrame") {
                return GingerLib.SwitchFrame(el);
            }
            else if (ControlAction == "FireMouseEvent") { 
                return GingerLib.fireMouseEvent(el, Value);
            }
            else if (ControlAction == "FireSpecialEvent") { 
                return GingerLib.fireSpecialEvent(el, Value);
            }
            else if (ControlAction == "GetListDetails") {
                return GingerLib.GetListDetails(el);
            } else if (ControlAction == "Enabled") {
                return GingerLib.IsEnabled(el);
            }
            else if (ControlAction == "ScrollDown") {
                return GingerLib.ScrollDown(el, Value);
            }
            else if (ControlAction == "ScrollUp") {
                return GingerLib.ScrollUp(el, Value);
            }
                //TODO: Add More actions handling here

            else {
                var pl = new GingerPayLoad("ERROR");
                pl.AddValueString("Action: "+ControlAction+" Not implemented Yet" );
                pl.ClosePackage();
                return pl;
            }
        }
    }

    function getMousePos(event) {
        CurrentX = event.clientX;
        CurrentY = event.clientY;        
    }

    GingerLib.StartEventListner = function () {        
        if (document.addEventListener) {            
            document.addEventListener("mouseover", getMousePos);           
        }
        else if (document.attachEvent)// Add to support erlier Version on IE .
        {            
            document.attachEvent("onmouseover", getMousePos);
        }
    }

    GingerLib.StartEventListnerFrame = function () {
        if (currentframe.contentDocument.addEventListener) {            
            currentframe.contentDocument.addEventListener("mouseover", getMousePos);
        }
        else if (document.attachEvent)// Add to support erlier Version on IE .
        {            
            currentframe.contentDocument.attachEvent("onmouseover", getMousePos);
        }
    }

    //----------------------------------------------------------------------------------------------------------------------
    // GetVisibleElements
    // Return PayLoad with ElementInfo for control under mouse
    //----------------------------------------------------------------------------------------------------------------------
    GingerLib.GetComponentFromCursor = function () {
        if (document.addEventListener)
        {
            var el = document.elementFromPoint(CurrentX, CurrentY);            
        }
        else if (document.attachEvent)// Add to support earlier Version on IE .
        {         
            el = document.elementFromPoint(CurrentX, CurrentY);            
        }

        // To support Spy on IFrame 
        // Fix ME
        
        var iframeXpath = "";        
        if ($(el).prop('tagName') == "IFRAME") {            
            GingerLib.SwitchFrame(el);
            currentframe = el;
            iframeXpath = GingerLib.GetElemXPath(el);
            var el = currentframe.contentDocument.elementFromPoint(CurrentX , CurrentY);
        }
        var pl = GingerLib.GetElementInfo(el, iframeXpath);
        if (pl == null) {            
            pl = new GingerPayLoad("HTMLElement");
            pl.AddValueString(GingerLib.GetElemTitle(el));
            pl.AddValueString(GingerLib.GetElemId(el));
            pl.AddValueString(GingerLib.GetElemValue(el));
            pl.AddValueString(GingerLib.GetElemName(el));
            pl.AddValueString(GingerLib.GetElemType(el));

            if (iframeXpath != "")//Path of Frame
                pl.AddValueString(iframeXpath);
            else 
                pl.AddValueString("");  // Path                   

            pl.AddValueString(GingerLib.GetElemXPath(el));
            pl.AddValueString(GingerLib.getElementTreeRelXPath(el));
            pl.ClosePackage();
        }
        return pl;
    }

    //----------------------------------------------------------------------------------------------------------------------
    // GetScreenShot
    // Return PayLoad with list of ElementInfo for each visible element
    //----------------------------------------------------------------------------------------------------------------------
    GingerLib.GetScreenShot = function () {

        try {
            var html2obj;
            if (currentframe == undefined)  		
				 html2obj = html2canvas(document.body);			
            else
                html2obj = html2canvas(currentframe.contentDocument.body);        
			var queue = html2obj.parse();
            var canvas = html2obj.render(queue);    
            var img = canvas.toDataURL("image/png").replace('data:image\/png;base64\,', '');
            return img;
            
        }
        catch (error) {			
		  return ("ERROR:" + error.message);
        }	
    }

    //----------------------------------------------------------------------------------------------------------------------
    // GetVisibleElements
    // Return PayLoad with list of ElementInfo for each visible element
    //----------------------------------------------------------------------------------------------------------------------
    GingerLib.GetVisibleElements = function () {
        //Very slow - find better way
        // return GingerLib.GetElements("body:visible *");
        // return GingerLib.GetElements("body *");

        var pl = new GingerPayLoad("HTML Elements");
        var Elems = [];
         $("body").find("*").each(function () {
             if (GingerLib.isVisible(this) || $(this).prop("type") == "checkbox") {

                 //This to support IFRAME elements.
                 if (($(this).prop('tagName')) == "IFRAME") {
                     var iframeXpath = GingerLib.GetElemXPath(this);
                     if (this.contentWindow) {
                         $($(this).contents().find('body').html()).find("*").each(function () {
                             var fel = $(this);
                             var fElemPL = GingerLib.GetElementInfo(fel, iframeXpath);
                             if (fElemPL != null) {
                                 Elems.push(fElemPL);
                             }
                         });

                     }
                 }

                 if (($(this).prop('tagName')) == "FRAMESET") {
                     var framesetXpath = GingerLib.GetElemXPath(this);
                     var fel = $(this);
                     var objFrameSet = GingerLib.GetElementInfo(fel, framesetXpath);
                     var frames = window.frames;

                     for (var i = 0; i < frames.length; i++) {
                         var frameXpath = GingerLib.GetElemXPath(frames[i].frameElement);
                         var frameName = frames[i].name;
                         var frameDocument = $('frame[name=' + frameName + ']', top.document)[0].contentDocument;
                         $(frameDocument).find('*').each(function () {
                             var fel = $(this);
                             var fElemPL = GingerLib.GetElementInfo(fel, frameXpath);
                             if (fElemPL != null) {
                                 Elems.push(fElemPL);
                             }

                         });
                     }
                 }
                var el = $(this);
                var ElemPL = GingerLib.GetElementInfo(el,"");
                if (ElemPL != null) {
                    Elems.push(ElemPL);
                }
            }
        });
        //TODO: Try this if it helps to speed up
        pl.AddListPayLoad(Elems);
        pl.ClosePackage();

        //call to function which will start mouse events listener to be used for Live Spy
        return pl;
    }

    GingerLib.isVisible = function (el) {

		if($(el).is(":visible")==false || $(el).css('display')=== 'none' || $(el).css('opacity')===0)
		{
			//if visible , display or opacity check is false return false straight a way
			return false;
		}
		else
        {
			//if element is visible then we do additional check to verify if it is on foreground
			return GingerLib.IsOnForeGround(el);
		}
    }


    GingerLib.IsOnForeGround=  function (element) {

         element.scrollIntoView();
		 if (element.offsetWidth === 0 || element.offsetHeight === 0) return false;
		
        var height = document.documentElement.clientHeight;
        if (height === 0)
        {
            height = document.body.clientHeight;
            if (height === 0 || height === undefined)
            {
                height = $(window).height();
            }
        }
        var rects = element.getClientRects();

	    on_top = function(r) {
            var x = (r.left + r.right) / 2, y = (r.top + r.bottom) / 2;
                return element.contains(document.elementFromPoint(x, y));          
		};
		
		for (var i = 0, l = rects.length; i < l; i++) {
			 var r = rects[i],
                in_viewport = r.top > 0 ? r.top <= height : (r.bottom > 0 && r.bottom <= height);
			if (in_viewport && on_top(r)) return true;
	    }

		return false;
	}

    //----------------------------------------------------------------------------------------------------------------------
    // GetElementChildren
    // Return PayLoad with list of ElementInfo for each visible element
    //----------------------------------------------------------------------------------------------------------------------
    GingerLib.GetElementChildren = function (XPath,Path) {
        var pl = new GingerPayLoad("HTML Element Children");
        var Elems = [];
        
        //This to support IFRAME elements.        
        GingerLib.SetFramebyPath(Path);

        var el = GingerLib.GetElementByXPath(XPath,currentframe);
        var element = $(el);

        //This to support IFRAME elements.        

        if (($(el).prop('tagName')) == "IFRAME") {
            GingerLib.SwitchFrame(el);
            $(el).contents().children().each(function () {
                var elChild = $(this);
                var ElemPL = GingerLib.GetElementInfo(elChild, XPath);
                Elems.push(ElemPL);
            });
        }
        else {
            $(el).children().each(function () {
                var elChild = $(this);                
                var ElemPL = GingerLib.GetElementInfo(elChild, Path);
                Elems.push(ElemPL);
            });
        }
       
        pl.AddListPayLoad(Elems);
        pl.ClosePackage();
        return pl;
    }

    //----------------------------------------------------------------------------------------------------------------------
    // Get Elements based on criteria
    // Return PayLoad with list of ElementInfo for each element found
    //----------------------------------------------------------------------------------------------------------------------
    GingerLib.GetElements = function (Selector)
    {
        var pl = new GingerPayLoad("HTML Elements");
        var Elems = [];
        $(Selector).each(function () {
            if ($(this).is(":visible")) {
                var el = $(this);
                var ElemPL = GingerLib.GetElementInfo(el,"");
                Elems.push(ElemPL);
            }
        });

        pl.AddListPayLoad(Elems);
        pl.ClosePackage();
        return pl;
    }

    //private functions - return the Payload
    GingerLib.GetElementInfo = function (el, contentframe)
    {
        var pl = new GingerPayLoad("HTMLElement");
        pl.AddValueString(GingerLib.GetElemTitle(el));  // Title   
        if (GingerLib.GetElemTitle(el).indexOf("/") == 0) {
            return null;
        }
        pl.AddValueString(GingerLib.GetElemId(el));  // ID 
        var sElemType = GingerLib.GetElemType(el);

        if (sElemType != "TBODY" && sElemType != "TABLE" && sElemType != "TD" && sElemType != "SCRIPT") {
            pl.AddValueString(GingerLib.GetElemValue(el));  // Value 
        }
        else
        {
            pl.AddValueString("");
        }

        pl.AddValueString(GingerLib.GetElemName(el));  // Name   
        pl.AddValueString(sElemType);  // Type   

        if (contentframe != "")//Path of Frame
            pl.AddValueString(contentframe);            
        else
            pl.AddValueString("");  // Path               

        var xPath = GingerLib.GetElemXPath(el[0]);     
        if (xPath == "") {
            return null;
        }
        pl.AddValueString(xPath);  // XPath                                  

        var RelxPath = GingerLib.getElementTreeRelXPath(el[0]);     
        pl.AddValueString(RelxPath);  // Rel XPath  

        pl.ClosePackage();
        return pl;
    }

    GingerLib.GetElemType = function (el) {
        var t = el.tagName;
        if (t == undefined)
        {
            t = el.prop("tagName");
        }
        if (t == "INPUT")
        {
            t += "." + el.type;
        }
        else if ((t == "A") || (t == "LI")) {
            t = "LINK";
        }
        return t;
    }

    GingerLib.GetElemId = function (el) {
        var id = el.id;
        try{
            if (id == undefined)
                id = el.attr("id");
            if (id != undefined) return id;
        }
        catch (error) {
            return "";
        }

        return "";
    }

    GingerLib.GetElemTitle = function (el) {
        var TagName = el.tagName;

        if (TagName == undefined)
            TagName = el.prop("tagName");

        if (TagName == "TABLE") {
            return "Table";
        }

        var id = el.id;
        if (id == undefined) {
            try{var id = el.attr("id");}
            catch (error) {
                id = "";
            }
        }
        if (id != undefined && id != "")
            return TagName + " ID=" + id;

        var name = el.name;
        if (name == undefined) {
            try{var name = el.attr("name");}
            catch (error) {
                name = "";
            }
        }
        if (name != undefined && name != "")
            return TagName + " Name=" + name;

        var val = el.value;
        if (val == undefined) {
            try{var val = el.attr("value");}
            catch (error) {
                val = "";
            }
        }
        if (val != undefined && val != "")
            return TagName + " " + GingerLib.GetShortName(val);
        
        // if we didn't get better naem just return the element TagName
        return TagName;
    }
    GingerLib.GetElemName = function (el) {

        var name = el.name;
        try{
            if (name == undefined)
                name = el.attr("name");
            if (name != undefined) return name;
        }
        catch (error) {
            return "";
        }
        // if we didn't get better name just return the element ""
        return "";
    }

    // Make sure the name doesn't contain new line, or too long
    // TODO: confirm CR/LF 10/13 are working or remove
    GingerLib.GetShortName = function (name) {
        var str = name;
        if (str.length > 50) str = str.substring(1, 50) + "...";
        return str;
    }

    GingerLib.GetElemValue = function (el) {
        var val;
        var status;
        var type;
        try {
            if (el.tagName == "SELECT")
            {
                val = GingerLib.getSelectedValue(el);
            }
            else if (el.tagName == "INPUT" && $(el).prop("type") == "checkbox")
            {
                val = GingerLib.validateCheckBox(el);  
            }
            else
            {
                val = el.value;
                if (val == undefined)
                {
                    val = $(el).text();
                }
                if (val == undefined)
                    val = el.prop("value");
            }
            if (val != undefined) {
                return val.replace(/^\s+|\s+$/g, "");
            }
            else
                return "?";
        }
        catch (error) {
            return "?";
        }
    }

    //----------------------------------------------------------------------------------------------------------------------
    // Add Ginger HighLight Style to page
    //----------------------------------------------------------------------------------------------------------------------
    GingerLib.AddGingerHighLightStyle = function () {

        //Try also JQuery:  $('<style>' + styles + '</style>').appendTo(document.head);
        // Below might not work on all browsers
        // dotted
        var css = '.GingerHighlight{ outline: red dotted thick;}',
       head = document.head || document.getElementsByTagName('head')[0],
       style = document.createElement('style');

        style.type = 'text/css';
        if (style.styleSheet) {
            style.styleSheet.cssText = css;
        } else {
            style.appendChild(document.createTextNode(css));
        }

        head.appendChild(style);
    }

    //----------------------------------------------------------------------------------------------------------------------
    // Get Element Properties
    //----------------------------------------------------------------------------------------------------------------------

    GingerLib.GetElementProperties = function (XPath,Path) {

        var pl = new GingerPayLoad("HTML Element Properties");

        //To Support IFrame
        GingerLib.SetFramebyPath(Path);

        var Props = [];
        var el = GingerLib.GetElementByXPath(XPath, currentframe);

        var element = $(el);
        //Add the attr defined on the element itself
        $(element.attributes).each(function () {
            var pl1 = GingerLib.ControlPropertyPayload(this.name, this.value);
            Props.push(pl1);
        });

        // Add generic HTML Element info

        pl.AddListPayLoad(Props);
        pl.ClosePackage();
        return pl;
    }

    //----------------------------------------------------------------------------------------------------------------------
    // Get ControlPropertyPayload
    //----------------------------------------------------------------------------------------------------------------------

    GingerLib.ControlPropertyPayload = function (Name, Value) {
        var plp = new GingerPayLoad("HTML Element Property");
        plp.AddValueString(Name);
        plp.AddValueString(Value);
        plp.ClosePackage();
        return plp;
    }

    //----------------------------------------------------------------------------------------------------------------------
    // HighLightElement
    //----------------------------------------------------------------------------------------------------------------------
    GingerLib.HighLightElement = function (XPath,Path) {
        //TODO: only once
        GingerLib.AddGingerHighLightStyle();

        if (GingerLib.CurrentHighlightElement != undefined) {
            $(GingerLib.CurrentHighlightElement).removeClass("GingerHighlight");
        }
        
        //This to support IFRAME elements.        
        GingerLib.SetFramebyPath(Path);

        el = GingerLib.GetElementByXPath(XPath, currentframe);
        $(el).addClass("GingerHighlight");
        GingerLib.CurrentHighlightElement = el;
    }

    //----------------------------------------------------------------------------------------------------------------------
    // Set Frame by Path
    //----------------------------------------------------------------------------------------------------------------------
    GingerLib.SetFramebyPath = function (Path) {
        //This to support IFRAME elements.        
        if (Path != "") {
            GingerLib.SwitchToDefaultFrame();
            var elFrame = GingerLib.GetElement("ByXPath", Path);
            if (elFrame == null)
                return null;
            GingerLib.SwitchFrame(elFrame);
        }
        else
            GingerLib.SwitchToDefaultFrame();
    }
    //----------------------------------------------------------------------------------------------------------------------
    // Write to Log - ////console
    //----------------------------------------------------------------------------------------------------------------------
    GingerLib.Log = function (txt) {
    }
    
    GingerLib.GetElementWithImplicitSync = function (LocateBy, LocateValue)
    {
        ErrorMessage="";
        var el = GingerLib.GetElement(LocateBy, LocateValue);    
        var bStopWaiting = false;
        var start = new Date().getTime();
        var elapsed = new Date().getTime() - start;
		var docReadyFlag= false;
        while (!bStopWaiting)
        {       
            if (elapsed > ImplicitWait * 1000)
            {              
                break;
            }
            if ((docState = document.readyState) == "complete")//The document and all sub- resources have finished loading.The state indicates that the load event is about to fire.
            {
				docReadyFlag=true;
                if (el == undefined)
                {
                    el = GingerLib.GetElement(LocateBy, LocateValue);
                }

				if(el!=undefined)
                {
					if (GingerLib.isVisible(el) || $(el).prop("type") == "checkbox")
					{					
						bStopWaiting = true;
					}
					else if (GingerLib.GetElemTag(el) == "SELECT")
					{				
						bStopWaiting = true;
					}
				}			
            }
			else
			{
				docReadyFlag=false;
            }

            GingerLib.Wait(500);

            elapsed = new Date().getTime() - start;         
        }
		if(!docState)
		{
			ErrorMessage="ERROR|Page not loaded fully within implicit wait";
		
			return undefined;
		}
		else if (el == undefined) 
		{            
           ErrorMessage = "ERROR|Web Element not Found: " + LocateBy + " " + LocateValue;
		
		   return undefined;
        }
		else if(!bStopWaiting)
		{
			ErrorMessage ="ERROR| Web Element was found but was not visible within implicit wait";
			
			return undefined;
		}
		else
		{
			return el;
		}       		      
    }
    GingerLib.GetElement = function (LocateBy, LocateValue) {

        ErrorMessage = "";
        // Use only JQuery so will work on any browser, use later on in Selenium driver too, so keep it generic
        if (LocateBy == "ByID") {
            if (currentframe != undefined) {//handle Frame 
                return currentframe.contentWindow.document.getElementById(LocateValue);
            }
            else {
                return document.getElementById(LocateValue);
            }

        }
        // TODO: replace to JQuery
        if (LocateBy == "ByName") {
            if (currentframe != undefined) {//handle Frame
                return currentframe.contentWindow.document.getElementsByName(LocateValue)[0];
            }
            if (document.getElementsByName(LocateValue).length == 1)
                return document.getElementsByName(LocateValue)[0];
            return null;
        }

        if (LocateBy == "ByXPath" || LocateBy == "ByRelXPath") {
            if (LocateValue != "")
               return GingerLib.GetElementByXPath(LocateValue,currentframe);
            else
                return null;
        }
        if (LocateBy == "ByJQuery") {
            return $(LocateValue)[0];
        }
    }

    GingerLib.setSelectedValue = function (selectObj, valueToSet) {
        if (currentframe != undefined) {//handle DDL on Frame
            $(selectObj).val(valueToSet).change();
            return
        }
        else {
            for (var i = 0; i < selectObj.options.length; i++) {
                if (selectObj.options[i].text == valueToSet) {
                    selectObj.selectedIndex = i;
                    GingerLib.fireAllChangeEvents(selectObj);
                    return;
                }
            }
        }
    }

    GingerLib.getSelectedValue = function (selectObj) {
        for (var i = 0; i < selectObj.options.length; i++) {
            if (selectObj.options[i].selected == true) {
                return selectObj.options[i].text;
            }
        }
        return "";
    }

    GingerLib.validateCheckBox = function (selectObj) {
        var Status;
        var sStatus;
        Status = selectObj.checked;
        sStatus = Status.toString();
        return sStatus;
    }
    GingerLib.SelectFromDropDown = function (e, Value) {
        var elelmentTag;
        try {
            elelmentTag = GingerLib.GetElemTag(e);
            if (elelmentTag == "SELECT") {
                //Set selected
                GingerLib.setSelectedValue(e, Value);
                var PLResp = new GingerPayLoad("Ok");
                PLResp.AddValueString("set to " + Value);
                PLResp.ClosePackage()
                return PLResp;
            }
        }

        catch (err) {
            var pl = new GingerPayLoad("ERROR");
            pl.AddValueString("ERROR: " + err + " at SelectFromDropDown");
            pl.ClosePackage();
            return pl;
        }
    }

    GingerLib.SetElementValue = function (e, Value) {
        var elelmentTag;
        try {
            elelmentTag = GingerLib.GetElemTag(e);
            // for Select we have different Action (select form DDL) 
            //this is left here to support Set Value on DDL.
            if (elelmentTag == "SELECT") {

                //Set selected
                GingerLib.setSelectedValue(e, Value);
                var PLResp = new GingerPayLoad("Ok");
                PLResp.AddValueString("set to " + Value);
                PLResp.ClosePackage()
                return PLResp;

            }
            if (elelmentTag == "SPAN") {       
                $(e).text(Value);
                var PLResp = new GingerPayLoad("Ok");
                PLResp.AddValueString("set to " + Value);
                PLResp.ClosePackage()
                return PLResp;

            }
            if (elelmentTag == "INPUT" && elelmentTag == "checkbox") {
                e.checked = (Value == "true");
                var PLResp = new GingerPayLoad("Ok");
                PLResp.AddValueString("set to " + Value);
                PLResp.ClosePackage()
                return PLResp;
            }
            else
            {
                //check if element is React object
                var reactElem = GingerLib.FindReact(e);
                if (reactElem != null) {
                    reactElem.props.onChange(Value);
                }
                else {
                    $(e).val(Value).change();          
                    if (currentframe == undefined)//Do not do this is if u are Frame.
                    {
                        GingerLib.fireAllChangeEvents(e);
                    }
                }
                var PLResp = new GingerPayLoad("Ok");
                PLResp.AddValueString("set to " + Value);
                PLResp.ClosePackage()
                return PLResp;
            }
        }
        catch (err) {
            var pl = new GingerPayLoad("ERROR");
            pl.AddValueString("ERROR: " + err + " at SetElementValue");
            pl.ClosePackage();
            return pl;
        }
    }
    GingerLib.FindReact = function (dom) {
        try {
            for (var key in dom) {
                if (key.startsWith("__reactInternalInstance$")) {
                    var compInternals = dom[key]._currentElement;
                    var compWrapper = compInternals._owner;
                    var comp = compWrapper._instance;
                    return comp;
                }
            }
        }
        catch (err) { }
        return null;
    };

    GingerLib.GetElemTag = function(e)
    {
        var tag;

        if (e.tagName != undefined) {
            tag = e.tagName;
        }
        else {
            tag = e.prop("tagName");
        }
        return tag;
    }

    GingerLib.fireSpecialEvent = function (el, operation) {
        try{
            var listCreateEvent = operation.split(',');
            if ("createEvent" in document)
            {
                    for (i = 0; i < listCreateEvent.length; i++) {
                        try {
                                    var evt = document.createEvent("HTMLEvents");
                                    evt.initEvent(listCreateEvent[i].toString(), false, true);
                                    el.dispatchEvent(evt);
                        }
                        catch (err) { }
                    }
                }
            else
            {
                for (i = 0; i < listCreateEvent.length; i++) {
                    try {
                            el.fireEvent("on" + listCreateEvent[i].toString());
                    }       
                    catch (err) { }
                }
            }
            var pl = new GingerPayLoad("OK");
            pl.AddValueString("Fire Special Event invoked successfully");  
            pl.ClosePackage();
            return pl;
        }
        catch (err) {
            var pl = new GingerPayLoad("ERROR");
            pl.AddValueString("ERROR: " + err + " at fireSpecialEvent");
            pl.ClosePackage();
            return pl;
        }
    }


    GingerLib.fireAllChangeEvents = function (el)
    {
        var tagName = GingerLib.GetElemTag(el);  

        if (tagName == "INPUT")
        {          
            if ("createEvent" in document) {
                var evt = document.createEvent("HTMLEvents");
                evt.initEvent("change", false, true);
                el.dispatchEvent(evt);
            }
            else
            el.fireEvent("onchange");
        }
        if (tagName == "SELECT")
        {
            if (el.getAttribute("onchange") != null) {
                // support control that fire Onchange event.
                el.onchange();
            }
            if (el.getAttribute("data-bind") != null) {
                //support control with data bind Attribute (knockout.js).
                $(el).change();
            }
            if (el.getAttribute("_data_id") != null) {
                    el.fireEvent("onchange");
            }
        }
            return;
    }

    GingerLib.SelectFromDropDownByIndex = function (el, Value) {
        try {
            var Response = new GingerPayLoad("Select From DropDown By Index");
            $(el).get(0).selectedIndex = Value;
            GingerLib.fireAllChangeEvents(el);
            Response.AddValueString("" + Value);
            Response.ClosePackage();
            return Response;
        }
        catch (err) {
            var pl = new GingerPayLoad("ERROR");
            pl.AddValueString("ERROR: " + err + " at SelectFromDropDownByIndex");
            pl.ClosePackage();
            return pl;
        }
    }

    GingerLib.GetListDetails = function (el) {
        var pl = new GingerPayLoad("List Items");
        var Elems = [];

        for (var i = 0; i < el.options.length; i++) {
            Elems.push(el.options[i].text);
        }
        pl.AddValueList(Elems);
        pl.ClosePackage();
        return pl;
    }

    GingerLib.GetElementValue = function (el) {
        try {
            var Response = new GingerPayLoad("ComponentValue");
            Response.AddValueString(GingerLib.GetElemValue(el));
            Response.ClosePackage();
            return Response;
        }
        catch (err) {
            var pl = new GingerPayLoad("ERROR");
            pl.AddValueString("ERROR: " + err + " at GetElementValue");
            pl.ClosePackage();
            return pl;
        }
    }

    GingerLib.SwitchFrame = function (el) {
        try {          
            var Response = new GingerPayLoad("Switch Frame");
            var iframe = $(el)[0];
            iframe.contentWindow.focus();
            currentframe = iframe;
            Response.AddValueString("" + iframe);
            Response.ClosePackage();            
            GingerLib.StartEventListnerFrame();
            return Response;

        }
        catch (err) {
            var pl = new GingerPayLoad("ERROR");
            pl.AddValueString("ERROR: " + err + " at Switch Frame");
            pl.ClosePackage();
            return pl;
        }

    }
    GingerLib.IsEnabled = function (el) {
        try {
            var Response = new GingerPayLoad("ComponentValue");
            var val = el.disabled;
            Response.AddValueString("" + !val);
            Response.ClosePackage();
            return Response;

        }
        catch (err) {
            var pl = new GingerPayLoad("ERROR");
            pl.AddValueString("ERROR: " + err + " at IsVisible");
            pl.ClosePackage();
            return pl;
        }
    }

    GingerLib.IsVisible = function (el) {
        try {
            var Response = new GingerPayLoad("ComponentValue");
            var val = GingerLib.isVisible(el);
            Response.AddValueString("" + val);
            Response.ClosePackage();
            return Response;

        }
        catch (err) {
            var pl = new GingerPayLoad("ERROR");
            pl.AddValueString("ERROR: " + err + " at IsVisible");
            pl.ClosePackage();
            return pl;
        }
    }
	
    GingerLib.fireMouseEvent = function(el,evtName) {
        try {
            var Response = new GingerPayLoad("FireMouseEvent");
            if (el.dispatchEvent) {
                var event = document.createEvent("MouseEvents");
                event.initMouseEvent(evtName, true, true, window,
                    0, 0, 0, 0, 0, false, false, false, false, 0, null);
                el.dispatchEvent(event);
            } else if (el.fireEvent) {
                event = document.createEventObject();
                event.button = 1;
                el.fireEvent("on" + evtName, event);
                el.fireEvent(evtName);
            } else {
                el[evtName]();
            }
            Response.AddValueString("Done");
            Response.ClosePackage();
            return Response;
        }
        catch (err) {
            var pl = new GingerPayLoad("ERROR");
            pl.AddValueString("ERROR: " + err + " at Fire mouse event");
            pl.ClosePackage();
            return pl;
        }
    }
    
    GingerLib.GetDataBindArrEvent = function (el) {

        var atr = el.getAttribute("data-bind").toString();
        var pEvent = atr.indexOf("event:");
        var start = atr.indexOf("{", pEvent)
        var end = atr.indexOf("}", start);
        var listEvent = atr.slice(start + 1, end);
        var arr = listEvent.split(",");

        var arrEventName = [];
        arr.forEach(function (item) {
            arrEventName.push(item.substring(0, item.indexOf(":")).trim());
        });
        return arrEventName;
     }
    GingerLib.ClickElement = function (el, Value) {
        try {
            var Response = new GingerPayLoad("ComponentValue");
            var eventfired = false;
            var overridePopUp = false;
            if (Value != undefined && Value.length!=0) {               
                var res = Value.split(',');//Handel popup message from browser
                var popUpType="";
                var popUpOperation = "";

                if (res[0] != undefined)
                    popUpType = GingerLib.TrimSpaces(res[0].toLowerCase());
                if (res[1] != undefined)
                    popUpOperation = GingerLib.TrimSpaces(res[1].toLowerCase());
              
                if (popUpType == "confirm") {
                    
                    overridePopUp = true;
                    oldconfirm = window.confirm;
                 
                    if (popUpOperation == "ok") {
                      
                        window.confirm = function () { return true; }
                    }
                    else {                        
                        window.confirm = function () { return false; }
                    }
                }               
                else if (popUpType== "alert") {
                 
                    overridePopUp = true;
                    oldalert = window.alert;
                    if (popUpOperation == "ok") {
                      
                        window.alert = function () { return true; }
                    }
                }
                else if (popUpType == "prompt") {
                   
                }

            }

            if (el.tagName == "INPUT" && $(el).prop("type") == "radio" && el.getAttribute("data-bind") != null) {
                var arr = GingerLib.GetDataBindArrEvent(el);
                for (var i = 0; i < arr.length; i++)
                {
                    if (arr[i] == "mousedown")
                    {
                        $(el).mousedown();
                        eventfired = true;
                    }
                }
                if (!eventfired)
                {
                    el.click();
                }
            }
            else {
                el.click();
            }
						
            Response.AddValueString("Done");
            Response.ClosePackage();
            if (overridePopUp) {
                if (popUpType == "confirm") {
                    window.confirm = oldconfirm;
                }
                else if (popUpType == "alert") {
                    window.alert = oldalert;
                }
            }		
			return Response;

			/*
			//TODO: Implement Wait for idle mechanism also for Widgets
			if(GingerLib.SyncForPageLoad())
			{
				 Response.AddValueString("Done");
				 Response.ClosePackage();
				 return Response;
			}
			else
			{
				 var pl = new GingerPayLoad("ERROR");
			     pl.AddValueString("Warning: Click is performed but page load timed out");
			     pl.ClosePackage();
				 return pl;
			}*/

        }
        catch (err) {
            var pl = new GingerPayLoad("ERROR");
            pl.AddValueString("ERROR: " + err + " at ClickElement");
            pl.ClosePackage();
            return pl;
        }
    }


	GingerLib.SyncForPageLoad =function()
	{
		  var start = new Date().getTime();
	      var elapsed = new Date().getTime() - start;
		  var bStopWaiting=false;
		  var idleTime;
		  
		  while (!bStopWaiting)
		  {       
            if (elapsed > ImplicitWait * 1000)
            {              
                break;
            }
            
			if ((docState = document.readyState) == "complete")
            {
				if(idleTime==undefined)
					idleTime=elapsed;

				else if((elapsed- idleTime)>3000)
					bStopWaiting=true;
			}
			else
				idleTime=undefined;

			elapsed = new Date().getTime() - start; 

	 	  }

		 return bStopWaiting;
	}

    GingerLib.Wait = function (waitTimeMS)
    {
        var start = new Date().getTime();
        var elapsed = new Date().getTime() - start;
        while (elapsed < waitTimeMS)
        {
            elapsed = new Date().getTime() - start;
        }
    }
	
    GingerLib.AsnycClickElement = function (el) {
        try {
            var Response = new GingerPayLoad("ComponentValue");
            setTimeout(function () { el.click(); }, 100)
            Response.AddValueString("Done");
            Response.ClosePackage();
            return Response;

        }
        catch (err) {
            var pl = new GingerPayLoad("ERROR");
            pl.AddValueString("ERROR: " + err + " at AsnycClickElement");
            pl.ClosePackage();
            return pl;
        }
    }
    GingerLib.ScrollDown = function (el, Value) {
        var speed = Number(GingerLib.TrimSpaces(Value));
        if (speed == 0 || isNaN(speed))//0 = empty string
            speed = 400;
        try {
            $(el).animate({
                scrollTop: $(el)[0].scrollHeight
            }, speed);
        }
        catch (err) {
            var pl = new GingerPayLoad("ERROR");
            pl.AddValueString("ERROR: " + err + " at ScrollDown");
            pl.ClosePackage();
            return pl;
        }
        var Response = new GingerPayLoad("OK");
        Response.AddValueString("Scroll down ended successfully")
        Response.ClosePackage();
        return Response;
    }
    GingerLib.ScrollUp = function (el, Value) {
        var speed = Number(GingerLib.TrimSpaces(Value));
        if (speed == 0 || isNaN(speed))//0 = empty string
            speed = 400;
        try {
            $(el).animate({
                scrollTop: 0
            }, speed);
        }
        catch (err) {
            var pl = new GingerPayLoad("ERROR");
            pl.AddValueString("ERROR: " + err + " at ScrollUp");
            pl.ClosePackage();
            return pl;
        }
        var Response = new GingerPayLoad("OK");
        Response.AddValueString("Scroll Up ended successfully")
        Response.ClosePackage();
        return Response;
    }

    //Custom Trim function
    GingerLib.TrimSpaces = function (el) {
        try {
            return el.trim();  
        }
        catch (err) {
            // Lower  versions of browser do not support trim. So we handle it explicitly
            return el.replace(/^\s+|\s+$/g, '');
        }
    }
}