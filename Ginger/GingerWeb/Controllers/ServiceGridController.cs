using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using GingerCoreNET.RunLib;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;


namespace GingerWeb.Controllers
{
    [Route("api/[controller]")]    
    public class ServiceGridController : ControllerBase
    {
        // GET: api/ServiceGrid/NodeList
        [HttpGet("[action]")]
        public IEnumerable<object> NodeList()
        {
            ObservableList<GingerNodeInfo> list = WorkSpace.Instance.LocalGingerGrid.NodeList;
            var data = list.Select(x =>
                                    new
                                    {
                                        name = x.Name,
                                        actionCount = x.ActionCount,
                                        host = x.Host,
                                        ip = x.IP,
                                        os = x.OS,
                                        sessionId = x.SessionID,
                                        serviceId = x.ServiceId,
                                        status= x.Status.ToString()

                                    });
                        
            return data;
        }

        //// GET: api/ServiceGrid/5
        //[HttpGet("{id}", Name = "Get")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST: api/ServiceGrid
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT: api/ServiceGrid/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE: api/ApiWithActions/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
