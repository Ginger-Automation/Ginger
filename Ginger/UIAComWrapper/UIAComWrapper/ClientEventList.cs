#region License
/*
Copyright © 2014-2023 European Support Limited

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
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Automation;

namespace UIAComWrapperInternal
{
    internal class EventListener
    {
        private int _eventId;
        private int[] _runtimeId;
        private Delegate _handler;

        public EventListener(int eventId, int[] runtimeId, Delegate handler)
        {
            Debug.Assert(handler != null);

            this._eventId = eventId;
            this._runtimeId = runtimeId;
            this._handler = handler;
        }

        public int EventId
        {
            get { return _eventId; }
        }

        public int[] RuntimeId
        {
            get { return _runtimeId; }
        }

        public Delegate Handler
        {
            get { return _handler; }
        }

        public override bool Equals(object obj)
        {
            EventListener listener = obj as EventListener;
            return (listener != null &&
                    this._eventId == listener.EventId &&
                    this._handler == listener.Handler &&
                    AutomationExtended.Compare(this._runtimeId, listener.RuntimeId));
        }

        public override int GetHashCode()
        {
            return _handler.GetHashCode();
        }
    }

    internal class FocusEventListener : EventListener, UIAutomationClient.IUIAutomationFocusChangedEventHandler
    {
        private AutomationFocusChangedEventHandler _focusHandler;

        public FocusEventListener(AutomationFocusChangedEventHandler handler) :
            base(AutomationElement_Extend.AutomationFocusChangedEvent.Id, null, handler)
        {
            Debug.Assert(handler != null);
            this._focusHandler = handler;
        }

        #region IUIAutomationFocusChangedEventHandler Members

        void UIAutomationClient.IUIAutomationFocusChangedEventHandler.HandleFocusChangedEvent(
            UIAutomationClient.IUIAutomationElement sender)
        {
            // Can't set the arguments -- they come from a WinEvent handler.
            AutomationFocusChangedEventArgsExtended args = new AutomationFocusChangedEventArgsExtended(0, 0);
            _focusHandler(AutomationElement_Extend.Wrap(sender), args);
        }

        #endregion
    }

    internal class BasicEventListener : EventListener, UIAutomationClient.IUIAutomationEventHandler
    {
        private AutomationEventHandler _basicHandler;

        public BasicEventListener(AutomationEventExtended eventKind, AutomationElement_Extend element, AutomationEventHandler handler) :
            base(eventKind.Id, element.GetRuntimeId(), handler)
        {
            Debug.Assert(handler != null);
            this._basicHandler = handler;
        }
        
        #region IUIAutomationEventHandler Members

        void  UIAutomationClient.IUIAutomationEventHandler.HandleAutomationEvent(
            UIAutomationClient.IUIAutomationElement sender, int eventId)
        {
            AutomationEventArgsExtended args;
            if (eventId != WindowPatternIdentifiers.WindowClosedEvent.Id)
            {
                args = new AutomationEventArgsExtended(AutomationEventExtended.LookupById(eventId));
            }
            else
            {
                args = new WindowClosedEventArgs((int[])sender.GetRuntimeId());
            }
            _basicHandler(AutomationElement_Extend.Wrap(sender), args);
        }

        #endregion
    }

    internal class PropertyEventListener : EventListener, UIAutomationClient.IUIAutomationPropertyChangedEventHandler
    {
        private AutomationPropertyChangedEventHandler _propChangeHandler;

        public PropertyEventListener(AutomationEventExtended eventKind, AutomationElement_Extend element, AutomationPropertyChangedEventHandler handler) :
            base(AutomationElement_Extend.AutomationPropertyChangedEvent.Id, element.GetRuntimeId(), handler)
        {
            Debug.Assert(handler != null);
            this._propChangeHandler = handler;
        }

        #region IUIAutomationPropertyChangedEventHandler Members

        void UIAutomationClient.IUIAutomationPropertyChangedEventHandler.HandlePropertyChangedEvent(
            UIAutomationClient.IUIAutomationElement sender, 
            int propertyId, 
            object newValue)
        {
            AutomationPropertyExtended property = AutomationPropertyExtended.LookupById(propertyId);
            object wrappedObj = Utility.WrapObjectAsProperty(property, newValue);
            AutomationPropertyChangedEventArgsExtended args = new AutomationPropertyChangedEventArgsExtended(
                property,
                null,
                wrappedObj);
            this._propChangeHandler(AutomationElement_Extend.Wrap(sender), args);
        }

        #endregion
    }

    internal class StructureEventListener : EventListener, UIAutomationClient.IUIAutomationStructureChangedEventHandler
    {
        private StructureChangedEventHandler _structureChangeHandler;

        public StructureEventListener(AutomationEventExtended eventKind, AutomationElement_Extend element, StructureChangedEventHandler handler) :
            base(AutomationElement_Extend.StructureChangedEvent.Id, element.GetRuntimeId(), handler)
        {
            Debug.Assert(handler != null);
            this._structureChangeHandler = handler;
        }

        #region IUIAutomationStructureChangedEventHandler Members

        void UIAutomationClient.IUIAutomationStructureChangedEventHandler.HandleStructureChangedEvent(UIAutomationClient.IUIAutomationElement sender, UIAutomationClient.StructureChangeType changeType, Array runtimeId)
        {
            StructureChangedEventArgsExtended args = new StructureChangedEventArgsExtended(
                (StructureChangeTypeExtended)changeType,
                (int[])runtimeId);
            this._structureChangeHandler(AutomationElement_Extend.Wrap(sender), args);
        }

        #endregion
    }

    internal class ClientEventList
    {
        private static readonly System.Collections.Generic.LinkedList<EventListener> _events = new LinkedList<EventListener>();

        public static void Add(EventListener listener)
        {
            lock (_events)
            {
                _events.AddLast(listener);
            }
        }

        public static EventListener Remove(AutomationEventExtended eventId, AutomationElement_Extend element, Delegate handler)
        {
            // Create a prototype to seek
            int[] runtimeId = (element == null) ? null : element.GetRuntimeId();
            EventListener prototype = new EventListener(eventId.Id, runtimeId, handler);
            lock (_events)
            {
                LinkedListNode<EventListener> node = _events.Find(prototype);
                if (node == null)
                {
                    throw new ArgumentException("event handler not found");
                }
                EventListener result = node.Value;
                _events.Remove(node);
                return result;
            }
        }

        public static void Clear()
        {
            lock (_events)
            {
                _events.Clear();
            }
        }
    }
}
