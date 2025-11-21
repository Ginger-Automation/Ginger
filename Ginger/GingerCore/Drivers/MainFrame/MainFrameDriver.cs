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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Console;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.MainFrame;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Open3270;
using Open3270.TN3270;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace GingerCore.Drivers.MainFrame
{
    public class MainFrameDriver : DriverBase, INotifyPropertyChanged, IWindowExplorer
    {
        #region GingerConfigs

        [UserConfigured]
        [UserConfiguredDefault("")]
        [UserConfiguredDescription("Host")]
        public string HostName { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("23")]
        [UserConfiguredDescription("Port")]
        public int HostPort { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("IBM-3278-2-E")]
        [UserConfiguredDescription("Terminal Type")]
        public string TermType { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Use SSL")]
        public bool SSL { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("true")]
        [UserConfiguredDescription("Set This to true if your screen loads blank")]
        public bool ForceRefreshonBlankScreenLoad { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("24")]
        [UserConfiguredDescription("Number of Rows in target Mainframe application")]
        public int MFRows { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("3")]
        [UserConfiguredDescription("Delay between Set Text and Send in Seconds")]
        public int DelayBwSetTextandSend { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("80")]
        [UserConfiguredDescription("Number of Columns in target Mainframe application")]
        public int MFColumns { get; set; }


        public int Rows = 24;
        public int Coloumn = 80;

        public override ePomElementCategory? PomCategory
        {
            get
            {
                if (base.PomCategory == null)
                {
                    return ePomElementCategory.Mainframe;
                }
                else
                {
                    return base.PomCategory;
                }
            }

            set => base.PomCategory = value;
        }

        #endregion GingerConfigs

        #region GingerFunctions

        public override bool IsWindowExplorerSupportReady()
        {
            return false;
        }

        public static partial class Fields
        {
            public static readonly string CaretIndex = "CaretIndex";
        }

        //private string screenText;
        private bool IsServerAvailable = false;

        private bool isConnected
        {
            get
            {
                try
                {
                    return MFE != null && MFE.IsConnected;
                }
                catch
                {
                    return false;
                }
            }
        }

        //private bool isConnecting;

        public MainFrameDriverWindow mDriverWindow = null;

        public override bool IsSTAThread()
        {
            return true;
        }

        public Terminal MFE;

        public BusinessFlow mBusinessFlow;

        public MainFrameDriver(BusinessFlow BF)
        {
            mBusinessFlow = BF;
        }

        public override void StartDriver()
        {
            CreateSTA(Launchdriver);
        }

        private void Launchdriver()
        {
            try
            {
                IsServerAvailable = GingerCore.Common.Utility.IsServerListening(this.HostName, HostPort);
                if (!IsServerAvailable)
                {
                    Reporter.ToStatus(eStatusMsgKey.MainframeIncorrectConfiguration);
                    return;
                }

                MFE = new Terminal(this.HostName, HostPort, TermType, SSL, MFRows, MFColumns, this);

                if (ConnectToMainframe())
                {
                    bool windowLoaded = false;
                    mDriverWindow = new MainFrameDriverWindow(this);
                    mDriverWindow.Loaded += (_, _) => windowLoaded = true;
                    mDriverWindow.Show();
                    mDriverWindow.Refresh();
                    // Wait for window loaded event with timeout
                    int timeout = 0;
                    while (!windowLoaded && timeout < 100)
                    {
                        System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Background);
                        Thread.Sleep(10);
                        timeout += 10;
                    }
                    Dispatcher = new DriverWindowDispatcher(mDriverWindow.Dispatcher);
                    Dispatcher.Invoke(new Action(() => OnDriverMessage(eDriverMessageType.DriverStatusChanged)));
                    System.Windows.Threading.Dispatcher.Run();

                }
                else
                {
                    mDriverWindow = null;

                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to Launch mainframe driver", ex);
                Dispatcher = new DummyDispatcher();
                throw;
            }
        }

        public new static partial class Fields
        {
            public static readonly string ScreenText = "ScreenText";
        }

        public override void CloseDriver()
        {
            if (!IsServerAvailable)
            {
                return;
            }
            MFE.Disconnect();
            if (mDriverWindow != null)
            {
                if (!mDriverWindow.IsClosing)
                {
                    mDriverWindow.Close();
                }
                OnDriverMessage(eDriverMessageType.DriverStatusChanged);
            }

        }

        public override void RunAction(Act act)
        {
            if (act.GetType().ToString() == "GingerCore.Actions.ActScreenShot")
            {
                TakeScreenShot(act);
                return;
            }

            try
            {
                switch (act.GetType().ToString())
                {
                    case "GingerCore.Actions.MainFrame.ActMainframeGetDetails":




                        PerformActMainframeGetDetails(act);

                        break;

                    case "GingerCore.Actions.MainFrame.ActMainframeSendKey":
                        ActMainframeSendKey MFSK = (ActMainframeSendKey)act;
                        MFE.SendKey(MFSK.KeyToSend);
                        break;

                    case "GingerCore.Actions.MainFrame.ActMainframeSetText":


                        PerformActMainframeSetText(act);

                        break;

                    default:
                        throw new NotSupportedException("Action not Implemented");
                }

                mDriverWindow.Refresh();
            }
            catch (Exception e)
            {
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                act.ExInfo = e.Message;
            }
        }

        private void PerformActMainframeGetDetails(Act act)
        {
            ActMainframeGetDetails MFGD = (ActMainframeGetDetails)act;
            //todo Implement get Type and others


            switch (MFGD.DetailsToFetch)
            {
                case ActMainframeGetDetails.eDetailsToFetch.GetText:

                    ActMainFrameGetText(act);

                    break;

                case ActMainframeGetDetails.eDetailsToFetch.GetDetailsFromText:

                    MainframeGetDetailsFromText(act);
                    break;

                case ActMainframeGetDetails.eDetailsToFetch.GetAllEditableFeilds:

                    XmlDocument XD = new XmlDocument();
                    XmlDeclaration dec = XD.CreateXmlDeclaration("1.0", null, null);
                    XD.AppendChild(dec);
                    XmlElement root = XD.CreateElement("EditableFields");
                    XD.AppendChild(root);

                    string CaretValuePair = @"<?xml version='1.0' encoding='UTF-8'?><nodes>";

                    XMLScreen XC = MFE.GetScreenAsXML();
                    foreach (XMLScreenField XSF in XC.Fields)
                    {
                        if (XSF.Attributes.Protected)
                        {
                            continue;
                        }
                        string node = "<node caret=\"" + XSF.Location.position.ToString() + "\" text=\"" + XSF.Text + "\"> </node>";

                        CaretValuePair = CaretValuePair + node;

                        XmlElement EditableField = XD.CreateElement("EditableField");
                        EditableField.SetAttribute("Caret", XSF.Location.position.ToString());
                        EditableField.SetAttribute("Text", XSF.Text);
                        root.AppendChild(EditableField);
                    }

                    act.AddOrUpdateReturnParamActual("Fields", XD.OuterXml);

                    break;

                case ActMainframeGetDetails.eDetailsToFetch.GetCurrentScreenAsXML:

                    Open3270.TN3270.XMLScreen XMLS = MFE.GetScreenAsXML();
                    System.Xml.Serialization.XmlSerializer xsSubmit = new System.Xml.Serialization.XmlSerializer(typeof(Open3270.TN3270.XMLScreen));
                    System.IO.StringWriter sww = new System.IO.StringWriter();
                    System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(sww);

                    xsSubmit.Serialize(writer, XMLS);
                    String ScreenXML = sww.ToString(); // Your XML

                    act.AddOrUpdateReturnParamActual("ScreenXML", ScreenXML);

                    break;
                default:
                    throw new NotSupportedException("The action is not supporte yet");


            }


        }

        private void MainframeGetDetailsFromText(Act act)
        {
            ActMainframeGetDetails MFGD = (ActMainframeGetDetails)act;
            int locx;
            int locy = -1;
            String[] MainFrameLines = MFE.screenText.Split('\n');
            int instance = 1;
            for (int i = 0; i < MainFrameLines.Length; i++)
            {
                locx = MainFrameLines[i].IndexOf(MFGD.ValueForDriver);
                if (locx >= 0)
                {
                    locy = i;
                    if (MFGD.TextInstanceType == ActMainframeGetDetails.eTextInstance.AllInstance)
                    {
                        if (locy != -1)
                        {
                            act.AddOrUpdateReturnParamActualWithPath("CaretPosition", (locy * (MFColumns + 1) + locx).ToString(), instance.ToString());
                            act.AddOrUpdateReturnParamActualWithPath("X", locx.ToString(), instance.ToString());
                            act.AddOrUpdateReturnParamActualWithPath("Y", locy.ToString(), instance.ToString());
                        }
                    }
                    else if (MFGD.TextInstanceType == ActMainframeGetDetails.eTextInstance.InstanceN)
                    {
                        int k = Int32.Parse(MFGD.TextInstanceNumber);
                        if (locy != -1 && instance == k)
                        {
                            act.AddOrUpdateReturnParamActual("CaretPosition", (locy * (MFColumns + 1) + locx).ToString());
                            act.AddOrUpdateReturnParamActual("X", locx.ToString());
                            act.AddOrUpdateReturnParamActual("Y", locy.ToString());
                            break;
                        }
                    }
                    else if (MFGD.TextInstanceType == ActMainframeGetDetails.eTextInstance.AfterCaretPosition)
                    {
                        if (Int32.Parse(MFGD.LocateValueCalculated.ToString()) < (locy * (MFColumns + 1) + locx))
                        {
                            act.AddOrUpdateReturnParamActual("CaretPosition", (locy * (MFColumns + 1) + locx).ToString());
                            act.AddOrUpdateReturnParamActual("X", locx.ToString());
                            act.AddOrUpdateReturnParamActual("Y", locy.ToString());
                            break;
                        }
                    }
                    else
                    {
                        if (locy != -1)
                        {
                            act.AddOrUpdateReturnParamActual("CaretPosition", (locy * (MFColumns + 1) + locx).ToString());
                            act.AddOrUpdateReturnParamActual("X", locx.ToString());
                            act.AddOrUpdateReturnParamActual("Y", locy.ToString());
                            break;
                        }
                    }
                }
                if (locy != -1)
                {
                    instance++;
                }
            }

        }

        private void PerformActMainframeSetText(Act act)
        {
            ActMainframeSetText MFST = (ActMainframeSetText)act;
            switch (MFST.SetTextMode)
            {
                case ActMainframeSetText.eSetTextMode.SetSingleField:
                    if (MFST.LocateBy == eLocateBy.ByXY)
                    {
                        string XY = MFST.LocateValueCalculated;

                        String[] XYSeparated = XY.Split(',');

                        int x = Int32.Parse(XYSeparated.ElementAt(0));
                        int y = Int32.Parse(XYSeparated.ElementAt(1));
                        if (x >= Coloumn || y >= Rows)
                        {
                            throw new ArgumentOutOfRangeException("X,Y out of bounds please use X/Y less than Rows/Columns configured in agent");
                        }
                        MFE.SetCaretIndex(x, y);
                    }
                    else
                    {
                        MFE.SetCaretIndex(Int32.Parse(act.LocateValueCalculated));
                    }

                    MFE.SendText(act.ValueForDriver);

                    break;

                case ActMainframeSetText.eSetTextMode.SetMultipleFields:

                    if (MFST.ReloadValue)
                    {
                        MFST.LoadCaretValueList();
                    }
                    foreach (ActInputValue AIV in MFST.CaretValueList)
                    {
                        MFE.SetCaretIndex(Int32.Parse(AIV.Param));
                        ValueExpression VE = new ValueExpression(this.Environment, this.BusinessFlow)
                        {
                            Value = AIV.Value
                        };
                        MFE.SendText(VE.ValueCalculated);
                    }

                    break;
                default:
                    throw new NotSupportedException("This action is not implemented yet");
            }
            if (MFST.SendAfterSettingText)
            {
                mDriverWindow.Refresh();
                try
                {
                    Thread.Sleep(DelayBwSetTextandSend * 1000);
                }
                catch
                {
                    Thread.Sleep(3000);
                }
                MFE.SendKey(TnKey.Enter);
            }
        }

        private void ActMainFrameGetText(Act act)
        {
            ActMainframeGetDetails MFGD = (ActMainframeGetDetails)act;
            if (MFGD.LocateBy == eLocateBy.ByCaretPosition)
            {
                if (String.IsNullOrEmpty(act.ValueForDriver))
                {
                    string MFText = MFE.GetTextatPosition(Int32.Parse(MFGD.LocateValueCalculated), 50);
                    MFText = MFText.Split().ElementAt(0).ToString();
                    MFGD.AddOrUpdateReturnParamActual("Value", MFText);
                }
                else
                {
                    act.AddOrUpdateReturnParamActual("Value", MFE.GetTextatPosition(Int32.Parse(act.LocateValueCalculated), Int32.Parse(act.ValueForDriver)));
                }
            }
            else if (MFGD.LocateBy == eLocateBy.ByXY)
            {
                string XY = MFGD.LocateValueCalculated;

                String[] XYSeparated = XY.Split(',');

                int x = Int32.Parse(XYSeparated.ElementAt(0));
                int y = Int32.Parse(XYSeparated.ElementAt(1));
                if (x >= Coloumn || y >= Rows)
                {
                    throw new ArgumentOutOfRangeException("X,Y out of bounds please use X/Y less than Rows/Columns configured in agent");
                }
                if (String.IsNullOrEmpty(act.ValueForDriver))
                {
                    string MFText = MFE.GetTextatPosition(x, y, 50);
                    MFText = MFText.Split().ElementAt(0).ToString();
                    MFGD.AddOrUpdateReturnParamActual("Value", MFText);
                }
                else
                {
                    act.AddOrUpdateReturnParamActual("Value", MFE.GetTextatPosition(x, y, Int32.Parse(act.ValueForDriver)));
                }
            }
            else
            {
                throw new NotSupportedException("Locater type is not supported for this action");
            }

        }



        public override Actions.Act GetCurrentElement()
        {
            mDriverWindow.Show();
            throw new NotImplementedException();
        }

        public override string GetURL()
        {
            throw new NotImplementedException();
        }



        public override void HighlightActElement(Actions.Act act)
        {
            throw new NotImplementedException();
        }

        public override ePlatformType Platform { get { return ePlatformType.MainFrame; } }

        public override bool IsRunning()
        {

            return isConnected;
        }

        public void TakeScreenShot(Act act)
        {
            int width = (int)mDriverWindow.Width;
            int height = (int)mDriverWindow.Height;
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(mDriverWindow);

            PngBitmapEncoder pngImage = new PngBitmapEncoder();
            pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            string FileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".png";
            using (Stream fileStream = File.Create(FileName))
            {
                pngImage.Save(fileStream);
                fileStream.Close();
            }

            using (Bitmap bmp = (Bitmap)Image.FromFile(FileName))
            {
                //TODO: Remove the temporary File created

                act.AddScreenShot(bmp);
            }
        }

        #endregion GingerFunctions

        #region windowexplorerfunctions

        public List<AppWindow> GetAppWindows()
        {
            List<AppWindow> AppWinList = [];
            AppWindow AppWin = new AppWindow
            {
                WindowType = AppWindow.eWindowType.Mainframe,
                Title = "mainframe"
            };
            AppWinList.Add(AppWin);
            return AppWinList;
        }

        public void SwitchWindow(string Title)
        {

        }


        public string GetFocusedControl()
        {
            mDriverWindow.Show();
            throw new NotImplementedException();
        }

        public object GetControlFromMousePosition()
        {
            mDriverWindow.Show();
            throw new NotImplementedException();
        }

        public ElementInfo LearnElementInfoDetails(ElementInfo EI, PomSetting pomSetting = null)
        {
            return EI;
        }

        public AppWindow GetActiveWindow()
        {
            AppWindow AppWin = new AppWindow
            {
                WindowType = AppWindow.eWindowType.Mainframe,
                Title = "mainframe"
            };
            return AppWin;
            throw new NotImplementedException();
        }

        #endregion windowexplorerfunctions

        #region terminalfuntions

        public XMLScreen GetRenderedScreen()
        {
            return MFE.GetScreenAsXML();
        }


        #endregion terminalfuntions

        #region terminalfuntions2

        /// <summary>
        /// Connect to mainframe server, create window only after the connection is successful
        /// </summary>
        private bool ConnectToMainframe()
        {
            //already verified  that server is available
            bool status = MFE.Connect();

            if (!status)
            {
                return false;
            }
            //window object will be created after successful connection

            int i = 0;
            while (!MFE.IsConnected && i < this.DriverLoadWaitingTime * 1000)
            {
                Thread.Sleep(100);
                i++;
            }

            if (i >= this.DriverLoadWaitingTime * 1000)
            {
                MFE.Disconnect();
                return false;
            }
            if (ForceRefreshonBlankScreenLoad)
            {
                string CurrentScrnTxt = MFE.ScreenText;
                if (String.IsNullOrEmpty(CurrentScrnTxt) || String.IsNullOrWhiteSpace(CurrentScrnTxt))
                {
                    MFE.SendKey(TnKey.Reset);
                }
            }

            return status;
        }


        public bool SetTextAtPosition(String CommandText, bool SendEnter = true, int? caret = null)
        {
            if (caret.HasValue)
            {
                MFE.SetCaretIndex(caret.Value);
            }
            MFE.SendText(CommandText);
            MFE.SendKey(TnKey.Enter);
            return true;
        }

        public bool SetTextAtPosition(String CommandText, int LocX, int LocY, bool SendEnter = true)
        {
            MFE.SetCaretIndex(LocX, LocY);

            MFE.SendText(CommandText);
            if (SendEnter)
            {
                MFE.SendKey(TnKey.Enter);
            }
            return true;
        }

        public int CaretIndex
        {
            get
            {
                return MFE.CaretIndex;
            }
            set
            {
                MFE.CaretIndex = value;
            }
        }

        public string ScreenText
        {
            get
            {
                return MFE.ScreenText;
            }
            set
            {
                MFE.ScreenText = value;
                this.OnPropertyChanged("ScreenText");
            }
        }

        #endregion terminalfuntions2

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion INotifyPropertyChanged

        #region WindowDriverFunctions

        public void HighLightElement(ElementInfo ElementInfo, bool locateElementByItLocators = false, IList<ElementInfo> MappedUIElements = null)
        {
        }

        ElementInfo IWindowExplorer.GetControlFromMousePosition()
        {
            throw new System.NotImplementedException();
        }

        public async Task<System.Collections.Generic.List<ElementInfo>> GetVisibleControls(PomSetting pomSetting, ObservableList<ElementInfo> foundElementsList = null, ObservableList<POMPageMetaData> PomMetaData = null, Bitmap ScreenShot = null)
        {
            return await Task.Run(() =>
            {
                List<ElementInfo> Eil = [];

                try
                {
                    foreach (XMLScreenField xf in MFE.GetScreenAsXML().Fields)
                    {
                        ElementInfo EI = new ElementInfo
                        {
                            ElementTitle = xf.Text
                        };
                        if (xf.Attributes.FieldType == "Hidden")
                        {
                            EI.ElementType = "Password";
                        }
                        if (xf.Attributes.Protected)
                        {
                            if (xf.Attributes.FieldType == "High")
                            {
                                EI.ElementType = "High";
                            }
                        }

                        //set the POM category
                        EI.SetLocatorsAndPropertiesCategory(this.PomCategory);

                        Eil.Add(EI);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to Get Controls", ex);
                }

                return Eil;
            });
        }

        public System.Collections.Generic.List<ElementInfo> GetElementChildren(ElementInfo ElementInfo)
        {
            throw new System.NotImplementedException();
        }

        public ObservableList<ControlProperty> GetElementProperties(ElementInfo ElementInfo)
        {
            return [];
            throw new System.NotImplementedException();
        }

        public ObservableList<ElementLocator> GetElementLocators(ElementInfo ElementInfo, PomSetting pomSetting = null)
        {
            throw new System.NotImplementedException();
        }

        object IWindowExplorer.GetElementData(ElementInfo ElementInfo, eLocateBy elementLocateBy, string elementLocateValue)
        {
            throw new System.NotImplementedException();
        }

        public ObservableList<ElementInfo> GetElements(ElementLocator EL)
        {
            throw new System.NotImplementedException();
        }

        #endregion WindowDriverFunctions

        void IWindowExplorer.UpdateElementInfoFields(ElementInfo eI)
        {
        }

        bool IWindowExplorer.IsElementObjectValid(object obj)
        {
            return true;
        }

        public void UnHighLightElements()
        {
        }


        public bool TestElementLocators(ElementInfo EI, bool GetOutAfterFoundElement = false, ApplicationPOMModel mPOM = null)
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

        List<AppWindow> IWindowExplorer.GetWindowAllFrames()
        {
            throw new NotImplementedException();
        }

        public bool IsRecordingSupported()
        {
            return false;
        }

        public bool IsPOMSupported()
        {
            return false;
        }

        public bool IsLiveSpySupported()
        {
            return false;
        }

        public bool IsWinowSelectionRequired()
        {
            return true;
        }

        public List<eTabView> SupportedViews()
        {
            return [];
        }

        public eTabView DefaultView()
        {
            return eTabView.none;
        }

        public string SelectionWindowText()
        {
            return "Window:";
        }

        public Task<object> GetPageSourceDocument(bool ReloadHtmlDoc)
        {
            return null;
        }

        public string GetCurrentPageSourceString()
        {
            return null;
        }

        public ObservableList<ElementLocator> GetElementFriendlyLocators(ElementInfo ElementInfo, PomSetting pomSetting = null)
        {
            throw new NotImplementedException();
        }
    }
}
