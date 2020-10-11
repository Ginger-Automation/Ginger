/*
Copyright © 2014-2020 European Support Limited

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

// This JS will be injected into Browser active page, for use in Selenium driver and Java driver for Widgets Recording
	
GingerRecorderLib = {};

// Define the GingerLib of function we can use, in this way it is kind of JS OO, will not overlap with other JS scripts running on the page

'use strict';
function define_GingerRecorderLib() {
    GingerRecorderLib = {};

	var actions =	[];
	var count	=	0;
    var recordingStarted = "false";
    var isActionFromTimeOut = false;
			
	//----------------------------------------------------------------------------------------------------------------------	 
	//  StartRecording
	//----------------------------------------------------------------------------------------------------------------------
    GingerRecorderLib.StartRecording = function () {
        recordingStarted = "true";

        $(":button").click(function (event) {

            element = event.currentTarget;
            GingerRecorderLib.AddRecordingtoQ(element, "Click", element.value, element.type, $(this).offset().left, $(this).offset().top);

        });

        $(":submit").click(function (event) {

            if (!isActionFromTimeOut) {
                element = event.currentTarget;
                GingerRecorderLib.AddRecordingtoQ(element, "Submit", element.value, element.type, $(this).offset().left, $(this).offset().top);

                setTimeout(function () { isActionFromTimeOut = true; element.click(); }, 1500);
                return false;
            }
            isActionFromTimeOut = false;

        });

        //checkbox click action
        $(":checkbox").click(function (event) {
            element = event.currentTarget;
            event.stopPropagation();
            GingerRecorderLib.AddRecordingtoQ(element, "Click", element.value, element.type, $(this).offset().left, $(this).offset().top);
        });

        //radio click action
        $(":radio").click(function (event) {
            element = event.currentTarget;
            event.stopPropagation();
            GingerRecorderLib.AddRecordingtoQ(element, "Click", element.value, element.type, $(this).offset().left, $(this).offset().top);
        });

        //text change action
        $(":text").change(function (event) {            
            element = event.currentTarget;
            GingerRecorderLib.AddRecordingtoQ(element, "SetValue", element.value, element.type, $(this).offset().left, $(this).offset().top);
        });

        $(":password").change(function (event) {

            element = event.currentTarget;
            GingerRecorderLib.AddRecordingtoQ(element, "SetValue", element.value, element.type, $(this).offset().left, $(this).offset().top);
        });

        //select change action
        $("select").change(function (event) {

            element = event.currentTarget;
            var optionSelected = $(this).find("option:selected");
            var elemValue = optionSelected.text();

            GingerRecorderLib.AddRecordingtoQ(element, "SelectByText", elemValue, element.type, $(this).offset().left, $(this).offset().top);

        });

        //select change action
        $("textarea").change(function (event) {

            element = event.currentTarget;
            GingerRecorderLib.AddRecordingtoQ(element, "SendKeys", element.value, element.type, $(this).offset().left, $(this).offset().top);

        });

        $('div').each(function (event) {

            $(this).on("click", function (event) {

                if (!GingerRecorderLib.IsClickeEventExist(this)) {
                    element = event.currentTarget;
                    GingerRecorderLib.AddRecordingtoQ(element, "Click", element.id, element.tagName, $(this).offset().left, $(this).offset().top);
                }
            });

        });

        $('p').on('click', function (event) {
            element = event.currentTarget;
            GingerRecorderLib.AddRecordingtoQ(element, "Click", element.id, element.tagName, $(this).offset().left, $(this).offset().top);

        });

        $('a').on('click', function (event) {
            element = event.currentTarget;
            var elemValue = $(this).attr('href');

            if (elemValue == null)
                elemValue = $(event.target).text();

            GingerRecorderLib.AddRecordingtoQ(element, "Click", elemValue, element.tagName, $(this).offset().left, $(this).offset().top);
            if (($(this).attr('href') !== undefined))
            {
                setTimeout(function () { window.location.href = elemValue }, 1500);
                return false;
            }
        });

        $('img').on('click', function (event) {
            element = event.currentTarget;
            GingerRecorderLib.AddRecordingtoQ(element, "Click", element.id, element.tagName, $(this).offset().left, $(this).offset().top);
        });

        $('li').on('click', function (event) {
            element = event.currentTarget;
            GingerRecorderLib.AddRecordingtoQ(element, "Click", element.id, element.tagName, $(this).offset().left, $(this).offset().top);
        });

        $(":file").click(function (event) {
            element = event.currentTarget;
            GingerRecorderLib.AddRecordingtoQ(element, "Click", element.value, element.type, $(this).offset().left, $(this).offset().top);
        });

        $('span').on('click', function (event) {
            element = event.currentTarget;
            var elemValue = $(this).text();
            GingerRecorderLib.AddRecordingtoQ(element, "Click", elemValue, element.tagName, $(this).offset().left, $(this).offset().top);
        });

    }

	//----------------------------------------------------------------------------------------------------------------------	 
	//  AddRecordingtoQ (adding values to PayLoad)
	//----------------------------------------------------------------------------------------------------------------------	
	
    GingerRecorderLib.AddRecordingtoQ = function (element, action, value, type, xCordinate, yCordinate) {       
        var PLR = new GingerPayLoad("html" + type);
		GingerRecorderLib.AddBestLocator(element, PLR);
		if (value != undefined || value != "")
        {
            PLR.AddValueString(value);			
		}	
		PLR.AddValueString(action);
        PLR.AddValueString(type);
        PLR.AddValueString(String(xCordinate));
        PLR.AddValueString(String(yCordinate));        
        PLR.ClosePackage();				
        actions[count] = PLR;
		count++; 
	}
	
	//----------------------------------------------------------------------------------------------------------------------	 
	//  AddBestLocator
	//----------------------------------------------------------------------------------------------------------------------		
	GingerRecorderLib.AddBestLocator = function(element, PLR){
		
		var xPath = "";
		
		if (element.id != "" && element.id != undefined) {		
			PLR.AddValueString("ByID");
			PLR.AddValueString(element.id);
			return;
		} 		
		if (element.name != "" && element.name != undefined) {
			PLR.AddValueString("ByName");
			PLR.AddValueString(element.name);
			return;
		}	
        else {
            xPath = GingerLib.GetElemXPath(element);
			PLR.AddValueString("ByXPath");
			PLR.AddValueString(xPath);
			return;		
		}
	}
	
	//----------------------------------------------------------------------------------------------------------------------	 
	//  LocateById - returns element value located by Id 
	//----------------------------------------------------------------------------------------------------------------------		
	GingerRecorderLib.LocateById = function(element){
		return element.id;		
	}

	//----------------------------------------------------------------------------------------------------------------------	 
	//  LocateByName - returns element value located by Name
	//----------------------------------------------------------------------------------------------------------------------		
	GingerRecorderLib.LocateByName = function(element){
		return element.name;		
	}
		
	//----------------------------------------------------------------------------------------------------------------------	 
	//  Get array actions[]
	//----------------------------------------------------------------------------------------------------------------------
	GingerRecorderLib.GetRecording = function (){
		
		var pl = new GingerPayLoad("HTML RecordedElems");
		pl.AddListPayLoad(actions);            
        pl.AddValueString(recordingStarted);
		pl.ClosePackage(); 		
				
		actions = [];
		count	=	0;
		return pl;
	}

	//----------------------------------------------------------------------------------------------------------------------	 
	//  StopRecording
	//----------------------------------------------------------------------------------------------------------------------	
	GingerRecorderLib.StopRecording = function (){
        $(":button").off("click");
        $(":submit").off("click");
        $(":checkbox").off("click");
        $(":radio").off("click");
        $(":text").off("change");
        $(":password").off("change");
        $("select").off("change");
        $("textarea").off("change");
        $('div').off("click");
        $('p').off("click");
        $('a').off("click");
        $('img').off("click");
        $('li').off("click");
        $(":file").off("click");
        $('span').off("click");


        actions = [];
        recordingStarted = false;
        return actions;
	}

	//----------------------------------------------------------------------------------------------------------------------	 
	//  IsClickeEventExist
	//----------------------------------------------------------------------------------------------------------------------	
	GingerRecorderLib.IsClickeEventExist = function (elem){
		var ev = $._data(elem, 'events');
		if(ev && ev.click){
			return true;
		}
    }

    //----------------------------------------------------------------------------------------------------------------------	 
    //  IsRecordStarted
    //----------------------------------------------------------------------------------------------------------------------	
    GingerRecorderLib.IsRecordStarted = function () {
        //  Return true if there is click event on the element
        //  Return el.haveclickevent…
        return recordingStarted;
    }

    GingerRecorderLib.IsRecordExist = function () {
        //  Return true if there is click event on the element
        //  Return el.haveclickevent…
        return "yes";
    }
}