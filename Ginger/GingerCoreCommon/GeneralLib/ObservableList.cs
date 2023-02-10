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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Xml;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Repository;


namespace Amdocs.Ginger.Common
{

    public enum Direction
    {
        Ascending, Descending
    }

    public class ObservableList<T> : ObservableCollection<T>, IObservableList
    {

        private List<T> mUndoData = null;
        private bool mDoingUndo = false;

        public new event PropertyChangedEventHandler PropertyChanged;
        public bool RaiseOnCollectionChanged = true;

        private object mCurrentItem;

        public ObservableList()
            : base()
        {
        }
        public ObservableList(IEnumerable<T> collection)
            : base(collection)
        {
        }
        public ObservableList(IList<T> list)
            : base(list)
        {
        }


        void IObservableList.Move(int oldIndex, int newIndex)
        {
            //SaveUndoData();
            if (newIndex >= 0 && newIndex < this.Count)
                if (oldIndex >= 0 && oldIndex < this.Count)
                    Move(oldIndex, newIndex);

        }


        // Override the base in case of Lazy load
        new public T this[int Index]
        {
            get { return Items[Index]; }
            set { Items[Index] = value; }
        }


        public object CurrentItem
        {
            get { return mCurrentItem; }
            set
            {
                if (mCurrentItem != value)
                {
                    mCurrentItem = value;
                    OnPropertyChanged("CurrentItem");
                }
            }
        }



        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }


        /// <summary>
        /// Moving to next item in the list
        /// It is not possible to pass after the last item
        /// </summary>
        /// <returns>True if move was successful, False if no change or we are at the last item</returns>
        public bool MoveNext()
        {

            int index = Items.Select((x, i) => object.Equals(x, mCurrentItem) ? i + 1 : -1).Where(x => x != -1).FirstOrDefault();
            if (index > 0 && index <= Count)
            {
                // Make sure last item remains active, do NOT put code to return null or alike, as all visible list need to have item mark in grid or it doesn't look good.
                // Check if it is possible to move next need to be in the calling function, can use the list check IsLastItem()
                CurrentItem = Items[index];
                return true;
            }
            else
            {
                throw new Exception("Current item not found in list for Move Next");
            }
        }



        public bool MoveNext(object Current)
        {
            int index = Items.Select((x, i) => object.Equals(x, Current) ? i + 1 : -1).Where(x => x != -1).FirstOrDefault();
            if (index > 0 && index <= Count)
            {
                // Make sure last item remains active, do NOT put code to return null or alike, as all visible list need to have item mark in grid or it doesn't look good.
                // Check if it is possible to move next need to be in the calling function, can use the list check IsLastItem()
                CurrentItem = Items[index];
                return true;
            }
            else
            {
                throw new Exception("Current item not found in list for Move Next");

            }
        }


        public bool MovePrev()
        {

            int index = Items.Select((x, i) => object.Equals(x, mCurrentItem) ? i - 1 : -1).Where(x => x != -1).FirstOrDefault();
            if (index >= 0)
            {
                CurrentItem = Items[index];
                return true;
            }
            else
            {
                throw new Exception("Current item not found in list for Move Prev");
            }
        }


        public bool IsLastItem()
        {
            return mCurrentItem.Equals(Items.LastOrDefault());
        }



        public new int Count
        {
            get
            {
                if (LazyLoad)
                {
                    LoadLazyInfo();
                }

                return base.Count;
            }
        }



        void IObservableList.SaveUndoData()
        {
            if (mDoingUndo) return;

            mUndoData = new List<T>();
            foreach (T item in this.Items)
            {
                mUndoData.Add(item);
            }
        }

