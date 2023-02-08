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

namespace Amdocs.Ginger.Repository
{
    public class ApplicationDBQueryModel : RepositoryItemBase
    {
        public string Name { get; set; }    //for example get customer by ID

        public string SQL { get; set; }   // SELELCT * FROM TBCustomers WHERE CUstomerID={CustID}
        

        public List<string> InputParams;  // CustomerID (int)

        public List<string> OutPutParams;  // FirstName, LastName

        public override string ItemName { get { return Name; } set { Name = value; } }
    }
}
