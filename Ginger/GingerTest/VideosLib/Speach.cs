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
