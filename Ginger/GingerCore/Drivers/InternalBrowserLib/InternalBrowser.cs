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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using mshtml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows.Controls;
using System.Xml;

namespace GingerCore.Drivers.InternalBrowserLib
{
    public class InternalBrowser : DriverBase, IWindowExplorer
    {        
        private InternalBrowserWindow mFrmBrowser;
        private WebBrowser mBrowserControl;

        bool IsBrowserLoaded = false;

        public override bool IsSTAThread()
        {
            return true;
        }

        public InternalBrowser(BusinessFlow BF)
        {            
            BusinessFlow = BF;            
        }

        public override void StartDriver()
        {            
            General.CheckRegistryValues();
            CreateSTA(ShowDriverWindow);
        }

        private void ShowDriverWindow()
        {
            mFrmBrowser = new InternalBrowserWindow(BusinessFlow);
            mBrowserControl = mFrmBrowser.browser;
            mFrmBrowser.Show();
            mFrmBrowser.mBusinessFlow = BusinessFlow;
            mFrmBrowser.IBDriver = this;
            IsBrowserLoaded = true;
            OnDriverMessage(eDriverMessageType.DriverStatusChanged);
            Dispatcher = new DriverWindowDispatcher(mFrmBrowser.Dispatcher);

            System.Windows.Threading.Dispatcher.Run();
        }

        public override void CloseDriver()
        {
            try
            {
                if (mFrmBrowser != null)
                {                    
                    mFrmBrowser.Close();
                    mFrmBrowser = null;                    
                }
                mBrowserControl = null;
                BusinessFlow = null;                
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error when try to close IB Driver - " + ex.Message);
            }
            IsBrowserLoaded = false;
        }        

        public override Act GetCurrentElement()
        {
            // HtmlElement e = mBrowserControl.;
            // mBrowserControl.Document.GetElementFromPoint

            //HtmlElement e = mBrowserControl.Document.ActiveElement;
            ////TODO: add all types of elements
            //if (e.TagName == "INPUT")
            //{
            //    string etype = e.GetAttribute("type");
            //    if (etype == "text")
            //    {
            //        ActTextBox a = new ActTextBox();
            //        SetActLocator(a, e);                    
            //        a.Value = e.GetAttribute("value");
            //        a.TextBoxAction = ActTextBox.eTextBoxAction.SetValue;
            //        a.ExInfo = e.OuterHtml;
            //        return a;
            //    }
            //    if (etype == "button")
            //    {
            //        ActButton a = new ActButton();
            //        SetActLocator(a, e);                          
            //        a.ExInfo = e.OuterHtml;
            //        return a;
            //    }
            //    if (etype == "select")
            //    {
            //        //TODO:
                    
            //    }
            //}
            return null;
        }
        
