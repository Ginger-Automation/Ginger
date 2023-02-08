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

using Amdocs.Ginger.Common.UIElement;
using GingerCore.Actions.Common;
using GingerCore.Actions.MainFrame;
using Open3270.TN3270;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GingerCore.Drivers.MainFrame
{
    public static class MainFrameUIHelper
    {
        /*
         *Method Calling Sequence
         *
         * 1: SetupMainframeDriverComponents - ONly Single When Launching Window it will set the relative sizes
         * 2: AddComponentsToScreen - call everytime when screen changes
         * 3: todo Resize all items : Whenever size change event get fired
         * */
        public static Brush DefaultBursh = Brushes.White;

        public static void RefreshCamvasComponents(MainFrameDriver MF, MainFrameDriverWindow MFDW, Canvas ConsoleCanvas, XMLScreen ScreenElements, XMLScreenField MXF = null)
        {
            DefaultBursh = Brushes.Cyan;
            ConsoleCanvas.Children.Clear();
            if (MFDW.HeightPerRow == 00 || MFDW.WidthPerCharachter == 00 || Double.IsInfinity(MFDW.WidthPerCharachter) || Double.IsInfinity(MFDW.HeightPerRow) || Double.IsNaN(MFDW.WidthPerCharachter) || Double.IsNaN(MFDW.HeightPerRow))
            {
                SetupMainframeDriverComponents(MF, MFDW, ConsoleCanvas);
            }

            if (MXF != null)
            {
                Control C = GetControlFromScreenField(MXF);

                Brush Brsh = CalculateBrush(MXF);
                ConfigureControl(C, MXF, ConsoleCanvas, MFDW);
                C.Foreground = Brsh;

                SetControlLocation(C, MFDW);
                return;
            }

            if (ScreenElements.Fields == null || ScreenElements == null)
            {
                return;
            }
            foreach (XMLScreenField XF in ScreenElements.Fields)
            {
                Control C = GetControlFromScreenField(XF);
                if (C == null)
                {
                    continue;
                }
                Brush Brsh = CalculateBrush(XF);
                ConfigureControl(C, XF, ConsoleCanvas, MFDW);
                C.Foreground = Brsh;

                SetControlLocation(C, MFDW);
            }
        }

        private static Control GetControlFromScreenField(XMLScreenField XF)
        {
            Console.WriteLine(XF.Location.length);
            Console.WriteLine(XF.Text);
            if (XF.Attributes.Protected && (XF.Attributes.FieldType == "Hidden" || XF.Location.length == 0))
            {
                return null;
            }
           
            if (XF.Attributes.Protected&& !String.IsNullOrEmpty(XF.Text))
            {
                return (Control)AddLabel(XF);
            }
            else if (XF.Attributes.FieldType == "Hidden")
            {
                return (Control)AddPasswordBox(XF);
            }
            else
            {
                return (Control)AddTextBox(XF);
            }
        }

        private static Brush CalculateBrush(XMLScreenField XF)
        {
            if (XF == null || XF.Attributes == null)
            {
                return DefaultBursh;
            }

            if (XF.Attributes.Foreground != null)
            {
                switch (XF.Attributes.Foreground)
                {
                    case "neutralBlack":
                        return new SolidColorBrush(Colors.Black);

                    case "blue":
                        return new SolidColorBrush(Colors.SkyBlue);

                    case "red":
                        return new SolidColorBrush(Colors.Red);
                    case "pink":
                        return new SolidColorBrush(Colors.Pink);
                    case "green":
                        return new SolidColorBrush(Colors.Green);

                    case "turquoise":
                        return new SolidColorBrush(Colors.Turquoise);
                    case "yellow":
                        return new SolidColorBrush(Colors.Yellow);
                    case "neutralWhite":
                        return new SolidColorBrush(Colors.WhiteSmoke);
                    case "black":
                        return new SolidColorBrush(Colors.Black);
                    case "deepBlue":
                        return new SolidColorBrush(Colors.DeepSkyBlue);
                    case "orange":
                        return new SolidColorBrush(Colors.Orange);
                    case "purple":
                        return new SolidColorBrush(Colors.Purple);
                    case "paleGreen":
                        return new SolidColorBrush(Colors.PaleGreen);
                    case "paleTurquoise":
                        return new SolidColorBrush(Colors.PaleTurquoise);
                    case "grey":
                        return new SolidColorBrush(Colors.Gray);
                    case "white":
                    default:
                        return new SolidColorBrush(Colors.White);
                }
            }
            else
            {
                if (XF.Attributes.FieldType == "High")
                {
                    return Brushes.Red;
                }
                return DefaultBursh;
            }            
        }

        public static void SetupMainframeDriverComponents(MainFrameDriver mDriver, MainFrameDriverWindow MFDW, Canvas ConsoleCanvas)
        {
            ConsoleCanvas.Children.Clear();
            string dots = String.Empty;
            for (int i = 0; i < mDriver.MFColumns; i++)
            {
                dots += ".";
            }
            List<object> LO = new List<object>();

            LO.Add(mDriver);
            LO.Add(MFDW);
            MFDW.HeightPerRow = ConsoleCanvas.ActualHeight / mDriver.MFRows;

            Label l = new Label();
            l.FontSize = 20;
            l.Tag = LO;
            l.Foreground = Brushes.White;
            l.FontFamily = new FontFamily("Courier New");
            ConsoleCanvas.Children.Add(l);
            l.Content = dots;
            l.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            l.Loaded += label_loaded;
        }

        private static void label_loaded(object sender, RoutedEventArgs e)
        {
            Label l = (Label)sender;

            List<object> lo = (List<object>)l.Tag;
            MainFrameDriver mdv = (MainFrameDriver)lo.ElementAt(0);
            MainFrameDriverWindow mdw = (MainFrameDriverWindow)lo.ElementAt(1);
            double d = l.ActualWidth / mdv.MFColumns;
            mdw.WidthPerCharachter = d;

            mdw.MinWidth = (mdv.MFColumns + 5) * d;

            mdw.MinHeight = (mdv.MFRows) * (l.ActualHeight);
            Console.WriteLine(d.ToString());
            mdw.Refresh();
        }

        private static void ConfigureControl(Control c, XMLScreenField x, Canvas ConsoleCanvas, MainFrameDriverWindow MFDW)
        {
            c.Tag = x;
            SetControlLocation(c, MFDW);
            ConsoleCanvas.Children.Add(c);
            c.GotFocus += Control_GotFocus;
            c.MouseEnter += Control_GotFocus;
        }

        private static void Control_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                Control C = (Control)sender;
                XMLScreenField XF = (XMLScreenField)C.Tag;
                MainFrameDriverWindow MFDW = (MainFrameDriverWindow)Window.GetWindow(C);
                if (MFDW == null)
                {
                    return;
                }
                MFDW.CaretXY.Text = XF.Location.left + "/" + XF.Location.top;
                MFDW.CaretIndex.Text = XF.Location.position.ToString();
            }
            finally
            {
            }
        }

        private static void SetControlLocation(Control c, MainFrameDriverWindow MFDW)
        {
            if (!c.Tag.GetType().ToString().Contains("XMLScreenField"))
            {
                return;
            }
            XMLScreenField Xldf = ((XMLScreenField)c.Tag);
            c.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            c.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            c.Margin = new Thickness(MFDW.WidthPerCharachter * Xldf.Location.left, MFDW.HeightPerRow * Xldf.Location.top, 0, 0);

            
            if (c.GetType() == typeof(TextBox))
            {
                ((TextBox)c).FontSize = 20;
            }
            else if (c.GetType() == typeof(PasswordBox))
            {
                ((PasswordBox)c).FontSize = 20;
            }
            else if (c.GetType() == typeof(Label))
            {
                ((Label)c).FontSize = 20;
            }
        }

        private static TextBox AddTextBox(XMLScreenField x)
        {
            TextBox TB = new TextBox();

            if (!String.IsNullOrEmpty(x.Text))
            {
                TB.Text = x.Text;
            }
            TB.MaxLength = x.Location.length;
            TB.LostFocus += Control_LostFocus;
            TB.BorderBrush = Brushes.YellowGreen;
            TB.Background = Brushes.Black;
            return TB;
        }

        public static void TB_KeyDown(object sender, KeyEventArgs e)
        {
            return;
        }

        private static void Control_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                MainFrameDriverWindow MDW = (MainFrameDriverWindow)(Window.GetWindow((Control)sender));
                if (MDW == null)
                {
                    return;
                }
                SetTextoMainframe(sender);

                MDW.Refresh();
            }
            finally
            {
            }
        }

        private static void SetTextoMainframe(object sender)
        {
            MainFrameDriver mdriver = null;
            XMLScreenField XF = null;
            string Command = null;
            MainFrameDriverWindow MDW = null;
            bool IsFeildPassword = false;
            if (sender.GetType() == typeof(TextBox))
            {
                TextBox TB = (TextBox)sender;
                XF = (XMLScreenField)TB.Tag;
                MDW = (MainFrameDriverWindow)Window.GetWindow(TB);
                if (MDW == null)
                {
                    return;
                }
                mdriver = MDW.mDriver;
                Command = TB.Text;
            }
            else if (sender.GetType() == typeof(PasswordBox))
            {
                IsFeildPassword = true;
                PasswordBox PWB = (PasswordBox)sender;
                XF = (XMLScreenField)PWB.Tag;
                MDW = (MainFrameDriverWindow)Window.GetWindow(PWB);
                mdriver = MDW.mDriver;
                Command = PWB.Password;
            }
            else
            {
                return;
            }

            if (XF == null || String.IsNullOrEmpty(Command) || mdriver == null)
            {
             
            }
            else
            {
                mdriver.SetTextAtPosition(Command, XF.Location.left, XF.Location.top, false);
                if (MDW.RecordBtn.IsChecked == true)
                {
                    try
                    {
                        ActMainframeSetText AMFST = new ActMainframeSetText();

                        AMFST.LocateBy = eLocateBy.ByCaretPosition;
                        AMFST.LocateValue = XF.Location.position.ToString();
                        AMFST.Value = Command;
                        if (mdriver.mBusinessFlow == null)
                        {
                            return;
                        }
                        if (IsFeildPassword)
                        {
                            AMFST.Description = "Set Password ";
                        }
                        else
                        {
                            AMFST.Description = "Set Text: " + Command;
                        }

                        mdriver.mBusinessFlow.CurrentActivity.Acts.Add((Actions.Act)AMFST);
                    }
                    finally
                    {
                    }
                }
            }
        }

        private static PasswordBox AddPasswordBox(XMLScreenField x)
        {
            PasswordBox PWB = new PasswordBox();
            if (!String.IsNullOrEmpty(x.Text))
            { PWB.Password = x.Text; };
            PWB.MaxLength = x.Location.length;
            PWB.LostFocus += Control_LostFocus;

            PWB.BorderThickness = new Thickness(1, 1, 1, 1);
            PWB.BorderBrush = Brushes.Green;
            PWB.Background = Brushes.Black;
            return PWB;
        }

        private static Label AddLabel(XMLScreenField x)
        {
            Label l = new Label() { Content = x.Text };
            l.BorderThickness = new Thickness(1);
            l.BorderBrush = Brushes.White;
            l.FontFamily = new FontFamily("Courier New");
            l.Background = Brushes.Black;
            return l;
        }
    }
}
