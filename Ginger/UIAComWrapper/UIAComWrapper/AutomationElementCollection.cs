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

using System.Diagnostics;
using System.Collections;
using UIAComWrapperInternal;

namespace System.Windows.Automation
{
    public class AutomationElementCollectionExtended : ICollection, IEnumerable
    {
        private UIAutomationClient.IUIAutomationElementArray _obj;

        internal AutomationElementCollectionExtended(UIAutomationClient.IUIAutomationElementArray obj)
        {
            Debug.Assert(obj != null);
            this._obj = obj;
        }

        internal static AutomationElementCollectionExtended Wrap(UIAutomationClient.IUIAutomationElementArray obj)
        {
            return (obj == null) ? null : new AutomationElementCollectionExtended(obj);
        }

        public virtual void CopyTo(Array array, int index)
        {
            int cElem = this._obj.Length;
            for (int i = 0; i < cElem; ++i)
            {
                array.SetValue(this[i], i + index);
            }
        }

        public void CopyTo(AutomationElement_Extend[] array, int index)
        {
            int cElem = this._obj.Length;
            for (int i = 0; i < cElem; ++i)
            {
                array.SetValue(this[i], i + index);
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new AutomationElementCollectionEnumerator(this._obj);
        }

        public int Count
        {
            get
            {
                return this._obj.Length;
            }
        }

        public virtual bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public AutomationElement_Extend this[int index]
        {
            get
            {
                return AutomationElement_Extend.Wrap(this._obj.GetElement(index));
            }
        }

        public virtual object SyncRoot
        {
            get
            {
                return this;
            }
        }
    }

    public class AutomationElementCollectionEnumerator : IEnumerator
    {
        private UIAutomationClient.IUIAutomationElementArray _obj;
        private int _index;
        private int _cElem;

        #region IEnumerator Members

        internal AutomationElementCollectionEnumerator(UIAutomationClient.IUIAutomationElementArray obj)
        {
            Debug.Assert(obj != null);
            this._obj = obj;
            this._cElem = obj.Length;
        }

        public object Current
        {
            get
            {
                return AutomationElement_Extend.Wrap(this._obj.GetElement(this._index));
            }
        }

        public bool MoveNext()
        {
            if (this._index < (this._cElem - 1))
            {
                ++this._index;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Reset()
        {
            this._index = 0;
        }

        #endregion
    }
}
