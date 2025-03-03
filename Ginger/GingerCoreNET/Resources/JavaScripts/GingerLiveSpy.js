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

// This JS will be injected into Browser active page, for use in Selenium driver and Java driver for Widgets 

GingerLibLiveSpy = {};

// Define the GingerLib of function we can use, in this way it is kind of JS OO, will not overlap with other JS scripts running on the page

'use strict';
function define_GingerLibLiveSpy() {
    GingerLibLiveSpy = {};

    //----------------------------------------------------------------------------------------------------------------------
    // To Highlight element we keep the current element and add/remove the GingerHighlight style to the element
    //----------------------------------------------------------------------------------------------------------------------
    var CurrentX;
    var CurrentClickedX;
    var CurrentY;
    var CurrentClickedY;

    //----------------------------------------------------------------------------------------------------------------------
    // Add Java Script to page
    //----------------------------------------------------------------------------------------------------------------------

    // Function to get all shadow roots recursively
    function getAllShadowRoots(node, result = []) {
        if (node.shadowRoot) {
            result.push(node.shadowRoot);
            // Recursively get shadow roots inside the current shadow root
            getAllShadowRoots(node.shadowRoot, result);
        }

        node = node.firstChild;
        while (node) {
            getAllShadowRoots(node, result);
            node = node.nextSibling;
        }

        return result;
    }

    // Get all shadow roots in the entire document
    const allShadowRoots = getAllShadowRoots(document);
    GingerLibLiveSpy.AddScript = function (JavaScriptCode) {
        var script = document.createElement('script');
        script.type = "text/javascript";
        script.text = JavaScriptCode;
        var head = document.getElementsByTagName('head')[0];
        head.appendChild(script);
        return "OK";
    }

    GingerLibLiveSpy.GetXPoint = function () {

        console.log("CurrentX: " + CurrentX);
        return CurrentX + "";
    }

    GingerLibLiveSpy.GetYPoint = function () {

        console.log("CurrentY: " + CurrentY);
        return CurrentY + "";
    }

    GingerLibLiveSpy.GetClickedXPoint = function () {
        return CurrentClickedX + "";
    }

    GingerLibLiveSpy.GetClickedYPoint = function () {
        return CurrentClickedY + "";
    }

    function getMousePos(event) {
        CurrentX = event.clientX;
        console.log("getMousePos-X " + CurrentX);
        CurrentY = event.clientY;
        console.log("getMousePos-Y " + CurrentY);
    }

    function getMouseClickedPos(event) {
        CurrentClickedX = event.clientX;
        CurrentClickedY = event.clientY;
    }

    GingerLibLiveSpy.StartEventListner = function () {
        console.log("Event started");
        if (document.addEventListener) {
            document.addEventListener("mouseover", getMousePos);
        }
        else if (document.attachEvent)// Add to support erlier Version on IE .
        {
            document.attachEvent("onmouseover", getMousePos);
        }
    }

    GingerLibLiveSpy.StartClickEventListner = function () {
        console.log("Event started");
        if (document.addEventListener) {
            document.addEventListener("click", getMouseClickedPos);

        }
        else if (document.attachEvent)// Add to support earlier Version on IE .
        {
            document.attachEvent("click", getMouseClickedPos);
        }
    }

    GingerLibLiveSpy.ElementFromPoint = function () {
        const X = GingerLibLiveSpy.GetXPoint();
        const Y = GingerLibLiveSpy.GetYPoint();
        let depth = 0;
        let resultElementFromPoint = null;
        function getCurrentDepth(element) {
            let depth = 0;

            while (element.parentNode) {
                element = element.parentNode;
                depth++;
            }
            return depth;
        }

        allShadowRoots.forEach((shadowRoot) => {
            let element = shadowRoot.elementFromPoint(X, Y);
            const currentDepth = getCurrentDepth(element);

            if (depth < currentDepth) {
                depth = currentDepth;
                resultElementFromPoint = element;
            }
        });

        if (!resultElementFromPoint) {
            return document.elementFromPoint(X, Y);
        }

        return resultElementFromPoint;
    }

    GingerLibLiveSpy.DeepestElementFromPoint = function () {
        const x = GingerLibLiveSpy.GetXPoint();
        const y = GingerLibLiveSpy.GetYPoint();

        let element = document.elementFromPoint(x, y);

        function getDeepestElement(currElement, x, y) {
            let children = currElement.querySelectorAll('*');
            if (children.length === 0)
                children = currElement.shadowRoot.querySelectorAll('*');

            for (let i = 0; i < children.length; i++) {
                let child = children[i];
                let bounds = child.getBoundingClientRect();
                if (x >= bounds.left && x <= bounds.right && y >= bounds.top && y <= bounds.bottom)
                    return getDeepestElement(child, x, y);
            }

            return currElement;
        }

        return getDeepestElement(element, x, y);
    }

    GingerLibLiveSpy.IsLiveSpyExist = function () {
        //  Return true if there is click event on the element
        //  Return el.haveclickevent…
        return "yes";
    }
}