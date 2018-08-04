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

using GingerPlugIns;
using GingerPlugIns.ActionsLib;
using GingerPlugIns.GeneralLib;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GingerPACTPlugIn.ActionEditPages
{
    /// <summary>
    /// Interaction logic for AddInteractionEditPage.xaml
    /// </summary>
    public partial class AddInteractionEditPage : Page
    {
        GingerAction mAct;

        List<HTTPHeader> RequestHeaders = new List<HTTPHeader>();

        List<HTTPHeader> ResponseHeaders = new List<HTTPHeader>();

        public AddInteractionEditPage(GingerAction act)
        {
            InitializeComponent();

            mAct = act;

            ActionParam AP1 = mAct.GetOrCreateParam("ProviderState");
            ProviderStateTextBox.BindControl(AP1);

            ActionParam AP2 = mAct.GetOrCreateParam("Description");
            DescriptionTextBox.BindControl(AP2);

            List<ComboBoxListItem> HTTPMethodList = new List<ComboBoxListItem>();
            HTTPMethodList.Add(new ComboBoxListItem() { Text = "Get", Value = "Get"});
            HTTPMethodList.Add(new ComboBoxListItem() { Text = "Head", Value = "Head" });
            HTTPMethodList.Add(new ComboBoxListItem() { Text = "Not Set", Value = "NotSet" });
            HTTPMethodList.Add(new ComboBoxListItem() { Text = "Patch", Value = "Patch" });
            HTTPMethodList.Add(new ComboBoxListItem() { Text = "Post", Value = "Post" });
            HTTPMethodList.Add(new ComboBoxListItem() { Text = "Put", Value = "Put" });            

            ActionParam AP3 = mAct.GetOrCreateParam("RequestTypeComboBox");
            RequestMethodComboBox.BindControl(AP3, HTTPMethodList);

            ActionParam AP5 = mAct.GetOrCreateParam("Path");
            PathTextBox.BindControl(AP5);

            List<ComboBoxListItem> StatusList = new List<ComboBoxListItem>();
            StatusList.Add(new ComboBoxListItem() { Text = "OK", Value = "200" });
            StatusList.Add(new ComboBoxListItem() { Text = "Created", Value = "201" });
            StatusList.Add(new ComboBoxListItem() { Text = "No Content", Value = "204" });
            StatusList.Add(new ComboBoxListItem() { Text = "Not Modified", Value = "304" });
            StatusList.Add(new ComboBoxListItem() { Text = "Bad Request", Value = "400" });
            StatusList.Add(new ComboBoxListItem() { Text = "Unauthorized", Value = "401" });
            StatusList.Add(new ComboBoxListItem() { Text = "Forbidden", Value = "403" });
            StatusList.Add(new ComboBoxListItem() { Text = "Not Found", Value = "404" });
            StatusList.Add(new ComboBoxListItem() { Text = "Conflict", Value = "409" });
            StatusList.Add(new ComboBoxListItem() { Text = "Internal Server Error", Value = "500" });
            StatusList.Add(new ComboBoxListItem() { Text = "Service Unavailable", Value = "503" });

            //TODO: after the above most common enable the user to see all available http code: http://www.restapitutorial.com/httpstatuscodes.html

            //TODO: fill the rest
            ActionParam AP4 = mAct.GetOrCreateParam("Status");
            ResponseStatusComboBox.BindControl(AP4, StatusList);
            RequestHeadersGrid.ItemsSource = RequestHeaders;
            RepsonseHeadersGrid.ItemsSource = ResponseHeaders;
        }

        private void AddRequestHeadersButton_Click(object sender, RoutedEventArgs e)
        {
            AddHeadersWindow w = new AddHeadersWindow(RequestHeaders);
            w.ShowDialog();
            RequestHeadersGrid.ItemsSource = RequestHeaders;
        }
    }
}
