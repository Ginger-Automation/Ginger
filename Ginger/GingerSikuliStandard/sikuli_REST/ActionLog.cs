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
 * Time: 9:19 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Collections.Generic;

namespace GingerSikuliStandard.sikuli_REST
{
	/// <summary>
	/// Description of Log.
	/// </summary>
	public class ActionLog
	{
		public static readonly String LogFileName = "log";
		public static readonly String LogFolder = "Sikuli4Net.Client.Logs";
		
		private String WorkingDir;
		private String LogFolderPath;
        public readonly String LogPath;
		
		public ActionLog()
		{
			WorkingDir = Directory.GetCurrentDirectory();
			LogFolderPath = Path.Combine(WorkingDir,LogFolder);
			if(!Directory.Exists(LogFolderPath))
			{
				Directory.CreateDirectory(LogFolderPath);
			}
			DateTime now = DateTime.Now;
			LogPath = Path.Combine(LogFolderPath,LogFileName + "." +now.ToShortDateString().Replace("/","") + now.ToShortTimeString().Replace(":","") + ".txt");
			Console.WriteLine("--Log for this test run can be found at: " + LogPath + "--");
			File.Create(LogPath).Close();
		}
		
		/// <summary>
		/// Method to write a line to the logfile and to the console.
		/// </summary>
		/// <param name="message"></param>
		public void WriteLine(String message)
		{
			List<String> line = new List<String>();
			if(File.Exists(LogPath))
			{
				String [] currentLines = File.ReadAllLines(LogPath);
				line.AddRange(currentLines);
			}
			line.Add(":::" + message + ":::");
			File.WriteAllLines(LogPath,line.ToArray());
			Console.WriteLine(":::" + message + ":::");
		}
	}
}
