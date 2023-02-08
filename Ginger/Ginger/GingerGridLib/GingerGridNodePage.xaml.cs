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

using GingerCoreNET.RunLib;
using System.Windows.Controls;

namespace Ginger.GingerGridLib
{
    /// <summary>
    /// Interaction logic for GingerGridNodePage.xaml
    /// </summary>
    public partial class GingerGridNodePage : Page
    {
        GingerNodeProxy mGingerNodeProxy;

        public GingerGridNodePage(GingerNodeProxy gingerNodeProxy)
        {
            InitializeComponent();
            mGingerNodeProxy = gingerNodeProxy;
            GingerNodeNameLabel.Content = gingerNodeProxy.Description;
        }

        private void StartLiveView()
        {
        }

        private void ScreenShotButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StartLiveView();
        }

        private void StartDriverButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!mGingerNodeProxy.IsConnected)
            {
                mGingerNodeProxy.Reserve();
            }
           
        }
    }
}
