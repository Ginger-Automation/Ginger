using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amdocs.Ginger.Common;
using GingerCore;
using GingerWeb.RepositoryLib;
using GingerWeb.UsersLib;
using Microsoft.AspNetCore.Mvc;

namespace GingerWeb.Controllers
{
    [Route("api/[controller]")]
    public class SampleDataController : Controller
    {
        private static string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        //[HttpGet("[action]")]
        //public IEnumerable<WeatherForecast> WeatherForecasts()
        //{
        //    var rng = new Random();
        //    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        DateFormatted = DateTime.Now.AddDays(index).ToString("d"),
        //        TemperatureC = rng.Next(-20, 55),
        //        Summary = Summaries[rng.Next(Summaries.Length)]
        //    });
        //}

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


        //public class WeatherForecast
        //{
        //    public string DateFormatted { get; set; }
        //    public int TemperatureC { get; set; }
        //    public string Summary { get; set; }

        //    public int TemperatureF
        //    {
        //        get
        //        {
        //            return 32 + (int)(TemperatureC / 0.5556);
        //        }
        //    }
        //}
    }
}
