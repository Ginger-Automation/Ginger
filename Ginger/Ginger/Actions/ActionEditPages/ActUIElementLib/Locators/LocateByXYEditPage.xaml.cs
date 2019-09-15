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

using System.Windows;
using System.Windows.Controls;
using GingerCore.Actions.Common;

namespace Ginger.Actions._Common.ActUIElementLib
{
    /// <summary>
    /// Interaction logic for LocateValueEditPage.xaml
    /// </summary>
    public partial class LocateByXYEditPage : Page
    {
        ActUIElement mAction;
        string mlocateValueField;
        object mlocateValueParentObject;

        public LocateByXYEditPage(ActUIElement Action, object locateValueParentObject, string locateValueField)
        {
            InitializeComponent();

            mAction = Action;
            mlocateValueField = locateValueField;
            mlocateValueParentObject = locateValueParentObject;

            double X;
            double Y;


            mAction.GetLocateByXYValues(out X, out Y, mlocateValueParentObject, mlocateValueField);

            txtLocateValueX.ValueTextBox.Text = X + "";
            txtLocateValueY.ValueTextBox.Text = Y + "";
        }

        private void txtLocateValueXY_LostFocus(object sender, RoutedEventArgs e)
        {
            double X;
            bool b = double.TryParse(txtLocateValueX.ValueTextBox.Text, out X);
            if (!b) X = 0;

            double Y;
            bool b2 = double.TryParse(txtLocateValueY.ValueTextBox.Text, out Y);
            if (!b2) Y = 0;
                        
            mAction.SetLocateByXYValues(X, Y, mlocateValueParentObject, mlocateValueField);
        }
    }
}
