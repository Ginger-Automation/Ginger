using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amdocs.Ginger.Common;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerWeb.RepositoryLib;
using GingerWeb.UsersLib;
using Microsoft.AspNetCore.Mvc;

namespace GingerWeb.Controllers
{
    [Route("api/[controller]")]
    public class SampleDataController : Controller
    {
        // temp remove from here !!!!!!!!!!!
        static bool bDone;

        [HttpGet("[action]")]
        public IEnumerable<BusinessFlowWrapper> WeatherForecasts()
        {
            if (!bDone)
            {
                General.init();
                bDone = true;
            }

            ObservableList<BusinessFlow> Bfs = General.SR.GetAllRepositoryItems<BusinessFlow>();
            List<BusinessFlowWrapper> list = new List<BusinessFlowWrapper>();
            foreach (BusinessFlow businessFlow in Bfs)
            {
                list.Add(new BusinessFlowWrapper(businessFlow));
            }
            
            
            return list;
        }

        [HttpPost("[action]")]
        public BusinessFlowWrapper RunBusinessFlow(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "String Service List Concat";
                //return null;
            }

            BusinessFlow BF = (from x in General.SR.GetAllRepositoryItems<BusinessFlow>() where x.Name == name select x).SingleOrDefault();
            RunFlow(BF);
            BusinessFlowWrapper businessFlowWrapper = new BusinessFlowWrapper(BF);
            return businessFlowWrapper;
        }

        void RunFlow(BusinessFlow businessFlow)
        {
            GingerRunner gingerRunner = new GingerRunner();
            gingerRunner.RunBusinessFlow(businessFlow, true);

            Console.WriteLine("Execution completed");
            Console.WriteLine("Business Flow Status: " + businessFlow.RunStatus);
            foreach (Activity activity in businessFlow.Activities)
            {
                Console.WriteLine("Activity: " + activity.ActivityName + " Status: " + activity.Status);

                Console.WriteLine("Actions Found:" + activity.Acts.Count);
                foreach (Act act in activity.Acts)
                {
                    Console.WriteLine("--");
                    Console.WriteLine("Action:" + act.Description);
                    Console.WriteLine("Description:" + act.ActionDescription);
                    Console.WriteLine("Type:" + act.ActionType);
                    Console.WriteLine("Class:" + act.ActClass);
                    Console.WriteLine("Status:" + act.Status);
                    Console.WriteLine("Error:" + act.Error);
                    Console.WriteLine("ExInfo:" + act.ExInfo);
                }
            }
        }


    }
}
