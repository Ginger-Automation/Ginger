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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using mshtml;
using GingerCore.Actions;
using System.Drawing;
using System.Windows.Threading;
using System.Collections;
using GingerCore.Drivers.InternalBrowserLib;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using GingerCore.GeneralLib;
using GingerCore.Actions.Common;
using Amdocs.Ginger.Common.UIElement;

namespace GingerCore.Drivers
{
    /// <summary>
    /// Interaction logic for InternalBrowserWindow.xaml
    /// </summary>
    public partial class InternalBrowserWindow : Window 
    {
        WebBrowserPage WBP;        
        public bool mBusy = false;
        public InternalBrowser IBDriver;
        public mshtml.HTMLDocument mDocument = null;
        public string Useragent;
        mshtml.IHTMLElement CurrentHighlightedElement;

        public BusinessFlow mBusinessFlow { get; set; }

        public System.Windows.Controls.WebBrowser browser;

        mshtml.IHTMLElement mCurrentElement = null;
        mshtml.IHTMLElement mPWLOElement = null;
        mshtml.IHTMLElement mPWLTElement = null;
        private Act mValidateAction_1;
        private Act mValidateAction_2;
        private Act mAction;
        private int elcount=0;
        private bool mPWLSelectingTarget = false;
        private eLocateBy ElemLocator = eLocateBy.ByID;
        private ActGenElement.eGenElementAction ElemAction = ActGenElement.eGenElementAction.Click;

        private string SavedMHTFilePath;
        public int CurrentSearchPosition;

        DomEventHandler DomEventHandlerMouseOver = null;
        DomEventHandler DomEventHandlerMouseClick = null;
        ObservableList<DeviceEmulation> DES =DeviceEmulation.DevicelistCombo();

        public InternalBrowserWindow(BusinessFlow Biz)
        {
            InitializeComponent();
            mBusinessFlow = Biz;

            lstActivities.DisplayMemberPath = "ActivityName";
            lstActivities.SelectedValuePath = "Guid";

            if (mBusinessFlow!= null)
            {
                lstActivities.ItemsSource = mBusinessFlow.Activities;
                // Select the first Acitivity
                if (mBusinessFlow.Activities!= null &&  mBusinessFlow.Activities.Count > 0)
                {
                    lstActivities.SelectedItem = lstActivities.Items[0];
                }
                //Hook When Biz Flow current Activity changes
                mBusinessFlow.PropertyChanged += BizFlowPropChanges;
            }                

            WBP = new WebBrowserPage();
            frmBrowser.Content = WBP;            
            browser = WBP.GetBrowser();

            browser.Navigated += browser_Navigated;
            browser.Navigating += browser_Navigating;
            browser.LoadCompleted += browser_LoadCompleted;
          
            RoutedCommand OpenEmenu = new RoutedCommand();
            OpenEmenu.InputGestures.Add(new KeyGesture(Key.E, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(OpenEmenu, OpenEmenuByKey));


            General.FillComboFromEnumObj(LocateByComboBox, ElemLocator);
            General.FillComboFromEnumObj(ActionCombotBox, ElemAction);
          
            foreach (DeviceEmulation DE in DES)
            {
                DeviceComboBox.Items.Add(DE.Devicename);
            }
        }
      
        private void BizFlowPropChanges(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //TODO: use const field string
            if (e.PropertyName == "CurrentActivity")
            {
               // lstActivities.SelectedItem = mBusinessFlow.CurrentActivity;
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                browser.GoBack();
            }
            catch (Exception ex) { Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}"); }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
            browser.Refresh();
            }
            catch (Exception ex) { Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}"); }
        }

        private void btnGotoURL_Click(object sender, RoutedEventArgs e)
        {
            DoGoToURL();
        }

        private void DoGoToURL()
        {
            try
            {
                //add http:// when no protocol is present
                if ((txtURL.Text.StartsWith("http://") == false) && (txtURL.Text.StartsWith("https://") == false) && (txtURL.Text.StartsWith("file://") == false))
                {
                    // Add http only if it is not local mht file
                    if (!txtURL.Text.EndsWith(".mht"))
                    {
                        txtURL.Text = "http://" + txtURL.Text;
                    }
                }

                if (btnRecord.IsChecked == true)
                {
                    AddGoToURLAction();
                }
                if (Useragent != null)
                {
                    browser.Navigate(txtURL.Text, null, null, Useragent);
                }
                else{
                    browser.Navigate(txtURL.Text);
                }
                WaitWhileBrowserBusy();
                
            }
            catch (Exception ex)
            {
                //TODO: show Bad URL...
                //Replacing msgbox with Reporter.ToUser
                Reporter.ToUser(eUserMsgKeys.GoToUrlFailure, txtURL.Text, ex.Message);
                //End
            }
        }

        private void btnRecord_Click(object sender, RoutedEventArgs e)
        {
            if (btnRecord.IsChecked == true)
            {                
                DoHookings();                
            }
        }

        private void AddGoToURLAction()
        {
            ActGotoURL a = new ActGotoURL();
            a.LocateBy = eLocateBy.NA;
            a.AddOrUpdateInputParamValue("Value", txtURL.Text);
            a.Description = "Goto App URL - " + txtURL.Text;
            mBusinessFlow.AddAct(a);
        }

        public void DoHookings()
        {           
            //Do based on recording /marker etc..                
           if (mDocument != null)
            {
                HookInputElementsEvents();
                HookLinksEvents();
            }
        }
                
        protected void HookLinksEvents()
        {
            // We want to capture all Links click for recording         
            IHTMLElementCollection elements = mDocument.links;
            DomEventHandler h=null;
            foreach (mshtml.IHTMLElement el in elements)
            {
                    string sel = el.GetType().ToString();
                     h = new DomEventHandler(delegate {
                         if (btnRecord.IsChecked != true)                         
                             ((mshtml.HTMLAnchorElement)el).detachEvent("onclick", h);                                                      
                         else
                             OnLinkClicked(el, EventArgs.Empty);
                         ((mshtml.HTMLAnchorElement)el).detachEvent("onclick", h);                                 
                    });
                    if (sel == "mshtml.HTMLAnchorElementClass")
                    {
                      
                        ((mshtml.HTMLAnchorElement)el).attachEvent("onclick", h);     
                    }
                    else if (sel == "mshtml.HTMLLinkElementClass")
                    {
                      
                        ((mshtml.HTMLLinkElement)el).attachEvent("onclick", h);
                    }
                    else
                    {
                        if (sel == "mshtml.HTMLAreaElementClass")
                        {
                      
                            ((mshtml.HTMLAreaElement)el).attachEvent("onclick", h);
                        }                       
                    }
            }
        }
      
        protected void HookMouseOverForEmenu()
        {
            // Hook all input tags: buttons, text etc - user input 
                IHTMLElementCollection elements = mDocument.getElementsByTagName("input");
                DomEventHandler h = null;
               
                //Hook Text Box and input elems - on Hover
                foreach (mshtml.HTMLInputElement el in elements)
                {
                    h = new DomEventHandler(delegate
                    {
                        OnElemHover(el, EventArgs.Empty);

                    });
                    el.attachEvent("onmouseover", h);
                }

                elements = mDocument.getElementsByTagName("label");
                foreach (mshtml.HTMLLabelElement el in elements)
                {
                    h = new DomEventHandler(delegate
                    {

                        OnElemHover(el, EventArgs.Empty);
                    });
                    el.attachEvent("onmouseover", h);
                }

                // Hook Drop Downs = Select
                elements = mDocument.getElementsByTagName("select");
                foreach (mshtml.HTMLSelectElement el in elements)
                {
                    h = new DomEventHandler(delegate
                    {

                        OnElemHover(el, EventArgs.Empty);
                    });
                    el.attachEvent("onmouseover", h);
                }

                // Hook Drop Downs = Select
                elements = mDocument.getElementsByTagName("a");
                foreach (mshtml.HTMLAnchorElement el in elements)
                {
                    h = new DomEventHandler(delegate
                    {

                        OnElemHover(el, EventArgs.Empty);
                    });
                    el.attachEvent("onmouseover", h);
                }

                //  Hook readonly elements
                string[] tags ={
                               "h1",
                               "h2",
                               "h3",
                               "h4",
                               "h5",
                               "h6",                               
                               "p",
                               "span",
                               "li",                        
                              "div"
                              //"b"
                       
                          };
                foreach (string tag in tags)
                {
                    elements = mDocument.getElementsByTagName(tag);
                    foreach (mshtml.HTMLGenericElement el in elements)
                    {

                        switch (tag)
                        {
                            case "h1":
                            case "h2":
                            case "h3":
                            case "h4":
                            case "h5":
                            case "h6":

                                if (((mshtml.HTMLHeaderElement)el).innerHTML != null && ((mshtml.HTMLHeaderElement)el).innerHTML.IndexOf("<") < 0 && ((mshtml.HTMLHeaderElement)el).innerText != null)
                                {
                                    h = new DomEventHandler(delegate
                                    {

                                        OnElemHover(el, EventArgs.Empty);
                                    });
                                    el.attachEvent("onmouseover", h);
                                }
                                break;
                            case "p":
                                if (((mshtml.HTMLParaElement)el).innerHTML != null && ((mshtml.HTMLParaElement)el).innerHTML.IndexOf("<") < 0 && ((mshtml.HTMLParaElement)el).innerText != null)
                                {
                                    h = new DomEventHandler(delegate
                                    {

                                        OnElemHover(el, EventArgs.Empty);
                                    });
                                    el.attachEvent("onmouseover", h);
                                }

                                break;
                            case "span":
                                if (((mshtml.HTMLSpanElement)el).innerHTML != null && ((mshtml.HTMLSpanElement)el).innerHTML.IndexOf("<") < 0 && ((mshtml.HTMLSpanElement)el).innerText != null)
                                {
                                    h = new DomEventHandler(delegate
                                    {

                                        OnElemHover(el, EventArgs.Empty);
                                    });
                                    el.attachEvent("onmouseover", h);
                                }
                                break;
                            case "li":
                                if (((mshtml.HTMLLIElement)el).innerHTML != null && ((mshtml.HTMLLIElement)el).innerHTML.IndexOf("<") < 0 && ((mshtml.HTMLLIElement)el).innerText != null)
                                {
                                    h = new DomEventHandler(delegate
                                    {

                                        OnElemHover(el, EventArgs.Empty);
                                    });
                                    el.attachEvent("onmouseover", h);
                                }
                                break;
                            case "div":
                                mshtml.HTMLDivElement div = (mshtml.HTMLDivElement)el;
                                if (div.innerText != null && div.innerHTML != null && div.innerHTML.IndexOf("<") < 0)
                                {
                                    h = new DomEventHandler(delegate
                                    {

                                        OnElemHover(el, EventArgs.Empty);
                                    });
                                    el.attachEvent("onmouseover", h);
                                }
                                break;
                        }
                    }
                }
        }

