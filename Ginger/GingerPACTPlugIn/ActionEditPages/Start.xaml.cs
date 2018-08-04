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
using System.Windows;
using System.Windows.Controls;

namespace GingerPACTPlugIn.ActionEditPages
{
    /// <summary>
    /// Interaction logic for Start.xaml
    /// </summary>
    public partial class Start : Page
    {
        GingerAction mAct;

        public Start(GingerAction act)
        {
            InitializeComponent();

            mAct = act;
            
            ActionParam AP2 = mAct.GetOrCreateParam("Port", "1234");
            Port.BindControl(AP2);

            ActionParam AP3 = mAct.GetOrCreateParam("DynamicPort", "false");

            DynamicPortCheckBox.BindControl(AP3);
        }

        private void DynamicPortChecked(object sender, RoutedEventArgs e)
        {
            Port.IsEnabled = false;
        }

        private void DynamicPortUnchecked(object sender, RoutedEventArgs e)
        {
            Port.IsEnabled = true;
        }
    }
}