        public override void RunAction(Act act)
        {
            //TODO: move to base class, and use only driver specific calls, so will not be duplicate

            string actClass = act.GetType().ToString();
            try
            {
                actClass = actClass.Replace("GingerCore.Actions.", "");
                switch (actClass)
                {
                    case "ActGotoURL":
                        ActGotoURL AGU = (ActGotoURL)act;
                        GotoURL(AGU);
                        break;
                    case "ActGenElement":
                        GenElementHandler((ActGenElement)act);
                        break;
                    case "ActSmartSync":
                        SmartSyncHandler((ActSmartSync)act);
                        break;
                    case "ActTextBox":
                        //TODO: switch case
                        ActTextBox ATB = (ActTextBox)act;
                        if (ATB.TextBoxAction == ActTextBox.eTextBoxAction.SetValue || ATB.TextBoxAction == ActTextBox.eTextBoxAction.SetValueFast) SetTextBoxValue(ATB);
                        if (ATB.TextBoxAction == ActTextBox.eTextBoxAction.GetValue) GetTextBoxValue(ATB);
                        if (ATB.TextBoxAction == ActTextBox.eTextBoxAction.IsDisabled) IsTextBoxDisabled(ATB);
                        if (ATB.TextBoxAction == ActTextBox.eTextBoxAction.GetFont) GetTextBoxFont(ATB);
                        if (ATB.TextBoxAction == ActTextBox.eTextBoxAction.IsPrepopulated) IsTextBoxPrepopulated(ATB);
                        if (ATB.TextBoxAction == ActTextBox.eTextBoxAction.IsDisplayed) IsTextBoxDisplayed(ATB);
                        if (ATB.TextBoxAction == ActTextBox.eTextBoxAction.GetInputLength) GetTextBoxInputLength(ATB);
                        break;
                    case "ActLabel":
                        ActLabel alb = (ActLabel)act;
                        if (alb.LabelAction == ActLabel.eLabelAction.IsVisible)
                            CheckLabelDisplayed(alb);
                        if (alb.LabelAction == ActLabel.eLabelAction.GetInnerText)
                            GetLabelName(alb);
                        break;
                    case "ActPassword":
                        ActPassword APwd = (ActPassword)act;
                        if (APwd.PasswordAction == ActPassword.ePasswordAction.SetValue) SetPasswordValue(APwd);
                        if (APwd.PasswordAction == ActPassword.ePasswordAction.GetSize) GetPasswordSize(APwd);
                        if (APwd.PasswordAction == ActPassword.ePasswordAction.IsDisabled) IsPasswordBoxDisabled(APwd);
                        break;
                    case "ActLink":
                        ActLink Alink = (ActLink)act;
                        if (Alink.LinkAction == ActLink.eLinkAction.GetValue)
                            GetLinkValue(Alink);
                        else if (Alink.LinkAction == ActLink.eLinkAction.Visible)
                            IsLinkVisible(Alink);
                        else
                            ClickLink(Alink);
                        break;
                    case "ActButton":
                        ActButtonHandler((ActButton)act);
                        break;
                    case "ActSubmit":
                        ActSubmit aSub = (ActSubmit)act;
                        doSubmit(aSub);
                        break;
                    case "ActRadioButton":
                        ActRadioButton rb = (ActRadioButton)act;
                        if (rb.RadioButtonAction == ActRadioButton.eActRadioButtonAction.GetValue)
                            GetRadioButtonValue(act, rb);
                        else if (rb.RadioButtonAction == ActRadioButton.eActRadioButtonAction.IsDisabled)
                            IsRadioButtonDisabled(act, rb);
                        else if (rb.RadioButtonAction == ActRadioButton.eActRadioButtonAction.GetAvailableValues)
                            GetRadioButtonAvailableValues(act, rb);

                        else
                        {
                            switch (act.LocateBy)
                            {
                                case eLocateBy.ByIndex:
                                    SelectRadioButtonByIndex(act, rb, Int32.Parse(act.GetInputParamCalculatedValue("Value")));
                                    break;
                                case eLocateBy.ByValue:
                                    SelectRadioButtonByValue(act, rb, act.GetInputParamCalculatedValue("Value"));
                                    break;
                                case eLocateBy.ByID:
                                    SelectRadioButtonByID(act, rb, act.GetInputParamCalculatedValue("Value"));
                                    break;
                                    
                                default:
                                    act.Error = act.LocateBy.ToString() + " not implemented for RadioButton yet.";
                                    break;
                            }
                        }
                        break;
                    // TODO: refactor and chaneg to ActMultiselectList -- add also Upgrade old XMLs
                    case "ActMultiselectList":
                        ActMultiselectList el = (ActMultiselectList)act;
                        string csv = act.GetInputParamCalculatedValue("Value"); string[] parts = csv.Split('|'); //TODO: make sure the values passed are separated by '|'
                        List<string> optionList = new List<string>(parts);
                        switch (el.ActMultiselectListAction)
                        {
                            case ActMultiselectList.eActMultiselectListAction.SetSelectedValueByIndex:
                                SelectMultiselectListOptionsByIndex(el, optionList.ConvertAll(s => Int32.Parse(s))); // need to convert list<string> to list<int>
                                break;
                            case ActMultiselectList.eActMultiselectListAction.SetSelectedValueByText:
                                SelectMultiselectListOptionsByText(el, optionList);
                                break;
                            case ActMultiselectList.eActMultiselectListAction.SetSelectedValueByValue:
                                SelectMultiselectListOptionsByValue(el, optionList);
                                break;
                            case ActMultiselectList.eActMultiselectListAction.ClearAllSelectedValues:
                                //Replacing msgbox with Reporter.ToUser
                                Reporter.ToUser(eUserMsgKey.MissingImplementation, el.ActMultiselectListAction.ToString());
                                //End

                                //TODO: implement ClearAllSelectedValues for ActMultiselectList
                                break;
                        }
                        break;
                    case "ActDropDownList":
                        ActDropDownList dd = (ActDropDownList)act;
                        switch (dd.ActDropDownListAction)
                        {
                            case ActDropDownList.eActDropDownListAction.SetSelectedValueByIndex:
                                SelectDropDownListOptionByIndex(dd, Int32.Parse(act.GetInputParamCalculatedValue("Value")));
                                break;
                            case ActDropDownList.eActDropDownListAction.SetSelectedValueByValue:
                                SelectDropDownListOptionByValue(dd, act.GetInputParamCalculatedValue("Value"));
                                break;
                            case ActDropDownList.eActDropDownListAction.SetSelectedValueByText:
                                SelectDropDownListOptionByText(dd, act.GetInputParamCalculatedValue("Value"));
                                break;
                            case ActDropDownList.eActDropDownListAction.GetValidValues:
                                GetDropDownListValidValues(act);
                                break;
                            case ActDropDownList.eActDropDownListAction.GetSelectedValue:
                                GetDropDownListSelectedValue(dd);
                                break;
                            case ActDropDownList.eActDropDownListAction.IsPrepopulated:
                                IsDropDownListPrepopulated(dd);
                                break;
                            case ActDropDownList.eActDropDownListAction.GetFont:
                                GetDropDownListFont(dd);
                                break;

                        }
                        break;
                    case "ActCheckbox":
                        ActCheckbox cb1 = (ActCheckbox)act;
                        switch (cb1.CheckboxAction)
                        {
                            case ActCheckbox.eCheckboxAction.Click:
                                ClickCheckbox(cb1);
                                break;
                            case ActCheckbox.eCheckboxAction.Check:
                                CheckCheckbox(cb1);
                                break;
                            case ActCheckbox.eCheckboxAction.Uncheck:
                                UncheckCheckbox(cb1);
                                break;
                            case ActCheckbox.eCheckboxAction.GetValue:
                                GetCheckboxValue(cb1);
                                break;
                            case ActCheckbox.eCheckboxAction.IsDisabled:
                                IsCheckboxEnable(cb1);
                                break;
                            case ActCheckbox.eCheckboxAction.IsDisplayed:
                                IsCheckboxDisplayed(cb1);
                                break;
                        }
                        break;

                    case "ActScreenShot":
                        mFrmBrowser.AddScreenShot(act);
                        break;

                    case "ActWebSitePerformanceTiming":
                        ActWebSitePerformanceTiming ABPT = (ActWebSitePerformanceTiming)act;
                        ActWebSitePerformanceTimingHandler(ABPT);
                        break;
                    case "ActPWL":
                        PWLElementHandler((ActPWL)act);
                        break;
                    default:

                        throw new Exception("Action unknown/not implemented for the Driver: " + this.GetType().ToString());
                }
            }
            catch (Exception ex)
            {
                act.Error = "Error While executing action: " + ex.Message;
            }

            if (mFrmBrowser != null)
            {
                mFrmBrowser.WaitWhileBrowserBusy();
            }
        }

