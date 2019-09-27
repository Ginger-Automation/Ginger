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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.GeneralLib;
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
using static GingerCore.Actions.Common.ActUIElement;
using static GingerCore.General;

namespace Ginger.Actions._Common.ActUIElementLib
{
    /// <summary>
    /// Interaction logic for ActUIElementEditPage.xaml
    /// </summary>
    public partial class ActUIElementEditPage : Page
    {
        ActUIElement mAction;
        PlatformInfoBase mPlatform;
        string mExistingPOMAndElementGuidString = null;

        public ActUIElementEditPage(ActUIElement act)
        {
            InitializeComponent();
            mAction = act;
            if (act.Platform == ePlatformType.NA)
            {
                act.Platform = GetActionPlatform();
            }
            mPlatform = PlatformInfoBase.GetPlatformImpl(act.Platform);
            List<eLocateBy> LocateByList = mPlatform.GetPlatformUIElementLocatorsList();
            ElementLocateByComboBox.BindControl(mAction, nameof(ActUIElement.ElementLocateBy), LocateByList);

            //if widgets element, only supported to java platform now.
            if (act.Platform.Equals(ePlatformType.Java))
            {
                ShowWidgetsElementCheckBox();
            }

            BindElementTypeComboBox();

            SetLocateValueFrame();
            ShowPlatformSpecificPage();
            ShowControlSpecificPage();
            ElementLocateByComboBox.SelectionChanged += ElementLocateByComboBox_SelectionChanged;
        }

        private void BindElementTypeComboBox()
        {
            ElementTypeComboBox.Items.Clear();
            if (Convert.ToBoolean(mAction.GetInputParamValue(Fields.IsWidgetsElement)))
            {
                ElementTypeComboBox.BindControl(mAction, nameof(ActUIElement.ElementType), mPlatform.GetPlatformWidgetsUIElementsType());
            }
            else
            {
                ElementTypeComboBox.BindControl(mAction, nameof(ActUIElement.ElementType), mPlatform.GetPlatformUIElementsType());
            }
        }

        private void ShowWidgetsElementCheckBox()
        {
            xWidgetElementCheckBox.Visibility = Visibility.Visible;
            BindingHandler.ActInputValueBinding(xWidgetElementCheckBox,CheckBox.IsCheckedProperty, mAction.GetOrCreateInputParam(Fields.IsWidgetsElement, "false"),new InputValueToBoolConverter());
        }

        private ePlatformType GetActionPlatform()
        {
            ePlatformType platform;
            if (mAction.Context != null && (Context.GetAsContext(mAction.Context)).BusinessFlow != null)
            {
                string targetapp = (Context.GetAsContext(mAction.Context)).BusinessFlow.CurrentActivity.TargetApplication;
                platform = (from x in WorkSpace.Instance.Solution.ApplicationPlatforms where x.AppName == targetapp select x.Platform).FirstOrDefault();
            }
            else
            {
                platform = WorkSpace.Instance.Solution.ApplicationPlatforms[0].Platform;
            }
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
            eLocateBy SelectedLocType = (eLocateBy)((ComboEnumItem)ElementLocateByComboBox.SelectedItem).Value;
            Page p = GetLocateValueEditPage(SelectedLocType);
            LocateValueEditFrame.Content = p;
            UpdateActionInfo(mAction.ElementAction);
            //if (SelectedLocType != eLocateBy.POMElement)
            //{
            //    ElementTypeComboBox.IsEnabled = true;
            //}
        }

        List<ActUIElement.eElementAction> mElementActionsList = new List<ActUIElement.eElementAction>();

        private void ElementTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Convert.ToBoolean(mAction.GetInputParamValue(Fields.IsWidgetsElement)))
            {
                mElementActionsList = mPlatform.GetPlatformUIElementActionsList(mAction.ElementType);
            }
            else
            {
                mElementActionsList = mPlatform.GetPlatformWidgetsUIActionsList(mAction.ElementType);
            }

            ElementActionComboBox.SelectionChanged -= ElementActionComboBox_SelectionChanged;
            ElementActionComboBox.BindControl(mAction, nameof(ActUIElement.ElementAction), mElementActionsList);
            ElementActionComboBox.SelectedValue = mAction.ElementAction;//need to fix binding to avoid it
            ElementActionComboBox.SelectionChanged += ElementActionComboBox_SelectionChanged;

