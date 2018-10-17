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
    //TODO: Add clear button, add copy to clipboard, save to file
    /// <summary>
    /// Interaction logic for DebugConsole.xaml
    /// </summary>
    public partial class DebugConsoleWindow : Page
    {
        public ConsoleTextWriter output;
        public ConsoleTextWriter errors;
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

        static Window mWindow = null;
        public static void Show()
        {
            if (mWindow == null)
            {
                DebugConsoleWindow ConsoleWindow = new DebugConsoleWindow();
                mWindow = new Window();
                mWindow.Title = "Ginger - Smart Console";
                mWindow.Content = ConsoleWindow;
                Console.SetOut(ConsoleWindow.output);
                Console.SetError(ConsoleWindow.errors);                
            }

            mWindow.Show();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void CrashButton_Click(object sender, RoutedEventArgs e)
        {
            ThreadStart newThreadStart = new ThreadStart(newThread_Execute);
            Thread newThread = new Thread(newThreadStart);
            newThread.Start();
        }

        void newThread_Execute()
        {
            throw new Exception("Thread crash");
        }

        private void NewMainWindowButton_Click(object sender, RoutedEventArgs e)
        {
            WorkSpace.Instance.LoadUserProfile();
            NewMainWindow w = new NewMainWindow();
            w.Show();
        }

        private void LongPathButton_Click(object sender, RoutedEventArgs e)
        {
            io.testPath();
        }
    }


    //TODO: Move to seperate class
    public class ConsoleTextWriter : TextWriter
    {
        TextBlock mTextBlock = null;
        int ErrorsCounter = 0;

        public ConsoleTextWriter(TextBlock output, Brush brush = null)
        {
            mTextBlock = output;
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
