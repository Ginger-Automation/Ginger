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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using GingerCore.Actions.Common;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions._Common.ActUIElementLib
{
    /// <summary>
    /// Interaction logic for UIElementMouseClickAndValidate.xaml
    /// </summary>
    public partial class UIElementSelectAndValidate : Page
    {
        public ActUIElement mAct;
        PlatformInfoBase mPlatform;

        public UIElementSelectAndValidate(ActUIElement Act, PlatformInfoBase Platform)
        {
            mAct = Act;
            mPlatform = Platform;
            InitializeComponent();

            //TODO: Binding of all UI elements            
            Value.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActUIElement.Fields.Value), true, false, UCValueExpression.eBrowserType.Folder);
            HandleElementType.BindControl(mAct, ActUIElement.Fields.HandleElementType, Platform.GetPlatformUIElementsType());
            HandleLocateByComboBox.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.HandleElementLocateBy), Platform.GetPlatformUIElementLocatorsList(), false, null);
            HandleLocatorValue.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActUIElement.Fields.HandleElementLocatorValue), true, false, UCValueExpression.eBrowserType.Folder);

            SubElement.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.SubElementType), Platform.GetSubElementType(mAct.ElementType).ToList(), false, null);
            SubElementLocateBy.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.SubElementLocateBy), Platform.GetPlatformUIElementLocatorsList(), false, null);
            SubElementLocatorValue.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActUIElement.Fields.SubElementLocatorValue), true, false, UCValueExpression.eBrowserType.Folder);
            GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(DefineHandleAction, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(ActUIElement.Fields.DefineHandleAction, "False"));
        }

        public Page GetPlatformEditPage()
        {
            string pageName = mPlatform.GetPlatformGenericElementEditControls();
            if (!String.IsNullOrEmpty(pageName))
            {
                string classname = "Ginger.Actions." + pageName;
                Type t = Assembly.GetExecutingAssembly().GetType(classname);
                if (t == null)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Action edit page not found - " + classname);
                    return null;
                }
                Page platformPage = (Page)Activator.CreateInstance(t, mAct);

                if (platformPage != null)
                {
                    return platformPage;
                }
            }
            return null;
        }

        private ePlatformType GetActionPlatform()
        {
            string targetapp = (Context.GetAsContext(mAct.Context)).BusinessFlow.CurrentActivity.TargetApplication;
            ePlatformType platform = WorkSpace.Instance.Solution.ApplicationPlatforms.FirstOrDefault(x => x.AppName == targetapp).Platform;
            return platform;
        }

        private void DefineHandleAction_Checked(object sender, RoutedEventArgs e)
        {
            HandleActionPanel.Visibility = Visibility.Visible;
        }

        private void DefineHandleAction_Unchecked(object sender, RoutedEventArgs e)
        {
            HandleActionPanel.Visibility = Visibility.Collapsed;
        }

        private void HandleElement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HandleActionType.Items.Clear();
            HandleLocatorValue.IsEnabled = true;

            List<ActUIElement.eElementAction> list = mPlatform.GetPlatformUIElementActionsList((eElementType)HandleElementType.SelectedValue);
            mAct.HandleElementType = (eElementType)HandleElementType.SelectedValue;
            HandleActionType.BindControl(mAct, ActUIElement.Fields.HandleActionType, list);
        }
    }
}
