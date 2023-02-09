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

using System;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.GeneralLib
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        bool mIsReady = false;

        public TestWindow(Page p)
        {
            InitializeComponent();

            MainFrame.Content = p;
        }

        public bool IsReady()
        {
            return mIsReady;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            mIsReady = true;
        }
    }
}