        private void PWLElementHandler(ActPWL act)
        {
            IHTMLElement e, e1;
            e = mFrmBrowser.TryGetActElementByLocator(act);            
            e1 = mFrmBrowser.TryGetActElementByLocator(new ActPWL() { LocateBy = act.OLocateBy, LocateValue = act.OLocateValue, LocateValueCalculated = act.OLocateValue });

            switch (act.PWLAction)
            {

                case ActPWL.ePWLAction.GetHDistanceLeft2Left:

                    if (e == null || e1 == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        act.AddOrUpdateReturnParamActual("Actual", Math.Abs(e.offsetLeft - e1.offsetLeft).ToString());
                    }
                    break;
                case ActPWL.ePWLAction.GetHDistanceLeft2Right:
                    if (e == null || e1 == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        act.AddOrUpdateReturnParamActual("Actual", Math.Abs(e.offsetLeft - e1.offsetLeft + e1.offsetWidth).ToString());
                    }
                    break;
                case ActPWL.ePWLAction.GetHDistanceRight2Right:
                    if (e == null || e1 == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        act.AddOrUpdateReturnParamActual("Actual", Math.Abs(e.offsetLeft - e1.offsetLeft - e1.offsetWidth + e.offsetWidth).ToString());
                    }
                    break;
                case ActPWL.ePWLAction.GetHDistanceRight2Left:
                    if (e == null || e1 == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        act.AddOrUpdateReturnParamActual("Actual", Math.Abs(e.offsetLeft - e1.offsetLeft + e.offsetWidth).ToString());
                    }
                    break;
                case ActPWL.ePWLAction.GetVDistanceTop2Top:
                    if (e == null || e1 == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        act.AddOrUpdateReturnParamActual("Actual", Math.Abs(e.offsetTop - e1.offsetTop).ToString());
                    }
                    break;
                case ActPWL.ePWLAction.GetVDistanceTop2Bottom:
                    if (e == null || e1 == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        act.AddOrUpdateReturnParamActual("Actual", (Math.Abs(e.offsetTop - e1.offsetTop) + e1.offsetHeight).ToString());
                    }
                    break;
                case ActPWL.ePWLAction.GetVDistanceBottom2Bottom:
                    if (e == null || e1 == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        act.AddOrUpdateReturnParamActual("Actual", Math.Abs(e.offsetTop + e.offsetHeight  - e1.offsetTop - e1.offsetHeight).ToString());
                    }
                    break;
                case ActPWL.ePWLAction.GetVDistanceBottom2Top:
                    if (e == null || e1 == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        act.AddOrUpdateReturnParamActual("Actual", Math.Abs(e.offsetLeft - e1.offsetLeft + e.offsetHeight).ToString());
                    }
                    break;
            }
        }

        private void ActWebSitePerformanceTimingHandler(ActWebSitePerformanceTiming ABPT)
        {
            object[] jsCode = { "window.performance.timing" };
            dynamic PerfObj = mBrowserControl.InvokeScript("eval", jsCode);

            ABPT.AddOrUpdateReturnParamActual("navigationStart", ((ulong)PerfObj.navigationStart).ToString());
            ABPT.AddOrUpdateReturnParamActual("unloadEventStart", ((ulong)PerfObj.unloadEventStart).ToString());
            ABPT.AddOrUpdateReturnParamActual("unloadEventEnd", ((ulong)PerfObj.unloadEventEnd).ToString());
            ABPT.AddOrUpdateReturnParamActual("redirectStart", ((ulong)PerfObj.redirectStart).ToString());
            ABPT.AddOrUpdateReturnParamActual("redirectEnd", ((ulong)PerfObj.redirectEnd).ToString());
            ABPT.AddOrUpdateReturnParamActual("fetchStart", ((ulong)PerfObj.fetchStart).ToString());
            ABPT.AddOrUpdateReturnParamActual("domainLookupStart", ((ulong)PerfObj.domainLookupStart).ToString());
            ABPT.AddOrUpdateReturnParamActual("domainLookupEnd", ((ulong)PerfObj.domainLookupEnd).ToString());
            ABPT.AddOrUpdateReturnParamActual("connectStart", ((ulong)PerfObj.connectStart).ToString());
            ABPT.AddOrUpdateReturnParamActual("connectEnd", ((ulong)PerfObj.connectEnd).ToString());

            //TODO: giving err check/fix, meanwhile empty val
            // ABPT.AddOrUpdateReturnParam("secureConnectionStart", ((ulong)PerfObj.secureConnectionStart).ToString());
            ABPT.AddOrUpdateReturnParamActual("secureConnectionStart", "");

            ABPT.AddOrUpdateReturnParamActual("requestStart", ((ulong)PerfObj.requestStart).ToString());
            ABPT.AddOrUpdateReturnParamActual("responseEnd", ((ulong)PerfObj.responseEnd).ToString());
            ABPT.AddOrUpdateReturnParamActual("domLoading", ((ulong)PerfObj.domLoading).ToString());
            ABPT.AddOrUpdateReturnParamActual("domInteractive", ((ulong)PerfObj.domInteractive).ToString());
            ABPT.AddOrUpdateReturnParamActual("domContentLoadedEventStart", ((ulong)PerfObj.domContentLoadedEventStart).ToString());
            ABPT.AddOrUpdateReturnParamActual("domContentLoadedEventEnd", ((ulong)PerfObj.domContentLoadedEventEnd).ToString());
            ABPT.AddOrUpdateReturnParamActual("domComplete", ((ulong)PerfObj.domComplete).ToString());
            ABPT.AddOrUpdateReturnParamActual("loadEventStart", ((ulong)PerfObj.loadEventStart).ToString());
            ABPT.AddOrUpdateReturnParamActual("loadEventEnd", ((ulong)PerfObj.loadEventEnd).ToString());

            ABPT.SetInfo();
        }

