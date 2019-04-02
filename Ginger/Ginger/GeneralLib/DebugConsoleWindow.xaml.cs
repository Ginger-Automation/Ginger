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
using Amdocs.Ginger.Common.GeneralLib;
using Ginger;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GingerWPF
{
    //TODO: Add save to file
    /// <summary>
    /// Interaction logic for DebugConsole.xaml
    /// </summary>
    public partial class DebugConsoleWindow : Page
    {
        //TODO: why we need to cosoletextwriter?
        public ConsoleTextWriter output;
        public ConsoleTextWriter errors;

        GenericWindow mConsoleWindow;
        public DebugConsoleWindow()
        {
            InitializeComponent();
            output = new ConsoleTextWriter(ConsoleTextBlock);
            errors = new ConsoleTextWriter(ConsoleTextBlock, Brushes.Red);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {            
            ConsoleTextBlock.Inlines.Clear();
        }

        private void OnTopButton_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Topmost = true;
        }

        public void ShowAsWindow()
        {

            Console.SetOut(this.output);
            Console.SetError(this.errors);

            Button CopyToClipboradBtn = new Button();
            CopyToClipboradBtn.Content = "Copy to Clipboard";
            CopyToClipboradBtn.Click += new RoutedEventHandler(CopyToClipboradBtn_Click);

            Button clearConsoleButton = new Button();
            clearConsoleButton.Content = "Clear";
            clearConsoleButton.Click += new RoutedEventHandler(ClearButton_Click);

           

            Button onTopButton = new Button();
            onTopButton.Content = "Pin on Top";
            onTopButton.Click += new RoutedEventHandler(OnTopButton_Click);

            Amdocs.Ginger.Common.ObservableList<Button> winButtons = new Amdocs.Ginger.Common.ObservableList<Button>();
            
                       
            winButtons.Add(onTopButton);
            winButtons.Add(clearConsoleButton);
            winButtons.Add(CopyToClipboradBtn);


            this.Width = 800;
            this.Height = 600;
            GingerCore.General.LoadGenericWindow(ref mConsoleWindow, App.MainWindow, eWindowShowStyle.Free, "Ginger - Smart Console", this, winButtons);
        }

        private void CopyToClipboradBtn_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(output.GetText()+"\n"+ errors.GetText());
        }
     
    }


    //TODO: Move to separate class
    public class ConsoleTextWriter : TextWriter
    {
        TextBlock mTextBlock = null;
        int ErrorsCounter = 0;

        public ConsoleTextWriter(TextBlock output, Brush brush = null)
        {
            mTextBlock = output;
        }

        public string GetText()
        {
            return mTextBlock.Text;
        }

        public override void WriteLine(string value)
        {
            base.WriteLine(value);


            mTextBlock.Dispatcher.BeginInvoke(new Action(() =>
            {
                //TODO: add expander make it cooler
                //Expander exp = new Expander() { Width = 50, Height = 50 };
                //exp.Content = ...
                //mTextBlock.Inlines.Add(exp);


                if (value.Contains("Error"))
                {
                    Label l = new Label() { Content = "=======>", Foreground = Brushes.Red };
                    mTextBlock.Inlines.Add(l);
                    ErrorsCounter++;
                }

                if (value.Contains("INFO"))
                {
                    System.Windows.Shapes.Ellipse e = new System.Windows.Shapes.Ellipse() { Width = 10, Height = 10, Stroke = Brushes.Yellow, StrokeThickness = 2 };
                    mTextBlock.Inlines.Add(e);
                }



                mTextBlock.Inlines.Add(value.ToString() + Environment.NewLine);
                ((ScrollViewer)mTextBlock.Parent).ScrollToEnd();
            }));
        }
        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}
