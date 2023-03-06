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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Amdocs.Ginger.Repository
{
    public class RepositoryCache
    {
        // We use weak ref so as long as someone keep ref to this obj we will keep it in cache and return the same
        // So as soon as obj is not ref GC will collect, cache will auto clean

        // for parallel we need to use Concurrent
        ConcurrentDictionary<string, WeakReference> mItems = new ConcurrentDictionary<string, WeakReference>();
        

        public Type RepositoryItemType { get; set; }

        public RepositoryCache(Type RepositoryItemType)
        {
            this.RepositoryItemType = RepositoryItemType;
        }
        
        public object this[string key]
        {
            get
            {
                WeakReference WR;                
                if (mItems.TryGetValue(key, out WR))
                {
                    if (WR.IsAlive) return WR.Target;
                    mItems.TryRemove(key, out WR);
                }
                return null;
            }
            set
            {                
                mItems[key] = new WeakReference(value);                
            }
        }

        public void Clear()
        {
            mItems.Clear();
        }

        public T GetItemByGuid<T>(Guid Guid)
        {
            T o = (T)(from x in mItems.Values where x.Target != null && ((RepositoryItemBase)x.Target).Guid.Equals(Guid) select x.Target).FirstOrDefault();            
            return o;
        }

        public void DeleteItem(string Key)
        {
            WeakReference wr;
            bool b = mItems.TryRemove(Key, out wr);
            if (b)
            {
                wr.Target = null;      // Is needed?                
            }
            else
            {
                throw new Exception("Error in delete RI - not found in cache, key=" + Key);
            }

        }


        /// <summary>
        ///  return list of all items in the cache
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> Items<T>()
        {
            var v = from x in mItems.Values select (T)x.Target;
            return v;
        }

        //public IEnumerable<object> Items()
        //{
        //    var v = from x in mItems.Values select x.Target;
        //    return v;
        //}

    }
}
