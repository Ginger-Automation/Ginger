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

using Amdocs.Ginger.Plugin.Core.Reporter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core.Database
{
    public interface IDatabase
    {
        string ConnectionString { get; set; }

        void InitReporter(IReporter reporter);
        bool TestConnection();

        string Name { get; }
        Boolean OpenConnection(Dictionary<string, string> parameters);
        void CloseConnection();
        List<string> GetTablesList(string Name= null);// Keyspace - Cassandra ??
        List<string> GetTablesColumns(string table);
        string RunUpdateCommand(string updateCmd, bool commit = true);
        string GetSingleValue(string Table, string Column, string Where);
        DataTable DBQuery(string Query); //  int? timeout = null : TODO // Return Data table 
        int GetRecordCount(string Query);        
    }
}