        private void SmartSyncHandler(ActSmartSync act)
        {
            IHTMLElement e = mFrmBrowser.TryGetActElementByLocator(act);
           
            Stopwatch st = new Stopwatch();
            int waitTime = 0;
            try
            {
                if (string.IsNullOrEmpty(act.GetInputParamValue("Value")))
                    waitTime = 5;
                else
                    waitTime = Convert.ToInt32(act.GetInputParamCalculatedValue("Value"));
            }
            catch (Exception)
            {
                waitTime = 5;
            }
            switch (act.SmartSyncAction)
            {
                case ActSmartSync.eSmartSyncAction.WaitUntilDisplay:
                    st.Reset();
                    st.Start();

                    while (!(e != null && (e.getAttribute("Displayed")==true || e.getAttribute("Enabled")==true)))
                    {
                        Thread.Sleep(100);
                        e = mFrmBrowser.TryGetActElementByLocator(act);
                        if (st.ElapsedMilliseconds > waitTime * 1000)
                        {
                            act.Error = "Smart Sync of WaitUntilDisplay is timeout";
                            break;
                        }
                    }
                    break;
                case ActSmartSync.eSmartSyncAction.WaitUntilDisapear:
                    st.Reset();

                    if (e == null)
                    {
                        return;
                    }
                    else
                    {
                        st.Start();

                        while (e != null && e.getAttribute("Displayed") != true)
                        {
                            Thread.Sleep(100);
                            e = mFrmBrowser.TryGetActElementByLocator(act);
                            if (st.ElapsedMilliseconds > waitTime * 1000)
                            {
                                act.Error = "Smart Sync of WaitUntilDisapear is timeout";
                                break;
                            }
                        }

                    }
                    break;
            }
            return;
        }

        private void GenElementHandler(ActGenElement act)
        {
            IHTMLElement e = null;
            switch (act.GenElementAction)
            {
                case ActGenElement.eGenElementAction.Click:
                    e = mFrmBrowser.TryGetActElementByLocator(act);
                    if (e != null) e.click();
                    break;
                case ActGenElement.eGenElementAction.GetInnerText:
                    e = mFrmBrowser.TryGetActElementByLocator(act);
                    if (e != null)
                    {
                        act.AddOrUpdateReturnParamActual("Actual", e.innerText);
                    }
                    break;
                case ActGenElement.eGenElementAction.GetValue:
                    e = mFrmBrowser.TryGetActElementByLocator(act);
                    if (e != null)
                    {
                        act.AddOrUpdateReturnParamActual("Actual", e.innerText);                       
                    }
                    break;
                case ActGenElement.eGenElementAction.GetCustomAttribute:
                    e = mFrmBrowser.TryGetActElementByLocator(act);
                    if (e != null)
                    {
                        string CustomAttributeReturnValue="";
                        XmlDocument CurrentNode = new XmlDocument();
                        CurrentNode.LoadXml(e.outerHTML);
                        if (CurrentNode.ChildNodes[0].Attributes.Count > 0)
                        {
                            foreach (XmlAttribute xa in CurrentNode.ChildNodes[0].Attributes)
                            {
                                if (xa.Name == act.Value){CustomAttributeReturnValue = xa.Value;}
                            }
                        }
                        act.AddOrUpdateReturnParamActual("Actual", CustomAttributeReturnValue);
                    }
                    break;

                case ActGenElement.eGenElementAction.SwitchFrame:
                    mshtml.FramesCollection frames = 
                        ((mshtml.HTMLDocument)mFrmBrowser.mDocument).frames;
                    mshtml.IHTMLWindow2 tFrame = null;
                    for (int i = 0; i < frames.length; i++)
                    {
                        object refIndex = i;
                        mshtml.IHTMLWindow2 frame = (mshtml.IHTMLWindow2)frames.item(refIndex);
                        if (frame.name == act.GetInputParamCalculatedValue("Value"))
                        {
                            tFrame = frame;
                            break;
                        }
                        else if (frame.name == null)
                        {
                            tFrame = frame;
                            break;
                        }

                    }
                    if (tFrame != null)
                    {
                        mFrmBrowser.mDocument = (mshtml.HTMLDocument)tFrame.document;
                    }
                    break;
                case ActGenElement.eGenElementAction.SelectFromDropDown:
                    e = mFrmBrowser.TryGetActElementByLocator(act);
                    mshtml.HTMLSelectElement se = (mshtml.HTMLSelectElement)e;
                    if (se != null)
                    {                        
                        try
                        {
                            se.selectedIndex = Convert.ToInt32(act.GetInputParamCalculatedValue("Value"));
                        }
                        catch (Exception )
                        {
                            try
                            {
                                se.setAttribute("text", act.GetInputParamCalculatedValue("Value"));
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    se.setAttribute("value", act.GetInputParamCalculatedValue("Value"));
                                }
                                catch (Exception)
                                {

                                }
                            }
                        }
                    }
                    break;
                case ActGenElement.eGenElementAction.GotoURL:
                    mFrmBrowser.GotoURL(act.GetInputParamCalculatedValue("Value"));
                    break;

                case ActGenElement.eGenElementAction.Visible:
                    e = mFrmBrowser.TryGetActElementByLocator(act);

                    if (e != null)
                        act.AddOrUpdateReturnParamActual("Actual", (e.style.display == "none") ? "False" : "True");
                    else
                        act.AddOrUpdateReturnParamActual("Actual", "False" );        
                    break;
                    
                case ActGenElement.eGenElementAction.SetValue:
                    e = mFrmBrowser.TryGetActElementByLocator(act);
                    if (e != null)
                        ((mshtml.HTMLInputTextElement)e).value = act.GetInputParamCalculatedValue("Value");    
                    break;
                    
                case ActGenElement.eGenElementAction.Wait:
                    Thread.Sleep(Convert.ToInt32(act.GetInputParamCalculatedValue("Value")) * 1000);
                    break;
                case ActGenElement.eGenElementAction.KeyType:
                    e = mFrmBrowser.TryGetActElementByLocator(act);
                   if (e != null)
                       ((mshtml.HTMLInputTextElement)e).value = act.GetInputParamCalculatedValue("Value");                       
                    break;
                    
                case ActGenElement.eGenElementAction.Back:                    
                    mFrmBrowser.browser.GoBack();
                    break;

                case ActGenElement.eGenElementAction.MsgBox:
                    string msg = act.GetInputParamCalculatedValue("Value");                    
                    Reporter.ToUser(eUserMsgKey.ScriptPaused);
                    break;

                default:
                    throw new Exception("Action unknown/not implemented for the Driver: " + this.GetType().ToString());
            }
        }

