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

using System.Collections.Generic;

namespace GingerCore.Environments
{
    public interface IDatabaseOperations
    {
        string ConnectionStringCalculated { get; }
        string PassCalculated { get; }
        string TNSCalculated { get; }
        string UserCalculated { get; }

        bool CheckUserCredentialsInTNS();
        void CloseConnection();
        bool Connect(bool displayErrorPopup = false);
        string CreateConnectionString();
        List<object> FreeSQL(string SQL, int? timeout = null);
        string fTableColWhere(string Table, string Column, string Where);
        string fUpdateDB(string updateCmd, bool commit);
        string GetCalculatedWithDecryptTrue(string value);
        string GetConnectionString();
        List<string> GetTablesColumns(string table);
        List<string> GetTablesList(string Keyspace = null);
        string GetRecordCount(string SQL);
        bool MakeSureConnectionIsOpen();
        void SplitUserIdPassFromTNS();
    }
}
