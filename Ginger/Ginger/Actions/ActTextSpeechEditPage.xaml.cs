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

using GingerCore.Actions;
using System.Windows.Controls;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActTextSpeechEditPage.xaml
    /// </summary>
    public partial class ActTextSpeechEditPage : Page
    {
        ActTextSpeech mAct;
        public ActTextSpeechEditPage(ActTextSpeech act)
        {
            InitializeComponent();
            mAct = act;
            Bind();
        }

        public void Bind()
        {
            App.FillComboFromEnumVal(TextSpeechActionComboBox, mAct.TextSpeechAction);
            App.ObjFieldBinding(TextSpeechActionComboBox, ComboBox.TextProperty, mAct, ActTextSpeech.Fields.TextSpeechAction);
            App.ObjFieldBinding(IntervalTextBox, TextBox.TextProperty, mAct, ActTextSpeech.Fields.Interval);
            App.ObjFieldBinding(WaveLocationTextBox, TextBox.TextProperty, mAct, ActTextSpeech.Fields.WaveLocation);
            App.ObjFieldBinding(TextToSayLoudTextBox, TextBox.TextProperty, mAct, ActTextSpeech.Fields.TextToSayLoud);
        }
    }
}
