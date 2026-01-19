
/* 
Copyright © 2014-2020 European Support Limited
Licensed under the Apache License, Version 2.0 (the "License")
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

    // ✅ Android TV / Focus based platforms
    var CurrentFocusedElement = null;

    //------------------------------------------------------------------------------------------------------------------
    // Shadow DOM Support
    //------------------------------------------------------------------------------------------------------------------
    function getAllShadowRoots(node, result = []) {
        if (node.shadowRoot) {
            result.push(node.shadowRoot);
            getAllShadowRoots(node.shadowRoot, result);
        }
        node = node.firstChild;
        while (node) {
            getAllShadowRoots(node, result);
            node = node.nextSibling;
        }
        return result;
    }

    var allShadowRoots = getAllShadowRoots(document);

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
    // Coordinate Getters
    //------------------------------------------------------------------------------------------------------------------
    GingerLibLiveSpy.GetXPoint = function () {
        return CurrentX + "";
    };

    GingerLibLiveSpy.GetYPoint = function () {
        return CurrentY + "";
    };

    GingerLibLiveSpy.GetClickedXPoint = function () {
        return CurrentClickedX + "";
    };

    GingerLibLiveSpy.GetClickedYPoint = function () {
        return CurrentClickedY + "";
    };

    //------------------------------------------------------------------------------------------------------------------
    // Mouse Handlers (Existing platforms)
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
    // ✅ Focus Handler (Android TV / D‑Pad navigation)
    //------------------------------------------------------------------------------------------------------------------
    function getFocusedElement(event) {
        CurrentFocusedElement = event.target;
    }

    //------------------------------------------------------------------------------------------------------------------
    // ✅ Remote Key Handler (Android TV)
    //------------------------------------------------------------------------------------------------------------------
    function getRemoteClick(event) {
        // 23 = DPAD_CENTER, 66 = ENTER
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
            document.addEventListener("mouseover", getMousePos);
        } else if (document.attachEvent) {
            document.attachEvent("onmouseover", getMousePos);
        }
    };

    GingerLibLiveSpy.StartClickEventListner = function () {
        if (document.addEventListener) {
            document.addEventListener("click", getMouseClickedPos);
        } else if (document.attachEvent) {
            document.attachEvent("onclick", getMouseClickedPos);
        }
    };

    // ✅ Android TV
    GingerLibLiveSpy.StartFocusEventListener = function () {
        if (document.addEventListener) {
            document.addEventListener("focus", getFocusedElement, true);
        } else if (document.attachEvent) {
            document.attachEvent("onfocusin", getFocusedElement);
        }
    };

    GingerLibLiveSpy.StartKeyEventListener = function () {
        if (document.addEventListener) {
            document.addEventListener("keydown", getRemoteClick);
        } else if (document.attachEvent) {
            document.attachEvent("onkeydown", getRemoteClick);
        }
    };

    //------------------------------------------------------------------------------------------------------------------
    // Element From Point (Mouse + Shadow DOM + Android TV fallback)
    //------------------------------------------------------------------------------------------------------------------
    GingerLibLiveSpy.ElementFromPoint = function () {

        var X = GingerLibLiveSpy.GetXPoint();
        var Y = GingerLibLiveSpy.GetYPoint();

        // ✅ Android TV & focus-only fallback
        if ((!X || !Y || X === "0" || Y === "0") && CurrentFocusedElement) {
            return CurrentFocusedElement;
        }

        var depth = 0;
        var resultElementFromPoint = null;

        function getCurrentDepth(element) {
            var d = 0;
            while (element && element.parentNode) {
                element = element.parentNode;
                d++;
            }
            return d;
        }

        for (var i = 0; i < allShadowRoots.length; i++) {
            var shadowRoot = allShadowRoots[i];
            var element = shadowRoot.elementFromPoint(X, Y);
            if (!element) continue;

            var currentDepth = getCurrentDepth(element);
            if (depth < currentDepth) {
                depth = currentDepth;
                resultElementFromPoint = element;
            }
        }

        if (!resultElementFromPoint) {
            return document.elementFromPoint(X, Y);
        }

        return resultElementFromPoint;
    };

    //------------------------------------------------------------------------------------------------------------------
    // Deepest Element From Point
    //------------------------------------------------------------------------------------------------------------------
    GingerLibLiveSpy.DeepestElementFromPoint = function () {

        var x = GingerLibLiveSpy.GetXPoint();
        var y = GingerLibLiveSpy.GetYPoint();

        var element = document.elementFromPoint(x, y) || CurrentFocusedElement;
        if (!element) return null;

        function getDeepestElement(currElement, x, y) {

            var children = currElement.querySelectorAll('*');

            if (children.length === 0 && currElement.shadowRoot) {
                children = currElement.shadowRoot.querySelectorAll('*');
            }

            for (var i = 0; i < children.length; i++) {
                var child = children[i];
                var bounds = child.getBoundingClientRect();

                if (x >= bounds.left && x <= bounds.right &&
                    y >= bounds.top && y <= bounds.bottom) {
                    return getDeepestElement(child, x, y);
                }
            }
            return currElement;
        }

        return getDeepestElement(element, x, y);
    };

    //------------------------------------------------------------------------------------------------------------------
    // Check LiveSpy Presence
    //------------------------------------------------------------------------------------------------------------------
    GingerLibLiveSpy.IsLiveSpyExist = function () {
        return "yes";
    };

    try {
    if (typeof define_GingerLibLiveSpy === 'function') define_GingerLibLiveSpy();
    window._GingerLiveSpy = window._GingerLiveSpy || GingerLibLiveSpy || {};

    (function () {
        function ensureOverlay() {
            var id = 'ginger-live-spy-overlay';
            var el = document.getElementById(id);
            if (!el) {
                el = document.createElement('div');
                el.id = id;
                el.style.position = 'fixed';
                el.style.pointerEvents = 'none';
                el.style.zIndex = 2147483646;
                el.style.border = '3px solid rgba(255,0,0,0.95)';
                el.style.background = 'rgba(255,0,0,0.08)';
                el.style.borderRadius = '4px';
                document.body.appendChild(el);
            }
            return el;
        }

        function clearOverlay() {
            var el = document.getElementById('ginger-live-spy-overlay');
            if (el && el.parentNode) {
                el.parentNode.removeChild(el);
            }
        }

        window._GingerLiveSpy.highlightElement = function (el) {
            try {
                if (!el) return "no-element";
                var rect = el.getBoundingClientRect();
                var overlay = ensureOverlay();
                overlay.style.left = rect.left + 'px';
                overlay.style.top = rect.top + 'px';
                overlay.style.width = rect.width + 'px';
                overlay.style.height = rect.height + 'px';
                overlay.style.display = 'block';
                return "ok";
            } catch (e) {
                return "error";
            }
        };

        window._GingerLiveSpy.highlightRect = function (x, y, w, h) {
            try {
                if (isNaN(x) || isNaN(y) || isNaN(w) || isNaN(h)) return "invalid";
                var overlay = ensureOverlay();
                overlay.style.left = (x) + 'px';
                overlay.style.top = (y) + 'px';
                overlay.style.width = (w) + 'px';
                overlay.style.height = (h) + 'px';
                overlay.style.display = 'block';
                return "ok";
            } catch (e) {
                return "error";
            }
        };

        window._GingerLiveSpy.clearHighlight = function () {
            try {
                clearOverlay();
                return "ok";
            } catch (e) {
                return "error";
            }
        };
    })();
} catch (e) { /* tolerate errors */ }
}
