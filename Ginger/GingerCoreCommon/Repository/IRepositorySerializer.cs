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

using Amdocs.Ginger.Common;
using System;

namespace Amdocs.Ginger.Repository
{
    public interface IRepositorySerializer
    {
        void DeserializeObservableListFromText<T>(ObservableList<T> observableList, string s);
        RepositoryItemBase DeserializeFromFile(Type type, string fileName);
        RepositoryItemBase DeserializeFromFile(string fileName);
        object DeserializeFromFileObj(Type type, string fileName);

        void SaveToFile(RepositoryItemBase ri, string FileName);
        string FileExt(RepositoryItemBase repositoryItemBase);

        string SerializeToString(RepositoryItemBase RI);

        object DeserializeFromText(Type t, string s, string filePath="");
      
    }
}