        private void GetTextBoxValue(ActTextBox ATB)
        {
            IHTMLElement e1 = mFrmBrowser.TryGetActElementByLocator(ATB);
            mshtml.HTMLInputTextElement t = (mshtml.HTMLInputTextElement)e1;
            if (t != null)
            {
                ATB.AddOrUpdateReturnParamActual("Actual", t.value);             
            }
        }

        private void GetTextBoxInputLength(ActTextBox ATB)
        {
            IHTMLElement e1 = mFrmBrowser.TryGetActElementByLocator(ATB);
            mshtml.HTMLInputTextElement t = (mshtml.HTMLInputTextElement)e1;
            if (t != null)
            {
                ATB.AddOrUpdateReturnParamActual("Actual", t.value.Length.ToString());
            }
        }

        private void GetTextBoxFont(ActTextBox ATB)
        {
            IHTMLElement e1 = mFrmBrowser.TryGetActElementByLocator(ATB);
            mshtml.HTMLInputTextElement t = (mshtml.HTMLInputTextElement)e1;
            if (t != null)
            {
                ATB.AddOrUpdateReturnParamActual("Actual", t.style.font);
            }
        }

        private void IsTextBoxPrepopulated(ActTextBox ATB)
        {
            IHTMLElement e1 = mFrmBrowser.TryGetActElementByLocator(ATB);
            mshtml.HTMLInputTextElement t = (mshtml.HTMLInputTextElement)e1;
            if (t != null)
            {
                ATB.AddOrUpdateReturnParamActual("Actual", (t.value.ToString().Trim() != "").ToString());
            }
        }

        private void IsTextBoxDisplayed(ActTextBox ATB)
        {
            IHTMLElement e1 = mFrmBrowser.TryGetActElementByLocator(ATB);
            mshtml.HTMLInputTextElement t = (mshtml.HTMLInputTextElement)e1;
            if (t != null)
            {

                ATB.AddOrUpdateReturnParamActual("Actual", (t.style.visibility != "hidden").ToString());
            }
        }
        
        private void IsTextBoxDisabled(ActTextBox ATB)
        {
            IHTMLElement e1 = mFrmBrowser.TryGetActElementByLocator(ATB);
            mshtml.HTMLInputTextElement t = (mshtml.HTMLInputTextElement)e1;
            if (t != null)
            {
                ATB.AddOrUpdateReturnParamActual("Actual", t.disabled+"");
            }
        }

        private void GetLabelName(ActLabel alb)
        {
            IHTMLElement e1 = mFrmBrowser.TryGetActElementByLocator(alb);
            mshtml.HTMLLabelElement t = (mshtml.HTMLLabelElement)e1;
            if (t != null)
            {
                alb.AddOrUpdateReturnParamActual("Actual", t.innerText);
            }
        }

        private void CheckLabelDisplayed(ActLabel alb)
        {
            IHTMLElement e1 = mFrmBrowser.TryGetActElementByLocator(alb);
            mshtml.HTMLLabelElement t = (mshtml.HTMLLabelElement)e1;
            if (t != null)
            {
                alb.AddOrUpdateReturnParamActual("Actual", true + "");
            }
        }
        
        private void GetDropDownListValidValues(Act act)
        {
            HTMLSelectElement element = (HTMLSelectElement)mFrmBrowser.TryGetActElementByLocator(act);
            if (element == null)
            {
                act.AddOrUpdateReturnParamActual("Actual", "ERROR - Element not Found");
                return;
            }

            //TODO: dup code with IBWindow, cretae validation - use one run.action using driver
            string s = "";
            IHTMLElementCollection options = element.children;
            foreach (IHTMLElement v in options)
            {
                //TODO: decide on delimeter, const global for app
                s = s + v.innerText + "|";
            }
            act.AddOrUpdateReturnParamActual("Actual", s);
        }

        private void doSubmit(ActSubmit aSub)
        {
            IHTMLElement e1 = mFrmBrowser.TryGetActElementByLocator(aSub);
            if (e1 != null)
            {
                e1.click();
            }
            else
            {
                aSub.Error = "Element not found - " + aSub.LocateBy + " - " + aSub.LocateValueCalculated;
            }
        }

        private void ClickLink(ActLink Alink)
        {
            IHTMLElement e1 = mFrmBrowser.TryGetActElementByLocator(Alink);
            mshtml.HTMLAnchorElement l = (mshtml.HTMLAnchorElement)e1;
            if (l != null)
            {
                l.click();
            }
        }

        private void GetLinkValue(ActLink Alink)
        {               
            IHTMLElement el = mFrmBrowser.TryGetActElementByLocator(Alink);
            if (el != null)
            {
                Alink.AddOrUpdateReturnParamActual("Actual",el.getAttribute("href"));
            }
        }

        private void IsLinkVisible(ActLink Alink)
        {
            //TODO: Enable link visibility check, if it's possible
            // I have searched through the dynamic properties available from the COM object at runtime 
            // and can't find one for visibility or display.

            Alink.AddOrUpdateReturnParamActual("Actual", "True");
        }

        private void ClickButton(ActButton b)
        {
            IHTMLElement e1 = mFrmBrowser.TryGetActElementByLocator(b);
            mshtml.HTMLButtonElement t = (mshtml.HTMLButtonElement)e1;
            if (t != null)
            {
                t.click();
            }
        }

