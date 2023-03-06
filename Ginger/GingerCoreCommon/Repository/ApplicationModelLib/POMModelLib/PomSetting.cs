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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.Common.UIElement;

namespace Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib
{
    public class PomSetting
    {
        public List<eElementType> filteredElementType { get; set; }

        public ObservableList<ElementLocator> ElementLocatorsSettingsList { get; set; }

        public string SpecificFramePath { get; set; }

        public List<string> relativeXpathTemplateList { get; set; }

        public bool LearnScreenshotsOfElements { get; set; }

        public bool isPOMLearn { get; set; }

    }
}
