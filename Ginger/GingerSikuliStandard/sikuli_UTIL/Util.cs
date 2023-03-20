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