        protected void HookInputElementsEvents()
        {
            // We want to capture all controls of type "input"
            mDocument = (mshtml.HTMLDocument)browser.Document;
            DomEventHandler h = null;
            if (mDocument == null) return;
            IHTMLElementCollection elements = (IHTMLElementCollection)mDocument.getElementsByTagName("select");
            foreach (mshtml.HTMLSelectElement el in elements)
            {                
                    h = new DomEventHandler(delegate
                    {
                        if (btnRecord.IsChecked != true)                        
                            ((mshtml.HTMLSelectElement)el).detachEvent("onchange", h);                                                    
                        else
                            OnListSelected(el, EventArgs.Empty);
                        ((mshtml.HTMLSelectElement)el).detachEvent("onchange", h);                               
                    });
                 
                    el.attachEvent("onchange", h);
             
            }
            elements = (IHTMLElementCollection)mDocument.getElementsByTagName("span");

            foreach (mshtml.HTMLSpanElement el in elements)
            {
                if (((mshtml.HTMLSpanElement)el).innerHTML != null && ((mshtml.HTMLSpanElement)el).innerHTML.IndexOf("<") < 0 && ((mshtml.HTMLSpanElement)el).innerText != null)
                {
                    h = new DomEventHandler(delegate
                    {
                        if (btnRecord.IsChecked != true)                        
                            ((mshtml.HTMLSpanElement)el).detachEvent("onclick", h);                                                   
                        else
                            OnElementClicked(el, EventArgs.Empty);
                        ((mshtml.HTMLSpanElement)el).detachEvent("onclick", h);                                  
                    });
               
                    el.attachEvent("onclick", h);
                }
            }
            elements = (IHTMLElementCollection)mDocument.getElementsByTagName("div");

            foreach (mshtml.HTMLDivElement el in elements)
            {
                mshtml.HTMLDivElement div = ((mshtml.HTMLDivElement)el);
                try
                {
                    var c = div.innerHTML;
                    if (div.innerHTML != null && div.innerHTML.IndexOf("<") < 0 && div.innerText != null)
                    {
                        h = new DomEventHandler(delegate
                        {
                            if (btnRecord.IsChecked != true)
                                ((mshtml.HTMLDivElement)el).detachEvent("onclick", h);
                            else
                                OnDivClicked(el, EventArgs.Empty);
                            ((mshtml.HTMLDivElement)el).detachEvent("onclick", h);
                        });

                        el.attachEvent("onclick", h);
                    }
                }
                catch
                {
                    //do nothing
                }
            }
            elements = (IHTMLElementCollection)mDocument.getElementsByTagName("button");

            foreach (mshtml.HTMLButtonElement el in elements)
            {
                    h = new DomEventHandler(delegate
                    {
                        if (btnRecord.IsChecked != true)
                          ((mshtml.HTMLButtonElement)el).detachEvent("onclick", h);
                        else
                            OnBtnClicked(el, EventArgs.Empty);
                        ((mshtml.HTMLButtonElement)el).detachEvent("onclick", h);                               
                    });
                    el.attachEvent("onclick", h);
            }
            elements = (IHTMLElementCollection)mDocument.getElementsByTagName("input");

            foreach (mshtml.HTMLInputElement el in elements)
            { 
                string elType = el.getAttribute("type");
                switch (elType)
                {
                    case "radio":
                    case "checkbox":
                    case "button":
                        {
                            // Capture button Click     
                             h = new DomEventHandler(delegate {
                                if (btnRecord.IsChecked != true)                                
                                    ((mshtml.HTMLInputElement)el).detachEvent("onclick", h);                                
                                else
                                    OnButtonClicked(el, EventArgs.Empty);
                                ((mshtml.HTMLInputElement)el).detachEvent("onclick", h);                                
                             });
                    
                            el.attachEvent("onclick", h);
                            
                            break;
                        }
                    case "submit":
                        {
                             h = new DomEventHandler(delegate {
                                if (btnRecord.IsChecked != true)                                
                                    ((mshtml.HTMLInputElement)el).detachEvent("onclick", h);                                                                    
                                else
                                    OnSubmitClicked(el, EventArgs.Empty);
                                ((mshtml.HTMLInputElement)el).detachEvent("onclick", h);                                
                            });
                     
                            el.attachEvent("onclick", h);
                            break;
                        }
                    case "password":
                    case "search":
                    // same as text
                    case "text":
                        {                           
                             h = new DomEventHandler(delegate {
                                 if (btnRecord.IsChecked != true)                                 
                                     ((mshtml.HTMLInputElement)el).detachEvent("onblur", h);                                 
                                 else                                     
                                     OnElementLostFocus(el, EventArgs.Empty);
                                 ((mshtml.HTMLInputElement)el).detachEvent("onblur", h);                                  
                             });
                       
                            el.attachEvent("onblur", h);

                            break;
                        }
                    case "datetime":
                        {
                            // Capture Text Blur                            
                            // el.AttachEventHandler("onblur", (sender, args) => OnElementLostFocus(el, EventArgs.Empty));
                            break;
                        }
                }
            }
        }
      
