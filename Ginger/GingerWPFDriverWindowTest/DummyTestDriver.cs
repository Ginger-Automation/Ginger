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

using Amdocs.Ginger.Plugin.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;

namespace GingerWPFDriverWindowTest
{

    public class DummyTestDriver : IServiceSession
    {
        IDummyTestDriverDisplay mDisplay = null;

        public List<string> Platforms => throw new NotImplementedException();

        public void AttachDisplay(IDummyTestDriverDisplay display)
        {
            mDisplay = display;
        }



        //public override string Name { get { return "Dummy Test Driver"; } }


        //public override void CloseDriver()
        //{
        //    Console.WriteLine("Closing driver");
        //    //mDisplay.WriteLine("Closing Web Services driver");
        //}

        //public override void StartDriver()
        //{
        //    Console.WriteLine("Starting driver");
        //}


        //[GingerAction("HTTP" , "HTTP Desc")]
        //public void HTTP(GingerAction gingerAction, string url)
        //{
        //    if (mDisplay != null)
        //    {
        //        mDisplay.URL = url;
        //    }
        //    // mDisplay.WriteLine("Geeting HTTP URL" + url);
        //    // example to show error checks
        //    if (!url.StartsWith("HTTP", StringComparison.InvariantCultureIgnoreCase))
        //    {
        //        gingerAction.AddError("WebServicesDriver.HTTP", "URL must start with HTTP - " + url);
        //        return;
        //    }

        //    using (var client = new HttpClient())
        //    {
        //        Stopwatch stopwatch = Stopwatch.StartNew();
        //        var result = client.GetAsync(url).Result;
        //        stopwatch.Stop();

        //        gingerAction.Output.Add("Status Code", result.StatusCode.ToString());
        //        gingerAction.Output.Add("Elapsed", stopwatch.ElapsedMilliseconds.ToString());
        //        if (mDisplay != null)
        //        {
        //            mDisplay.AddLog("Elapsed - " + stopwatch.ElapsedMilliseconds);
        //        }
        //    }

        //    gingerAction.ExInfo = "URL: " + url;
        //}




        // Activated from Display
        public void DoHTTP(string url)
        {
            throw new NotImplementedException();
        }

        public void StartSession()
        {
            throw new NotImplementedException();
        }

        public void StopSession()
        {
            throw new NotImplementedException();
        }
    }
}
