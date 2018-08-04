#region License
/*
Copyright © 2014-2018 European Support Limited

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

using GingerPACTPlugIn.ActionsLib;
using GingerPACTPlugIn.PACTTextEditorLib;
using GingerPlugIns;
using PactNet.Mocks.MockHttpService.Models;
using System;
using System.Collections.Generic;

namespace GingerPACTPlugIn
{
    public class PACTPlugIn : IGingerPlugIn
    {
        public string Description()
        {
            return "PACT Plugin - Mock service server/Service Virtulazion";
        }

        public string GetConfigPage()
        {
            throw new NotImplementedException();
        }

        public string Name()
        {
            return "PACT";
        }

        public string ID()
        {
            return "PACT";
        }

        public string PlugInVersion()
        {
            return "1.0";
        }

        public List<PlugInCapability> Capabilities()
        {
            // Here we add all the capabilities of this Plug In
            List<PlugInCapability> list = new List<PlugInCapability>();

            list.Add(new PACTActions());   // This plug in can add new actions and run them, we can add only one

            list.Add(new PACTTextEditor());  // This Plug in know to edit PACT file, we can add more then one 

            return list;
        }
    }
}
