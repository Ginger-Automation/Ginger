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

using System.Windows;
using System.Windows.Controls;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using GingerCore.Actions.Common;
using GingerCore.Platforms.PlatformsInfo;

namespace Ginger.Actions._Common.ActUIElementLib
{
    /// <summary>
    /// Interaction logic for ActJavaEditPage.xaml
    /// </summary>
    public partial class UIElementDragAndDropEditPage : Page
    {
        ActUIElement mAction;

        public UIElementDragAndDropEditPage(ActUIElement Action, PlatformInfoBase mPlatform) 
        {
            mAction = Action;
            InitializeComponent();
            DragDropType.Init(mAction.GetOrCreateInputParam(ActUIElement.Fields.DragDropType), mPlatform.GetPlatformDragDropTypeList(), false, new SelectionChangedEventHandler(DragDropType_SelectionChanged));
            TargetElement.BindControl(mAction, ActUIElement.Fields.TargetLocateBy, mPlatform.GetPlatformUIElementsType());
            TargetLocateByComboBox.BindControl(mAction, ActUIElement.Fields.TargetLocateBy, mPlatform.GetPlatformUIElementLocatorsList());
            TargetLocatorValue.Init(Context.GetAsContext(mAction.Context), mAction.GetOrCreateInputParam(ActUIElement.Fields.TargetLocateValue), true, false, UCValueExpression.eBrowserType.Folder);
            SourceDragXY.Init(Context.GetAsContext(mAction.Context), mAction.GetOrCreateInputParam(ActUIElement.Fields.SourceDragXY), true, false);
            TargetDropXY.Init(Context.GetAsContext(mAction.Context), mAction.GetOrCreateInputParam(ActUIElement.Fields.TargetDropXY), true, false);
            ElementSpecificControl();
            InitTargetLocateValue();
        }

        private void DragDropType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DragDropType == null || DragDropType.ComboBoxSelectedValue == null)
                return;
            if(DragDropType.ComboBoxSelectedValue.ToString() == ActUIElement.eElementDragDropType.MouseDragDrop.ToString())
            {
                DragXY.Visibility = Visibility.Visible;
                DropXY.Visibility = Visibility.Visible;
            }
            else
            {
                DragXY.Visibility = Visibility.Collapsed;
                DropXY.Visibility = Visibility.Collapsed;
            }
        }
        private void ElementSpecificControl()
        {
            if(TargetLocateByComboBox.SelectedValue.ToString() == eLocateBy.ByXY.ToString())
            {
                TargetXYGrid.Visibility = Visibility.Visible;
                TargetLocatorValue.Visibility = Visibility.Collapsed;
            }
            else
            {
                TargetXYGrid.Visibility = Visibility.Collapsed;
                TargetLocatorValue.Visibility = Visibility.Visible;
            }
        }

        private void TargetLocateByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TargetLocateByComboBox.SelectedValue == null)
                return;
            ElementSpecificControl();
        }

        private void InitTargetLocateValue()
        {
            txtLocateValueX.Init(Context.GetAsContext(mAction.Context), mAction.GetOrCreateInputParam(ActUIElement.Fields.XCoordinate), true, false, UCValueExpression.eBrowserType.Folder);
            txtLocateValueY.Init(Context.GetAsContext(mAction.Context), mAction.GetOrCreateInputParam(ActUIElement.Fields.YCoordinate), true, false, UCValueExpression.eBrowserType.Folder);
        }
    }
}
