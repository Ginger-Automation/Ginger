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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ginger
{
    /// <summary>
    /// Interaction logic for ucTag.xaml
    /// </summary>
    public partial class ucTag : UserControl
    {
        public Guid GUID { get { return mTag.Guid; } }

        RepositoryItemTag mTag;
        BrushConverter bc = new BrushConverter();
        public ucTag(RepositoryItemTag tag)
        {
            InitializeComponent();
            mTag = tag;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(lblTagName, Label.ContentProperty, mTag, RepositoryItemTag.Fields.Name);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(lblTagName, Label.ToolTipProperty, mTag, RepositoryItemTag.Fields.Description);            
            closeImage.Visibility = Visibility.Hidden;

            xDeleteTagBtn.Tag = tag;

            tagStack.ToolTip = tag.Name;
            lblTagName.ToolTip = tag.Name;
        }

        public void SetLabelText(string text)
        {
            lblTagName.Content = text;
        }


        private void tagStack_MouseLeave(object sender, MouseEventArgs e)
        {
            tagStack.Background = (Brush)FindResource("$SelectionColor_VeryLightBlue");
            closeImage.Visibility = Visibility.Collapsed;
        }

        private void tagStack_MouseEnter(object sender, MouseEventArgs e)
        {
            tagStack.Background = (Brush)FindResource("$SelectionColor_Pink");
            closeImage.Visibility = Visibility.Visible;
        }
    }
}