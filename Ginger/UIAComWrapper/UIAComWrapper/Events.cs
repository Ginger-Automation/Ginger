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

using System.Runtime.InteropServices;

namespace System.Windows.Automation
{
    public class AutomationEventArgsExtended : EventArgs
    {
        
        private AutomationEventExtended _eventId;

        
        public AutomationEventArgsExtended(AutomationEventExtended eventId)
        {
            this._eventId = eventId;
        }

        
        public AutomationEventExtended EventId
        {
            get
            {
                return this._eventId;
            }
        }
    }

    public delegate void AutomationEventHandler(object sender, AutomationEventArgsExtended e);

    public sealed class WindowClosedEventArgs : AutomationEventArgsExtended
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

    public sealed class AsyncContentLoadedEventArgs : AutomationEventArgsExtended
    {
        
        private AsyncContentLoadedState _asyncContentState;
        private double _percentComplete;

        
        public AsyncContentLoadedEventArgs(AsyncContentLoadedState asyncContentState, double percentComplete)
            : base(AutomationElementIdentifiersExtended.AsyncContentLoadedEvent)
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

    public sealed class AutomationPropertyChangedEventArgsExtended : AutomationEventArgsExtended
    {
        
        private object _newValue;
        private object _oldValue;
        private AutomationPropertyExtended _property;

        
        public AutomationPropertyChangedEventArgsExtended(AutomationPropertyExtended property, object oldValue, object newValue)
            : base(AutomationElementIdentifiersExtended.AutomationPropertyChangedEvent)
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

        public AutomationPropertyExtended Property
        {
            get
            {
                return this._property;
            }
        }
    }

    public delegate void AutomationPropertyChangedEventHandler(object sender, AutomationPropertyChangedEventArgsExtended e);

    public class AutomationFocusChangedEventArgsExtended : AutomationEventArgsExtended
    {
        
        private int _idChild;
        private int _idObject;

        
        public AutomationFocusChangedEventArgsExtended(int idObject, int idChild)
            : base(AutomationElement_Extend.AutomationFocusChangedEvent)
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

    public delegate void AutomationFocusChangedEventHandler(object sender, AutomationFocusChangedEventArgsExtended e);


    [Guid("e4cfef41-071d-472c-a65c-c14f59ea81eb"), ComVisible(true)]
    public enum StructureChangeTypeExtended
    {
        ChildAdded,
        ChildRemoved,
        ChildrenInvalidated,
        ChildrenBulkAdded,
        ChildrenBulkRemoved,
        ChildrenReordered
    }

    public sealed class StructureChangedEventArgsExtended : AutomationEventArgsExtended
    {
        
        private int[] _runtimeID;
        private StructureChangeTypeExtended _structureChangeType;

        
        public StructureChangedEventArgsExtended(StructureChangeTypeExtended structureChangeType, int[] runtimeId)
            : base(AutomationElementIdentifiersExtended.StructureChangedEvent)
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

        
        public StructureChangeTypeExtended StructureChangeType
        {
            get
            {
                return this._structureChangeType;
            }
        }
    }

    public delegate void StructureChangedEventHandler(object sender, StructureChangedEventArgsExtended e);
}
