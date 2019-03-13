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
using System.Windows.Input;

namespace GingerCore.Animation 
{
    /// <summary>
    /// Interaction logic for BotDialog.xaml 
    /// </summary>
    public partial class BotDialog : Window
    {
        public BotDialog()
        {
            InitializeComponent();
        }

        private void BtnExitHover_MouseLeave(object sender, MouseEventArgs e)
        {
            BtnExit.Height = 24;
            BtnExitHover.Height = 0;
        }

        private void BtnExit_MouseEnter(object sender, MouseEventArgs e)
        {
            BtnExit.Height = 0;
            BtnExitHover.Height = 24;
        }

        private void BtnExitHover_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        public void Init(string header, string message)
        {
            TB_BoardHeader.Text = header;
            TB_MSGBoard.Text = message;
        }
    }
}
