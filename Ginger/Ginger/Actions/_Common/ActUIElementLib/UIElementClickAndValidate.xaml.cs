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
using GingerCore.Actions.Common;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;

namespace Ginger.Actions._Common.ActUIElementLib
{
    /// <summary>
    /// Interaction logic for UIElementMouseClickAndValidate.xaml
    /// </summary>
    public partial class UIElementClickAndValidateEditPage : Page
    {
        public ActUIElement mAct;        
        public enum eClickType
        {
            [EnumValueDescription("Click")]
            Click,
            [EnumValueDescription("Simple Click")]
            SimpleClick,
            [EnumValueDescription("Click At")]
            ClickAt,
            [EnumValueDescription("Mouse Click")]
            MouseClick,
            [EnumValueDescription("Async Click")]
            AsyncClick
        }

        public enum eValidationType
        {
            [EnumValueDescription("Exist")]
            Exist,
            [EnumValueDescription("Does not exist")]
            NotExist,
            [EnumValueDescription("Enabled")]
            Enabled,
            [EnumValueDescription("Visible")]
            Visible
        }

        public UIElementClickAndValidateEditPage(ActUIElement Act, PlatformInfoBase mPlatform)
        {
            mAct = Act;
            InitializeComponent();

            //TODO: Binding of all UI elements
            ClickType.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.ClickType), mPlatform.GetPlatformUIClickTypeList(), false, null);
            xValidationType.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.ValidationType), mPlatform.GetPlatformUIValidationTypesList(), false, null);
            xValidationElementTypeComboBox.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.ValidationElementType), mPlatform.GetPlatformUIElementsType(), false, null);
            xValidationElementLocateByComboBox.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.ValidationElementLocateBy), mPlatform.GetPlatformUIElementLocatorsList(), false, null);
            SetLocateValueFrame();
            //LocatorValue.Init(mAct.GetOrCreateInputParam(ActUIElement.Fields.ValidationElementLocatorValue), true, false, UCValueExpression.eBrowserType.Folder);
            GingerCore.General.ActInputValueBinding(LoopThroughClicks, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(ActUIElement.Fields.LoopThroughClicks, "False"));

            xValidationElementLocateByComboBox.ComboBox.SelectionChanged += ElementLocateByComboBox_SelectionChanged;
        }




        private void ElementLocateByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //mAction.LocateValue = string.Empty;
            //mAction.LocateValueCalculated = string.Empty;
            //mAction.ElementLocateValue = string.Empty;

            SetLocateValueFrame();
        }

        private void SetLocateValueFrame()
        {
            LocateValueEditFrame.Content = null;
            if (xValidationElementLocateByComboBox.ComboBox.SelectedItem == null)
            {
                return;
            }
            eLocateBy SelectedLocType = (eLocateBy)((GingerCore.General.ComboItem)xValidationElementLocateByComboBox.ComboBox.SelectedItem).Value;
            Page p = GetLocateValueEditPage(SelectedLocType);
            LocateValueEditFrame.Content = p;
            //UpdateActionInfo(mAction.ElementAction);
            if (SelectedLocType != eLocateBy.POMElement)
            {
                xValidationElementTypeComboBox.ComboBox.IsEnabled = true;
            }
        }

        //private void UpdateActionInfo(ActUIElement.eElementAction SelectedAction)
        //{
        //    // TODO - Add case for KeyboardChange event for LocateValue
        //    // TODO - Add KeyboardChangeEventHandler for LocateValueEditPage

        //    ActionInfoLabel.Text = string.Empty;
        //    TextBlockHelper text = new TextBlockHelper(ActionInfoLabel);

        //    ActionInfoLabel.Visibility = Visibility.Visible;
        //    if (mAction.ElementType.ToString() != null && mAction.ElementType.ToString() != "" && mAction.ElementType != eElementType.Unknown)
        //    {
        //        text.AddBoldText(string.Format("Configured '{0}'", GetEnumValueDescription(typeof(eElementType), mAction.ElementType)));
        //        if (mAction.ElementLocateBy.ToString() != null && mAction.ElementLocateBy.ToString() != "" && mAction.ElementLocateBy.ToString() != ActUIElement.eElementAction.Unknown.ToString())
        //        {
        //            text.AddBoldText(string.Format(" to be located by '{0}'", GetEnumValueDescription(typeof(eLocateBy), mAction.ElementLocateBy)));
        //        }

        //        if (SelectedAction.ToString() != null && SelectedAction.ToString() != ActUIElement.eElementAction.Unknown.ToString())
        //        {
        //            text.AddBoldText(string.Format(" to perform '{0}' operation.", GetEnumValueDescription(typeof(ActUIElement.eElementAction), SelectedAction)));
        //        }
        //    }
        //    else
        //    {
        //        if (mAction.ElementLocateBy.ToString() != null && mAction.ElementLocateBy.ToString() != "" && mAction.ElementLocateBy.ToString() != ActUIElement.eElementAction.Unknown.ToString())
        //        {
        //            text.AddBoldText(string.Format(" '{0}'", GetEnumValueDescription(typeof(eLocateBy), mAction.ElementLocateBy)));
        //        }
        //        if (mAction.TargetLocateBy.ToString() != null && mAction.TargetLocateBy.ToString() != "" && mAction.TargetLocateBy.ToString() != ActUIElement.eElementAction.Unknown.ToString())
        //        {
        //            text.AddBoldText(string.Format(" '{0}'", GetEnumValueDescription(typeof(eLocateBy), mAction.TargetLocateBy)));
        //        }
        //        if (mAction.TargetElementType.ToString() != null && mAction.TargetElementType.ToString() != "" && mAction.TargetElementType.ToString() != ActUIElement.eElementAction.Unknown.ToString())
        //        {
        //            if (!string.IsNullOrEmpty(text.GetText()))
        //            {
        //                text.AddBoldText(string.Format(" '{0}'", GetEnumValueDescription(typeof(eElementType), mAction.TargetElementType)));
        //            }
        //            else
        //            {
        //                text.AddBoldText(string.Format(" '{0}'", GetEnumValueDescription(typeof(eElementType), mAction.ElementType)));
        //            }
        //        }
        //        if (mAction.ElementType.ToString() != null && mAction.ElementType.ToString() != "" && mAction.ElementType.ToString() != ActUIElement.eElementAction.Unknown.ToString())
        //        {
        //            text.AddBoldText(string.Format(" '{0}'", GetEnumValueDescription(typeof(eElementType), mAction.ElementType)));
        //        }
        //        if (SelectedAction.ToString() != null && SelectedAction.ToString() != "" && SelectedAction != ActUIElement.eElementAction.Unknown)
        //        {
        //            text.AddBoldText(string.Format(" '{0}' operation", GetEnumValueDescription(typeof(ActUIElement.eElementAction), SelectedAction)));
        //        }
        //    }
        //}

        private Page GetLocateValueEditPage(eLocateBy SelectedLocType)
        {
            switch (SelectedLocType)
            {
                case eLocateBy.POMElement:
                    xValidationElementTypeComboBox.IsEnabled = false;
                    LocateByPOMElementPage locateByPOMElementPage = new LocateByPOMElementPage(mAct,LocateByPOMElementPage.eLocateByPOMElementPageContext.ClickAndValidatePage);
                    //locateByPOMElementPage.ElementChangedPageEvent -= POMElementChanged;
                    //locateByPOMElementPage.ElementChangedPageEvent += POMElementChanged;
                    return locateByPOMElementPage;
                case eLocateBy.ByXY:
                    return new LocateByXYEditPage(mAct);
                default:
                    return new LocateValueEditPage(mAct);
            }
        }

        //string mExistingPOMAndElementGuidString = null;

        //private void POMElementChanged()
        //{
           
        //    if (mExistingPOMAndElementGuidString != mAct.ElementLocateValue)
        //    {
        //        mAction.AddOrUpdateInputParamValue(ActUIElement.Fields.ValueToSelect, string.Empty);
        //    }
        //    ShowControlSpecificPage();
        //}



        public Page GetPlatformEditPage()
        {
            PlatformInfoBase mPlatform = null;
            if (mPlatform != null)
            {
                string pageName = mPlatform.GetPlatformGenericElementEditControls();
                if (!String.IsNullOrEmpty(pageName))
                {
                    string classname = "Ginger.Actions." + pageName;
                    Type t = Assembly.GetExecutingAssembly().GetType(classname);
                    if (t == null)
                    {
                        throw new Exception("Action edit page not found - " + classname);
                    }
                    Page platformPage = (Page)Activator.CreateInstance(t, mAct);

                    if (platformPage != null)
                    {
                        return platformPage;
                    }
                } 
            }
            return null;
        }

        private ePlatformType GetActionPlatform()
        {
            string targetapp = App.BusinessFlow.CurrentActivity.TargetApplication;
            ePlatformType platform = (from x in App.UserProfile.Solution.ApplicationPlatforms where x.AppName == targetapp select x.Platform).FirstOrDefault();
            return platform;
        }
    }
}
