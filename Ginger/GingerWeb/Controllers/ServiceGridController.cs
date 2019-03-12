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
