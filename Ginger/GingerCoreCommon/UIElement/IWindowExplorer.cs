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

using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.UIElement
{
    public enum eTabView
    {
        Screenshot, TreeView, GridView, PageSource, none
    }

    public interface IWindowExplorer
    {
        List<AppWindow> GetAppWindows();
        void SwitchWindow(string Title);
        void HighLightElement(ElementInfo ElementInfo, bool locateElementByItLocators = false, IList<ElementInfo> MappedUIElements = null);
        void UnHighLightElements();
        string GetFocusedControl();
        ElementInfo GetControlFromMousePosition();
        AppWindow GetActiveWindow();
        Task<List<ElementInfo>> GetVisibleControls(PomSetting pomSetting, ObservableList<ElementInfo> foundElementsList = null, ObservableList<POMPageMetaData> PomMetaData = null, Bitmap ScreenShot = null);
        List<ElementInfo> GetElementChildren(ElementInfo ElementInfo);
        // Get All element properties to be displayed in properties 
        ObservableList<ControlProperty> GetElementProperties(ElementInfo ElementInfo);

        ObservableList<ElementLocator> GetElementLocators(ElementInfo ElementInfo, PomSetting pomSetting = null);

        ObservableList<ElementLocator> GetElementFriendlyLocators(ElementInfo ElementInfo, PomSetting pomSetting = null);

        ObservableList<OptionalValue> GetOptionalValuesList(ElementInfo ElementInfo, eLocateBy elementLocateBy, string elementLocateValue);

        // Get the data of the element
        // For Combo box: will return all valid values - options available - List<ComboBoxElementItem>
        // For Table: will return list of rows data: List<TableElementItem>        
        object GetElementData(ElementInfo ElementInfo, eLocateBy elementLocateBy, string elementLocateValue);

        bool IsRecordingSupported();

        bool IsPOMSupported();

        bool IsLiveSpySupported();

        bool IsWinowSelectionRequired();

        List<eTabView> SupportedViews();

        eTabView DefaultView();

        string SelectionWindowText();

        ObservableList<ElementInfo> GetElements(ElementLocator EL);

        void UpdateElementInfoFields(ElementInfo eI);

        bool IsElementObjectValid(object obj);

        bool TestElementLocators(ElementInfo Element, bool GetOutAfterFoundElement = false, ApplicationPOMModel mPOM = null);
        void CollectOriginalElementsDataForDeltaCheck(ObservableList<ElementInfo> originalList);

        ElementInfo GetMatchingElement(ElementInfo latestElement, ObservableList<ElementInfo> originalElements);

        void StartSpying();
        ElementInfo LearnElementInfoDetails(ElementInfo EI, PomSetting pomSetting = null);
        List<AppWindow> GetWindowAllFrames();

        string GetCurrentPageSourceString();

        Task<object> GetPageSourceDocument(bool ReloadHtmlDoc);
    }
}
