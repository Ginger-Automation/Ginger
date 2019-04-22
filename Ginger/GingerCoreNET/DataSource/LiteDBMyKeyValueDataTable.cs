using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.DataSource
{
    public class LiteDBMyKeyValueDataTable
    {
        [BsonId]
        public int GINGER_ID { get; set; }
        public string GINGER_KEY_NAME { get; set; }
        public string GINGER_KEY_VALUE { get; set; }
        public DateTime GINGER_LAST_UPDATE_DATETIME { get; set; }
        public string GINGER_LAST_UPDATED_BY { get; set; }

        public override string ToString()
        {
            return string.Format("GINGER_ID={0}  GINGER_LAST_UPDATE_DATETIME = {1}  GINGER_LAST_UPDATED_BY ={2}  GINGER_KEY_NAME= {3}, GINGER_KEY_VALUE={4}", this.GINGER_ID, this.GINGER_LAST_UPDATE_DATETIME, this.GINGER_LAST_UPDATED_BY, this.GINGER_KEY_NAME, this.GINGER_KEY_VALUE);
        }
    }
}