        //TODO: move to seperate class
        [ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
        public class DomEventHandler
        {
            [ComVisible(false)]

            public delegate void Callback(ref object result, object[] args);

            [ComVisible(false)]
            private Callback _callback;


            [DispId(0)]
            public object Method(params object[] args)
            {
                var result = Type.Missing; 
                _callback(ref result, args);
                return result;
            }

            public DomEventHandler(Callback callback)
            {
                _callback = callback;
            }
        }

        protected void OnElemHover(object sender, EventArgs args)
        {            
            mshtml.IHTMLElement el = (mshtml.IHTMLElement)sender;
            ShowElementMenu(el);

            txtOuterHTML.Text = el.outerHTML;
                       
            RoutedCommand AddValidation1 = new RoutedCommand();
            AddValidation1.InputGestures.Add(new KeyGesture(Key.D1, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(AddValidation1, btnAction1_Click));
                       
            RoutedCommand AddValidation2 = new RoutedCommand();
            AddValidation2.InputGestures.Add(new KeyGesture(Key.D2, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(AddValidation2, btnAction2_Click));
                        
            RoutedCommand AddAction = new RoutedCommand();
            AddAction.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(AddAction, btnAddAction_Click));     
        }

        internal void ShowElementMenu(mshtml.IHTMLElement e)
        {
            string elType = e.tagName;
            string elName="";
            try{

                if (e.getAttribute("type").Trim() != "")
                    elName = e.getAttribute("type").Trim();
                else if (e.getAttribute("Value").Trim()!="")
                    elName = e.getAttribute("Value");
                else if (e.getAttribute("Name").Trim()!="")
                    elName = e.getAttribute("Name");
            }catch(Exception ex)
            { Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}"); }
            string elValue = (e.innerText == null) ? elName : e.innerText; 
            eTagName.Content = elType + "-" + elValue;
            doElemMenu(e);
            ShowEmenu(e);
        }

        private void ShowEmenu(mshtml.IHTMLElement e)
        {
            System.Drawing.Rectangle rect = GetPosByMouse(e); 
            //To avoid eMenu covers the small elements
            rect.X += 101;
            
            Thickness marginThickness = new Thickness(); 
            marginThickness.Top = rect.Top ; 
            marginThickness.Left = rect.Left - 100;
            bdrEmenu.Margin = marginThickness;
        }

        private void doElemMenu(mshtml.IHTMLElement e)
        {
             string elTag = e.tagName;

            //TODO: impl all other tags
            switch (elTag.ToUpper())
            {
                case "INPUT":
                    CreateInputValidations((mshtml.HTMLInputElement)e);
                    
                    CreateInputAction((mshtml.HTMLInputElement)e);
                    break;
                case "LABEL":
                    CreateLabelValidations((mshtml.HTMLLabelElement)e);
                    break;
                case "SELECT":
                    CreateSelectValidations((mshtml.HTMLSelectElement)e);
                    CreateSelectAction((mshtml.HTMLSelectElement)e);
                    break;
                case "A":
                    CreateLinkValidations((mshtml.HTMLAnchorElement)e);
                    CreateLinkAction((mshtml.HTMLAnchorElement)e);
                    break;
                default:
                    CreateGenericValidations(e);
                    break;
            }
        }
        
        private void SetElementLocator(mshtml.IHTMLElement el, out eLocateBy LocateBy, out string LocateValue)
        {
            LocateBy = eLocateBy.NA;
            LocateValue="";

            //By ID
            if (el.id != null)
            {
                LocateBy = eLocateBy.ByID;
                LocateValue = el.id;
                return;
            }

            //By name            
            string name = GetAttrValue(el,"name");
                          
            if (name !=null)
            {             
                LocateBy = eLocateBy.ByName;
                LocateValue = name;
                return;
            }


            string href = GetAttrValue(el, "href");
            if (href != null && href.IndexOf("void(0)") < 0)
            {
                // mshtml.HTMLAnchorElement ela = (mshtml.HTMLAnchorElement)el;    
                LocateBy = eLocateBy.ByHref;
                //for HREF we use path name since it is partial url, the full HREF conatins http://... which is not the href in the html, this way we can also move env with no change
                LocateValue = GetRealHREFfromOuterHTML(el.outerHTML); 
                return;
            }
            else if (href != null && href.IndexOf("void(0)") >= 0)
            {
                if (el.innerText != null)
                {
                    LocateBy = eLocateBy.ByLinkText;
                    LocateValue = el.innerText;
                    return;
                }
            }
            
            string elvalue = GetAttrValue(el, "value");
            if (elvalue != null)
            {
                LocateBy = eLocateBy.ByValue;
                LocateValue = elvalue;
                return;
            }

            if (el.innerText != null)
            {
                LocateBy = eLocateBy.ByLinkText;
                LocateValue = el.innerText;
                return;
            }
            
            // Last is goto by XPath
            LocateBy = eLocateBy.ByXPath;
            LocateValue = WBP.GetElementXPath(CurrentHighlightedElement);

            //TODO: find way to get CSS at least the simpe one like "input[type=search]"
            // LocateValue = "CSS TODO:";
            return;
        }

        private string GetRealHREFfromOuterHTML(string OuterHTML)
        {
            string href ="";
            //make sure we have href
            int i = OuterHTML.IndexOf("href");
            if (i>0)
            {
                // search for first "
                int i2 = OuterHTML.IndexOf("\"", i);
                int i3 = OuterHTML.IndexOf("\"", i2+1);

                if (i2>0 && i3 > 0)
                {
                    href = OuterHTML.Substring(i2+1, i3 - i2 - 1);
                }
            }
            return href;
        }

        private string GetAttrValue(mshtml.IHTMLElement el, string attr)
        {
            dynamic value = el.getAttribute(attr);

            if (value==null) return null;
            var s = DBNull.Value.Equals(value) ? null : (string) value.ToString();
            return s == string.Empty ? null : s;
        }

        private void SetActLocator(Act a, mshtml.IHTMLElement el)
        {
            eLocateBy LocateBy;
            string LocateValue;
            SetElementLocator(el, out LocateBy, out LocateValue);
            a.LocateBy = LocateBy;
            a.LocateValue = LocateValue;
        }
        
        public System.Drawing.Rectangle GetAbsoluteRectangle(HtmlElement element)
        {
            //get initial rectangle
            System.Drawing.Rectangle rect = element.OffsetRectangle;

            //update with all parents' positions
            HtmlElement currParent = element.OffsetParent;
            while (currParent != null)
            {
                rect.Offset(currParent.OffsetRectangle.Left, currParent.OffsetRectangle.Top);
                currParent = currParent.OffsetParent;
            }
            return rect;
        }

        internal void HighLight(mshtml.IHTMLElement e)
        {
            System.Drawing.Rectangle rect = GetAbsoluteRectangle(e);
            Thickness marginThickness = new Thickness
            {
                Top = rect.Top + 120,
                Left = rect.Left - 60
            };

            bdrOverLay.Margin = marginThickness;
            puOverlay.Visibility = System.Windows.Visibility.Visible;
        }

        public System.Drawing.Rectangle GetAbsoluteRectangle(mshtml.IHTMLElement element)
        {
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(element.offsetLeft, element.offsetTop, element.offsetWidth, element.offsetHeight);
            //update with all parents' positions
            mshtml.IHTMLElement currParent = element.offsetParent;
            while (currParent != null)
            {
            rect.Offset(currParent.offsetLeft, currParent.offsetTop);
                currParent = currParent.offsetParent;
            }
            return rect;
        }

        public System.Drawing.Rectangle GetPosByMouse(mshtml.IHTMLElement element)
        {
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(element.offsetLeft, element.offsetTop, element.offsetWidth, element.offsetHeight);
            System.Drawing.Point point = System.Windows.Forms.Control.MousePosition;
               rect.X = point.X;
                rect.Y = point.Y;
            return rect;
        }

        protected void OnElementLostFocus(object sender, EventArgs args)
        {
            mshtml.HTMLInputTextElement el = (mshtml.HTMLInputTextElement)sender;     
            string elValue = el.value;
            ActTextBox a = new ActTextBox();
            SetActLocator(a, (mshtml.IHTMLElement)el);
            a.TextBoxAction = ActTextBox.eTextBoxAction.SetValue;
            a.AddOrUpdateInputParamValue("Value",elValue);
            a.Description = "Enter value in " + a.LocateValue;
            mBusinessFlow.AddAct(a);    
        }

        protected void OnElementClicked(object sender, EventArgs args)
        {
            mshtml.HTMLSpanElement el = (mshtml.HTMLSpanElement)sender;      
            string elValue = el.innerText;
            //TODO: show user the action going on

            ActGenElement a = new ActGenElement();
            SetActLocator(a, (mshtml.IHTMLElement)el);
            a.GenElementAction= ActGenElement.eGenElementAction.Click;
            a.Description = "Click  Webelement " + elValue;

            mBusinessFlow.AddAct(a);
        }

        protected void OnDivClicked(object sender, EventArgs args)
        {
            mshtml.HTMLDivElement el = (mshtml.HTMLDivElement)sender;    
            string elValue = el.innerText;

            //TODO: show user the action going on

            ActGenElement a = new ActGenElement();
            SetActLocator(a, (mshtml.IHTMLElement)el);
            a.GenElementAction = ActGenElement.eGenElementAction.Click;
            a.Description = "Click  Webelement " + elValue;

            mBusinessFlow.AddAct(a);
        }

        protected void OnBtnClicked(object sender, EventArgs args)
        {
            mshtml.HTMLButtonElement el = (mshtml.HTMLButtonElement)sender;        
            string elValue = el.innerText;

            //TODO: show user the action going on

            ActButton a = new ActButton();
            SetActLocator(a, (mshtml.IHTMLElement)el);
            a.ButtonAction = ActButton.eButtonAction.Click;
            a.Description = "Click Button " + elValue;

            mBusinessFlow.AddAct(a);
        }

        protected void OnButtonClicked(object sender, EventArgs args)
        {
            mshtml.HTMLInputTextElement el = (mshtml.HTMLInputTextElement)sender;
            string elValue = el.value;

            //TODO: show user the action going on

            ActButton a = new ActButton();
            SetActLocator(a, (mshtml.IHTMLElement)el);
            a.ButtonAction = ActButton.eButtonAction.Click;
            a.Description = "Click Button " + elValue;

            mBusinessFlow.AddAct(a);            
        }

        protected void OnSubmitClicked(object sender, EventArgs args)
        {
            mshtml.HTMLInputTextElement el = (mshtml.HTMLInputTextElement)sender;            
            IHTMLFormElement f = el.form;
            
            ActSubmit a = new ActSubmit();
            
            SetActLocator(a, (IHTMLElement)el);

            a.Description = "Submit Page - " + el.id;
            mBusinessFlow.AddAct(a);            
        }

        protected void OnListSelected(object sender, EventArgs args)
        {
            bool reset = true;
            mshtml.HTMLSelectElement sel = (mshtml.HTMLSelectElement)sender;
            mshtml.HTMLElementCollection options = (mshtml.HTMLElementCollection)sel.options;
            mshtml.HTMLOptionElement option;
            string lElemnts = null;

            for (int i = 0; i < options.length; i++)
            {
                option = (mshtml.HTMLOptionElement)options.item(i, null);
                if (option.selected)
                {
                    ActMultiselectList a = new ActMultiselectList();
                    SetActLocator(a, (mshtml.IHTMLElement)sel);
                    if (reset)
                    {
                        ActMultiselectList b = new ActMultiselectList();
                        SetActLocator(b, (mshtml.IHTMLElement)sel);
                        b.AddOrUpdateInputParamValue("Value", i.ToString());
                        b.Description = "Clearing previous selections";
                        b.ActMultiselectListAction = ActMultiselectList.eActMultiselectListAction.ClearAllSelectedValues;
                        mBusinessFlow.AddAct(b);
                        reset = false;
                        Console.WriteLine("Clear previous selections ");
                    }
                    lElemnts = lElemnts + i.ToString() + ",";
                    a.Description = "Select #" + lElemnts + " choice from multiselect list";
                    a.AddOrUpdateInputParamValue("Value", i.ToString());
                    a.ActMultiselectListAction = ActMultiselectList.eActMultiselectListAction.SetSelectedValueByIndex;
                    mBusinessFlow.AddAct(a);
                }
            }
        }

        protected void OnBodyClicked(object sender, EventArgs args)
        {
            int newelcount=((mshtml.HTMLDocument)sender).all.length;
            if(newelcount!=elcount)
            {
                if (btnRecord.IsChecked==true)
                     DoHookings();
                elcount = newelcount;
            }
        }

        protected void OnLinkClicked(object sender, EventArgs args)
        {
            mshtml.HTMLAnchorElement el = (mshtml.HTMLAnchorElement)sender;                        
            string eText = el.innerText;
            ActLink a = new ActLink();
            SetActLocator(a, (mshtml.IHTMLElement)el);            
            a.Description = "Click Link: " + eText;            
            a.LinkAction = ActLink.eLinkAction.Click;
            mBusinessFlow.AddAct(a);            
        }

        private const string DisableScriptError =
            @"function noError() {
                return true;
            }
            window.onerror = noError;";
        
        public void HideJsScriptErrors(System.Windows.Controls.WebBrowser wb)
        {
            // IWebBrowser2 interface
            // Exposes methods that are implemented by the WebBrowser control  
            // Searches for the specified field, using the specified binding constraints.
            FieldInfo fld = typeof(System.Windows.Controls.WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fld == null)
                return;
            object obj = fld.GetValue(wb);
            if (obj == null)
                return;
            // Silent: Sets or gets a value that indicates whether the object can display dialog boxes.
            obj.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, obj, new object[] { true });
        }