        void IObservableList.Undo()
        {
            if (mUndoData != null)
            {
                mDoingUndo = true;
                this.Items.Clear();
                foreach (T item in mUndoData)
                {
                    this.Items.Add(item);
                }
                mDoingUndo = false;

                //Refresh the list on UI
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }


        /// <summary>
        /// Return the list as IQueryable sorted based on orderByProperty
        /// </summary>
        /// <param name="orderByProperty">property name, example: nameof(BusinessFlow.Name)</param>
        /// <param name="desc">default is ascending, set to true for descending order</param>
        /// <returns></returns>
        public IQueryable<T> OrderBy(string orderByProperty, bool desc = false)
        {
            // We create a lambda expression based on the requested property
            IEnumerable<T> items = new List<T>();
            IQueryable<T> source = Items.AsQueryable();
            string command = desc ? "OrderByDescending" : "OrderBy";
            var type = typeof(T);
            var property = type.GetProperty(orderByProperty);
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);
            var resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { type, property.PropertyType }, source.Expression, Expression.Quote(orderByExpression));
            return source.Provider.CreateQuery<T>(resultExpression);
        }



        //Todo created wrapper bc .Clear() is protected and using just .Clear() will not always send property change notification 
        public void ClearAll()
        {
            this.ClearItems();

            //TODO: clear MS if lazy load 
        }

        public void Append(ObservableList<T> ItemsToadd)
        {
            foreach(T item  in ItemsToadd)
            {
                this.Items.Add(item);
            }

        }

        public IEnumerable<T> ItemsAsEnumerable()
        {
            return Items.AsEnumerable<T>();
        }
                
        string mFilterStringData = null;
        
       

        protected new IList<T> Items
        {
            get
            {
                if (LazyLoad)
                {
                    LoadLazyInfo();
                }
                return base.Items;
            }
        }

        public LazyLoadListDetails LazyLoadDetails { get; set; }

        bool IObservableList.LazyLoad 
        { 
            get { return LazyLoad; } 
        }
        public bool LazyLoad 
        { 
            get 
            { 
                if (LazyLoadDetails == null)
                {
                    return false;
                }
                else
                {
                    return (!LazyLoadDetails.DataWasLoaded);
                }
            } 
        }

        bool mAvoidLazyLoad = false;
        public bool AvoidLazyLoad { get { return mAvoidLazyLoad; } set { mAvoidLazyLoad = value; } }

        public new IEnumerator<T> GetEnumerator()
        {
            if (LazyLoad)
            {
                LoadLazyInfo();
            }
            return base.GetEnumerator();
        }

        bool loadingata = false;
        public void LoadLazyInfo()
        {
            if (!LazyLoad) return;
            if (loadingata) // //since several functions can call in parallel we might enter when status is already loadingdata, so we wait for it to complete, then return
            {
                int count = 0;
                while (loadingata && count < 1000)  // Max 10 seconds
                {
                    Thread.Sleep(10);
                    count++;
                }
                return;
            }

            //since several function can try to get data we need to lock and verify before we convert 
            lock (this)
            {
                loadingata = true;
                // We need 2nd check as it might changed after lock released
                if (!LazyLoad) return;   //since several functions can try to get data we need to lock and verify before we convert 

                try
                {
                    NewRepositorySerializer.DeserializeObservableListFromText(this, LazyLoadDetails.DataAsString);
                }
                catch (Exception ex)
                {                    
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to Deserialize the lazy load list '{0}' from file '{1}'", LazyLoadDetails.Config.ListName, LazyLoadDetails.XmlFilePath), ex);
                }

                LazyLoadDetails.DataAsString = null;
                LazyLoadDetails.DataWasLoaded = true;
                loadingata = false;
                mAvoidLazyLoad = true;
            }
        }

        public override string ToString()
        {
            if (LazyLoad)
            {
                return "Items=? Lazy Load=true, items will be populated on demand";
            }
            else
            {
                return base.ToString();
            }
        }

        public void RemoveItem(T obj)
        {
            base.Remove(obj);
        }

        public List<object> ListItems
        {
            get
            {
                return Items.Cast<object>().ToList();
            }
        }

        public bool SyncCurrentItemWithViewSelectedItem { get; set; } = true;
        public bool SyncViewSelectedItemWithCurrentItem { get; set; } = true;
        public string FilterStringData
        {
            get
            {
                return mFilterStringData;
            }
            set
            {
                if (mFilterStringData != value)
                {
                    mFilterStringData = value;
                    OnPropertyChanged("FilterStringData");
                }
            }
        }

        

        public ObservableList<NewType> ListItemsCast<NewType>()
        {
            ObservableList<NewType> list = new ObservableList<NewType>();
            var v = Items.Cast<NewType>().ToList();
            foreach (NewType item in v)
            {
                list.Add(item);
            }
            return list;
        }

        public void AddToFirstIndex(T obj)
        {
            // TODO: why not to use Insert ?
            Add(obj);
            Move(Count - 1, 0);
        }

        /// <summary>
        /// Load the ObservableList data from provided List
        /// </summary>
        /// <param name="List">List with same data type</param>
        /// <param name="clearList">Set as 'True' in case the observablelist data should be cleared first</param>
        public void FromList(List<T> List, bool clearList= false)
        {
            if (clearList)
            {
                this.Clear();
            }

            foreach (T o in List)
            {
                this.Add(o);
            }
        }

        /// <summary>
        /// Return List with data of ObservableList
        /// </summary>
        /// <returns></returns>
        public List<T> ToList()
        {
            List<T> list = new List<T>();
            foreach (T o in this)
            {
                list.Add(o);
            }
            return list;
        }
    }
}
