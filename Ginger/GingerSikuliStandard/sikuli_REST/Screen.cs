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
 * Time: 9:07 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Newtonsoft.Json;
using GingerSikuliStandard.sikuli_JSON;
using GingerSikuliStandard.sikuli_UTIL;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace GingerSikuliStandard.sikuli_REST
{
    /// <summary>
    /// Description of Screen.
    /// </summary>
    public class Screen
    {
        private String serviceURL;

        public Screen()
        {
            serviceURL = "http://localhost:8080/sikuli/api/";
            //serviceURL = string.Format("http://localhost:{0}/sikuli/api/", SocketHelper.GetOpenPort());
        }

        /// <summary>
        /// Method to find the specified pattern on the screen
        /// </summary>
        /// <param name="pattern">The pattern object passed to the tool for searching</param>
        public void Find(Pattern pattern, bool highlight = false)
        {
            json_Find jFind = new json_Find(pattern.ToJsonPattern(), highlight);
            String jFindS = JsonConvert.SerializeObject(jFind);
            json_Result jResult = json_Result.getJResult(MakeRequest("find", jFindS));
            FailIfResultNotPASS(jResult);
        }
        /// <summary>
        /// Method to click on the specified pattern
        /// </summary>
        /// <param name="pattern">The pattern object passed to the tool for clicking</param>
        /// <param name="kmod">Any key modifiers to press while clicking.  example: Control, Shift, Enter, etc...</param>
        public void Click(Pattern pattern, KeyModifier kmod = KeyModifier.NONE, bool highlight = false)
        {
            if (highlight)
            {
                Find(pattern, highlight);
            }
            json_Click jClick = new json_Click(pattern.ToJsonPattern(), kmod);
            String jClickS = JsonConvert.SerializeObject(jClick);
            json_Result jResult = json_Result.getJResult(MakeRequest("click", jClickS));
            FailIfResultNotPASS(jResult);
        }
        public void Click(Pattern pattern, bool highlight)
        {
            Click(pattern, KeyModifier.NONE, highlight);
        }
        /// <summary>
        /// Method to double click on the specified pattern
        /// </summary>
        /// <param name="pattern">The pattern object passed to the tool for clicking</param>
        public void DoubleClick(Pattern pattern, KeyModifier kmod = KeyModifier.NONE, bool highlight = false)
        {
            if (highlight)
            {
                Find(pattern, highlight);
            }
            json_Click jClick = new json_Click(pattern.ToJsonPattern(), kmod);
            String jClickS = JsonConvert.SerializeObject(jClick);
            json_Result jResult = json_Result.getJResult(MakeRequest("doubleclick", jClickS));
            FailIfResultNotPASS(jResult);
        }
        public void DoubleClick(Pattern pattern, bool highlight)
        {
            DoubleClick(pattern, KeyModifier.NONE, highlight);
        }
        /// <summary>
        /// Method to right click on a specified pattern
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="kmod"></param>
        public void RightClick(Pattern pattern, KeyModifier kmod = KeyModifier.NONE, bool highlight = false)
        {
            if (highlight)
            {
                Find(pattern, highlight);
            }
            json_Click jClick = new json_Click(pattern.ToJsonPattern(), kmod);
            String jClickS = JsonConvert.SerializeObject(jClick);
            json_Result jResult = json_Result.getJResult(MakeRequest("rightclick", jClickS));
            FailIfResultNotPASS(jResult);
        }
        public void RightClick(Pattern pattern, bool highlight)
        {
            RightClick(pattern, KeyModifier.NONE, highlight);
        }
        /// <summary>
        /// Method to wait for a specific Pattern to appear on the screen.  If it does not appear by the specified timeout (in seconds), the action fails.
        /// </summary>
        /// <param name="pattern">The pattern object passed to the tool for waiting</param>
        /// <param name="timeout">The timeout, in seconds, before the action fails</param>
        public void Wait(Pattern pattern, Double timeout = 15)
        {
            json_Wait jWait = new json_Wait(pattern.ToJsonPattern(), timeout);
            String jWaitS = JsonConvert.SerializeObject(jWait);
            json_Result jResult = json_Result.getJResult(MakeRequest("wait", jWaitS));
            FailIfResultNotPASS(jResult);
        }
        /// <summary>
        /// Method to wait for a specific Pattern to disappear from the screen.  If it does not disappear by the specified timeout, a value of false is returned.  Otherwise, true is returned.
        /// </summary>
        /// <param name="pattern">The pattern object passed to the tool for waiting</param>
        /// <param name="timeout">The timeout, in seconds, before the action returns false</param>
        /// <returns>True if object vanishes, false otherwise</returns>
        public bool WaitVanish(Pattern pattern, Double timeout = 15)
        {
            json_WaitVanish jWaitVanish = new json_WaitVanish(pattern.ToJsonPattern(), timeout);
            String jWaitVanishS = JsonConvert.SerializeObject(jWaitVanish);
            json_WaitVanish jWaitVanish_Result = json_WaitVanish.getJWaitVanish(MakeRequest("waitvanish", jWaitVanishS));
            FailIfResultNotPASS(jWaitVanish_Result.jResult);
            return jWaitVanish_Result.patternDisappeared;
        }
        /// <summary>
        /// Method to check if a pattern exists on the screen, waiting for the specified timeout for the object to appear.  Returns true if object exists, or else false.
        /// </summary>
        /// <param name="pattern">The pattern object passed to the tool for searching</param>
        /// <param name="timeout">The timeout, in seconds, before the action returns false</param>
        /// <returns>True if object exists, false otherwise</returns>
        public bool Exists(Pattern pattern, Double timeout = 15)
        {
            json_Exists jExists = new json_Exists(pattern.ToJsonPattern(), timeout);
            String jExistsS = JsonConvert.SerializeObject(jExists);
            json_Exists jExists_Result = json_Exists.getJExists(MakeRequest("exists", jExistsS));
            FailIfResultNotPASS(jExists_Result.jResult);
            return jExists_Result.patternExists;
        }
        /// <summary>
        /// Method to type the specified text into the specified pattern after locating it on the screen
        /// </summary>
        /// <param name="pattern">The pattern object passed to the tool for typing</param>
        /// <param name="text">The text to type in the pattern, if it is found</param>
        /// <param name="kmod">Any key modifiers to press while typing.  example: Control, Shift, Enter, etc...</param>
        public void Type(Pattern pattern, String text, KeyModifier kmod = KeyModifier.NONE)
        {
            json_Type jType = new json_Type(pattern.ToJsonPattern(), text, kmod);
            String jTypeS = JsonConvert.SerializeObject(jType);

            // use "text" instead of type for GetText
            json_Result jResult = json_Result.getJResult(MakeRequest("type", jTypeS));
            FailIfResultNotPASS(jResult);
        }
        /// <summary>
        /// Method to click and drag from one pattern to another
        /// </summary>
        /// <param name="clickPattern">The pattern to start the drag from</param>
        /// <param name="dropPattern">The pattern to drop at</param>
        public void DragDrop(Pattern clickPattern, Pattern dropPattern)
        {
            json_DragDrop jDragDrop = new json_DragDrop(clickPattern.ToJsonPattern(), dropPattern.ToJsonPattern());
            String jDragDropS = JsonConvert.SerializeObject(jDragDrop);
            json_Result jResult = json_Result.getJResult(MakeRequest("dragdrop", jDragDropS));
            FailIfResultNotPASS(jResult);
        }

        /// <summary>
        /// Method to make a request to the service with the specified URL extension and the specified Json object.
        /// </summary>
        /// <param name="requestURLExtension">The URL extenstion that the request is sent to. example: "find"</param>
        /// <param name="jsonObject">The Json Object, usually a pattern, that is being passed through the POST request</param>
        /// <returns></returns>
        private String MakeRequest(String requestURLExtension, String jsonObject)
        {
            Util.Log.WriteLine("Making Request to Service: " + serviceURL + requestURLExtension + " POST: " + jsonObject);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serviceURL + requestURLExtension);
            request.Accept = "application/json";
            request.Method = "POST";
            request.ContentType = "application/json";
            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(jsonObject);
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            String resultString;
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                resultString = reader.ReadToEnd();
            }
            Util.Log.WriteLine(resultString);
            return resultString;
        }
        /// <summary>
        /// Method to check the json_Result object and throw an exception if the Result is not PASSing
        /// </summary>
        /// <param name="jResult">the json_Result to check</param>
        public void FailIfResultNotPASS(json_Result jResult)
        {
            Util.Log.WriteLine("Result: " + jResult.result + " Message: " + jResult.message + " Stacktrace: " + jResult.stacktrace);
            if (!jResult.ToActionResult().Equals(ActionResult.PASS))
            {
                throw new SikuliActionException(jResult.ToActionResult(), jResult.message);
            }
        }
    }
}