            if (mElementActionsList.Count == 0)
            {
                mAction.ElementAction = eElementAction.Unknown;
            }
            else if (mAction.ElementType != eElementType.Unknown && !mElementActionsList.Contains(mAction.ElementAction))
            {
                mAction.ElementAction = mElementActionsList[0];//defualt operation for element type should be first one
            }

            xElementTypeImage.ImageType = ElementInfo.GetElementTypeImage(mAction.ElementType);
            UpdateActionInfo(mAction.ElementAction);
        }

        private void ElementActionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UIElementActionEditPageFrame.Visibility = Visibility.Collapsed;
            ResetActUIFields();
            if (ElementActionComboBox.SelectedItem == null)
            {
                return;
            }
            else
            {
                UpdateActionInfo(mAction.ElementAction);
                ShowControlSpecificPage();
            }
            mAction.OnPropertyChanged(nameof(Act.ActionType));
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
            var pageContent = GetControlSpecificPageContent();

            if (pageContent != null)
            {
                UIElementActionEditPageFrame.Content = pageContent;
                UIElementActionEditPageFrame.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private Page GetControlSpecificPageContent()
        {
            Page pageContent;
            switch (mAction.ElementAction)
            {
                case eElementAction.TableAction:
                case eElementAction.TableRowAction:
                case eElementAction.TableCellAction:
                case eElementAction.JEditorPaneElementAction:
                    pageContent = new UIElementTableConfigPage(mAction, mPlatform);
                    break;

                case eElementAction.ClickAndValidate:
                    pageContent = new UIElementClickAndValidateEditPage(mAction, mPlatform);
                    break;

                case eElementAction.SendKeysAndValidate:
                    pageContent = new UIElementSendKeysAndValidate(mAction, mPlatform);
                    break;

                case eElementAction.SelectandValidate:
                    //TODO insert implementation for UIMouseClickAndValidate
                    pageContent = new UIElementSelectAndValidate(mAction, mPlatform);
                    break;

                case eElementAction.DragDrop:
                    pageContent = new UIElementDragAndDropEditPage(mAction, mPlatform);
                    break;

                case eElementAction.DoubleClick:
                case eElementAction.WinClick:
                case eElementAction.MouseClick:
                case eElementAction.MousePressRelease:
                case eElementAction.ClickXY:
                case eElementAction.DoubleClickXY:
                case eElementAction.SendKeysXY:
                    pageContent = null;
                    if ((mAction.Platform.Equals(ePlatformType.Java) && (mAction.ElementType.Equals(eElementType.RadioButton)
                                                               || mAction.ElementType.Equals(eElementType.CheckBox))
                                                               || mAction.ElementType.Equals(eElementType.ComboBox)
                                                               || mAction.ElementType.Equals(eElementType.Button)
                                                               )
                        || (mAction.Platform.Equals(ePlatformType.Web)))
                    {
                        pageContent = new UIElementXYCoordinatePage(mAction);
                    }
                    break;

                default:
                    pageContent = null;
                    break;
            }
            if (pageContent == null)
            {
                pageContent = GetDefaultPageContent();
            }
            
            return pageContent;
        }

        private Page GetDefaultPageContent()
        {
            //configure action specific edit page
            Page elementEditPage = GetRequiredConfigPage();

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
            return elementEditPage;
        }

        private Page GetRequiredConfigPage()
        {
            List<ElementConfigControl> elementList = new List<ElementConfigControl>();
            List<string> possibleValues = new List<string>();

            switch (mAction.ElementAction)
            {
                case eElementAction.SetValue:
                case eElementAction.MultiSetValue:
                case eElementAction.SendKeys:
                case eElementAction.SetDate:
                case eElementAction.SendKeyPressRelease:
                case eElementAction.SetText:
                case eElementAction.SelectByText:
                case eElementAction.GetAttrValue:
                case eElementAction.RunJavaScript:
                    if (mAction.ElementLocateBy == eLocateBy.POMElement && (mAction.ElementAction == eElementAction.SetValue ||
                                                                        mAction.ElementAction == eElementAction.SetText ||
                                                                        mAction.ElementAction == eElementAction.SelectByText ||
                                                                        mAction.ElementAction == eElementAction.SendKeys))
                    {
                        elementList.Add(GetPomOptionalValuesComboBox(Fields.Value, ePomElementValuesType.Values));
                    }
                    else
                    {
                        possibleValues = string.IsNullOrEmpty(mAction.GetInputParamValue(Fields.Value)) ? new List<string>() { "" } :
                                    new List<string>() { mAction.GetInputParamValue(Fields.Value) };

                        elementList.Add(GetElementConfigControl("Value", Fields.Value, eElementType.TextBox, possibleValues));
                    }
                    break;

                case eElementAction.Select:
                    if (mAction.ElementType != eElementType.RadioButton)
                    {
                        if (mAction.ElementLocateBy == eLocateBy.POMElement)
                        {
                            elementList.Add(GetPomOptionalValuesComboBox(ActUIElement.Fields.ValueToSelect, ePomElementValuesType.Values));
                        }
                        else
                        {
                            possibleValues = String.IsNullOrEmpty(mAction.GetInputParamValue(ActUIElement.Fields.ValueToSelect)) ? new List<string>() { "" } :
                                            new List<string>() { mAction.GetInputParamValue(ActUIElement.Fields.ValueToSelect) };
                            elementList.Add(GetElementConfigControl("Value", Fields.ValueToSelect, eElementType.ComboBox, possibleValues));
                        }
                    }
                    break;

                //added for Widgets
                case eElementAction.TriggerJavaScriptEvent:
                    //add combobox
                    possibleValues = GetJavaScriptEventList();
                    elementList.Add(GetElementConfigControl("Event", Fields.ValueToSelect, eElementType.ComboBox, possibleValues));

                    //add checkbox
                    elementList.Add(GetElementConfigControl("Mouse Event", Fields.IsMouseEvent, eElementType.CheckBox, new List<string> { "false" }, MouseEventCheckBox_click));
                    break;

                case eElementAction.SelectByIndex:
                case eElementAction.SetSelectedValueByIndex:
                    if (mAction.ElementType != eElementType.RadioButton)
                    {
                        if (mAction.ElementLocateBy == eLocateBy.POMElement)
                        {
                            elementList.Add(GetPomOptionalValuesComboBox(ActUIElement.Fields.ValueToSelect, ePomElementValuesType.Indexs));
                        }
                        else
                        {
                            possibleValues = String.IsNullOrEmpty(mAction.GetInputParamValue(ActUIElement.Fields.ValueToSelect)) ? new List<string>() { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" } :
                                                   new List<string>() { mAction.GetInputParamValue(ActUIElement.Fields.ValueToSelect) };

                            elementList.Add(GetElementConfigControl("Value", Fields.ValueToSelect, eElementType.ComboBox, possibleValues));

                        }
                    }
                    break;

                case eElementAction.Click:
                case eElementAction.AsyncClick:
                case eElementAction.DoubleClick:
                    if (mAction.ElementType == eElementType.MenuItem || mAction.ElementType.Equals(eElementType.TreeView))
                    {
                        possibleValues = String.IsNullOrEmpty(mAction.GetInputParamValue(ActUIElement.Fields.ValueToSelect)) ? new List<string>() { "" } :
                                               new List<string>() { mAction.GetInputParamValue(ActUIElement.Fields.ValueToSelect) };

                        elementList.Add(GetElementConfigControl("Value", Fields.ValueToSelect, eElementType.TextBox, possibleValues));
                    }
                    break;

                case eElementAction.GetControlProperty:
                    //TODO: find a better way to bind list of enum with possible values.
                    List<ActUIElement.eElementProperty> propertyList = mPlatform.GetPlatformElementProperties();
                    List<string> propertyListString = new List<string>();
                    foreach (ActUIElement.eElementProperty property in propertyList)
                    {
                        propertyListString.Add(property.ToString());
                    }

                    elementList.Add(GetElementConfigControl("Name", Fields.ValueToSelect, eElementType.ComboBox, propertyListString));
                    break;

                default:
                    Reporter.ToLog(eLogLevel.DEBUG, mAction.ElementAction.ToString() + "not required config page.");
                    break;
            }


            Page elementEditPage = null;
            if (elementList.Count != 0)
            {
                elementEditPage = GetConfigPage(elementList);
            }
            return elementEditPage;
        }

        private List<string> GetJavaScriptEventList()
        {
            var eventList = new List<string>();
            
            if (Convert.ToBoolean(mAction.GetInputParamValue(Fields.IsMouseEvent)))
            {
                eventList.Add("onmousedown");
                eventList.Add("onmouseleave");
                eventList.Add("onmouseout");
                eventList.Add("onmouseover");
                eventList.Add("onmouseup");
            }
            else
            {
                eventList.Add("onkeydown");
                eventList.Add("onkeyup");
                eventList.Add("onblur");
                eventList.Add("onfocus");
                eventList.Add("onchange");
            }
 
            return eventList;
        }

        public Page GetConfigPage(List<ElementConfigControl> configControlsList)
        {
            StackPanel dynamicPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center };

            UserControlsLib.UCComboBox comboBox;
            Label elementLabel;
            Page dynamicPage = new Page();
            foreach (ElementConfigControl element in configControlsList)
            {
                if (element.ControlType == eElementType.ComboBox)
                {
                    elementLabel = CretateLabel(element);
                    comboBox = CreateComboBox(element);

                    comboBox.Init(mAction.GetOrCreateInputParam(element.BindedString), isVENeeded: true, context: Context.GetAsContext(mAction.Context));
                    ((Ginger.UserControlsLib.UCComboBox)comboBox).ComboBox.ItemsSource = element.PossibleValues;
                    if (mAction.ElementLocateBy == eLocateBy.POMElement)
                    {
                        ((Ginger.UserControlsLib.UCComboBox)comboBox).ComboBox.SelectedValue = element.DefaultValue;
                        comboBox.ComboBoxObject.Style = this.FindResource("$FlatEditInputComboBoxStyle") as Style;
                    }
                    dynamicPanel.Children.Add(elementLabel);
                    dynamicPanel.Children.Add(comboBox);
                }
                else if (element.ControlType == eElementType.TextBox)
                {
                    elementLabel = CretateLabel(element);
                    UCValueExpression txtBox = CreateTextBox(element);

                    txtBox.Init(Context.GetAsContext(mAction.Context), mAction.GetOrCreateInputParam(element.BindedString), isVENeeded: true);
                    ((Ginger.Actions.UCValueExpression)txtBox).ValueTextBox.Text = element.PossibleValues.ElementAt(0);
                    dynamicPanel.Children.Add(elementLabel);
                    dynamicPanel.Children.Add(txtBox);
                }
                else if (element.ControlType == eElementType.CheckBox)
                {
                    CheckBox dyanamicCheckBox = new CheckBox();
                    dyanamicCheckBox.Content = element.Title;
                    dyanamicCheckBox.HorizontalAlignment = HorizontalAlignment.Left;
                    dyanamicCheckBox.VerticalAlignment = VerticalAlignment.Center;
                    dyanamicCheckBox.IsChecked = false;
                    dyanamicCheckBox.Width = 100;
                    dyanamicCheckBox.Margin = new Thickness() { Left = 5};

                    if (element.ElementEvent != null)
                    {
                        dyanamicCheckBox.Click += new RoutedEventHandler(element.ElementEvent);
                    }
                    BindingHandler.ActInputValueBinding(dyanamicCheckBox, CheckBox.IsCheckedProperty, mAction.GetOrCreateInputParam(element.BindedString, "false"));
                    dynamicPanel.Children.Add(dyanamicCheckBox);
                }
            }
            dynamicPage.Content = dynamicPanel;
            return dynamicPage;
        }

        private void MouseEventCheckBox_click(object sender, EventArgs e)
        {
            mAction.AddOrUpdateInputParamValue(Fields.ValueToSelect, "");
            GetDefaultPageContent();
        }

        private static UCValueExpression CreateTextBox(ElementConfigControl element)
        {
            return new Ginger.Actions.UCValueExpression()
            {
                Name = element.Title.ToString().Replace(" ", ""),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 600,
                Margin = new Thickness(10, 0, 0, 0)
            };
        }

        private static UserControlsLib.UCComboBox CreateComboBox(ElementConfigControl element)
        {
            return new UserControlsLib.UCComboBox()
            {
                Name = element.Title,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 300,
                Margin = new Thickness(10, 0, 0, 0)
            };
        }

        private Label CretateLabel(ElementConfigControl element)
        {
            return new Label()
            {
                Style = this.FindResource("$LabelStyle") as Style,
                Content = element.Title + ":",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14
            };
        }

        private ElementConfigControl GetElementConfigControl(string title, string bindedString, eElementType elementType, List<string> possibleValue, RoutedEventHandler routedEvent=null)
        {
            return new ElementConfigControl()
            {
                Title = title,
                BindedString = bindedString,
                ControlType = elementType,
                PossibleValues = possibleValue,
                ElementEvent = routedEvent
            };
        }

        enum ePomElementValuesType { Values, Indexs }
        private ElementConfigControl GetPomOptionalValuesComboBox(string Valuefield, ePomElementValuesType valuesType)
        {
            ElementConfigControl optionalValuesCombo = new ElementConfigControl();
            optionalValuesCombo.Title = "Value";
            optionalValuesCombo.ControlType = eElementType.ComboBox;
            optionalValuesCombo.BindedString = Valuefield;
            optionalValuesCombo.PossibleValues = GetPomElementOptionalValues(valuesType);
            optionalValuesCombo.DefaultValue = !String.IsNullOrEmpty(mAction.GetInputParamValue(Valuefield)) ?
                                    mAction.GetInputParamValue(Valuefield) : GetPomElementOptionalValuesDefaultValue(valuesType);
            return optionalValuesCombo;
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
                    //ElementTypeComboBox.IsEnabled = false;
                    LocateByPOMElementPage locateByPOMElementPage = new LocateByPOMElementPage(Context.GetAsContext(mAction.Context), mAction, nameof(ActUIElement.ElementType), mAction, nameof(ActUIElement.ElementLocateValue));
                    locateByPOMElementPage.ElementChangedPageEvent -= POMElementChanged;
                    locateByPOMElementPage.ElementChangedPageEvent += POMElementChanged;
                    return locateByPOMElementPage;
                case eLocateBy.ByXY:
                    return new LocateByXYEditPage(mAction, mAction, ActUIElement.Fields.ElementLocateValue);
                default:
                    return new LocateValueEditPage(Context.GetAsContext(mAction.Context), mAction, ActUIElement.Fields.ElementLocateValue);
            }
        }



        private void POMElementChanged()
        {
            if (mExistingPOMAndElementGuidString != mAction.ElementLocateValue)
            {
                mAction.AddOrUpdateInputParamValue(ActUIElement.Fields.ValueToSelect, string.Empty);
            }
            ShowControlSpecificPage();
        }

        private void UpdateActionInfo(ActUIElement.eElementAction SelectedAction)
        {
            // TODO - Add case for KeyboardChange event for LocateValue
            // TODO - Add KeyboardChangeEventHandler for LocateValueEditPage

            xActionInfoLabel.Text = string.Empty;
            TextBlockHelper text = new TextBlockHelper(xActionInfoLabel);

            xActionInfoLabel.Visibility = Visibility.Visible;
            if (mAction.ElementType.ToString() != null && mAction.ElementType.ToString() != "" && mAction.ElementType != eElementType.Unknown)
            {
                text.AddBoldText(string.Format("Configured '{0}'", GetEnumValueDescription(typeof(eElementType), mAction.ElementType)));
                if (mAction.ElementLocateBy.ToString() != null && mAction.ElementLocateBy.ToString() != "" && mAction.ElementLocateBy.ToString() != ActUIElement.eElementAction.Unknown.ToString())
                {
                    text.AddBoldText(string.Format(" to be located by '{0}'", GetEnumValueDescription(typeof(eLocateBy), mAction.ElementLocateBy)));
                }

                if (SelectedAction.ToString() != null && SelectedAction.ToString() != ActUIElement.eElementAction.Unknown.ToString())
                {
                    text.AddBoldText(string.Format(" to perform '{0}' operation.", GetEnumValueDescription(typeof(ActUIElement.eElementAction), SelectedAction)));
                }
            }
            else
            {
                if (mAction.ElementLocateBy.ToString() != null && mAction.ElementLocateBy.ToString() != "" && mAction.ElementLocateBy.ToString() != ActUIElement.eElementAction.Unknown.ToString())
                {
                    text.AddBoldText(string.Format(" '{0}'", GetEnumValueDescription(typeof(eLocateBy), mAction.ElementLocateBy)));
                }
                if (mAction.TargetLocateBy.ToString() != null && mAction.TargetLocateBy.ToString() != "" && mAction.TargetLocateBy.ToString() != ActUIElement.eElementAction.Unknown.ToString())
                {
                    text.AddBoldText(string.Format(" '{0}'", GetEnumValueDescription(typeof(eLocateBy), mAction.TargetLocateBy)));
                }
                if (mAction.TargetElementType.ToString() != null && mAction.TargetElementType.ToString() != "" && mAction.TargetElementType.ToString() != ActUIElement.eElementAction.Unknown.ToString())
                {
                    if (!string.IsNullOrEmpty(text.GetText()))
                    {
                        text.AddBoldText(string.Format(" '{0}'", GetEnumValueDescription(typeof(eElementType), mAction.TargetElementType)));
                    }
                    else
                    {
                        text.AddBoldText(string.Format(" '{0}'", GetEnumValueDescription(typeof(eElementType), mAction.ElementType)));
                    }
                }
                if (mAction.ElementType.ToString() != null && mAction.ElementType.ToString() != "" && mAction.ElementType.ToString() != ActUIElement.eElementAction.Unknown.ToString())
                {
                    text.AddBoldText(string.Format(" '{0}'", GetEnumValueDescription(typeof(eElementType), mAction.ElementType)));
                }
                if (SelectedAction.ToString() != null && SelectedAction.ToString() != "" && SelectedAction != ActUIElement.eElementAction.Unknown)
                {
                    text.AddBoldText(string.Format(" '{0}' operation", GetEnumValueDescription(typeof(ActUIElement.eElementAction), SelectedAction)));
                }
            }
        }

        /// <summary>
        /// This method is used to get the current selected POM
        /// </summary>
        /// <returns></returns>
        private ElementInfo GetElementInfoFromCurerentPOMSelected()
        {
            ElementInfo selectedPOMElement = null;
            mExistingPOMAndElementGuidString = mAction.ElementLocateValue;
            if (mAction.ElementLocateValue != null && mAction.ElementLocateValue.Contains("_"))
            {
                string[] pOMandElementGUIDs = mAction.ElementLocateValue.Split('_');
                Guid selectedPOMGUID = new Guid(pOMandElementGUIDs[0]);
                ApplicationPOMModel currentPOM = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<ApplicationPOMModel>(selectedPOMGUID);
                if (currentPOM != null)
                {
                    Guid selectedPOMElementGUID = new Guid(pOMandElementGUIDs[1]);
                    selectedPOMElement = (ElementInfo)currentPOM.MappedUIElements.Where(z => z.Guid == selectedPOMElementGUID).FirstOrDefault();
                }
            }
            return selectedPOMElement;
        }

        /// <summary>
        /// This method is used to get the POM element Optional values
        /// </summary>
        /// <returns></returns>
        private List<string> GetPomElementOptionalValues(ePomElementValuesType valuesType)
        {
            List<string> optionalValues = new List<string>();
            ElementInfo selectedPOMElement = GetElementInfoFromCurerentPOMSelected();
            if (selectedPOMElement != null && selectedPOMElement.OptionalValuesObjectsList.Count > 0)
            {
                if (valuesType == ePomElementValuesType.Values)
                {
                    optionalValues = selectedPOMElement.OptionalValuesObjectsList.Select(s => s.Value).ToList();
                }
                else
                {
                    optionalValues = selectedPOMElement.OptionalValuesObjectsList.Select(s => selectedPOMElement.OptionalValuesObjectsList.IndexOf(s).ToString()).ToList();
                }
            }
            return optionalValues;
        }

        private string GetPomElementOptionalValuesDefaultValue(ePomElementValuesType valuesType)
        {
            string defaultValue = string.Empty;
            ElementInfo selectedPOMElement = GetElementInfoFromCurerentPOMSelected();
            if (selectedPOMElement != null && selectedPOMElement.OptionalValuesObjectsList.Count > 0)        //For new implementation
            {
                OptionalValue defValue = selectedPOMElement.OptionalValuesObjectsList.Where(s => s.IsDefault == true).FirstOrDefault();
                if (defValue != null)
                {
                    if (valuesType == ePomElementValuesType.Values)
                    {
                        defaultValue = defValue.Value;
                    }
                    else
                    {
                        defaultValue = selectedPOMElement.OptionalValuesObjectsList.IndexOf(defValue).ToString();
                    }
                }
            }
            return defaultValue;
        }

        private void XWidgetsElementType_Click(object sender, RoutedEventArgs e)
        {
            BindElementTypeComboBox();
            ShowPlatformSpecificPage();
        }
    }
}