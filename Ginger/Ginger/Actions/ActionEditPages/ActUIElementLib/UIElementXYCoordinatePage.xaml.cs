#region License
/*
Copyright © 2014-2024 European Support Limited

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
using GingerCore.Actions.Common;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions._Common.ActUIElementLib
{
    /// <summary>
    /// Interaction logic for UIElementXYCoordinatePage.xaml
    /// </summary>
    public partial class UIElementXYCoordinatePage : Page
    {
        public ActUIElement mAct;
        public UIElementXYCoordinatePage(ActUIElement Act)
        {
            mAct = Act;
            InitializeComponent();
            xXCoordinate.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActUIElement.Fields.XCoordinate, mAct.GetInputParamValue(ActUIElement.Fields.XCoordinate)));
            xYCoordinate.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActUIElement.Fields.YCoordinate, mAct.GetInputParamValue(ActUIElement.Fields.YCoordinate)));
            xValue.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActUIElement.Fields.Value, mAct.GetInputParamValue(ActUIElement.Fields.Value)));

            if (mAct.ElementData != null)
            {
                string[] spliter = new string[] { "," };
                string[] coordinates = Convert.ToString(mAct.ElementData).Split(spliter, StringSplitOptions.RemoveEmptyEntries);
                mAct.AddOrUpdateInputParamValue("XCoordinate", coordinates[0]);
                mAct.AddOrUpdateInputParamValue("YCoordinate", coordinates[1]);
            }

            if (mAct.ElementAction == ActUIElement.eElementAction.SendKeysXY)
            {
                xValuePanel.Visibility = Visibility.Visible;
            }
            else
            {
                xValuePanel.Visibility = Visibility.Collapsed;
            }
        }
    }
}
