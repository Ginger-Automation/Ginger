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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GingerCore.Actions.REST;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using GingerCore;
using GingerCore.Actions;
using Amdocs.Ginger.Repository;
using GingerTestHelper;

namespace UnitTests.NonUITests
{
    [TestClass]
    [Level3]
    public class RESTTest 
    {
        
        [TestInitialize]
        public void TestInitialize()
        {
        }

        // Need to have our own REST server, so will not be dependent on David server
        
        [TestMethod]        
        public void WebServices_RestAction()
        {

            ActREST a2 = new ActREST();
            try
            {



                a2.RequestType = ActREST.eRequestType.GET;
                a2.ReqHttpVersion = ActREST.eHttpVersion.HTTPV11;
                a2.ContentType = ActREST.eContentType.JSon;
                a2.CookieMode = ActREST.eCookieMode.None;
                a2.SecurityType = ActREST.eSercurityType.None;
                a2.EndPointURL.ValueForDriver = "https://reqres.in/api/users/2";

                //Act
                a2.AddNewReturnParams = true;
                a2.Execute();
            }
            finally
            {
                //running validation only in case of sucess
                if (((int)a2.ResponseCode).ToString().StartsWith("2"))
                {
                    Assert.AreEqual(17, a2.ReturnValues.Count);


                    Assert.AreEqual("Weaver", a2.ReturnValues.Where(x => x.Param == "last_name").First().Actual);
                }
            }
        }

    }
}
