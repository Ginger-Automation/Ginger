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

using System.Collections.Generic;
using System.Windows;

namespace GingerPACTPlugIn.ActionEditPages
{
    /// <summary>
    /// Interaction logic for AddHeadersWindow.xaml
    /// </summary>
    public partial class AddHeadersWindow : Window
    {
        List<HTTPHeader> mHeaders;

        List<HTTPHeader> list = new List<HTTPHeader>();

        public AddHeadersWindow(List<HTTPHeader> headers)
        {
            InitializeComponent();

            mHeaders = headers;
            
            list.Add(new HTTPHeader() { Key = "accept", Value= "application/json" });
            list.Add(new HTTPHeader() { Key = "accept-encoding", Value = "gzip, deflate" });
            list.Add(new HTTPHeader() { Key = "accept-language", Value = "en-US,en;q=0.8" });
            list.Add(new HTTPHeader() { Key = "user-agent", Value = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.101 Safari/537.36" });
            //TODO: add more common used headers

            MainGrid.ItemsSource = list;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            
            foreach (HTTPHeader h in list)
            {
                if (h.Selected)
                {
                    mHeaders.Add(h);
                }
            }
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
