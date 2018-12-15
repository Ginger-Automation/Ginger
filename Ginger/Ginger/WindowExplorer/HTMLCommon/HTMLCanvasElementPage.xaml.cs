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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Drivers;
using GingerCore.Drivers.Common;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.WindowExplorer.HTMLCommon
{
    /// <summary>
    /// Interaction logic for HTMLCanvasElementPage.xaml
    /// </summary>
    public partial class HTMLCanvasElementPage : Page
    {
        ElementInfo ElementInfo;
        ObservableList<Act> actList;    
        
        public HTMLCanvasElementPage(ObservableList<Act> list, ElementInfo elementInfo)
        {
            InitializeComponent();
            ElementInfo = elementInfo;
            InjectScriptAndStartEvent();
            actList = list;
        }

        private void InjectScriptAndStartEvent()
        {
            ((SeleniumDriver)ElementInfo.WindowExplorer).InjectGingerLiveSpyAndStartClickEvent(ElementInfo);
        }

        private void RetreiveElementLocationButton_Click(object sender, RoutedEventArgs e)
        {
            string x = string.Empty;
            string y = string.Empty;
            try
            {
                //TODO: Make Generic support not only for web selenium
                string xAndY = ((SeleniumDriver)ElementInfo.WindowExplorer).GetXAndYpointsfromClickEvent(ElementInfo);
                string[] spliter = new string[] { "," };
                string[] cordinations = xAndY.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
                x = cordinations[0];
                y = cordinations[1]; 
            }
            catch
            {
                ((SeleniumDriver)ElementInfo.WindowExplorer).InjectGingerLiveSpyAndStartClickEvent(ElementInfo);
                Reporter.ToUser(eUserMsgKeys.ClickElementAgain);
            }

            if (x == "undefined" || x == "undefined")
            {
                ((SeleniumDriver)ElementInfo.WindowExplorer).StartClickEvent(ElementInfo);
                Reporter.ToUser(eUserMsgKeys.ClickElementAgain);
            }
            else
            {
                XOffset.Text = x;
                YOffset.Text = y;                              
                Amdocs.Ginger.Common.GeneralLib.General.AddOrUpdateInputParamValue("XCoordinate", x, ElementInfo.actInputValue);
                Amdocs.Ginger.Common.GeneralLib.General.AddOrUpdateInputParamValue("YCoordinate", y, ElementInfo.actInputValue);
            }
            //TODO list actions
        }
    }
}
