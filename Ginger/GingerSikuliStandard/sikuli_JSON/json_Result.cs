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
