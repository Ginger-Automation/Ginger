/*
 * Created by SharpDevelop.
 * User: Alan
 * Date: 6/18/2014
 * Time: 3:04 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using GingerSikuliStandard.sikuli_REST;

namespace GingerSikuliStandard.sikuli_UTIL
{
	/// <summary>
	/// Description of Util.
	/// </summary>
	public static class Util
	{
		private static ActionLog _Log;
		public static ActionLog Log
		{
			get
			{
				if(_Log == null)
				{
					_Log = new ActionLog();
				}
				return _Log;
			}
		}
	}
}
