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

using System.Windows.Controls;
using GingerCore.Actions.Common;

namespace Ginger.Actions._Common
{
    /// <summary>
    /// Interaction logic for ActDeviceEditPage.xaml
    /// </summary>
    public partial class ActDeviceEditPage : Page
    {
        ActDevice mAct = null;

        public ActDeviceEditPage(ActDevice act)
        {
            InitializeComponent();

            mAct = act;

            DeviceActionComboBox.BindControl(mAct, ActDevice.Fields.DeviceAction);
            SizeTextBox.BindControl(mAct.GetOrCreateInputParam(ActDevice.Fields.Size));
            BitRateTextBox.BindControl(mAct.GetOrCreateInputParam(ActDevice.Fields.BitRate));
            TimeLimitTextBox.BindControl(mAct.GetOrCreateInputParam(ActDevice.Fields.TimeLimit));
        }
    }
}
