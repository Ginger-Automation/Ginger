#region License
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
#endregion

using System.Runtime.InteropServices;

namespace System.Windows.Automation
{
    public class AutomationEventArgs : EventArgs
    {
        
        private AutomationEvent _eventId;

        
        public AutomationEventArgs(AutomationEvent eventId)
        {
            this._eventId = eventId;
        }

        
        public AutomationEvent EventId
        {
            get
            {
                return this._eventId;
            }
        }
    }

    public delegate void AutomationEventHandler(object sender, AutomationEventArgs e);

    public sealed class WindowClosedEventArgs : AutomationEventArgs
    {
        
        private int[] _runtimeId;

        
        public WindowClosedEventArgs(int[] runtimeId)
            : base(WindowPatternIdentifiers.WindowClosedEvent)
        {
            if (runtimeId == null)
            {
                throw new ArgumentNullException("runtimeId");
            }
            this._runtimeId = (int[])runtimeId.Clone();
        }

        public int[] GetRuntimeId()
        {
            return (int[])this._runtimeId.Clone();
        }
    }

    [Guid("d8e55844-7043-4edc-979d-593cc6b4775e"), ComVisible(true)]
    public enum AsyncContentLoadedState
    {
        Beginning,
        Progress,
        Completed
    }

    public sealed class AsyncContentLoadedEventArgs : AutomationEventArgs
    {
        
        private AsyncContentLoadedState _asyncContentState;
        private double _percentComplete;

        
        public AsyncContentLoadedEventArgs(AsyncContentLoadedState asyncContentState, double percentComplete)
            : base(AutomationElementIdentifiers.AsyncContentLoadedEvent)
        {
            this._asyncContentState = asyncContentState;
            this._percentComplete = percentComplete;
        }

        
        public AsyncContentLoadedState AsyncContentLoadedState
        {
            get
            {
                return this._asyncContentState;
            }
        }

        public double PercentComplete
        {
            get
            {
                return this._percentComplete;
            }
        }
    }

    public sealed class AutomationPropertyChangedEventArgs : AutomationEventArgs
    {
        
        private object _newValue;
        private object _oldValue;
        private AutomationProperty _property;

        
        public AutomationPropertyChangedEventArgs(AutomationProperty property, object oldValue, object newValue)
            : base(AutomationElementIdentifiers.AutomationPropertyChangedEvent)
        {
            this._oldValue = oldValue;
            this._newValue = newValue;
            this._property = property;
        }

        
        public object NewValue
        {
            get
            {
                return this._newValue;
            }
        }

        public object OldValue
        {
            get
            {
                return this._oldValue;
            }
        }

        public AutomationProperty Property
        {
            get
            {
                return this._property;
            }
        }
    }

    public delegate void AutomationPropertyChangedEventHandler(object sender, AutomationPropertyChangedEventArgs e);

    public class AutomationFocusChangedEventArgs : AutomationEventArgs
    {
        
        private int _idChild;
        private int _idObject;

        
        public AutomationFocusChangedEventArgs(int idObject, int idChild)
            : base(AutomationElement.AutomationFocusChangedEvent)
        {
            this._idObject = idObject;
            this._idChild = idChild;
        }

        
        public int ChildId
        {
            get
            {
                return this._idChild;
            }
        }

        public int ObjectId
        {
            get
            {
                return this._idObject;
            }
        }
    }

    public delegate void AutomationFocusChangedEventHandler(object sender, AutomationFocusChangedEventArgs e);


    [Guid("e4cfef41-071d-472c-a65c-c14f59ea81eb"), ComVisible(true)]
    public enum StructureChangeType
    {
        ChildAdded,
        ChildRemoved,
        ChildrenInvalidated,
        ChildrenBulkAdded,
        ChildrenBulkRemoved,
        ChildrenReordered
    }

    public sealed class StructureChangedEventArgs : AutomationEventArgs
    {
        
        private int[] _runtimeID;
        private StructureChangeType _structureChangeType;

        
        public StructureChangedEventArgs(StructureChangeType structureChangeType, int[] runtimeId)
            : base(AutomationElementIdentifiers.StructureChangedEvent)
        {
            if (runtimeId == null)
            {
                throw new ArgumentNullException("runtimeId");
            }
            this._structureChangeType = structureChangeType;
            this._runtimeID = (int[])runtimeId.Clone();
        }

        public int[] GetRuntimeId()
        {
            return (int[])this._runtimeID.Clone();
        }

        
        public StructureChangeType StructureChangeType
        {
            get
            {
                return this._structureChangeType;
            }
        }
    }

    public delegate void StructureChangedEventHandler(object sender, StructureChangedEventArgs e);
}
