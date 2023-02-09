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
using System.Data;
using System.Runtime.Serialization;

namespace GingerCoreNET.DataSource
{
    /// <summary>
    ///     Represents one table of in-memory data.
    /// </summary>
    [Serializable]
    public class LiteDataTable : DataTable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:LiteDBViewer.LiteDataTable" /> class with no arguments.
        /// </summary>
        public LiteDataTable()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:LiteDBViewer.LiteDataTable" /> class with the specified table name.
        /// </summary>
        /// <param name="tableName">
        ///     The name to give the table. If <paramref name="tableName" /> is null or an empty string, a
        ///     default name is given when added to the <see cref="T:System.Data.DataTableCollection" />.
        /// </param>
        public LiteDataTable(string tableName)
            : base(tableName)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:LiteDBViewer.LiteDataTable" /> class using the specified table name
        ///     and namespace.
        /// </summary>
        /// <param name="tableName">
        ///     The name to give the table. If <paramref name="tableName" /> is null or an empty string, a
        ///     default name is given when added to the <see cref="T:System.Data.DataTableCollection" />.
        /// </param>
        /// <param name="tableNamespace">The namespace for the XML representation of the data stored in the DataTable. </param>
        public LiteDataTable(string tableName, string tableNamespace)
            : base(tableName, tableNamespace)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:LiteDBViewer.LiteDataTable" /> class with the
        ///     <see cref="T:System.Runtime.Serialization.SerializationInfo" /> and the
        ///     <see cref="T:System.Runtime.Serialization.StreamingContext" />.
        /// </summary>
        /// <param name="info">The data needed to serialize or deserialize an object.</param>
        /// <param name="context">The source and destination of a given serialized stream. </param>
        protected LiteDataTable(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        ///     Gets the row type.
        /// </summary>
        /// <returns>
        ///     Returns the type of the <see cref="T:LiteDBViewer.LiteDataRow" />.
        /// </returns>
        protected override Type GetRowType()
        {
            return typeof (LiteDataRow);
        }

        /// <summary>
        ///     Creates a new row from an existing row.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Data.DataRow" /> derived class.
        /// </returns>
        /// <param name="builder">A <see cref="T:System.Data.DataRowBuilder" /> object. </param>
        protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
        {
            return new LiteDataRow(builder);
        }
    }
}