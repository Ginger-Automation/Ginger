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

// TODO: use this one make me work
GingerLib.GetElemXPath = function (el) {
    // Another impl for XPath

    // #1            

    // #2

    //FireBug XPath            
    var element = el;
    //TODO:Handle Smart XPATH Later
        return GingerLib.getElementTreeXPath(element);
}

GingerLib.getElementTreeXPath = function (element) {
    //This is bit faster compared to all one
    var xpath = '';
    try {        
        for (; element && element.nodeType == 1; element = element.parentNode) {         
            var tagname = element.tagName;
            if (tagname.indexOf(":") > 0)
                var tagname = tagname.substring(0, tagname.indexOf(":"));
            var id = +$(element.parentNode).children(tagname).index(element) + 1;            
            id > 1 ? (id = '[' + id + ']') : (id = '');            
            if (element.tagName.indexOf(":") > 0) {
                var index = +$(element).parent().children().index(element) +1                
                xpath = '/*[' + index + "]" + id + xpath;
            }             
            else
                xpath = '/' + element.tagName.toLowerCase() + id + xpath;           
        }        
    }
    catch (error) {
        return "";
    }
    if (xpath.indexOf("select") > 0 && xpath.indexOf("/option") > 0)
        {
            xpath = xpath.replace("/option","")
        }
    
    if (xpath.indexOf('/html') != 0 && xpath != "")
        xpath = "/" + xpath;
    return xpath;
};

GingerLib.getElementTreeRelXPath = function (element) {
    //This is bit faster compared to all one
    var xpath = '';
    try {
        for (; element && element.nodeType == 1; element = element.parentNode) {            
            var tagname = element.tagName;
            var xtagName = element.tagName.toLowerCase();
            if (tagname.indexOf(":") > 0) {
                tagname = tagname.substring(0, tagname.indexOf(":"));
                xtagName = "*";
            }

            var id = element.id;
            if (id != undefined){
                if (id != "") {
                    xpath = '//' + xtagName.toLowerCase() + '[@id=\'' + id + '\']' + xpath;
                    return xpath;
                }            
            }
            var text = element.name;
            if (text != undefined) {
                if (text != "") {
                    xpath = '//' + xtagName.toLowerCase() + '[@name=\'' + text + '\']' + xpath;
                    return xpath;
                }
            }
            var idx = +$(element.parentNode).children(tagname).index(element) + 1;
            idx > 1 ? (idx = '[' + idx + ']') : (idx = '');

            if (element.tagName.indexOf(":") > 0) {
                var index = +$(element).parent().children().index(element) + 1
                xpath = '/' + xtagName + '[' + index + ']' + idx + xpath;
            }
            else
                xpath = '/' + xtagName + idx + xpath;
        }
    }
    catch (error) {
        return "";
    }
    return xpath;
};

GingerLib.getElementTreeSmartXPath = function (element) {
    //This is bit faster compared to all one
    var xpath = '';
    try {
        for (; element && element.nodeType == 1; element = element.parentNode) {
            var tagname = element.tagName;
            var xtagName = element.tagName.toLowerCase();
            if (tagname.indexOf(":") > 0) {
                tagname = tagname.substring(0, tagname.indexOf(":"));
                xtagName = "*";
            }

            var id = element.id;
            if (id != undefined) {
                if (id != "") {
                    xpath = '//' + xtagName.toLowerCase() + '[@id=\'' + id + '\']' + xpath;
                    return xpath;
                }
            }
            var text = element.name;
            if (text != undefined) {
                if (text != "") {
                    xpath = '//' + xtagName.toLowerCase() + '[@name=\'' + text + '\']' + xpath;
                    return xpath;
                }
            }

            var node = element.parentNode.firstChild;
            var i = 1;
            while (node && node.nodeType === 1 && node !== element) {
                var ntagname = node.tagName;
                var nxtagName = node.tagName.toLowerCase();
                if (ntagname.indexOf(":") > 0) {
                    ntagname = ntagname.substring(0, ntagname.indexOf(":"));
                    nxtagName = "*";
                }

                var id = node.id;
                if (id != undefined) {
                    if (id != "") {
                        xpath = '//' + nxtagName.toLowerCase() + '[@id=\'' + id + '\']/..' + xpath;
                        return xpath;
                    }
                }
                var text = element.name;
                if (text != undefined) {
                    if (text != "") {
                        xpath = '//' + nxtagName.toLowerCase() + '[@name=\'' + text + '\']/..' + xpath;
                        return xpath;
                    }
                }
                node = node.nextElementSibling || node.nextSibling;
                i = +i + 1;
            }

            var idx = +$(element.parentNode).children(tagname).index(element) + 1;
            idx > 1 ? (idx = '[' + idx + ']') : (idx = '');

            if (element.tagName.indexOf(":") > 0) {
                var index = +$(element).parent().children().index(element) + 1
                xpath = '/' + xtagName + '[' + index + ']' + idx + xpath;
            }
            else
                xpath = '/' + xtagName + idx + xpath;
        }
    }
    catch (error) {
        return "";
    }
    return xpath;
};

GingerLib.GetElementByXPath = function (XPath, currentframe) {
    //Do only once
    if (GingerLib.wgxpathinstall != "Done") {
        wgxpath.install();
        GingerLib.wgxpathinstall = "Done";
    }
    
    if (currentframe != undefined) //handle Frame        
        var xPathRes = document.evaluate(XPath, currentframe.contentDocument, null, XPathResult.ORDERED_NODE_ITERATOR_TYPE, null);
    else
        var xPathRes = document.evaluate(XPath, document.body, null, XPathResult.ORDERED_NODE_ITERATOR_TYPE, null);
    // Get the first element matching
    if (xPathRes == null)
        return null;
    
    var nextElement = xPathRes.iterateNext();
    return nextElement;
};
