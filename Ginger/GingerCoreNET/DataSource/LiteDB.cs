using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace GingerCoreNET.DataSource
{
    public class MyKeyValueDataTable
    {
        public int Id { get; set; }
        public string GINGER_KEY_NAME { get; set; }
        public string GINGER_KEY_VALUE { get; set; }
        public DateTime GINGER_LAST_UPDATE_DATETIME { get; set; }
        public DateTime GINGER_LAST_UPDATED_BY { get; set; }
        
    }
    class LiteDB
    { 
        void LiteDatabase()
        {
            try
            {
                using (var db = new LiteDatabase(@"C:/MyLiteData.db"))
                {
                    var Employees = db.GetCollection<MyKeyValueDataTable>("MyKeyValueDataTable");
                    
                }
            }
            catch (Exception ex)
            {

            }
        }

    }
}




