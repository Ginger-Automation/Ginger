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
"use strict";

var args = arguments;
var source = args[0],
    target = args[1],
    offsetX = args[2] || 0,
    offsetY = args[3] || 0;
var sourceDoc = source.ownerDocument,
    docView = sourceDoc.defaultView,
    sourceBox = source.getBoundingClientRect(),
    targetBox = target ? target.getBoundingClientRect() : sourceBox,
    x1 = sourceBox.left + (sourceBox.width >> 1),
    y1 = sourceBox.top + (sourceBox.height >> 1),
    x2 = targetBox.left + (targetBox.width >> 1) + offsetX,
    y2 = targetBox.top + (targetBox.height >> 1) + offsetY,
    data = Object.create(Object.prototype, {
        _items: {
            value: {}
        },
        effectAllowed: {
            value: "all",
            writable: !0
        },
        dropEffect: {
            value: "move",
            writable: !0
        },
        files: {
            get: function get() {
                return this._items.Files;
            }
        },
        types: {
            get: function get() {
                return Object.keys(this._items);
            }
        },
        setData: {
            value: function value(e, t) {
                this._items[e] = t;
            }
        },
        getData: {
            value: function value(e) {
                return this._items[e];
            }
        },
        clearData: {
            value: function value(e) {
                delete this._items[e];
            }
        },
        setDragImage: {
            value: function value(e) { }
        }
    });

function emit(element, type, delay, callback) {
    var event = sourceDoc.createEvent("DragEvent");
    event.initMouseEvent(type, !0, !0, docView, 0, 0, 0, x1, y1, !1, !1, !1, !1, 0, null), Object.defineProperty(event, "dataTransfer", {
        get: function get() {
            return data;
        }
    }), element.dispatchEvent(event), docView.setTimeout(callback, delay);
}

targetBox = target.getBoundingClientRect(), emit(source, "dragstart", 101, function () {
    var i = target.getBoundingClientRect();
    x1 = i.left + x2 - targetBox.left, y1 = i.top + y2 - targetBox.top, emit(target, "dragenter", 1, function () {
        emit(target, "dragover", 101, function () {
            target = sourceDoc.elementFromPoint(x1, y1), emit(target, "drop", 1, function () {
                emit(source, "dragend", 1, callback);
            });
        });
    });
});