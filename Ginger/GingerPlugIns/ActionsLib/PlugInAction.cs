#region License
/*
Copyright © 2014-2018 European Support Limited

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


namespace GingerPlugIns.ActionsLib
{
    public class PlugInAction
    {
        /// <summary>
        /// The name of the Action on UI
        /// </summary>
        public string Description { get; set; }

        public string EditPage { get; set; }

        public string ID { get; set; }  // Uniqe ID of the action

        /// <summary>
        /// More descriptive information on the action
        /// </summary>
        public string UserDescription { get; set; }

        /// <summary>
        /// Use Case decription for the user to know when to use this action
        /// </summary>
        public string UserRecommendedUseCase { get; set; }
    }
}
