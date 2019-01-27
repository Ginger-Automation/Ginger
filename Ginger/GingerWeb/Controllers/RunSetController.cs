using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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


        //[HttpPost("[action]")]
        //public RunBusinessFlowResult RunBusinessFlow([FromBody] RunBusinessFlowRequest runBusinessFlowRequest)
        //{
        //    RunBusinessFlowResult runBusinessFlowResult = new RunBusinessFlowResult();

        //    if (string.IsNullOrEmpty(runBusinessFlowRequest.name))
        //    {
        //        runBusinessFlowResult.Status = "Name cannot be null";
        //        return runBusinessFlowResult;
        //    }

        //    BusinessFlow BF = (from x in General.SR.GetAllRepositoryItems<BusinessFlow>() where x.Name == runBusinessFlowRequest.name select x).SingleOrDefault();
        //    if (BF == null)
        //    {
        //        runBusinessFlowResult.Status = "Name cannot be null";
        //        return runBusinessFlowResult;
        //    }

        //    RunFlow(BF);

        //    runBusinessFlowResult.Status = "Executed - BF.Status=" + BF.RunStatus;



        //    return runBusinessFlowResult;
        //}

    }
}