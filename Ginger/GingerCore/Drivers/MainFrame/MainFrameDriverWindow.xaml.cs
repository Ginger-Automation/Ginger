#region License
/*
Copyright © 2014-2023 European Support Limited

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

using GingerCore.Actions.MainFrame;
using Open3270;
using Open3270.TN3270;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GingerCore.Drivers.MainFrame
{
    /// <summary>
    /// Interaction logic for Ginger_MainFrame_Emulator.xaml
    /// </summary>
    public partial class MainFrameDriverWindow : Window
    {
        public MainFrameDriver mDriver = null;
        public bool IsRecording;
        public double WidthPerCharachter = 20;
        public double HeightPerRow = 20;
        private TnKey TNkEy = TnKey.F1;
        public bool IsClosing;

        public MainFrameDriverWindow(MainFrameDriver MFD)
        {
            mDriver = MFD;

            InitializeComponent();

            GingerCore.General.FillComboFromEnumObj(KeytoSend, TNkEy);

            MainFrameUIHelper.SetupMainframeDriverComponents(mDriver, this, ConsoleCanvas);
        }
       
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void MainFrameWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsClosing = true;
            mDriver.CloseDriver();
        }

        #region custom-properties

        public static readonly DependencyProperty CaretLocationProperty =
            DependencyProperty.RegisterAttached("CaretLocation", typeof(int), typeof(AttachedProperties), new PropertyMetadata(new PropertyChangedCallback(CaretChanged)));

        private static void CaretChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBox tb = d as TextBox;
            if (tb != null)
            {
                tb.CaretIndex = (int)e.NewValue;
            }
        }

        #endregion custom-properties

        public void Refresh()
        {
            try
            {
                XMLScreen XMLS = mDriver.MFE.GetScreenAsXML();

                MainFrameUIHelper.RefreshCamvasComponents(mDriver, this, ConsoleCanvas, XMLS, null);
            }
            catch (Exception mfe)
            {
                XMLScreen XMLS = new XMLScreen();
                XMLScreenField XF = new XMLScreenField();
                XF.Text = "Mainframe not Connected " + mfe.Message;
                XF.Attributes = new XMLScreenAttributes();
                XF.Attributes.Protected = true;
                XF.Location = new XMLScreenLocation();
                XF.Location.position = 0;
                XF.Location.top = 0;
                XF.Location.left = 0;
                XF.Location.length = XF.Text.Length;

                MainFrameUIHelper.RefreshCamvasComponents(mDriver, this, ConsoleCanvas, XMLS, XF);
            }
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void CaretIndex_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<int> XY = mDriver.MFE.GetXYfromCaretIndex(mDriver.CaretIndex);

            CaretXY.Text = XY.ElementAt(0) + @"/" + XY.ElementAt(1);
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            mDriver.MFE.SendKey(TnKey.Reset);
        }

        private void ConsoleCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MainFrameUIHelper.SetupMainframeDriverComponents(mDriver, this, ConsoleCanvas);
        }

        private void SendKey_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TnKey key = (TnKey)Enum.Parse(typeof(TnKey), KeytoSend.SelectedValue.ToString());
                mDriver.MFE.SendKey(key);

                if (RecordBtn.IsChecked == true)
                {
                    if (mDriver.mBusinessFlow == null)
                        return;
                    ActMainframeSendKey AMSK = new ActMainframeSendKey();
                    AMSK.Description = ("Send " + key.ToString() + " Key to Mainframe ");
                    mDriver.mBusinessFlow.CurrentActivity.Acts.Add((Actions.Act)AMSK);
                }

                Refresh();
            }
            finally
            {
            }
        }
    }
}