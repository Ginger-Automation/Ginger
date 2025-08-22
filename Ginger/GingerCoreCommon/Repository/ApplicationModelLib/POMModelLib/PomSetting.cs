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


using System.Collections.Generic;
using System.ComponentModel;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib
{
    public class PomSetting : RepositoryItemBase
    {

        private ObservableList<UIElementFilter> mFilteredElementType;
        [IsSerializedForLocalRepository]
        public ObservableList<UIElementFilter> FilteredElementType { get { return mFilteredElementType; } set { if (mFilteredElementType != value) { mFilteredElementType = value; OnPropertyChanged(nameof(FilteredElementType)); } } }


        private ObservableList<ElementLocator> mElementLocatorsSettingsList;

        [IsSerializedForLocalRepository]
        public ObservableList<ElementLocator> ElementLocatorsSettingsList { get { return mElementLocatorsSettingsList; } set { if (mElementLocatorsSettingsList != value) { mElementLocatorsSettingsList = value; OnPropertyChanged(nameof(ElementLocatorsSettingsList)); } } }

        private string mSpecificFramePath;
        [IsSerializedForLocalRepository]
        public string SpecificFramePath { get { return mSpecificFramePath; } set { if (mSpecificFramePath != value) { mSpecificFramePath = value; OnPropertyChanged(nameof(SpecificFramePath)); } } }

        private ObservableList<CustomRelativeXpathTemplate> mRelativeXpathTemplateList;
        [IsSerializedForLocalRepository]
        public ObservableList<CustomRelativeXpathTemplate> RelativeXpathTemplateList { get { return mRelativeXpathTemplateList; } set { if (mRelativeXpathTemplateList != value) { mRelativeXpathTemplateList = value; OnPropertyChanged(nameof(RelativeXpathTemplateList)); } } }

        private bool mLearnScreenshotsOfElements;
        [IsSerializedForLocalRepository]
        public bool LearnScreenshotsOfElements { get { return mLearnScreenshotsOfElements; } set { if (mLearnScreenshotsOfElements != value) { mLearnScreenshotsOfElements = value; OnPropertyChanged(nameof(LearnScreenshotsOfElements)); } } }

        private bool misPOMLearn;
        [IsSerializedForLocalRepository]
        public bool isPOMLearn { get { return misPOMLearn; } set { if (misPOMLearn != value) { misPOMLearn = value; OnPropertyChanged(nameof(isPOMLearn)); } } }

        private bool mLearnShadowDomElements;
        [IsSerializedForLocalRepository]
        public bool LearnShadowDomElements { get { return mLearnShadowDomElements; } set { if (mLearnShadowDomElements != value) { mLearnShadowDomElements = value; OnPropertyChanged(nameof(LearnShadowDomElements)); } } }

        private bool mLearnPOMByAI;
        [IsSerializedForLocalRepository]
        public bool LearnPOMByAI { get { return mLearnPOMByAI; } set { if (mLearnPOMByAI != value) { mLearnPOMByAI = value; OnPropertyChanged(nameof(LearnPOMByAI)); } } }
        public override string ItemName
        {
            get;
            set;
        }

        private string _processingTime;
        public string ProcessingTime
        {
            get => _processingTime;
            set
            {
                _processingTime = value;
                OnPropertyChanged(nameof(ProcessingTime));
            }
        }
    }
}
