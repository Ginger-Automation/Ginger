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

namespace GingerCore.NoSqlBase
{
    public abstract class NoSqlBase
    {
        public Environments.Database Db = null;

        public enum eNoSqlOperations
        {
            freesql = 1,
        }

        public GingerCore.Actions.ActDBValidation.eDBValidationType Action { get; set; }

        public abstract List<string> GetTableList(string Keyspace);
        public abstract List<string> GetKeyspaceList();
        public abstract List<string> GetColumnList(string table);
        
        public abstract void PerformDBAction();

        public abstract bool Connect();
        public abstract bool MakeSureConnectionIsOpen();

    }
}
