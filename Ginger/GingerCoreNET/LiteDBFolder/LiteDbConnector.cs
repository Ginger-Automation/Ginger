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
            return baseColl.Find(query).ToList();
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
