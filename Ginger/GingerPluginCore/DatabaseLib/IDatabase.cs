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

namespace Amdocs.Ginger.Plugin.Core.DatabaseLib
{
    // Interface for the basic database operation

    // Mark it as plugin interface so will be written to the services json
    [GingerInterface("IDatabase", "Database Interface")]
    public interface IDatabase
    {        
        Boolean OpenConnection();
        void CloseConnection();

        object ExecuteQuery(string Query); //  int? timeout = null : TODO // Return Data table         

        void InitReporter(IReporter reporter);
    }
}
