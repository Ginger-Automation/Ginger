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

using System;

namespace GingerCore.Drivers.Selenium.SeleniumBMP
{
    public class Entry
    {
        public int Time { get; set; }
        public Request Request { get; set; }
        public Response Response { get; set; }
        public DateTime StartedDateTime { get; set; }
        public Timings Timings { get; set; }
        public string PageRef { get; set; }
        public Cache Cache { get; set; }
        public string ServerIpAddress { get; set; }
        public string Connection { get; set; }
        public string Comment { get; set; }
    }
}