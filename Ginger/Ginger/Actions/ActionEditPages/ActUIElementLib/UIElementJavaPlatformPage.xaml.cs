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

using System;
using System.Windows.Controls;
using Amdocs.Ginger.Common.UIElement;
using GingerCore.Actions.Common;

namespace Ginger.Actions._Common.ActUIElementLib
{
    /// <summary>
    /// Interaction logic for ActJavaEditPage.xaml
    /// </summary>
    public partial class UIElementJavaPlatformPage : Page
    {
        ActUIElement mAction;
        public UIElementJavaPlatformPage(ActUIElement Action)
        {
            InitializeComponent();            

            mAction = Action;

            //if widgets then no need to show and bind WaitforIdle field
            if (Convert.ToBoolean(mAction.GetInputParamValue(ActUIElement.Fields.IsWidgetsElement)))
            {
                lblWaitforIdleComboBox.Visibility = System.Windows.Visibility.Collapsed;
                WaitforIdleComboBox.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }
            if (mAction.ElementType.ToString() == eElementType.Table.ToString())
            {
                if (mAction.GetInputParamValue(ActUIElement.Fields.ControlAction) == ActUIElement.eElementAction.Click.ToString())
                {
                    mAction.GetOrCreateInputParam(ActUIElement.Fields.WaitforIdle, ActUIElement.eWaitForIdle.Medium.ToString());
                }
            }
            else
            {
                if (mAction.ElementAction.ToString() == ActUIElement.eElementAction.Click.ToString())
                {
                    mAction.GetOrCreateInputParam(ActUIElement.Fields.WaitforIdle, ActUIElement.eWaitForIdle.Medium.ToString());
                }
                else
                    mAction.GetOrCreateInputParam(ActUIElement.Fields.WaitforIdle, ActUIElement.eWaitForIdle.None.ToString());
            }
            WaitforIdleComboBox.Init(mAction.GetOrCreateInputParam(ActUIElement.Fields.WaitforIdle), typeof(ActUIElement.eWaitForIdle));

        }       
    }
}
