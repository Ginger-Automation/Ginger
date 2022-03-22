#region License
/*
Copyright © 2014-2022 European Support Limited

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

using System.Text;

namespace GingerCore.Drivers.Selenium.SeleniumBMP
{
    public class LimitOptions
    {
        public int? UpstreamKbps { get; set; }
        public int? DownstreamKbps { get; set; }
        public int? Latency { get; set; }
        
        public string ToFormData()
        {
            var builder = new StringBuilder(50);
            string delimiter = "";
            
            if (UpstreamKbps.HasValue)            
            {
                builder.AppendFormat("upstream_kbps={0}", UpstreamKbps.Value);
                delimiter = "&";
            }

            if (DownstreamKbps.HasValue)
            {
                builder.AppendFormat("{0}downstream_kbps={1}", delimiter, DownstreamKbps.Value);
                delimiter = "&";
            }

            if (Latency.HasValue)
                builder.AppendFormat("{0}latency={1}", delimiter, Latency.Value);            

            return builder.ToString();
        }
    }
}