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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using amdocs.ginger.GingerCoreNET;
using Ginger.Run;
using GingerWeb.UsersLib;
using Microsoft.AspNetCore.Mvc;

namespace GingerWeb.Controllers
{
    [Route("api/[controller]")]
    public class RunSetController : Controller
    {
        

        [HttpGet("[action]")]
        public IEnumerable<object> RunSets()
        {        
            IEnumerable<RunSetConfig> runSets = General.SR.GetAllRepositoryItems<RunSetConfig>().OrderBy(x => x.Name);
            var data = runSets.Select(x =>
                                    new
                                    {
                                        name = x.Name,
                                        description = x.Description,                                        
                                    });

            return data;
        }


        public class RunRunSetResult
        {
            public string name { get; set; }
            public string Status { get; internal set; }
        }

        public class RunRunSetRequest
        {
            public string name { get; set; }
        }


        [HttpPost("[action]")]
        public RunRunSetResult RunRunSet([FromBody] RunRunSetRequest runRunSetRequest)
        {
            RunRunSetResult runBusinessFlowResult = new RunRunSetResult();

            if (string.IsNullOrEmpty(runRunSetRequest.name))
            {
                runBusinessFlowResult.Status = "Name cannot be null";
                return runBusinessFlowResult;
            }

            RunSetConfig runSet = (from x in General.SR.GetAllRepositoryItems<RunSetConfig>() where x.Name == runRunSetRequest.name select x).SingleOrDefault();
            if (runSet == null)
            {
                runBusinessFlowResult.Status = "Name cannot be null";
                return runBusinessFlowResult;
            }

            RunrunSet(runSet);

            runBusinessFlowResult.Status = "Executed";



            return runBusinessFlowResult;
        }

        private void RunrunSet(RunSetConfig runSetConfig)
        {
            RunsetExecutor runsetExecutor = new RunsetExecutor();
            runsetExecutor.RunSetConfig = runSetConfig;
            WorkSpace.Instance.RunsetExecutor = runsetExecutor;
            runsetExecutor.RunRunset();
        }
    }
}