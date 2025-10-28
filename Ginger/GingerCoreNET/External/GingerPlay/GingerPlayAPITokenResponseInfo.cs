#region License
/*
Copyright © 2014-2025 European Support Limited

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

namespace Amdocs.Ginger.CoreNET.External.GingerPlay
{
    /// <summary>
    /// Represents the response from GingerPlay API token endpoint
    /// </summary>
    public class GingerPlayAPITokenResponseInfo
    {

        public string access_token { get; set; }
        public double expires_in { get; set; }
        public string token_type { get; set; }

        public bool IsTokenValid => !string.IsNullOrEmpty(access_token) && expires_in > 0;
    }
}
