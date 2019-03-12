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

var r = 0;
var XpathTextBox;
var ValueTextBox;
var innerHTMLTextBox;
var currElem;

function xpath(el) {
	if (typeof el == "string") return document.evaluate(el, document, null, 0, null)
	if (!el || el.nodeType != 1) return ''
	if (el.id) return "//*[@id='" + el.id + "']"
	var sames = [].filter.call(el.parentNode.children, function (x) { return x.tagName == el.tagName })
	return xpath(el.parentNode) + '/' + el.tagName.toLowerCase() + (sames.length > 1 ? '[' + ([].indexOf.call(sames, el) + 1) + ']' : '')
}

function openGingerWin() {

	// We open popup window with same url as what we have then change the content so we don't get access violation of CSS

	var popup = window.open(window.URL, "_blank", "width=500, height=250");
	
	popup.document.write('<head>\r\n<title>\r\n Ginger Browser/Widgets ToolBox \r\n</title>\r\n</head> <body> <h3> Click on Web Element to see info </h3> </body>');

	var div1 = popup.document.createElement("div");	
	var v1 = popup.document.createTextNode("XPath");	
	div1.appendChild(v1)
	v1.width = "100px";

	XpathTextBox = popup.document.createElement("input");	
	XpathTextBox.style.width = "80%";	
	div1.appendChild(XpathTextBox)
	popup.document.body.appendChild(div1);
	

	var div2 = popup.document.createElement("div");
	var v3 = popup.document.createTextNode("value");
	v3.width = "100px";
	ValueTextBox = popup.document.createElement("input");
	ValueTextBox.style.width = "80%";
	div2.appendChild(v3)	
	div2.appendChild(ValueTextBox)
	popup.document.body.appendChild(div2);

	var div3 = popup.document.createElement("div");
	var v5 = popup.document.createTextNode("innerHTML");
	innerHTMLTextBox = popup.document.createElement("textarea");
	innerHTMLTextBox.style.width = "100%";
	innerHTMLTextBox.style.height = "120px";
	
	div3.appendChild(v5)
	div3.appendChild(innerHTMLTextBox)
	popup.document.body.appendChild(div3);
}

openGingerWin();

$('*').hover(
    function (e) {
		//Remove last highlighed elem
    	if (currElem != this)
    	{
    		$(currElem).css('border', 'none');

    		// Highlight new elem
    		$(this).css('border', '1px solid red');

    		currElem = this;

    		$(this).mousedown(function () {
    			r++;
    			var XPath = xpath(currElem);
    			XpathTextBox.value = XPath;
    			ValueTextBox.value = r + currElem.innerHTML;
    			innerHTMLTextBox.value = currElem.outerHTML;
    		});
    	}
        
        e.preventDefault();
        e.stopPropagation();
        return false;
    }, function (e) {
        $(this).css('border', 'none');
        e.preventDefault();
        e.stopPropagation();
        return false;
    }
);