        private void browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                mDocument = (mshtml.HTMLDocument)browser.Document;            
            }
            catch (System.InvalidCastException eICE)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {eICE.Message}");
            }
            txtURL.Text = mDocument.url;
            HideJsScriptErrors(browser);
        }

        private void SetStatus(string txt)
        {
            lblStatus.Content = txt;
        }

        private void browser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            mBusy = true;            
            UnLightCurrentHighlightedElement();
            SetStatus("Loading - " + e.Uri);           
        }

        private void btnEmenu_Click(object sender, RoutedEventArgs e)
        {
            ShowHideEmenu();            
        }

        private void ShowHideEmenu()
        {
            if (btnEmenu.IsChecked == true)
            {
                puEmenu.IsOpen = true;
                HookMouseOverForEmenu();                         
            }
            else
            {
                puEmenu.IsOpen = false;
                //TODO: un hook mouse over
            }
        }

        private void btnPin_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = btnPin.IsChecked == true;
        }
        
        private void browser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                mDocument = (mshtml.HTMLDocument)browser.Document;
            }
            catch(System.InvalidCastException eICE)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {eICE.Message}");
            }

            if (btnMarker.IsChecked == true) SetDocMouseOver();

            if (btnRecord.IsChecked == true)
            {
                DomEventHandler h = null;
                h = new DomEventHandler(delegate
                {
                    if (btnRecord.IsChecked != true)
                        mDocument.detachEvent("onfocusin", h);
                    else
                        OnBodyClicked(mDocument, EventArgs.Empty);
                    mDocument.detachEvent("onfocusin", h);
                });                
                mDocument.attachEvent("onfocusin", h);
            }
            else
            {
                //TODO: un hook recording - not needed as new page shoudl not have hooks
            }

            if (btnEmenu.IsChecked == true)
            {
                Dispatcher.BeginInvoke(new Action(() => HookMouseOverForEmenu()), DispatcherPriority.SystemIdle, null);
            }
            
            SetStatus("Ready");
            mBusy = false;                
        }

        private void SetDocMouseOver()
        {                  
            DomEventHandlerMouseOver = new DomEventHandler(delegate
            {                
                mshtml.IHTMLElement elem = mDocument.parentWindow.@event.srcElement;
                HighLightElement(elem);
                ShowCurrentElementInfoAndLocator();                
            });
            mDocument.attachEvent("onmouseover", DomEventHandlerMouseOver);
        }

        private void SetDocMouseClick()
        {
            DomEventHandlerMouseClick = new DomEventHandler(delegate
            {                
                mshtml.IHTMLElement elem = mDocument.parentWindow.@event.srcElement;
                UnHookMarker();
                btnMarker.IsChecked = false;
                
            });
            mDocument.attachEvent("onclick", DomEventHandlerMouseClick);

        }

        private void ShowCurrentElementInfoAndLocator()
        {
            txtOuterHTML.Text = CurrentHighlightedElement.outerHTML;

            eLocateBy LocType;
            string LocValue;
            SetElementLocator(CurrentHighlightedElement, out LocType,out LocValue);
            LocateByComboBox.SelectedValue = LocType.ToString();
            LocateValueTextBox.Text = LocValue;
        }

        private void HighLightElement(mshtml.IHTMLElement elem)
        {            
            UnLightCurrentHighlightedElement();
            CurrentHighlightedElement = elem;
            if (elem != null)
            {
                elem.style.setAttribute("border", "solid 1px #ff0000");
            }
        }

        private void UnLightCurrentHighlightedElement()
        {
            if (CurrentHighlightedElement != null)
            {
                CurrentHighlightedElement.style.setAttribute("border", "solid 0px #000000");
            }                
        }

        public void WaitWhileBrowserBusy()
        {                        
            //TODO: wait for browser to be ready - add timeout
            while (mBusy)
            {
                DispatcherFrame frame = new DispatcherFrame();
                browser.Dispatcher.BeginInvoke( DispatcherPriority.Background,new DispatcherOperationCallback(ExitFrame), frame);
                Dispatcher.PushFrame(frame);
            }
        }
        
        public object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;
            return null;
        }


        public Bitmap GetScreenShot()
        {
            this.Topmost = true;
            GingerCore.General.DoEvents();
            var topLeftCorner = frmBrowser.PointToScreen(new System.Windows.Point(0, 0));
            var topLeftGdiPoint = new System.Drawing.Point((int)topLeftCorner.X, (int)topLeftCorner.Y);
            var size = new System.Drawing.Size((int)frmBrowser.ActualWidth, (int)frmBrowser.ActualHeight);

            var screenShot = new Bitmap((int)frmBrowser.ActualWidth, (int)frmBrowser.ActualHeight);

            using (var graphics = Graphics.FromImage(screenShot))
            {
                graphics.CopyFromScreen(topLeftGdiPoint, new System.Drawing.Point(),
                    size, CopyPixelOperation.SourceCopy);
            }
            this.Topmost = false;
            return screenShot;
        }

        internal void AddScreenShot(Act act)
        {
            Bitmap tempBmp = GetScreenShot();
            act.AddScreenShot(tempBmp);
        }

        internal void GotoURL(string p)
        {
            if (!p.StartsWith("http://") && !p.StartsWith("https://") && !p.StartsWith(@"C:\"))
            {
               //Don't do anything if URL provided by user doesn't have protocol (e.g., www.google.com) since Ginger will try again afterwards using "http://" 
                p = "http://" + p;
            }
            mBusy = true;            
            browser.Navigate(p);  
        }

        internal void SideBySide()
        {
            this.WindowState = System.Windows.WindowState.Normal;
            this.Width = Screen.PrimaryScreen.WorkingArea.Width * 0.70;
            this.Top = 0;
            this.Left = Screen.PrimaryScreen.WorkingArea.Width * 0.30;
            this.Height = Screen.PrimaryScreen.WorkingArea.Height;
        }

        internal IHTMLElement TryGetActElementByLocator(Act act,bool alwaysReturn=false)
        {
            if (act == null) return null;
            try
            {                
                IHTMLElement e= TryGetActElementByLocator(act, act.LocateBy, act.LocateValueCalculated);
                if (e == null)
                {
                    act.Error = "Element not found - " + act.LocateBy + " - " + act.LocateValueCalculated + Environment.NewLine;
                }
                return e;
            }
            catch (Exception e)
            {
                if (alwaysReturn)
                {
                    return null;
                }
                else
                {
                    act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                    act.Error = "Element not found - " + act.LocateBy + " - " + act.LocateValueCalculated + Environment.NewLine + e.Message;
                    return null;
                }
            }
        }

        public void HighLightActElement(Act act)
        {            
            IHTMLElement e = TryGetActElementByLocator(act, act.LocateBy, act.LocateValueCalculated);
            if (e != null)
            {
                HighLightElement(e);
            }
        }

        /// <summary>
        /// Return the 1st element from an IHTMLElement collection that has a matching attribute (e.g., target="_blank")
        /// </summary>
        /// <param name="elementCollection"></param>
        /// <param name="A"></param>
        /// <param name="LocateBy"></param>
        /// <param name="LocateValue"></param>
        /// <param name="AttributeLabel"></param>
        /// <returns></returns>
        private IHTMLElement getElementFromCollectionByAttribute(IHTMLElementCollection elementCollection, Act A,
            eLocateBy LocateBy, string LocateValue, string AttributeLabel)
        {       
            // This steps through all elements in the collection passed and returns 
            // the first one that has a matching attribute/value pair.
            foreach (mshtml.IHTMLElement el in elementCollection)
            {
                IHTMLElement l = (IHTMLElement)el;
                try
                {
                    string val = null;
                    if (AttributeLabel == "href")
                    {
                        val = GetRealHREFfromOuterHTML(l.outerHTML);
                    }
                    else
                    {                                            
                        val = l.getAttribute(AttributeLabel);
                    }
                    if (!string.IsNullOrEmpty(val))
                    {                                            
                        if (val == LocateValue)
                        {
                            return (IHTMLElement)el;
                        }
                    }
                }
                catch (Exception e)
                {
                    A.Error = e.ToString() + Environment.NewLine ;
                }
            }
            return null;
        }

        private IHTMLElement getElementFromCollectionByInnerText(IHTMLElementCollection elementCollection, Act A,
            eLocateBy LocateBy, string LocateValue)
        {
            //This is a list of tags that we don't want to look at when when stepping through the DOM looking for matches.
            List<string> tagsToBeIgnoredWhenMatching = new List<string>(new string[] { "section", "head","table", "td", "tr", "ul", "form", "ol" });
            
            Regex reg = new Regex(LocateValue.Replace("{RE:", "").Replace("}", ""), RegexOptions.Compiled);
            IHTMLElementCollection elementsById = elementCollection;
            int iElementsMatchedById = 0;
            IHTMLElement currentElementById = null;
            // trim any white space from locator
            if (!String.IsNullOrEmpty(LocateValue)) { LocateValue=LocateValue.Trim(); }
            // now find it

            foreach (mshtml.IHTMLElement el in elementsById)
            {                 
                IHTMLElement l = (IHTMLElement)el;
                if (String.IsNullOrEmpty(l.innerText)) 
                {
                    continue; 
                }
                else 
                {
                    if (               
                        ((l.innerText.Trim() == LocateValue)) &&
                        tagsToBeIgnoredWhenMatching.FindAll(s => String.Compare(s,l.tagName,true)==0).Count<=0              
                        )
                    {
                        switch (A.ActClass)
                        {
                            case "GingerCore.Actions.ActLink":
                                if (el.GetType().ToString() != "mshtml.HTMLAnchorElementClass"
                                    &&
                                    el.GetType().ToString() != "mshtml.HTMLLinkElementClass")
                                    continue;
                                break;
                            case "GingerCore.Actions.ActLabel":
                                if (el.GetType().ToString() != "mshtml.HTMLLabelElementClass")
                                    continue;
                                break;

                        }
                        iElementsMatchedById++;
                        currentElementById = (IHTMLElement)el;

                        //Go out whenever first element found
                        //TODO: add option of warning when more than one, however for speed we go on the first
                        return ((IHTMLElement)currentElementById);
                    }
                }             
            }
            if (iElementsMatchedById > 0)
            {
                if (iElementsMatchedById > 1)
                {
                    string s = "\"" + LocateValue + "\" using \"" + LocateBy.ToString() + "\" matches " +
                        iElementsMatchedById.ToString() + " elements." + Environment.NewLine;

                    //TODO: add Action Warning new field= s;
                }
                else //  just 1 match
                {
                    return ((IHTMLElement)currentElementById);
                }
            }
            return null;
        }

        private IHTMLElement getElementsByIdReg(string ElementType,string sLocVal)
        {
            if (sLocVal == null) return null;

            if (sLocVal.IndexOf("{RE:")<0)
                return mDocument.getElementById(sLocVal);

            Regex reg=new Regex(sLocVal.Replace("{RE:","").Replace("}",""),RegexOptions.Compiled);
            
            IHTMLElementCollection el = mDocument.all;
            foreach (mshtml.IHTMLElement e in el)
            {
                switch (ElementType)
                {
                    case "GingerCore.Actions.ActLink":
                        if (e.GetType().ToString() != "mshtml.HTMLAnchorElementClass"
                            &&
                            e.GetType().ToString() != "mshtml.HTMLLinkElementClass")
                        { continue; }
                        else
                        {
                            if (reg.Matches(e.id.ToString()).Count > 0)
                                return e;
                            else
                                continue;
                        }
                        
                    case "GingerCore.Actions.ActCheckbox":
                    case "GingerCore.Actions.ActButton":
                    case "GingerCore.Actions.ActSubmit":
                    case "GingerCore.Actions.ActTextBox":
                    case "GingerCore.Actions.RadioButton":
                        if (e.GetType().ToString() != "mshtml.HTMLInputElementClass")
                        { continue; }
                        else
                        {
                            if (e.id!=null && reg.Matches(e.id.ToString()).Count > 0)
                                return e;
                            else
                                continue;
                        }
                    case "GingerCore.Actions.MultiselectList":
                        if (e.GetType().ToString() != "mshtml.HTMLSelectElementClass")
                        { continue; }
                        else
                        {
                            if (reg.Matches(e.id.ToString()).Count > 0)
                                return e;
                            else
                                continue;
                        }
                }                
            }
            return null;
        }

        private IHTMLElement getElementsByNameReg(string ElementType, string sLocVal)
        {
            if (sLocVal.IndexOf("{RE:") < 0)
                if(mDocument.getElementsByName(sLocVal).length>=0)
                    return mDocument.getElementsByName(sLocVal).item(Type.Missing, 0);
                

            Regex reg = new Regex(sLocVal.Replace("{RE:", "").Replace("}", ""), RegexOptions.Compiled);

            IHTMLElementCollection el = mDocument.all;
            foreach (mshtml.IHTMLElement e in el)
            {
                switch (ElementType)
                {
                    case "GingerCore.Actions.ActLink":
                        if (e.GetType().ToString() != "mshtml.HTMLAnchorElementClass"
                            &&
                            e.GetType().ToString() != "mshtml.HTMLLinkElementClass")
                        { continue; }
                        else
                        {
                            if (reg.Matches(e.getAttribute("Name")).Count > 0)
                                return e;
                            else
                                continue;
                        }

                    case "GingerCore.Actions.ActCheckbox":
                    case "GingerCore.Actions.ActButton":
                    case "GingerCore.Actions.ActSubmit":
                    case "GingerCore.Actions.ActTextBox":
                    case "GingerCore.Actions.RadioButton":
                        if (e.GetType().ToString() != "mshtml.HTMLInputElementClass")
                        { continue; }
                        else
                        {
                            if (reg.Matches(e.getAttribute("Name")).Count > 0)
                                return e;
                            else
                                continue;
                        }
                    case "GingerCore.Actions.MultiselectList":
                        if (e.GetType().ToString() != "mshtml.HTMLSelectElementClass")
                        { continue; }
                        else
                        {
                            if (reg.Matches(e.getAttribute("Name")).Count > 0)
                                return e;
                            else
                                continue;
                        }

                }
            }
            return null;
        }
        internal IHTMLElement TryGetActElementByLocator(Act A, eLocateBy LocateBy, string LocateValue)
        {
            if (String.IsNullOrEmpty(LocateValue)) return null;

            //This is a list of tags that we don't want to look at when when stepping through the DOM looking for matches.
            //List<string> tagsToBeIgnoredWhenMatching = new List<string>(new string[] { "div", "section", "table", "td", "tr", "p", "li", "ul", "form", "ol", "span" });
            mshtml.IHTMLElement e = null;
            Regex reg=null;
            switch (LocateBy)
            {
                case eLocateBy.ByID:
                    e = getElementsByIdReg(A.ActClass,A.LocateValueCalculated);
                    break;

                case eLocateBy.ByName:
                    //IHTMLElementCollection c = mDocument.getElementsByName(A.LocateValue);
                    //if (c.length == 1)
                    //{
                    //    e= c.item(Type.Missing,0);                        
                    //}
                    //else
                    //{
                    //    //TODO: more than one elem found???
                    //}
                    e=getElementsByNameReg(A.ActClass, A.LocateValueCalculated);
                    break;

                case eLocateBy.ByLinkText:
                    e = getElementFromCollectionByInnerText(
                                mDocument.all,
                                A,
                                A.LocateBy,
                                A.LocateValueCalculated);

                    break;

                case eLocateBy.ByValue:
                    
                         var AllDocElems = mDocument.all as IEnumerable;      

                          //TODOL: cureently getting input element, need to handle also other type
                         var inputs = AllDocElems.OfType<mshtml.HTMLInputElement>();
                           //TODO: check performance
                           
                         if (A.LocateValueCalculated.IndexOf("{RE:") < 0)
                             { e = (IHTMLElement)inputs.First(i => i.value == A.LocateValueCalculated); }
                             else
                             {
                                 reg = new Regex(A.LocateValueCalculated.Replace("{RE:", "").Replace("}", ""), RegexOptions.Compiled);
                                 foreach (var el in inputs)
                                 {
                                     if (el.value!=null && reg.Matches(el.value).Count > 0)
                                         return (IHTMLElement)el;

                                 }
                             }
                    break;
                case eLocateBy.ByHref:
                    e = getElementFromCollectionByAttribute(
                                mDocument.links,
                                A,
                                A.LocateBy,
                                A.LocateValueCalculated,
                                "href");
                    break;
                case eLocateBy.ByCSS:
                    //TODO: fixme go by CSS
                    // Last we go by CSS or XPath
                    IHTMLWindow2 w = mDocument.parentWindow;
                    try
                    {
                       // dynamic Velems = null;
                       // string sScript1 = String.Format("alert('{0}')", LocateValue);
                       // w.execScript(sScript1);
                        //mshtml.HTMLDocument dom = (mshtml.HTMLDocument)browser.Document;
                       // dom.q

                        // string sScript = String.Format("document.querySelectorAll('{0}')", LocateValue);                    
                       // string sScript = String.Format("document.querySelectorAll('#body');", LocateValue);
                       // Velems = w.execScript(LocateValue);
                        // dynamic elems = null;
                        return null; // elems[0];
                    }
                    catch (Exception e1)
                    {
                        Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, e1.Message);
                    }
                    return null;
                    //TODO:
                case eLocateBy.ByXPath:
                    e = WBP.GetElementByXPath(LocateValue);                                                                                
                    break;
                case eLocateBy.NA:
                    //Do nothing
                    break;
                default:
                    A.Error = "Locator Not implemented yet - " + LocateBy;
                    break;
            }
            return e;
        }

        private void lstActivities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            mBusinessFlow.CurrentActivity = (Activity)lstActivities.SelectedItem;
            ShowActions(); 
        }
        private void lstActions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mBusinessFlow.CurrentActivity.Acts.CurrentItem = (Act)lstActions.SelectedItem;            
        }

        private void ShowActions()
        {
            //TODO: temp remove me later as it is not good practice
            if (mBusinessFlow.CurrentActivity == null) return;
            lstActions.ItemsSource = mBusinessFlow.CurrentActivity.Acts;
            lstActions.DisplayMemberPath = Act.Fields.Description;         
        }
        
        //TODO: create inputBoxWindow
        private void btnAddActivity_Click(object sender, RoutedEventArgs e)
        {
            string newActivityName=string.Empty;
            if (InputBoxWindow.OpenDialog("New " + GingerDicser.GetTermResValue(eTermResKey.Activity), GingerDicser.GetTermResValue(eTermResKey.Activity) + " Name:", ref newActivityName))
            {
                if (!String.IsNullOrEmpty(newActivityName))
                {
                    Activity a = new Activity() { Active=true};
                    a.ActivityName = newActivityName;
                    a.TargetApplication = mBusinessFlow.MainApplication;
                    mBusinessFlow.Activities.Add(a);
                }
            }
        }

        private void btnScreenShot_Click(object sender, RoutedEventArgs e)
        {            
            Bitmap bmpScreenShot = GetScreenShot();
            //TODO: create windows temp file
            string filename = @"C:\temp\sc1.png"; // Path.te.GetTempFileName();
            bmpScreenShot.Save(filename);
            System.Diagnostics.Process.Start(filename);
        }

        private void MainRibbon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainRibbon.SelectedItem == HTMLRibbon)
            {
                frmHTML.Visibility = System.Windows.Visibility.Visible;
                frmBrowser.Visibility = System.Windows.Visibility.Collapsed;
            }
            if (MainRibbon.SelectedItem == IB_Ribbon)
            {
                frmHTML.Visibility = System.Windows.Visibility.Collapsed;
                frmBrowser.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void RibbonLoaded(object sender, RoutedEventArgs e)
        {
            Grid child = VisualTreeHelper.GetChild((DependencyObject)sender, 0) as Grid;
            if (child != null)
            {
                //hide the ribbon QuickAccessToolBar
                child.RowDefinitions[0].Height = new GridLength(0);   
  
                //set browser frame on top free dpace
                if (frmBrowser != null)
                {
                    Thickness margin = frmBrowser.Margin;
                    margin.Top = -22;
                    frmBrowser.Margin = margin;
                }
                if (frmHTML != null)
                {
                    Thickness margin = frmHTML.Margin;
                    margin.Top = -22;
                    frmHTML.Margin = margin;
                }
            }
        }

        private void btnViewSource_Click(object sender, RoutedEventArgs e)
        {
            mshtml.HTMLDocument d = (mshtml.HTMLDocument)browser.Document;
            frmHTML.Text = d.body.innerHTML;
            btnSearchSource.IsEnabled = true;
            frmBrowser.Focus();
        }

        private void btnMarker_Click(object sender, RoutedEventArgs e)
        {
            if (btnMarker.IsChecked == true)
            {
                SetDocMouseOver();
                SetDocMouseClick();
            }
            else
            {
                UnHookMarker();
            }
        }

        private void UnHookMarker()
        {
            UnLightCurrentHighlightedElement();
            mDocument.detachEvent("onmouseover", DomEventHandlerMouseOver);
            mDocument.detachEvent("onclick", DomEventHandlerMouseClick);
        }
        
        private void btnAction1_Click(object sender, RoutedEventArgs e)
        {
            mBusinessFlow.AddAct((Act)mValidateAction_1.CreateCopy());
        }

        private void btnAction2_Click(object sender, RoutedEventArgs e)
        {
            mBusinessFlow.AddAct((Act)mValidateAction_2.CreateCopy());
        }

        private void btnAddAction_Click(object sender, RoutedEventArgs e)
        {
            mBusinessFlow.AddAct((Act)mAction.CreateCopy());
        }

        private void btnAddPWLAction_Click(object sender, RoutedEventArgs e)
        {
            if (!mPWLSelectingTarget)
            {
                mPWLSelectingTarget = true;
                btnAddPWLAction.Content = "Add PWL Target Element";
                mPWLOElement = mCurrentElement;
            }
            else
            {
                mPWLSelectingTarget = false;
                mPWLTElement = mCurrentElement;                
                btnAddPWLAction.Content = "Add PWL Orig Element";
                mBusinessFlow.AddAct(CreatePWLActionForElement(mPWLOElement, mPWLTElement,new ActPWL()));
            }
            HighLightElement(mCurrentElement);
        }

        private void OpenEmenuByKey(object sender, RoutedEventArgs e)
        {
            btnEmenu.IsChecked = !btnEmenu.IsChecked;            
            ShowHideEmenu();         
        }
        
        #region Create Action for Quick eMenu buttons

        private Act CreateActionForElement(mshtml.IHTMLElement element,Act ac)
        {         
            mCurrentElement = element;
            SetActionLocator(ac, element);
            ac.Active = true;
            return ac;
        }

        private Act CreatePWLActionForElement(mshtml.IHTMLElement OElement, mshtml.IHTMLElement TElement,ActPWL ac)
        {
            eLocateBy LocateBy;
            string LocateValue;
            mCurrentElement = OElement;
            SetActionLocator(ac, OElement);
            SetElementLocator(TElement, out LocateBy, out LocateValue);
            ac.OLocateBy = LocateBy;
            ac.OLocateValue = LocateValue;
            ac.Description = "Measure the distance between " + OElement.tagName + " and " + TElement.tagName;
            ac.Active = true;
            return ac;
        }

        private void SetActionLocator(Act ac, mshtml.IHTMLElement el)
        {
            eLocateBy LocateBy;
            string LocateValue;
            SetElementLocator(el, out LocateBy, out LocateValue);            
            ac.LocateBy = LocateBy;
            ac.LocateValue = LocateValue;
        }

        private void SetAddActionButton(string ButtonText, string TagName, string Description, mshtml.IHTMLElement element,Act act)
        {
            Act newAc = CreateActionForElement(element,act);
            newAc.Description = Description;            
            btnAddAction.Content = ButtonText;
            btnAddAction.ToolTip = Description;
            mAction = newAc;
        }
    
        private void CreateInputAction(mshtml.HTMLInputElement element)
        {
            string stype = element.getAttribute("type").ToUpper();
            SetAddActionButtonForINPUT(stype, element);
        }

        private void SetAddActionButtonForINPUT(string sType, mshtml.HTMLInputElement element)
        {
            switch (sType)
            {
                case "CHECKBOX":
                    SetAddActionButton(ButtonText: "Click Check button(Ctrl+S)",
                                                TagName: "CHECKBOX",
                                                Description: "Check/uncheck checkbox: " + element.name,
                                                element: (mshtml.IHTMLElement)element,
                                                act: new ActCheckbox() { CheckboxAction=ActCheckbox.eCheckboxAction.Click});

                    
                    break;
                case "SUBMIT":
                case "BUTTON":

                    SetAddActionButton(ButtonText: "Click Button(Ctrl+S)",
                                                TagName: "Button",
                                                Description: "Click button " + element.value,
                                                element: (mshtml.IHTMLElement)element,
                                                act: new ActButton() { ButtonAction=ActButton.eButtonAction.Click});
                    
                    break;
                case "DATE":
                case "DATETIME":
                case "DATETIME-LOCAL":
                case "EMAIL":
                case "FILE":
                case "MONTH":
                case "NUMBER":
                case "SEARCH":
                case "TIME":
                case "URL":
                case "WEEK":
                case "TEXT":
                    SetAddActionButton(ButtonText: "Set the " + sType + " box(Ctrl+S)",
                                                TagName: "TEXTBOX",
                                                Description: "Set the " + sType + " box " + element.innerText + " value",
                                                element: (mshtml.IHTMLElement)element,
                                                act: new ActTextBox() { TextBoxAction=ActTextBox.eTextBoxAction.SetValue});
                                       
                    break;
                case "PASSWORD":
                    SetAddActionButton(ButtonText: "Set Password(Ctrl+S)",
                                                TagName: "PASSWORD",
                                                Description: "Set password field",
                                                element: (mshtml.IHTMLElement)element,
                                                act: new ActPassword() { PasswordAction=ActPassword.ePasswordAction.SetValue});
                    break;
                case "RADIO":
                    SetAddActionButton(ButtonText: "Check Radiobutton(Ctrl+S)",
                                                TagName: "RADIO",
                                                Description: "Check this Radionbutton: " + element.value,
                                                element: (mshtml.IHTMLElement)element,
                                                act: new ActRadioButton() { RadioButtonAction=ActRadioButton.eActRadioButtonAction.SelectByIndex});
                    
                    break;
            }
        }

        private void CreateSelectAction(HTMLSelectElement element)
        {

            SetAddActionButton(ButtonText: "Select From List(Ctrl+S)",
                                TagName: "SELECT",
                                Description: "Select From DD List: " + element.name,
                                element: (mshtml.IHTMLElement)element,
                                act: new ActDropDownList() { ActDropDownListAction=ActDropDownList.eActDropDownListAction.SetSelectedValueByText});


        }

        private void CreateLinkAction(HTMLAnchorElement element)
        {
            SetAddActionButton(ButtonText: "Click Link(Ctrl+S)",
                                TagName: "LINK",
                                Description: "Click Link: " + element.href,
                                element: (mshtml.IHTMLElement)element,
                                act: new ActLink() { LinkAction=ActLink.eLinkAction.Click});
        }       
        #endregion

        #region Create Validation for Quick Emenu Buttons
        
        private void CreateInputValidations(mshtml.HTMLInputElement element)
        {
            string stype=element.getAttribute("type").ToUpper();
            SetActionButtonsForINPUT(stype, element);          
        }
        
        private void SetActionButtonsForINPUT(string sType, mshtml.HTMLInputElement element)
        {
            switch (sType)
            {
                case "CHECKBOX":
                    SetActionButton1(ButtonText: "CheckBox Current Status(Ctrl+1)",
                                                Description: "Validate if CheckBox is currently checked: " + element.@checked.ToString(),
                                                element: (mshtml.IHTMLElement)element,
                                                Expected: element.@checked.ToString(),                                                
                                                act: new ActCheckbox() { CheckboxAction = ActCheckbox.eCheckboxAction.GetValue});

                    SetActionButton2(ButtonText: "CheckBox Is Disabled(Ctrl+2)",
                                        Description: "Validate if CheckBox is disabled or not " + element.disabled,
                                        element: (mshtml.IHTMLElement)element,
                                        Expected: element.disabled + "",
                                        act: new ActCheckbox() { CheckboxAction = ActCheckbox.eCheckboxAction.IsDisabled });
                    break;
                case "SUBMIT":
                case "BUTTON":

                    SetActionButton1(ButtonText: "Button Text(Ctrl+1)",
                                                Description: "Validate if Button Text is: " + element.value,
                                                element: (mshtml.IHTMLElement)element,
                                                Expected: element.value + "",
                                                act: new ActButton() { ButtonAction=ActButton.eButtonAction.GetValue});
                    SetActionButton2(ButtonText: "Button Is Disabled(Ctrl+2)",                                   
                                   Description: "Validate if Button is disabled or not ",
                                   element: (mshtml.IHTMLElement)element,
                                   Expected: element.disabled + "",
                                   act: new ActButton() { ButtonAction = ActButton.eButtonAction.IsDisabled });
               break;
                //case "COLOR":
                case "DATE":
                case "DATETIME":
                case "DATETIME-LOCAL":
                case "EMAIL":
                case "FILE":
                case "MONTH":
                case "NUMBER":
                case "SEARCH":
                case "TIME":
                case "URL":
                case "WEEK":
                case "TEXT":
               SetActionButton1(ButtonText: sType + " box Current Content(Ctrl+1)",
                                                Description: "Validate if the " + sType + " box " + element.innerText + " currently content ",
                                                element: (mshtml.IHTMLElement)element,
                                                Expected: element.value + "",
                                                act: new ActTextBox() {TextBoxAction=ActTextBox.eTextBoxAction.GetValue });

                    SetActionButton2(ButtonText: sType + " Is Disabled(Ctrl+2)",                                       
                                        Description: "Validate if  " + sType + " box is disabled or not",
                                        element: (mshtml.IHTMLElement)element,
                                        Expected: element.disabled+"",
                                        act: new ActTextBox() { TextBoxAction = ActTextBox.eTextBoxAction.IsDisabled });
                    break;
                case "PASSWORD":
                    SetActionButton1(ButtonText: "Password Size (Ctrl+1)",
                                                Description: "Validate if the PASSWORD size is " + element.getAttribute("size"),
                                                element: (mshtml.IHTMLElement)element,
                                                Expected: element.getAttribute("size") + "",
                                                act: new ActPassword() { PasswordAction=ActPassword.ePasswordAction.GetSize});

                    SetActionButton2(ButtonText: "Is Disabled(Ctrl+2)",                                    
                                        Description: "Validate if PASSWORD is disabled or not",
                                        element: (mshtml.IHTMLElement)element,
                                        Expected: element.disabled+"",
                                                act: new ActPassword() { PasswordAction = ActPassword.ePasswordAction.IsDisabled });
                    break;
                case "RADIO":
                    SetActionButton1(ButtonText: "Current value(Ctrl+2)",
                                                Description: "Validate RadioButton's current value: " + element.value,
                                                element: (mshtml.IHTMLElement)element,
                                                Expected: element.value + "",
                                                act: new ActRadioButton() {RadioButtonAction=ActRadioButton.eActRadioButtonAction.GetValue });

                    SetActionButton2(ButtonText: "Is Disabled(Ctrl+2)",
                                        Description: "Validate if RadioButton is disabled or not",
                                        element: (mshtml.IHTMLElement)element,
                                        Expected:element.disabled+"",
                                                act: new ActRadioButton() { RadioButtonAction = ActRadioButton.eActRadioButtonAction.IsDisabled });
                    break;
            }
        }
        
       private void SetActionButton1(string ButtonText, string Description, mshtml.IHTMLElement element,string Expected,Act act)
        {
            Act newAc = CreateActionForElement(element, act);
            newAc.AddOrUpdateReturnParamExpected("Actual", Expected);
           
            newAc.Description = Description;
            btnAction1.Content = ButtonText;
            btnAction1.ToolTip = Description;
            mValidateAction_1 = newAc;
        }

       private void SetActionButton2(string ButtonText, string Description, mshtml.IHTMLElement element, string Expected, Act act)
       {
           Act newAc = CreateActionForElement(element, act);
           newAc.AddOrUpdateReturnParamExpected("Actual", Expected);

           newAc.Description = Description;
           btnAction2.Content = ButtonText;
           btnAction2.ToolTip = Description;
           mValidateAction_2 = newAc;
       }

       private void CreateLabelValidations(HTMLLabelElement element)
       {
           SetActionButton1(ButtonText: "Validate Value(Ctrl+1)",
                               Description: "Validate Label: " + element.innerText,
                               element: (mshtml.IHTMLElement)element,
                               Expected: element.innerText,
                                               act: new ActLabel() { LabelAction=ActLabel.eLabelAction.GetInnerText
                                               });
           SetActionButton2(ButtonText: "Validate Visibility(Ctrl+2)",
                               Description: "Validate Label visible",
                               element: (mshtml.IHTMLElement)element,
                               Expected: "True",
                                               act: new ActLabel()
                                               {
                                                   LabelAction = ActLabel.eLabelAction.IsVisible
                                               });
       }

        private void CreateGenericValidations(IHTMLElement element)
        {
            SetActionButton1(ButtonText: "Validate Value(Ctrl+1)",
                                Description: "Validate obejct: " + element.innerText,
                                element: (mshtml.IHTMLElement)element,
                                Expected: element.innerText,
                                               act: new ActGenElement() {GenElementAction=ActGenElement.eGenElementAction.GetInnerText });
            SetActionButton2(ButtonText: "Validate Visibility(Ctrl+2)",
                               Description: "Validate object visible",
                               element: (mshtml.IHTMLElement)element,
                               Expected: "True",
                                               act: new ActGenElement() { GenElementAction = ActGenElement.eGenElementAction.Visible });
        }

        private void CreateSelectValidations(HTMLSelectElement element)
        {
            string s = "";

            // Get all Drop down options
            IHTMLElementCollection options = element.children;
            foreach (IHTMLElement v in options)
            {
                //TODO: decide on delimeter, const global for app, use same in driver when getting the value - DUP code 
                s = s + v.innerText + "|";
            }

            SetActionButton1(ButtonText: "Validate List(Ctrl+1)",
                                Description: "Validate List: " + s,
                                element: (mshtml.IHTMLElement)element,
                                Expected: s,
                                               act: new ActDropDownList() { ActDropDownListAction=ActDropDownList.eActDropDownListAction.GetValidValues});

            SetActionButton2(ButtonText: "Validate Value(Ctrl+2)",
                                Description: "Validate Selected Text " + element.value ,
                                element: (mshtml.IHTMLElement)element,
                                Expected: element.value,
                                               act: new ActDropDownList() { ActDropDownListAction = ActDropDownList.eActDropDownListAction.GetSelectedValue });

            
        }

        private void CreateLinkValidations(HTMLAnchorElement element)
        {
            SetActionButton1(ButtonText: "Validate Link(Ctrl+1)",
                                Description: "Validate Link: " + element.href,
                                element: (mshtml.IHTMLElement)element,
                                Expected: element.href,
                                               act: new ActLink() {LinkAction=ActLink.eLinkAction.GetValue });

            SetActionButton2(ButtonText: "Validate Visibility(Ctrl+2)",
                                Description: "Validate Link's visible",
                                element: (mshtml.IHTMLElement)element,
                                Expected: element.style.display == "False" ? "False" : "True",
                                               act: new ActLink() {LinkAction=ActLink.eLinkAction.Visible });
        }
        #endregion

        private void txtURL_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DoGoToURL();
            }
        }

        private void BrowseMHTButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "Select MHT files folder";

            string DocsFolder = mBusinessFlow.FileName;
            int i = DocsFolder.IndexOf("BusinessFlows");
            if (i > 0)
            {
                //TODO: fix me not working shoing the folder of docs
                DocsFolder = DocsFolder.Substring(0, i) + "Documents";                
            }
            else
            {
                DocsFolder = @"C:\";
            }
            dlg.SelectedPath = DocsFolder;
            dlg.ShowNewFolderButton = true;
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string Folder = dlg.SelectedPath;
                mhtComboBox.Items.Clear();
                if (!Directory.Exists(Folder)) Directory.CreateDirectory(Folder);
                string[] fileEntries = Directory.GetFiles(Folder, "*.mht");
                foreach (string s in fileEntries)
                {
                    mhtComboBox.Items.Add(s);
                }
            }
            SavedMHTFilePath = dlg.SelectedPath;
        }

        private void mhtComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string filename = mhtComboBox.SelectedValue.ToString();
            txtURL.Text = filename;
            DoGoToURL();
        }

        private void SaveMHTButton_Click(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrEmpty(SavedMHTFilePath))
            {                
                Reporter.ToUser(eUserMsgKeys.MissingFileLocation);
            }
            else
                {
                    CDO.Message msg = new CDO.MessageClass();
                    CDO.Configuration cfg = new CDO.ConfigurationClass();
                    msg.Configuration = cfg;
                   // msg.CreateMHTMLBody("http://www.cnn.com", CDO.CdoMHTMLFlags.cdoSuppressAll, "", "");

                    mshtml.HTMLDocument doc = (mshtml.HTMLDocument)browser.Document;
                    msg.HTMLBody = doc.body.innerHTML;

                    string FN = "";
                    InputBoxWindow.OpenDialog("File Name", "Enter the file name", ref FN);
                    
                    // remove invalid characters from filename
                    foreach (char invalidChar in Path.GetInvalidFileNameChars()) 
                    {
                        FN = FN.Replace(invalidChar.ToString(), "");
                    }
                    FN = FN.Replace(@".", ""); 

                    if(!String.IsNullOrEmpty(FN))
                    {
                        msg.GetStream().SaveToFile(SavedMHTFilePath + "\\" + FN + ".mhtml", ADODB.SaveOptionsEnum.adSaveCreateOverWrite);
                    }
            }
        }

        private void btnRunStep_Click(object sender, RoutedEventArgs e)
        {
            //TODO: make me working
            IBDriver.RunAction((Act)mBusinessFlow.CurrentActivity.Acts.CurrentItem);
        }

        private void eHTMLCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (eHTMLRibbonGroup != null)
                eHTMLRibbonGroup.Visibility = System.Windows.Visibility.Visible;
        }

        private void eHTMLCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (eHTMLRibbonGroup != null)
                eHTMLRibbonGroup.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ActsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (ActsRibbonGroup != null)
                ActsRibbonGroup.Visibility = System.Windows.Visibility.Visible;
        }

        private void ActsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (ActsRibbonGroup != null)
                ActsRibbonGroup.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void MHTCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (MHTRibbonGroup != null)
                MHTRibbonGroup.Visibility = System.Windows.Visibility.Visible;
        }

        private void MHTCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (MHTRibbonGroup != null)
                MHTRibbonGroup.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void LocateValueTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            TestLocator();
        }

        private void TestLocator()
        {
            UnLightCurrentHighlightedElement();
            dynamic d = LocateByComboBox.SelectedItem;
            eLocateBy locType = d.Value;
            ActGenElement act = new ActGenElement();
            act.LocateBy = locType;
            act.LocateValue = LocateValueTextBox.Text;
            act.LocateValueCalculated = LocateValueTextBox.Text;
            mshtml.IHTMLElement elem = TryGetActElementByLocator(act);
            if (elem != null)
            {              
                HighLightElement(elem);
            }
            else
            {
                //TODO: reporter                
                Reporter.ToUser(eUserMsgKeys.ElementNotFound);
            }
        }

        private void CreateActionButton_Click(object sender, RoutedEventArgs e)
        {
            ActGenElement act = new ActGenElement();            
            dynamic d = LocateByComboBox.SelectedItem;
            if (d!=null)
            {
                act.LocateBy = d.Value;                     
            }
            
            act.LocateValue = LocateValueTextBox.Text;
            act.Active = true;
            dynamic d2 = ActionCombotBox.SelectedItem;
            act.GenElementAction = d2.Value;
            act.Description = d2.text + " " + act.LocateValue;
            mBusinessFlow.CurrentActivity.Acts.Add(act);
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            TestLocator();
        }

        private void NewActionConfigButton_Click(object sender, RoutedEventArgs e)
        {
            CreateNewActionPage CNAP = new CreateNewActionPage();
            CNAP.ShowAsWindow();
        }

        private void LocateValueButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LocateByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dynamic d = LocateByComboBox.SelectedItem;
            eLocateBy LocType = d.Value;
            if (LocType == eLocateBy.ByXPath)
            {
                LocateValueTextBox.Text = WBP.GetElementXPath(CurrentHighlightedElement);
            }
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (txtURL != null && Col_0 != null && Col_1 != null)
                txtURL.Width = (Col_0.ActualWidth + Col_1.ActualWidth) - 100;

            foreach (DeviceEmulation de in DES)
            {
                if (de.Devicename == Convert.ToString(DeviceComboBox.SelectedItem))
                {


                    double ratio = WBP.WindowHeight / de.Height;

                    // WBP.Height =de.Height; 
                    WBP.Width = de.Width * ratio;
                   

                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (browser != null)
            {
                browser.Dispose();
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {            
            IBDriver.CloseDriver();
        }
        
        private void DeviceComboBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (Convert.ToString(DeviceComboBox.SelectedItem) == "Desktop")
            {
                WBP.Height = frmBrowser.Height;

                WBP.Width = frmBrowser.Width;
            }

            else
            {
                foreach (DeviceEmulation de in DES)
                {
                    if (de.Devicename == Convert.ToString(DeviceComboBox.SelectedItem))
                    {
                        double ratio = WBP.WindowHeight / de.Height;
                        WBP.Width = de.Width*ratio;
                        Useragent = "User-Agent:" + de.UserAgent;
                    }
                }
            }
        }

        private void btnSearchSource_Clik(object sender, RoutedEventArgs e)
        {
            string search = txtSourceSearch.Text;
            if (search != null)
            {
                string temp = frmHTML.Text;
                frmHTML.Focus();
                int Searchposition = temp.IndexOf(search);
                if (Searchposition == -1)
                {                    
                    Reporter.ToUser(eUserMsgKeys.TextNotFound);
                    return;
                }
                frmHTML.SelectionStart = Searchposition;
                frmHTML.SelectionLength = search.Length;
                btnNxtSearchSource.IsEnabled = true;
                CurrentSearchPosition = Searchposition;
                btnPrevSearchSource.IsEnabled = false;
            }
            else
                Reporter.ToUser(eUserMsgKeys.ProvideSearchString);         
        }

        private void btnSearchSourceNext_Clik(object sender, RoutedEventArgs e)
        {
            string search = txtSourceSearch.Text;
            if (search != null)
            {
                string temp = frmHTML.Text;
                int Searchposition = temp.IndexOf(search, (CurrentSearchPosition+search.Length));
                if (Searchposition == -1)
                {                    
                    Reporter.ToUser(eUserMsgKeys.NoTextOccurrence);
                    return;
                }
                frmHTML.SelectionStart = Searchposition;
                frmHTML.SelectionLength = search.Length;
                btnNxtSearchSource.IsEnabled = true;
                CurrentSearchPosition = Searchposition;
                btnPrevSearchSource.IsEnabled = true;
            }
            else
                Reporter.ToUser(eUserMsgKeys.ProvideSearchString);            
        }

        private void btnSearchSourcePrevious_Click(object sender, RoutedEventArgs e)
        {
            string search = txtSourceSearch.Text;
            if (search != null)
            {
                string temp = frmHTML.Text;
                int Searchposition = temp.LastIndexOf(search,CurrentSearchPosition);
                if (Searchposition == -1)
                {                    
                    Reporter.ToUser(eUserMsgKeys.NoTextOccurrence);
                    return;
                }
                frmHTML.SelectionStart = Searchposition;
                frmHTML.SelectionLength = search.Length;
                btnNxtSearchSource.IsEnabled = true;
                CurrentSearchPosition = Searchposition;
                btnPrevSearchSource.IsEnabled = true;
            }
            else                
                Reporter.ToUser(eUserMsgKeys.ProvideSearchString);
        }
    }

   
    public class ListBoxExtenders : DependencyObject
    {
        public static readonly DependencyProperty AutoScrollToEndProperty = DependencyProperty.RegisterAttached("AutoScrollToEnd", typeof(bool), typeof(ListBoxExtenders), new UIPropertyMetadata(default(bool), OnAutoScrollToEndChanged));

        /// <summary>
        /// Returns the value of the AutoScrollToEndProperty
        /// </summary>
        /// <param name="obj">The dependency-object whichs value should be returned</param>
        /// <returns>The value of the given property</returns>
        public static bool GetAutoScrollToEnd(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollToEndProperty);
        }

        /// <summary>
        /// Sets the value of the AutoScrollToEndProperty
        /// </summary>
        /// <param name="obj">The dependency-object whichs value should be set</param>
        /// <param name="value">The value which should be assigned to the AutoScrollToEndProperty</param>
        public static void SetAutoScrollToEnd(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollToEndProperty, value);
        }
        
        public static ObservableList<DeviceEmulation> DevicesForEmulation()
        {
            ObservableList<DeviceEmulation> Devices = new ObservableList<DeviceEmulation>();
           
            string devicelistpath=Directory.GetCurrentDirectory()+@"\Device.xml";
            Devices = (ObservableList<DeviceEmulation>)RepositoryItem.LoadFromFile(typeof(ObservableList<DeviceEmulation>), devicelistpath);

            return Devices;
        }

        /// <summary>
        /// This method will be called when the AutoScrollToEnd
        /// property was changed
        /// </summary>
        /// <param name="s">The sender (the ListBox)</param>
        /// <param name="e">Some additional information</param>
        public static void OnAutoScrollToEndChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            //Commented due to issue - when openning flow and going to automate page it goes to last act
            // Need to run one stepm and have IB open
            // TODO: fix me

            //var listBox = (System.Windows.Controls.ListBox)s;
            //var listBoxItems = listBox.Items;
            //var data = listBoxItems.SourceCollection as INotifyCollectionChanged;

            //var scrollToEndHandler = new System.Collections.Specialized.NotifyCollectionChangedEventHandler(
            //    (s1, e1) =>
            //    {
            //        if (listBox.Items.Count > 0)
            //        {
            //            object lastItem = listBox.Items[listBox.Items.Count - 1];
            //            listBoxItems.MoveCurrentTo(lastItem);
            //            listBox.ScrollIntoView(lastItem);
            //        }
            //    });

            //if ((bool)e.NewValue)
            //    data.CollectionChanged += scrollToEndHandler;
            //else
            //    data.CollectionChanged -= scrollToEndHandler;
        }
    }
}
