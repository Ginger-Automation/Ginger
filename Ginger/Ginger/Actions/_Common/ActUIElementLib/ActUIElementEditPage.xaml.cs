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

using Amdocs.Ginger.Common.UIElement;
using GingerCore.Actions.Common;
using GingerCore.Helpers;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ginger.Actions._Common.ActUIElementLib
{
    /// <summary>
    /// Interaction logic for ActUIElementEditPage.xaml
    /// </summary>
    public partial class ActUIElementEditPage : Page
    {
        ActUIElement mAction;
        PlatformInfoBase mPlatform;

        public ActUIElementEditPage(ActUIElement act)
        {
            InitializeComponent();
            mAction = act;
            ePlatformType ActivityPlatform = GetActionPlatform();
            mPlatform = PlatformInfoBase.GetPlatformImpl(ActivityPlatform);

            ElementTypeComboBox.BindControl(mAction, ActUIElement.Fields.ElementType, mPlatform.GetPlatformUIElementsType());
            if ((act.ElementType == eElementType.Unknown) && (act.ElementAction == ActUIElement.eElementAction.Unknown))
            {
                ElementLocateByComboBox.SelectedValue = Enum.GetName(typeof(eLocateBy), eLocateBy.POMElement);
            }

            ElementLocateByComboBox.BindControl(mAction, ActUIElement.Fields.ElementLocateBy, mPlatform.GetPlatformUIElementLocatorsList());
            SetLocateValueFrame();

            ShowPlatformSpecificPage();
            ShowControlSpecificPage();          

            ElementLocateByComboBox.SelectionChanged += ElementLocateByComboBox_SelectionChanged;
        }

        private ePlatformType GetActionPlatform()
        {
            string targetapp = App.BusinessFlow.CurrentActivity.TargetApplication;
            ePlatformType platform = (from x in App.UserProfile.Solution.ApplicationPlatforms where x.AppName == targetapp select x.Platform).FirstOrDefault();
            return platform;
        }

        private void ResetActUIFields()
        {
            mAction.AddOrUpdateInputParamValue(ActUIElement.Fields.XCoordinate, String.Empty);
            mAction.AddOrUpdateInputParamValue(ActUIElement.Fields.Value, String.Empty);
            mAction.AddOrUpdateInputParamValue(ActUIElement.Fields.ValueToSelect, String.Empty);
            mAction.AddOrUpdateInputParamValue(ActUIElement.Fields.YCoordinate, String.Empty);
            mAction.AddOrUpdateInputParamValue(ActUIElement.Fields.ControlAction, String.Empty);
        }

        private void ElementLocateByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //clear locateValue
            mAction.LocateValue = string.Empty;
            mAction.LocateValueCalculated = string.Empty;
            mAction.ElementLocateValue = string.Empty;

            SetLocateValueFrame();
        }

        private void SetLocateValueFrame()
        {
            LocateValueEditFrame.Content = null;
            if (ElementLocateByComboBox.SelectedItem == null)
            {
                return;
            }
            eLocateBy SelectedLocType = (eLocateBy)((GingerCore.General.ComboEnumItem)ElementLocateByComboBox.SelectedItem).Value;
            Page p = GetLocateValueEditPage(SelectedLocType);
            LocateValueEditFrame.Content = p;
            UpdateActionInfo(mAction.ElementAction);
            if (SelectedLocType != eLocateBy.POMElement)
            {
                ElementTypeComboBox.IsEnabled = true;
            }
        }

        private void ElementTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ElementLocateByComboBox.IsEnabled = true;
            if (!String.IsNullOrEmpty(ElementTypeComboBox.SelectionBoxItem.ToString()))
            {
                if (ElementTypeComboBox.SelectedValue != null && !String.IsNullOrEmpty(ElementTypeComboBox.SelectedValue.ToString()))
                {
                    if (ElementTypeComboBox.SelectedValue.ToString() != ElementTypeComboBox.SelectionBoxItem.ToString())
                    {
                        ResetActUIFields();
                    }
                }
            }
            List<ActUIElement.eElementAction> list = mPlatform.GetPlatformUIElementActionsList(mAction.ElementType);
            ElementTypeImage.Source = GetImageSource(mAction.Image);
            ElementActionComboBox.BindControlWithGrouping(mAction, ActUIElement.Fields.ElementAction, list);
            UpdateActionInfo(mAction.ElementAction);
            UIElementActionEditPageFrame.Visibility = Visibility.Collapsed;
            if (mAction.ElementType != eElementType.Unknown && mAction.ElementAction != ActUIElement.eElementAction.Unknown)
            {
                ShowControlSpecificPage();
            }
            ElementTypeComboBox.Refresh();
        }

        private void ElementActionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!String.IsNullOrEmpty(ElementActionComboBox.SelectionBoxItem.ToString()))
            {
                if (ElementActionComboBox.SelectedValue != null && !String.IsNullOrEmpty(ElementActionComboBox.SelectedValue.ToString()))
                {
                    if (ElementActionComboBox.SelectedValue.ToString() != ElementActionComboBox.SelectionBoxItem.ToString())
                    {
                        ResetActUIFields();
                    }
                }
                else
                    // will reset the action combobox to unknown whenever element type is changes
                    ElementActionComboBox.SelectedValue = ActUIElement.eElementAction.Unknown;
            }
            if (ElementActionComboBox.SelectedValue == null)
            {
                return;
            }
            else
            {
                UpdateActionInfo(mAction.ElementAction);
                ShowControlSpecificPage();
            }
        }

        public Page GetPlatformEditPage()
        {
            string pageName = mPlatform.GetPlatformGenericElementEditControls();
            if (!String.IsNullOrEmpty(pageName))
            {
                string classname = "Ginger.Actions._Common.ActUIElementLib." + pageName;
                Type t = Assembly.GetExecutingAssembly().GetType(classname);
                if (t == null)
                {
                    throw new Exception("Action edit page not found - " + classname);
                }
                Page platformPage = (Page)Activator.CreateInstance(t, mAction);

                if (platformPage != null)
                {
                    return platformPage;
                }
            }          
            return null;
        }

        void ShowPlatformSpecificPage()
        {
            Page platformEditPage = GetPlatformEditPage();
            if (platformEditPage == null)
            {
                PlatformSpecificFrame.Content = null;
                PlatformSpecificFrame.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                PlatformSpecificFrame.Content = platformEditPage;
                PlatformSpecificFrame.Visibility = System.Windows.Visibility.Visible;
            }
        }

        void ShowControlSpecificPage()
        {
            if ((mAction.ElementAction == ActUIElement.eElementAction.TableCellAction) || (mAction.ElementAction == ActUIElement.eElementAction.TableRowAction) 
                || (mAction.ElementAction == ActUIElement.eElementAction.TableAction))
            {              
                if (mAction.ElementType == eElementType.Table)
                {
                    UIElementActionEditPageFrame.Content = new UIElementTableConfigPage(mAction, mPlatform);
                    UIElementActionEditPageFrame.Visibility = System.Windows.Visibility.Visible;
                }
                else
                    UIElementActionEditPageFrame.Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (mAction.ElementAction == ActUIElement.eElementAction.ClickAndValidate)
            {
                //TODO insert implementation for UIMouseClickAndValidate
                UIElementActionEditPageFrame.Content = new UIElementClickAndValidateEditPage(mAction, mPlatform);
                UIElementActionEditPageFrame.Visibility = System.Windows.Visibility.Visible;
            }
            else if (mAction.ElementAction == ActUIElement.eElementAction.SendKeysAndValidate)
            {
                //TODO insert implementation for UIMouseClickAndValidate
                UIElementActionEditPageFrame.Content = new UIElementSendKeysAndValidate(mAction, mPlatform);
                UIElementActionEditPageFrame.Visibility = System.Windows.Visibility.Visible;
            }
            else if ((mAction.ElementAction == ActUIElement.eElementAction.JEditorPaneElementAction))// ||
               // (mAction.ElementAction == ActUIElement.eElementAction.JEditorPaneElementAction))
            {
                UIElementActionEditPageFrame.Content = new UIElementTableConfigPage(mAction, mPlatform);//UIElementJEditorPanePage(mAction, mPlatform);
                UIElementActionEditPageFrame.Visibility = System.Windows.Visibility.Visible;
            }
            else if(mAction.ElementAction==ActUIElement.eElementAction.DragDrop)
            {
                UIElementActionEditPageFrame.Content = new UIElementDragAndDropEditPage(mAction, mPlatform);
                UIElementActionEditPageFrame.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                List<ElementConfigControl> configControlsList = GetRequiredConfigControls();
                Page elementEditPage = null;
                if (configControlsList.Count != 0)
                {
                    elementEditPage = GetConfigPage(configControlsList);
                }
                if (elementEditPage == null)
                {
                    UIElementActionEditPageFrame.Content = null;
                    UIElementActionEditPageFrame.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    UIElementActionEditPageFrame.Content = elementEditPage;
                    UIElementActionEditPageFrame.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        public Page GetConfigPage(List<ElementConfigControl> configControlsList)
        {
            StackPanel dynamicPanel = new StackPanel { Orientation = Orientation.Horizontal };

            UserControlsLib.UCComboBox comboBox;
            Label elementLabel;
            Page dynamicPage = new Page();
            foreach (ElementConfigControl element in configControlsList)
            {

                if (element.ControlType == eElementType.ComboBox)
                {
                    elementLabel = new Label()
                    {
                        Content = element.Title,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        FontSize = 14
                    };
                    comboBox = new UserControlsLib.UCComboBox()
                    {
                        Name = element.Title,
                        Width = 590,
                        Margin = new Thickness(99, 20, 20, 10)
                    };

                    comboBox.Init(mAction.GetOrCreateInputParam(element.BindedString), isVENeeded: true);
                    ((Ginger.UserControlsLib.UCComboBox)comboBox).ComboBox.ItemsSource = element.PossibleValues;

                    dynamicPanel.Children.Add(elementLabel);
                    dynamicPanel.Children.Add(comboBox);
                }
                else if (element.ControlType == eElementType.TextBox)
                {
                    elementLabel = new Label()
                    {
                        Content = element.Title,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        FontSize=14
                    };
                    Ginger.Actions.UCValueExpression txtBox = new Ginger.Actions.UCValueExpression()
                    {
                        Name = element.Title.ToString().Replace(" ", ""),
                        Width = 590,
                        Margin = new Thickness(99, 20, 20, 10)
                    };

                    txtBox.Init(mAction.GetOrCreateInputParam(element.BindedString), isVENeeded: true);
                    ((Ginger.Actions.UCValueExpression)txtBox).ValueTextBox.Text = element.PossibleValues.ElementAt(0);
                    dynamicPanel.Children.Add(elementLabel);
                    dynamicPanel.Children.Add(txtBox);
                }
            }
            dynamicPage.Content = dynamicPanel;
            return dynamicPage;
        }
       
        public List<ElementConfigControl> GetRequiredConfigControls()
        {
            List<ElementConfigControl> elementList = new List<ElementConfigControl>();

            if (new ActUIElement.eElementAction[] {     ActUIElement.eElementAction.SetValue, ActUIElement.eElementAction.SendKeys, ActUIElement.eElementAction.SetDate,
                                                        ActUIElement.eElementAction.SendKeyPressRelease, ActUIElement.eElementAction.SetText,
                                                        ActUIElement.eElementAction.SetSelectedValueByIndex, ActUIElement.eElementAction.Select, ActUIElement.eElementAction.SelectByText,
                                                        ActUIElement.eElementAction.RunJavaScript}.Contains(mAction.ElementAction))
            {
                //if (mAction.ElementType == eElementType.TextBox || mAction.ElementType == eElementType.ComboBox || mAction.ElementType == eElementType.CheckBox ||
                //    mAction.ElementType == eElementType.Unknown)
                //{
                    elementList.Add(new ElementConfigControl()
                    {
                        Title = "Value", 
                        BindedString = ActUIElement.Fields.Value,
                        ControlType = eElementType.TextBox,
                        PossibleValues = String.IsNullOrEmpty(mAction.GetInputParamValue(ActUIElement.Fields.Value)) ? new List<string>() { "" } :
                        mAction.GetInputParamValue(ActUIElement.Fields.Value).Split(',').ToList()
                    });
                //}
            }
            else if ((mAction.ElementAction == ActUIElement.eElementAction.Select))
            {
                if (mAction.ElementType != eElementType.RadioButton)
                {
                    elementList.Add(new ElementConfigControl()
                    {
                        Title = "Value",
                        BindedString = ActUIElement.Fields.ValueToSelect,
                        ControlType = eElementType.ComboBox,
                        PossibleValues = String.IsNullOrEmpty(mAction.GetInputParamValue(ActUIElement.Fields.ValueToSelect)) ? new List<string>() { "" } :
                        mAction.GetInputParamValue(ActUIElement.Fields.ValueToSelect).Split(',').ToList()
                    });
                }
            }
            else if ((mAction.ElementAction == ActUIElement.eElementAction.SelectByIndex))
            {
                if (mAction.ElementType != eElementType.RadioButton)
                {
                    elementList.Add(new ElementConfigControl()
                    {
                        Title = "Value",
                        BindedString = ActUIElement.Fields.ValueToSelect,
                        ControlType = eElementType.ComboBox,
                        PossibleValues = String.IsNullOrEmpty(mAction.GetInputParamValue(ActUIElement.Fields.ValueToSelect)) ? new List<string>() { "0", "1", "2", "3" } :
                        mAction.GetInputParamValue(ActUIElement.Fields.ValueToSelect).Split(',').ToList()
                    });
                }
            }
            else if ((mAction.ElementAction == ActUIElement.eElementAction.Click))
            {
                if (mAction.ElementType == eElementType.MenuItem)
                {
                    elementList.Add(new ElementConfigControl()
                    {
                        Title = "Value",
                        BindedString = ActUIElement.Fields.ValueToSelect,
                        ControlType = eElementType.TextBox,
                        PossibleValues = String.IsNullOrEmpty(mAction.GetInputParamValue(ActUIElement.Fields.Value)) ? new List<string>() { "" } :
                        mAction.GetInputParamValue(ActUIElement.Fields.Value).Split(',').ToList()
                    });
                }
            }
            else if ((mAction.ElementAction == ActUIElement.eElementAction.DoubleClick) || (mAction.ElementAction == ActUIElement.eElementAction.WinClick) || (mAction.ElementAction == ActUIElement.eElementAction.MouseClick) ||
                (mAction.ElementAction == ActUIElement.eElementAction.MousePressRelease))
            {
                if (mAction.ElementType == eElementType.RadioButton || mAction.ElementType == eElementType.CheckBox ||
                mAction.ElementType == eElementType.ComboBox || mAction.ElementType == eElementType.Button)
                {
                    elementList.Add(new ElementConfigControl()
                    {
                        Title = "XCoordinate",
                        BindedString = ActUIElement.Fields.XCoordinate,
                        ControlType = eElementType.TextBox,
                        PossibleValues = String.IsNullOrEmpty(mAction.GetInputParamValue(ActUIElement.Fields.XCoordinate)) ? new List<string>() { "0" } :
                        mAction.GetInputParamValue(ActUIElement.Fields.XCoordinate).Split(',').ToList()
                    });
                    elementList.Add(new ElementConfigControl()
                    {
                        Title = "YCoordinate",
                        BindedString = ActUIElement.Fields.YCoordinate,
                        ControlType = eElementType.TextBox,
                        PossibleValues = String.IsNullOrEmpty(mAction.GetInputParamValue(ActUIElement.Fields.YCoordinate)) ? new List<string>() { "0" } :
                        mAction.GetInputParamValue(ActUIElement.Fields.YCoordinate).Split(',').ToList()
                    });
                }
            }
            else if ((mAction.ElementAction == ActUIElement.eElementAction.GetControlProperty))
            {
                //TODO: find a better way to bind list of enum with possible values.
                List<ActUIElement.eElementProperty> propertyList = mPlatform.GetPlatformElementProperties();
                List<string> propertyListString = new List<string>();
                foreach(ActUIElement.eElementProperty property in propertyList)
                {
                    propertyListString.Add(property.ToString());
                }

                elementList.Add(new ElementConfigControl()
                {
                    Title = "Name",
                    BindedString = ActUIElement.Fields.ValueToSelect,
                    ControlType = eElementType.ComboBox,
                    PossibleValues = propertyListString
                });                
            }
            return elementList;
        }

        ImageSource GetImageSource(System.Drawing.Image image)
        {
            if (image != null)
            {
                MemoryStream ms = new MemoryStream();
                image.Save(ms, image.RawFormat);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();
                return bi;
            }
            return null;
        }      

        private Page GetLocateValueEditPage(eLocateBy SelectedLocType)
        {
            switch (SelectedLocType)
            {
                case eLocateBy.POMElement:                 
                    ElementTypeComboBox.IsEnabled = false;
                    return new LocateByPOMElementPage(mAction);
                case eLocateBy.ByXY:                   
                    return new LocateByXYEditPage(mAction);
                default:                 
                    return new LocateValueEditPage(mAction);
            }
        }

        private void UpdateActionInfo(ActUIElement.eElementAction SelectedAction)
        {
            // TODO - Add case for KeyboardChange event for LocateValue
            // TODO - Add KeyboardChangeEventHandler for LocateValueEditPage

            ActionInfoLabel.Text = string.Empty;
            TextBlockHelper text = new TextBlockHelper(ActionInfoLabel);
            
            ActionInfoLabel.Visibility = Visibility.Visible;
            if (mAction.ElementType.ToString() != null && mAction.ElementType.ToString() != "" && mAction.ElementType != eElementType.Unknown)
            {
                text.AddBoldText("Select " + mAction.ElementType.ToString() + " ");
                if (mAction.ElementLocateBy.ToString() != null && mAction.ElementLocateBy.ToString() != "" && mAction.ElementLocateBy.ToString() != ActUIElement.eElementAction.Unknown.ToString())
                {
                    text.AddBoldText(mAction.ElementLocateBy.ToString().ToLower());
                }
               
                if (SelectedAction.ToString() != null && SelectedAction.ToString() != ActUIElement.eElementAction.Unknown.ToString())
                {
                    text.AddUnderLineText(" to perform " + SelectedAction.ToString() + " operation");
                }
            }
            else
            {
                if (mAction.ElementLocateBy.ToString() != null && mAction.ElementLocateBy.ToString() != "" && mAction.ElementLocateBy.ToString() != ActUIElement.eElementAction.Unknown.ToString())
                {
                    text.AddBoldText("  " + mAction.ElementLocateBy.ToString());
                }
                if (mAction.TargetLocateBy.ToString() != null && mAction.TargetLocateBy.ToString() != "" && mAction.TargetLocateBy.ToString() != ActUIElement.eElementAction.Unknown.ToString())
                {
                    text.AddBoldText("  " + mAction.TargetLocateBy.ToString());
                }
                if (mAction.TargetElementType.ToString() != null && mAction.TargetElementType.ToString() != "" && mAction.TargetElementType.ToString() != ActUIElement.eElementAction.Unknown.ToString())
                {
                    if (!string.IsNullOrEmpty(text.GetText()))
                    {
                        text.AddBoldText("  " + mAction.TargetElementType.ToString());
                    }
                    else
                        text.AddBoldText("  " + mAction.ElementType.ToString());
                }
                if (mAction.ElementType.ToString() != null && mAction.ElementType.ToString() != "" && mAction.ElementType.ToString() != ActUIElement.eElementAction.Unknown.ToString())
                {
                    if (!string.IsNullOrEmpty(text.GetText()))
                    {
                        text.AddBoldText("  " + mAction.ElementType.ToString());
                    }
                    else
                        text.AddBoldText("  " + mAction.ElementType.ToString());
                }
                if (SelectedAction.ToString() != null && SelectedAction.ToString() != "" && SelectedAction != ActUIElement.eElementAction.Unknown)
                {
                    text.AddUnderLineText("  " + SelectedAction.ToString() + " operation");
                }
            }
        }        
    }
}