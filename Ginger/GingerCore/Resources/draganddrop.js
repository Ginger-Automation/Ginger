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

function createEvent(typeOfEvent) {
    var event = document.createEvent("CustomEvent");
    event.initCustomEvent(typeOfEvent, true, true, null);
    event.dataTransfer = {
        data: {},
        setData: function (key, value) {
            this.data[key] = value;
        },
        getData: function (key) {
            return this.data[key];
        }
    };
    return event;
}
function dispatchEvent(element, event, transferData) {
    if (transferData !== undefined) {
        event.dataTransfer = transferData;
    }
    if (element.dispatchEvent) {
        element.dispatchEvent(event);
    } else if (element.fireEvent) {
        element.fireEvent("on" + event.type, event);
    }
}
function simulateHTML5DragAndDrop(element, target) {
    var dragStartEvent = createEvent('dragstart');
    dispatchEvent(element, dragStartEvent);
    var dropEvent = createEvent('drop');
    dispatchEvent(target, dropEvent, dragStartEvent.dataTransfer);
    var dragEndEvent = createEvent('dragend');
    dispatchEvent(element, dragEndEvent, dropEvent.dataTransfer);
}