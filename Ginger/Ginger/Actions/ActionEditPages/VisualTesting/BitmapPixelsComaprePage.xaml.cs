#region License
/*
Copyright © 2014-2025 European Support Limited

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

using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.VisualTesting;
using System.Windows.Controls;

namespace Ginger.Actions.VisualTesting
{
    /// <summary>
    /// Interaction logic for BitmapPixelsComaprePage.xaml
    /// </summary>
    public partial class BitmapPixelsComaprePage : Page
    {
        public VisualCompareAnalyzerIntegration visualCompareAnalyzerIntegration = new VisualCompareAnalyzerIntegration();

        public BitmapPixelsComaprePage(ActVisualTesting mAct)
        {
            InitializeComponent();

            InitLayout();
            ActInputValue AIV = mAct.GetOrCreateInputParam(ActVisualTesting.Fields.ErrorMetric);
            ErrorMetricComboBox.Init(AIV, typeof(ImageMagick.ErrorMetric));
        }

        public void InitLayout()
        {
            visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetScreenSizeSelectionVisibility, eVisualTestingVisibility.Visible);
            visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetBaselineSectionVisibility, eVisualTestingVisibility.Visible);
            visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetTargetSectionVisibility, eVisualTestingVisibility.Visible);
            visualCompareAnalyzerIntegration.OnVisualTestingEvent(VisualTestingEventArgs.eEventType.SetResultsSectionVisibility, eVisualTestingVisibility.Visible);
        }
    }
}
