
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
http://www.apache.org/licenses/LICENSE-2.0
*/

'use strict';

// This JS will be injected into Browser active page,
// for use in Selenium driver, Java driver, mobile, TV, etc.

var GingerLibLiveSpy = {};

function define_GingerLibLiveSpy() {

    GingerLibLiveSpy = {};

    //------------------------------------------------------------------------------------------------------------------
    // Global State
    //------------------------------------------------------------------------------------------------------------------
    var CurrentX;
    var CurrentY;
    var CurrentClickedX;
    var CurrentClickedY;

    // ✅ Android TV / Focus-based platforms
    var CurrentFocusedElement = null;

    //------------------------------------------------------------------------------------------------------------------
    // Utilities
    //------------------------------------------------------------------------------------------------------------------
    function getAllShadowRoots(node, result) {
        result = result || [];
        if (!node) return result;

        if (node.shadowRoot) {
            result.push(node.shadowRoot);
            getAllShadowRoots(node.shadowRoot, result);
        }

        var child = node.firstChild;
        while (child) {
            getAllShadowRoots(child, result);
            child = child.nextSibling;
        }

        return result;
    }

    function normalizeNode(el) {
        return (el && el.nodeType === 3) ? el.parentElement : el;
    }

    //------------------------------------------------------------------------------------------------------------------
    // Script Injection
    //------------------------------------------------------------------------------------------------------------------
    GingerLibLiveSpy.AddScript = function (JavaScriptCode) {
        var script = document.createElement('script');
        script.type = "text/javascript";
        script.text = JavaScriptCode;
        document.getElementsByTagName('head')[0].appendChild(script);
        return "OK";
    };

    //------------------------------------------------------------------------------------------------------------------
    // Coordinate Getters (DO NOT change return type)
    //------------------------------------------------------------------------------------------------------------------
    GingerLibLiveSpy.GetXPoint = function () { return CurrentX + ""; };
    GingerLibLiveSpy.GetYPoint = function () { return CurrentY + ""; };
    GingerLibLiveSpy.GetClickedXPoint = function () { return CurrentClickedX + ""; };
    GingerLibLiveSpy.GetClickedYPoint = function () { return CurrentClickedY + ""; };

    //------------------------------------------------------------------------------------------------------------------
    // Mouse / Pointer Handlers (Web)
    //------------------------------------------------------------------------------------------------------------------
    function getMousePos(event) {
        CurrentX = event.clientX;
        CurrentY = event.clientY;
    }

    function getMouseClickedPos(event) {
        CurrentClickedX = event.clientX;
        CurrentClickedY = event.clientY;
    }

    //------------------------------------------------------------------------------------------------------------------
    // ✅ Android TV / D‑Pad Handlers
    //------------------------------------------------------------------------------------------------------------------
    function getFocusedElement(event) {
        CurrentFocusedElement = event.target;
    }

    function getRemoteClick(event) {
        // Android TV ENTER keys
        if (event.keyCode === 23 || event.keyCode === 66) {
            CurrentClickedX = 0;
            CurrentClickedY = 0;
            CurrentFocusedElement = document.activeElement;
        }
    }

    //------------------------------------------------------------------------------------------------------------------
    // Event Listener Starters
    //------------------------------------------------------------------------------------------------------------------
    GingerLibLiveSpy.StartEventListner = function () {
        if (document.addEventListener) {
            // ✅ Keep mouseover + add mousemove for accuracy
            document.addEventListener("mouseover", getMousePos, true);
            document.addEventListener("mousemove", getMousePos, true);
        } else if (document.attachEvent) {
            document.attachEvent("onmouseover", getMousePos);
        }
    };

    GingerLibLiveSpy.StartClickEventListner = function () {
        if (document.addEventListener) {
            document.addEventListener("click", getMouseClickedPos, true);
        } else if (document.attachEvent) {
            document.attachEvent("onclick", getMouseClickedPos);
        }
    };

    // ✅ Android TV
    GingerLibLiveSpy.StartFocusEventListener = function () {
        document.addEventListener("focus", getFocusedElement, true);
    };

    GingerLibLiveSpy.StartKeyEventListener = function () {
        document.addEventListener("keydown", getRemoteClick, true);
    };

    //------------------------------------------------------------------------------------------------------------------
    // Element From Point (Web + Shadow DOM + Android TV fallback)
    //------------------------------------------------------------------------------------------------------------------
    GingerLibLiveSpy.ElementFromPoint = function () {

        var X = +GingerLibLiveSpy.GetXPoint();
        var Y = +GingerLibLiveSpy.GetYPoint();

        // ✅ Android TV fallback
        if ((!X || !Y) && CurrentFocusedElement) {
            return CurrentFocusedElement;
        }

        var element = document.elementFromPoint(X, Y);
        element = normalizeNode(element);

        // ✅ Dynamic shadow DOM support (SAFE)
        var shadowRoots = getAllShadowRoots(document, []);
        for (var i = 0; i < shadowRoots.length; i++) {
            try {
                var el = shadowRoots[i].elementFromPoint(X, Y);
                if (el) element = normalizeNode(el);
            } catch (e) { }
        }

        return element;
    };

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

    //------------------------------------------------------------------------------------------------------------------
    // ✅ Correct Deepest Element (SAFE, backward compatible)
    //------------------------------------------------------------------------------------------------------------------
    GingerLibLiveSpy.DeepestElementFromPoint = function () {

        var x = +GingerLibLiveSpy.GetXPoint();
        var y = +GingerLibLiveSpy.GetYPoint();

        var base = GingerLibLiveSpy.ElementFromPoint();
        if (!base) return null;

        var candidate = base;

        while (true) {
            var found = null;
            var children = candidate.shadowRoot
                ? candidate.shadowRoot.children
                : candidate.children;

            for (var i = 0; i < children.length; i++) {
                var r = children[i].getBoundingClientRect();
                if (x >= r.left && x <= r.right &&
                    y >= r.top && y <= r.bottom) {
                    found = children[i];
                    break;
                }
            }

            if (!found) break;
            candidate = found;
        }

        return candidate;
    };

    //------------------------------------------------------------------------------------------------------------------
    // Check LiveSpy Presence
    //------------------------------------------------------------------------------------------------------------------
    GingerLibLiveSpy.IsLiveSpyExist = function () {
        return "yes";
    };
}

// ✅ Initialize once (NO recursion)
try {
    define_GingerLibLiveSpy();
} catch (e) { }
