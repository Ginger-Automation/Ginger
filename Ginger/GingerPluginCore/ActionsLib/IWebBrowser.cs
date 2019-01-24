//#region License
///*
//Copyright © 2014-2018 European Support Limited

//Licensed under the Apache License, Version 2.0 (the "License")
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License. 
//*/
//#endregion


namespace Amdocs.Ginger.Plugin.Core
{
    [GingerInterface("WebBrowser", "Web Browser Automation")]
    public interface IWebBrowser 
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gingerAction"></param>
        /// <param name="URL"></param>
        /// Output: 
        /// Param | Value
        /// URL   | http://www.zzz
        [GingerAction("Navigate", "Navigate to URL")]
        void Navigate(IGingerAction gingerAction, string URL);


        /// <summary>
        /// Define what is the output expected
        /// gingerAction.AddOutput("submit","aaa")
        /// </summary>
        /// <param name="gingerAction"></param>
        [GingerAction("Submit", "Submit the page")]
        void Submit(IGingerAction gingerAction);
    }
}
