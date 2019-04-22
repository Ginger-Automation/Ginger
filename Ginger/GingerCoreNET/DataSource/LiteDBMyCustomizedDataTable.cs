using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.DataSource
{
    class LiteDBMyCustomizedDataTable
    {
        [BsonId]
        public int GINGER_ID { get; set; }
        public DateTime GINGER_LAST_UPDATE_DATETIME { get; set; }
        public string GINGER_LAST_UPDATED_BY { get; set; }
        public bool GINGER_USED { get; set; }

        public override string ToString()
        {
            return string.Format("GINGER_ID={0}  GINGER_LAST_UPDATE_DATETIME = {1}  GINGER_LAST_UPDATED_BY ={2}  GINGER_USED= {3}", this.GINGER_ID, this.GINGER_LAST_UPDATE_DATETIME, this.GINGER_LAST_UPDATED_BY, this.GINGER_USED);
        }
    }
}
