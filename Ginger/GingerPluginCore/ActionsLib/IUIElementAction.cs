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

    public enum eElementType
    {
        TextBox,
        ComboBox,
        Button
        //TDDO: add all the rest
        // do not put grid
    }

    public enum eLocateBy
    {
        Id,
        Name,
        XPath,
        Text
        //TDDO: add all the rest
    }

    public enum eElementAction
    {
        Click,
        SetValue,
        GetValue
        //TDDO: add all the rest
    }

    [GingerInterface("IUIElementAction", "UI Element Action")]
    public interface IUIElementAction 
    {
        void UIElementAction(GingerAction gingerAction, eElementType elementType, eLocateBy locateBy, string locateValue, eElementAction elementAction, string value = null);
    }
}
