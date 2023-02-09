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
using GingerCore.Actions;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions.ActionEditPages
{
    /// <summary>
    /// Interaction logic for ActValidationEditPage.xaml
    /// </summary>
    public partial class ActValidationEditPage : Page
    {
        ActValidation mActValidation;
        public ActValidationEditPage(ActValidation actValidation)
        {
            mActValidation = actValidation;
            InitializeComponent();
            xCalcEngineUCRadioButtons.Init(typeof(ActValidation.eCalcEngineType), xCalcEngineRBsPanel, mActValidation.GetOrCreateInputParam(nameof(ActValidation.CalcEngineType), ActValidation.eCalcEngineType.VBS.ToString()), CalcEngineType_SelectionChanged);          
            xValidationUCValueExpression.Init(Context.GetAsContext(mActValidation.Context), mActValidation, nameof(ActValidation.Condition));
            SetWarnMsgView();
        }
        private void CalcEngineType_SelectionChanged(object sender, RoutedEventArgs e)
        {
            SetWarnMsgView();
        }

        private void SetWarnMsgView()
        {
            if (mActValidation.CalcEngineType == ActValidation.eCalcEngineType.CS)
            {
                xVBSWarnHelpLabel.Visibility = Visibility.Collapsed;
            }
            else
            {
                xVBSWarnHelpLabel.Visibility = Visibility.Visible;
            }
        }

    }
}
