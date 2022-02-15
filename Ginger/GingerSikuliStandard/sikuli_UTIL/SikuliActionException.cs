/*
 * Created by SharpDevelop.
 * User: Alan
 * Date: 6/8/2014
 * Time: 9:46 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace GingerSikuliStandard.sikuli_UTIL
{
	/// <summary>
	/// An exception thrown when a json_Result with a Result of FAIL is returned from the service, containing the error message.
	/// </summary>
	public class SikuliActionException : Exception
	{
		public SikuliActionException() : base()
		{
		}
		
		public SikuliActionException(ActionResult result, String message) : base("Result: " + result.ToString() + message)
		{
		}
	}
}
