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


namespace GingerCore.ALM.Rally
{
    /// <summary>
    /// </summary>
    public class RallyTestStep
    {
        public RallyTestStep(string name, string rallyIndex, string description, string expectedResult)
        {
            Name = name;
            RallyIndex = rallyIndex;
            Description = description;
            ExpectedResult = expectedResult;
        }

        public string Name { get; set; }

        public string RallyIndex { get; set; }

        public string Description { get; set; }

        public string ExpectedResult { get; set; }
    }
}
