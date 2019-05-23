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

using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

namespace Amdocs.Ginger.CoreNET.LiteDBFolder
{
    public class LiteDbConnector
    {
        public string ConnectionString { get; set; }
        public LiteDbConnector(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        public EntityBuilder<T> GetMapper<T>()
        {
            return BsonMapper.Global.Entity<T>();
        }
        public LiteCollection<T> GetCollection<T>(string collectionName)
        {
            LiteCollection<T> collection = null;
            try
            {
                using (var db = new LiteDatabase(this.ConnectionString))
                {
                    collection = db.GetCollection<T>(collectionName);
                }
            }
            catch(Exception)
            {

            }
            return collection;
        }

        public bool DeleteCollectionItems<T>(LiteCollection<T> baseColl, Query query)
        {
            bool result = false;
            try
            {
                using (var db = new LiteDatabase(this.ConnectionString))
                {
                    result = baseColl.Delete(query) > 0;
                }
            }
            catch(Exception ex)
            {

            }
            return result;
        }

        public List<T> FilterCollection<T>(LiteCollection<T> baseColl, Query query)
        {
            return baseColl.IncludeAll().Find(query).ToList();
        }

        public void SetCollection<T>(LiteCollection<T> baseColl, List<T> updateData)
        {
            try
            {
                using (var db = new LiteDatabase(this.ConnectionString))
                {
                    baseColl.Upsert(updateData);
                }
            }
            catch(Exception ex)
            {

            }
        }

        public void SaveImage(string imagePath, string imageName)
        {
            try
            {
                using (var db = new LiteDatabase(this.ConnectionString))
                {
                    db.FileStorage.Upload(imageName, imagePath);
                }
            }
            catch(Exception ex)
            {

            }
        }

        public void GetImage(string imagePath)
        {
            try
            {
                using (var db = new LiteDatabase(this.ConnectionString))
                {
                    using (FileStream fs = File.Create(this.ConnectionString))
                    {
                        var file = db.FileStorage.Download(imagePath, fs);
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }
    }
}
