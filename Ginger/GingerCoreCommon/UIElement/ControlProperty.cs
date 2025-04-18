#region License
/*
Copyright © 2014-2025 European Support Limited

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


using Amdocs.Ginger.Repository;


namespace Amdocs.Ginger.Common.UIElement
{
    //TODO: rename to UIElementProperty
    public class ControlProperty : RepositoryItemBase
    {
        [IsSerializedForLocalRepository]
        public string Name { get; set; }

        [IsSerializedForLocalRepository]
        public string Value { get; set; }


        private ePomElementCategory? mCategory;
        [IsSerializedForLocalRepository]
        public ePomElementCategory? Category
        {
            get { return mCategory; }
            set { if (mCategory != value) { mCategory = value; OnPropertyChanged(nameof(Category)); } }
        }

        [IsSerializedForLocalRepository(true)]
        public bool ShowOnUI { get; set; } = true;

        public override string ItemName { get { return Name; } set { Name = value; } }

    }
}
