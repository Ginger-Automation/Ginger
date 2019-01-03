#region License
/*
Copyright © 2014-2018 European Support Limited

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
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace GingerTest.VideosLib
{
    public class Speach
    {

        // Bsic draft version for voice
        static SpeechSynthesizer synthesizer = null;
        public static void Say(string txt)
        {

            // SSML - https://docs.microsoft.com/en-us/cortana/skills/speech-synthesis-markup-language
            if (synthesizer == null)
            {
                synthesizer = new SpeechSynthesizer();
                synthesizer.Volume = 100;  // 0...100
                synthesizer.Rate = -2;     // -10...10
                synthesizer.SelectVoice("Microsoft Zira Desktop");
            }
            //string xml = System.IO.File.ReadAllText(@"c:\temp\speak.xml");
            //synthesizer.SpeakSsml(xml);

            //using (SpeechSynthesizer synth = new SpeechSynthesizer())
            //{

            //    // Output information about all of the installed voices. 
            //    Console.WriteLine("Installed voices -");
            //    foreach (InstalledVoice voice in synth.GetInstalledVoices())
            //    {
            //        VoiceInfo info = voice.VoiceInfo;
            //        Console.WriteLine(" Voice Name: " + info.Name);
            //    }
            //}





            //// Synchronous
            synthesizer.Speak(txt);


            //PromptBuilder builder = new PromptBuilder();
            //builder.AppendText("click the add button at the top of the grid");

            //// Speak the prompt.
            //synthesizer.Speak(builder);

            //// Asynchronous
            //synthesizer.SpeakAsync("click the add button at the top of the grid");
   
        }
    }
}
