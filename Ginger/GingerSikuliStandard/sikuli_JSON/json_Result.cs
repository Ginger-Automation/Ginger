#region License
/*
Copyright © 2014-2023 European Support Limited

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

/*
 * Created by SharpDevelop.
 * User: Alan
 * Date: 6/8/2014
 * Time: 9:05 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using GingerSikuliStandard.sikuli_UTIL;
using Newtonsoft.Json;

namespace GingerSikuliStandard.sikuli_JSON
{
	/// <summary>
	/// Description of json_Result.
	/// </summary>
	public class json_Result
	{
		public json_Result()
		{
		}
		
		public String message {get; set;}
		public String result {get; set;}
        public String stacktrace { get; set; }
		
		public ActionResult ToActionResult()
		{
			if(result.Equals(ActionResult.FAIL.ToString()))
			{
				return ActionResult.FAIL;
			}
			else if(result.Equals(ActionResult.PASS.ToString()))
			{
				return ActionResult.PASS;
			}
			else
			{
				return ActionResult.UNKNOWN;
			}
		}

        public static json_Result getJResult(String json)
        {
            json_Result jResult = JsonConvert.DeserializeObject<json_Result>(json);
            return jResult;
        }
	}
}