        private void ActButtonHandler(ActButton actButton)
        {
            //TODO: add logic for exceptions, which currently do nothing and catch everything
            IHTMLElement e1 = mFrmBrowser.TryGetActElementByLocator(actButton);
            // defaults to this if no operation succeeds
            actButton.AddOrUpdateReturnParamActual("Actual", "");
            if (actButton.ButtonAction == ActButton.eButtonAction.GetValue)
            {

                 actButton.AddOrUpdateReturnParamActual("Actual",e1.getAttribute("Value"));
                return;
            }
            else if (actButton.ButtonAction == ActButton.eButtonAction.IsDisabled)
            {
                try {
                    actButton.AddOrUpdateReturnParamActual("Actual",  e1.getAttribute("Disabled")); }
                catch (Exception ) {
                    if (actButton.GetReturnParam("Return Value") == "")
                        actButton.AddOrUpdateReturnParamActual("Actual","False");
                }
                return;
            }
            else if (actButton.ButtonAction == ActButton.eButtonAction.GetFont)
            {                
                try {
                actButton.AddOrUpdateReturnParamActual("Actual",e1.style.font);
                }
                catch (Exception ex){ Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
                return;
            }
            else if (actButton.ButtonAction == ActButton.eButtonAction.IsDisplayed)
            {
                try {
                actButton.AddOrUpdateReturnParamActual("Actual", e1.style.display);
                }
                catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
                return;
            }
            else
            {
                ClickButton(actButton);
            }
        }
        
        private void SetTextBoxValue(ActTextBox ATB)
        {            
            IHTMLElement e1 = mFrmBrowser.TryGetActElementByLocator(ATB);
            mshtml.HTMLInputTextElement t = (mshtml.HTMLInputTextElement)e1;
            if (t != null)
            {
                t.value = ATB.GetInputParamCalculatedValue("Value");
            }            
        }

        private void SetPasswordValue(ActPassword a)
        {
            IHTMLElement e1 = mFrmBrowser.TryGetActElementByLocator(a);
            mshtml.HTMLInputTextElement t = (mshtml.HTMLInputTextElement)e1;
            if (t != null) t.value = a.GetInputParamCalculatedValue("Value");   
        }

        private void GetPasswordSize(ActPassword a)
        {
            IHTMLElement e1 = mFrmBrowser.TryGetActElementByLocator(a);
            mshtml.HTMLInputTextElement t = (mshtml.HTMLInputTextElement)e1;
            if (t != null) { a.AddOrUpdateReturnParamActual("Actual", t.getAttribute("size") + "");
           
            }  
        }

        private void IsPasswordBoxDisabled(ActPassword a)
        {
            IHTMLElement e1 = mFrmBrowser.TryGetActElementByLocator(a);
            mshtml.HTMLInputTextElement t = (mshtml.HTMLInputTextElement)e1;
            if (t != null) a.AddOrUpdateReturnParamActual("Actual", t.getAttribute("Disabled") + ""); 
        }

        #region Checkbox methods

        private void CheckCheckbox(ActCheckbox cb)
        {
            IHTMLElement e1 =mFrmBrowser.TryGetActElementByLocator(cb);
            mshtml.IHTMLElement2 t = (mshtml.IHTMLElement2)e1;
            if (e1.getAttribute("checked") == false)
            {
                e1.setAttribute("checked", true);
            }
        }

        private void ClickCheckbox(ActCheckbox cb)
        {
            IHTMLElement e1 = mFrmBrowser.TryGetActElementByLocator(cb);
            mshtml.IHTMLElement2 t = (mshtml.IHTMLElement2)e1;
            if (e1.getAttribute("checked") == false)
            {
                e1.setAttribute("checked", true);
            }
            else
            {
                e1.setAttribute("checked", false);
            }
        }

        private void UncheckCheckbox(ActCheckbox cb)
        {
            IHTMLElement e1 = mFrmBrowser.TryGetActElementByLocator(cb);
            mshtml.IHTMLElement t = (mshtml.IHTMLElement)e1;
            if (e1.getAttribute("checked") == true)
            {
                e1.setAttribute("checked", false);
            }
        }

        private void IsCheckboxEnable(ActCheckbox cb)
        {
            IHTMLElement e1 = mFrmBrowser.TryGetActElementByLocator(cb);
            mshtml.IHTMLElement t = (mshtml.IHTMLElement)e1;
            cb.AddOrUpdateReturnParamActual("Actual", Convert.ToString(t.getAttribute("disabled")));
        }

        private void GetCheckboxValue(ActCheckbox cb)
        {
            IHTMLElement e1 = mFrmBrowser.TryGetActElementByLocator(cb);
            mshtml.IHTMLElement t = (mshtml.IHTMLElement)e1;
            cb.AddOrUpdateReturnParamActual("Actual", Convert.ToString(t.getAttribute("checked")));
        }

        private void IsCheckboxDisplayed(ActCheckbox cb)
        {
            IHTMLElement e1 = mFrmBrowser.TryGetActElementByLocator(cb);
            mshtml.IHTMLElement t = (mshtml.IHTMLElement)e1;
            cb.AddOrUpdateReturnParamActual("Actual", e1.style.display);
        }
        
        #endregion //Checkbox methods

        #region Radio Button methods

        private void SelectRadioButtonByIndex(Act a, ActRadioButton rb, int selectedIndex)
        {
            List<IHTMLElement> RBs = LocateRadioButtonElements(a, rb.LocateBy, rb.LocateValueCalculated);
            for (int i = 0; i < RBs.Count; i++)
            {
                if (i == selectedIndex)
                {
                    RBs[i].click();
                    i = RBs.Count;
                }
            }
        }
        
        private void SelectRadioButtonByValue(Act a, ActRadioButton rb, string val)
        {

            List<IHTMLElement> RBs = LocateRadioButtonElements(a, rb.LocateBy, rb.LocateValueCalculated);
                for (int i = 0; i < RBs.Count; i++)
                {
                    if (RBs[i].getAttribute("value") == val)
                    {
                        RBs[i].click();
                        i = RBs.Count;
                    }
                }
        }

        private void SelectRadioButtonByID(Act a, ActRadioButton rb, string val)
        {
            List<IHTMLElement> RBs = LocateRadioButtonElements(a, rb.LocateBy, rb.LocateValueCalculated);
            for (int i = 0; i < RBs.Count; i++)
            {
                if (RBs[i].getAttribute("id") == val)
                {
                    RBs[i].click();
                    i = RBs.Count;
                }
            }
        }

        private List<IHTMLElement> LocateRadioButtonElements(Act A, eLocateBy LocatorType, string LocValue)
        {
            List<IHTMLElement> l = new List<IHTMLElement>();
            IHTMLElementCollection elm = mFrmBrowser.mDocument.all;
            foreach (IHTMLElement h in elm)
            {
                if (h.tagName.ToUpper() == "INPUT" && h.getAttribute("TYPE").ToUpper() == "RADIO")
                {
                    switch (LocatorType)
                    {
                        case eLocateBy.ByID:
                            if (h.getAttribute("ID") == LocValue) l.Add(h);
                            break;
                        case eLocateBy.ByName:
                            if (h.getAttribute("NAME") == LocValue) l.Add(h);
                            break;
                        case eLocateBy.ByCSS:
                            if (h.getAttribute("CLASS") == LocValue) l.Add(h);
                            break;
                        case eLocateBy.ByValue:
                            if (h.getAttribute("value") == LocValue) l.Add(h);

                            break;
                        case eLocateBy.ByXPath:
                            A.Error = LocatorType.ToString() + " hasn't been implemented for Radio Buttons in the internal browser yet.";
                            break;
                        default:
                            A.Error = LocatorType.ToString() + " hasn't been implemented for Radio Buttons in the internal browser yet.";
                            break;
                    }
                }
            }
            return l;
        }

        private void GetRadioButtonValue(Act a, ActRadioButton rb)
        {
            List<IHTMLElement> RBs = LocateRadioButtonElements(a, rb.LocateBy, rb.LocateValueCalculated);
            for (int i = 0; i < RBs.Count; i++)
            {
                if (RBs[i].getAttribute("checked") == true)
                {
                    rb.AddOrUpdateReturnParamActual("Actual", RBs[i].getAttribute("value"));
                    return;
                }
            }
        }

        private void IsRadioButtonDisabled(Act a, ActRadioButton rb)
        {
            List<IHTMLElement> RBs = LocateRadioButtonElements(a, rb.LocateBy, rb.LocateValueCalculated);
            for (int i = 0; i < RBs.Count; i++)
            {
                rb.AddOrUpdateReturnParamActual("Actual", RBs[i].getAttribute("Disabled") + "");
            }
        }

        private void GetRadioButtonAvailableValues(Act a, ActRadioButton rb)
        {
            string aValues = "";
            List<IHTMLElement> RBs = LocateRadioButtonElements(a, rb.LocateBy, rb.LocateValueCalculated);
            for (int i = 0; i < RBs.Count; i++)
            {
                aValues = RBs[i].getAttribute("value") + "|"+aValues;
            }
            rb.AddOrUpdateReturnParamActual("Actual", aValues);
        }
        
        #endregion // Radio Button methods

        #region DropDownList methods

        private void SelectDropDownListOptionByIndex(ActDropDownList dd, int i)
        {
            mshtml.HTMLSelectElement sel = (mshtml.HTMLSelectElement)mFrmBrowser.TryGetActElementByLocator(dd);
            if(sel!=null)
                sel.selectedIndex = i;
        }
        
        private void SelectDropDownListOptionByText(ActDropDownList dd, string s)
        {
            mshtml.HTMLSelectElement sel = (mshtml.HTMLSelectElement)mFrmBrowser.TryGetActElementByLocator(dd);
            if (sel.length>0)
            {
                for (int i=0; i < sel.length-1;i++)
                {
                    if (sel.item(i).text==s) 
                    {
                        sel.selectedIndex = i;
                        return;
                    }
                }
            }
        }

        private void SelectDropDownListOptionByValue(ActDropDownList dd, string s)
        {
            mshtml.HTMLSelectElement sel = (mshtml.HTMLSelectElement)mFrmBrowser.TryGetActElementByLocator(dd);
            if (sel != null)
                sel.setAttribute("value", s);
        }

        private void GetDropDownListSelectedValue(ActDropDownList dd)
        {
            HTMLSelectElement element = (HTMLSelectElement)mFrmBrowser.TryGetActElementByLocator(dd);
            if (element == null)
            {
                dd.AddOrUpdateReturnParamActual("Actual", "ERROR - Element not Found");
                return;
            }
            dd.AddOrUpdateReturnParamActual("Actual", element.options[element.selectedIndex].value);
        }

        private void IsDropDownListPrepopulated(ActDropDownList dd)
        {
            HTMLSelectElement element = (HTMLSelectElement)mFrmBrowser.TryGetActElementByLocator(dd);
            if (element == null)
            {
                dd.AddOrUpdateReturnParamActual("Actual", "ERROR - Element not Found");
                return;
            }
            dd.AddOrUpdateReturnParamActual("Actual", (element.options[element.selectedIndex].value.trim() != "").ToString());
        }

        private void GetDropDownListFont(ActDropDownList dd)
        {
            HTMLSelectElement element = (HTMLSelectElement)mFrmBrowser.TryGetActElementByLocator(dd);
            if (element == null)
            {
                dd.AddOrUpdateReturnParamActual("Actual","ERROR - Element not Found");
                return;
            }
            dd.AddOrUpdateReturnParamActual("Actual", element.style.font);
        }

        #endregion //DropDownList methods

        #region MultiselectList methods

        private void SelectMultiselectListOptionsByIndex(ActMultiselectList l, List<int> vals)
        {
           // HtmlElement h = getActElementByLocator(l);
           // HtmlElementCollection coll = mBrowserControl.Document.GetElementsByTagName("option");
           // int i = 0;
           //foreach (HtmlElement element in coll)
           //{
           //    if (element.Parent==h) 
           //    {
           //        i++;
           //        if (vals.Contains(i))
           //        {
           //            element.SetAttribute("selected", "true");
           //        }
           //    }
           //}
        }

        private void SelectMultiselectListOptionsByText(ActMultiselectList l, List<string> vals)
        {
            //HtmlElement h = getActElementByLocator(l);
            //HtmlElementCollection coll = mBrowserControl.Document.GetElementsByTagName("option");
            //int i = 0;
            //foreach (HtmlElement element in coll)
            //{
            //    if (element.Parent == h)
            //    {
            //        i++;
            //        if (vals.Contains(element.InnerHtml))
            //        {
            //            element.SetAttribute("selected", "true");
            //        }
            //    }
            //}
        }

        private void SelectMultiselectListOptionsByValue(ActMultiselectList l, List<string> vals)
        {
            //HtmlElement h = getActElementByLocator(l);
            //HtmlElementCollection coll = mBrowserControl.Document.GetElementsByTagName("option");
            //int i = 0;
            //foreach (HtmlElement element in coll)
            //{
            //    if (element.Parent == h)
            //    {
            //        i++;
            //        if (vals.Contains(element.GetAttribute("value")))
            //        {
            //            element.SetAttribute("selected", "true");
            //        }
            //    }
            //}
        }

        #endregion //MultiselectList methods
        
        private void GotoURL(ActGotoURL act)
        {
            mFrmBrowser.GotoURL(act.ValueForDriver);
        }

        public override string GetURL()
        {
            return "TBD";
        }

        

      

        public override void HighlightActElement(Act act)
        {
            if(mFrmBrowser!=null)
                mFrmBrowser.HighLightActElement(act);
        }

        public void HalfHalf()
        {
            mFrmBrowser.SideBySide();
        }

        public override ePlatformType Platform { get { return ePlatformType.Web; } }

        public override bool IsRunning()
        {
          return IsBrowserLoaded;
        }
        
        List<AppWindow> IWindowExplorer.GetAppWindows()
        {
            List<AppWindow> list = new List<AppWindow>();
            AppWindow AW = new AppWindow();
            AW.Title = "Current Window"; //TODO: fix me to get current title
            AW.RefObject = mBrowserControl.Document;
            AW.WindowType = AppWindow.eWindowType.InternalBrowserWebPageDocument;
            list.Add(AW);
            return list;
        }
        
        List<ElementInfo> IWindowExplorer.GetVisibleControls(List<eElementType> filteredElementType, ObservableList<ElementInfo> foundElementsList = null,bool isPOMLearn = false)
        {
            //TODO: impl
            return null;
        }

        List<ElementInfo> IWindowExplorer.GetElementChildren(ElementInfo ElementInfo)
        {
            //TODO: impl
            return null;
        }

        void IWindowExplorer.SwitchWindow(string Title)
        {

        }

        
        void IWindowExplorer.HighLightElement(ElementInfo ElementInfo, bool locateElementByItLocators = false)
        {
            //TODO: keep old element border and restore it
            //string scriptRemoveBorder = "arguments[0].style.border='0px'";
            //if (mCurrentHighlightedElement != null)
            //{
            //    try
            //    {
            //        ((IJavaScriptExecutor)Driver).ExecuteScript(scriptRemoveBorder, mCurrentHighlightedElement);
            //    }
            //    catch
            //    {
            //        //do nothing since the element might not displayed anymore
            //    }

            //}

            //IWebElement elem = (IWebElement)TVI.NodeObject();
            //if (elem == null) return;

            //mCurrentHighlightedElement = elem;
            //string script = "arguments[0].style.border='2px solid red'";
            //((IJavaScriptExecutor)Driver).ExecuteScript(script, elem);
        }

        ObservableList<ControlProperty> IWindowExplorer.GetElementProperties(ElementInfo ElementInfo)
        {
            return null;
        }

        ObservableList<ElementLocator> IWindowExplorer.GetElementLocators(ElementInfo ElementInfo)
        {
            return null;
        }

        object IWindowExplorer.GetElementData(ElementInfo ElementInfo, eLocateBy elementLocateBy, string elementLocateValue)
        {
            return null;
        }
        
        string IWindowExplorer.GetFocusedControl()
        {
            //TODO: return current focused control of the window
            return null;
        }

        ElementInfo IWindowExplorer.GetControlFromMousePosition()
        {
            return null;
        }

        public ElementInfo LearnElementInfoDetails(ElementInfo EI)
        {
            return EI;
        }


        AppWindow IWindowExplorer.GetActiveWindow()
        {
            AppWindow aw = new AppWindow();
            aw.Title = "Current Window";
            return aw;
        }
        
        public override void StartRecording()
        {

        }

        public override void StopRecording()
        {
            
        }

        ObservableList<ElementInfo> IWindowExplorer.GetElements(ElementLocator EL)
        {
            throw new Exception("Not implemented yet for this driver");
        }

        void IWindowExplorer.UpdateElementInfoFields(ElementInfo eI)
        {

        }

        bool IWindowExplorer.IsElementObjectValid(object obj)
        {
            return true;
        }

        void IWindowExplorer.UnHighLightElements()
        {
            throw new NotImplementedException();
        }

        public bool TestElementLocators(ElementInfo EI, bool GetOutAfterFoundElement = false)
        {
            throw new NotImplementedException();
        }

        public void CollectOriginalElementsDataForDeltaCheck(ObservableList<ElementInfo> originalList)
        {
            throw new NotImplementedException();
        }

        public ElementInfo GetMatchingElement(ElementInfo latestElement, ObservableList<ElementInfo> originalElements)
        {
            throw new NotImplementedException();
        }

        public void StartSpying()
        {
            throw new NotImplementedException();
        }

        ObservableList<OptionalValue> IWindowExplorer.GetOptionalValuesList(ElementInfo ElementInfo, eLocateBy elementLocateBy, string elementLocateValue)
        {
            throw new NotImplementedException();
        }
    }
}
