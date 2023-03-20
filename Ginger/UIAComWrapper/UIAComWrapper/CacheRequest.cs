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

using System.Collections;
using System.Diagnostics;
using UIAComWrapperInternal;

namespace System.Windows.Automation
{
    public sealed class CacheRequest
    {
        
        private UIAutomationClient.IUIAutomationCacheRequest _obj;
        private object _lock;
        private int _cRef;
        [ThreadStatic]
        private static Stack _cacheStack;
        internal static readonly CacheRequest DefaultCacheRequest = new CacheRequest();

        
        internal CacheRequest(UIAutomationClient.IUIAutomationCacheRequest obj)
        {
            Debug.Assert(obj != null);
            this._obj = obj;
            this._lock = new object();
        }

        public CacheRequest() 
        {
            this._obj = AutomationExtended.Factory.CreateCacheRequest();
            this._lock = new object();
        }

        public IDisposable Activate()
        {
            this.Push();
            return new CacheRequestActivation(this);
        }

        public void Add(AutomationPatternExtended pattern)
        {
            Utility.ValidateArgumentNonNull(pattern, "pattern");
            lock (this._lock)
            {
                this.CheckAccess();
                this._obj.AddPattern(pattern.Id);
            }
        }

        public void Add(AutomationPropertyExtended property)
        {
            Utility.ValidateArgumentNonNull(property, "property");
            lock (this._lock)
            {
                this.CheckAccess();
                this._obj.AddProperty(property.Id);
            }
        }

        private void CheckAccess()
        {
            if ((this._cRef != 0) || (this == DefaultCacheRequest))
            {
                throw new InvalidOperationException("Can't modify an active cache request");
            }
        }

        public CacheRequest Clone()
        {
            return new CacheRequest(this._obj.Clone());
        }

        public void Pop()
        {
            if (((_cacheStack == null) || (_cacheStack.Count == 0)) || (_cacheStack.Peek() != this))
            {
                throw new InvalidOperationException("Only the top cache request can be popped");
            }
            _cacheStack.Pop();
            lock (this._lock)
            {
                this._cRef--;
            }
        }

        public void Push()
        {
            if (_cacheStack == null)
            {
                _cacheStack = new Stack();
            }
            _cacheStack.Push(this);
            lock (this._lock)
            {
                this._cRef++;
            }
        }

        
        public AutomationElementMode AutomationElementMode
        {
            get
            {
                return (AutomationElementMode)this._obj.AutomationElementMode;
            }
            set
            {
                lock (this._lock)
                {
                    this.CheckAccess();
                    this._obj.AutomationElementMode = (UIAutomationClient.AutomationElementMode)value;
                }
            }
        }

        public static CacheRequest Current
        {
            get
            {
                if ((_cacheStack != null) && (_cacheStack.Count != 0))
                {
                    return (CacheRequest)_cacheStack.Peek();
                }
                return DefaultCacheRequest;
            }
        }

        internal static UIAutomationClient.IUIAutomationCacheRequest CurrentNativeCacheRequest
        {
            get
            {
                return CacheRequest.Current.NativeCacheRequest;
            }
        }

        public ConditionExtended TreeFilter
        {
            get
            {
                return ConditionExtended.Wrap(this._obj.TreeFilter);
            }
            set
            {
                Utility.ValidateArgumentNonNull(value, "TreeFilter");
                lock (this._lock)
                {
                    this.CheckAccess();
                    this._obj.TreeFilter = value.NativeCondition;
                }
            }
        }

        public TreeScopeExtended TreeScope
        {
            get
            {
                return (TreeScopeExtended)this._obj.TreeScope;
            }
            set
            {
                lock (this._lock)
                {
                    this.CheckAccess();
                    this._obj.TreeScope = (UIAutomationClient.TreeScope)value;
                }
            }
        }

        internal UIAutomationClient.IUIAutomationCacheRequest NativeCacheRequest
        {
            get
            {
                return this._obj;
            }
        }
    }

    internal class CacheRequestActivation : IDisposable
    {
        
        private CacheRequest _request;

        
        internal CacheRequestActivation(CacheRequest request)
        {
            this._request = request;
        }

        public void Dispose()
        {
            if (this._request != null)
            {
                this._request.Pop();
                this._request = null;
            }
        }
    }
}
