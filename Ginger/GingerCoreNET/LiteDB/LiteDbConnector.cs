using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Amdocs.Ginger.CoreNET.LiteDB
{
    public class LiteDbConnector
    {
        public string ConnectionString { get; set; }
        public LiteDbConnector(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        public LiteCollection<T> GetCollection<T, K>(string collectionName, Dictionary<string, Func<T, K>> refDict = null)
        {
            LiteCollection<T> collection = null;
            if (refDict != null)
            {
                var mapper = BsonMapper.Global;
                foreach (var item in refDict)
                    mapper.Entity<T>()
                    .DbRef(x => item.Value, item.Key);
            }
            using (var db = new LiteDatabase(this.ConnectionString))
            {
                collection = db.GetCollection<T>(collectionName);
            }
            return collection;
        }

        public void SetCollection<T>(LiteCollection<T> baseColl, List<T> updateData)
        {
            using (var db = new LiteDatabase(this.ConnectionString))
            {
                baseColl.Upsert(updateData);
            }
        }

        public void DeleteCollection<T>(LiteCollection<T> baseColl, Query query)
        {
            using (var db = new LiteDatabase(this.ConnectionString))
            {
                baseColl.Delete(query);
            }
        }

        public void SaveImage(string imagePath, string imageName)
        {
            using (var db = new LiteDatabase(this.ConnectionString))
            {
                db.FileStorage.Upload(imageName, imagePath);
            }
        }

        public void GetImage(string imagePath)
        {
            using (var db = new LiteDatabase(this.ConnectionString))
            {
                using (FileStream fs = File.Create(this.ConnectionString))
                {
                    var file = db.FileStorage.Download(imagePath, fs);
                }
            }
        }
    }

}
