using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using GingerCoreNET.RunLib;
using GingerWeb.RepositoryLib;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace GingerWeb.Controllers
{
    [Route("api/[controller]")]    
    public class ServiceGridController : ControllerBase
    {
        // GET: api/ServiceGrid/NodeList
        [HttpGet("[action]")]
        public IEnumerable<GingerNodeInfoWrapper> NodeList()
        {
            ObservableList<GingerNodeInfo> list = WorkSpace.Instance.LocalGingerGrid.NodeList;
            List<GingerNodeInfoWrapper> gingerNodeInfoWrappers = new List<GingerNodeInfoWrapper>();
            foreach (GingerNodeInfo gingerNodeInfo in list)
            {
                gingerNodeInfoWrappers.Add(new GingerNodeInfoWrapper(gingerNodeInfo));
            }
            return gingerNodeInfoWrappers;
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
