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
using Amdocs.Ginger.Common;
using Ginger;
using Ginger.ActionLib;
using GingerCore.Actions;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.BusinessFlowsLib
{
    /// <summary>
    /// Interaction logic for GingerActionEditPage.xaml
    /// </summary>
    public partial class GingerActionEditPage : Page
    {
        GenericWindow mWindow = null;
       

        public GingerActionEditPage(Act act)
        {
            InitializeComponent();

            //mAction = act;
            //DescriptionTextBox.BindControl(act, nameof(Act.Description));
            //OldClassNameTextBox.BindControl(act, nameof(Act.OldClassName));
            //ErrorLabel.BindControl(act, nameof(Act.Error));
            //ExInfoLabel.BindControl(act, nameof(Act.ExInfo));
            //IDTextBox.BindControl(act, nameof(Act.ID));

            //StatusImageMaker.BindControl(act, nameof(Act.StatusIcon));

            ////TODO: lazy loading on click only
            //ActionConfigPage ACP = new ActionConfigPage(act);
            //ConfigFrame.SetContent(ACP);

            //OutputFrame.SetContent(new ActionOutputPage(act));
        }

        Button RunButton;
        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool ShowCancelButton = true)
        {
            Button OKButton = new Button();
            OKButton.Content = "Ok";
            OKButton.Click += OKButtonClick;

            RunButton = new Button();
            RunButton.Content = "Run";
            RunButton.Click += RunButtonClick;

            ObservableList<Button> winButtons = new ObservableList<Button>();
            winButtons.Add(OKButton);
            winButtons.Add(RunButton);

            this.Height = 800;
            this.Width = 1000;
            GenericWindow.LoadGenericWindow(ref mWindow, null, windowStyle, this.Title, this, winButtons, ShowCancelButton, "Cancel");
        }

        private async void RunButtonClick(object sender, RoutedEventArgs e)
        {
            //RunButton.IsEnabled = false;
            //mAction.IsSingleAction = true; // prevent the move next, so we can do run action and stay on the same action
            //var v = await WorkSpace.Instance.GingerRunner.RunActionAsync(mAction);
            //mAction.IsSingleAction = false;
            //RunButton.IsEnabled = true;
        }

        void OKButtonClick(object sender, RoutedEventArgs e)
        {
            mWindow.Close();
        }
    }
}