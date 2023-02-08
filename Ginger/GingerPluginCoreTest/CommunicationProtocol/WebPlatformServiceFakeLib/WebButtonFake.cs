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

using Ginger.Plugin.Platform.Web.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace GingerPluginCoreTest.CommunicationProtocol.WebPlatformServiceFakeLib
{
    public class WebButtonFake : GingerWebElementFake, IButton
    {
        public void Click()
        {
            
        }

        public void DoubleClick()
        {
            throw new NotImplementedException();
        }

        public string GetValue()
        {
            throw new NotImplementedException();
        }

        public void JavascriptClick()
        {
            throw new NotImplementedException();
        }

        public void MouseClick()
        {
            throw new NotImplementedException();
        }

        public void MultiClick()
        {
            throw new NotImplementedException();
        }

        public void Submit()
        {
            throw new NotImplementedException();
        }
    }
}
