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

using Amdocs.Ginger.Repository;
using GingerCore.Properties;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text;
using GingerCore.Helpers;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common;

namespace GingerCore.Actions
{

    public class ActTextSpeech : ActWithoutDriver
    {
        public override string ActionDescription { get { return "Text Speech Action"; } }
        public override string ActionUserDescription { get { return "Performs Text Speech Action"; } }

        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
        {
            TBH.AddText("Use this action in case you want to perform any Text Speech actions.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To perform a Radio button action, Select TextSpeech action,TextToSayLoud,WaveLocation,Interval and value");
        }        

        public override string ActionEditPage { get { return "ActTextSpeechEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

        public override bool IsSelectableAction { get { return false; } }
        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    AddAllPlatforms();
                }
                return mPlatforms;
            }
        }

        public new static partial class Fields
        {
            public static string Interval = "Interval";
            public static string WaveLocation = "WaveLocation";
            public static string TextToSayLoud = "TextToSayLoud";
            public static string TextSpeechAction = "TextSpeechAction";
        }

        public override String ActionType
        {
            get
            {
                return TextSpeechAction.ToString();
            }
        }


        public override System.Drawing.Image Image { get { return Resources.ActGotoURL; } }

        public enum eTextSpeechAction
        {
            TextToSpeech = 1,
            SpeechToText = 2,
            TextToWave = 3,
            MicrophonetoText = 4

        }

        [IsSerializedForLocalRepository]
        public eTextSpeechAction TextSpeechAction { get; set; }

        [IsSerializedForLocalRepository]
        public int Interval { get; set; }
        public string WaveLocation { get; set; }
        public string TextToSayLoud { get; set; }

        public override void Execute()
        {
            switch (TextSpeechAction)
            {
                case eTextSpeechAction.TextToSpeech:
                    Speech();
                    break;

                case eTextSpeechAction.SpeechToText:
                    ReadAudio();
                    break;

                case eTextSpeechAction.MicrophonetoText:
                    CaptureAudio();
                    break;

                default:

                    break;

            }
        }

        private void CaptureAudio() //Microphone to Text
        {

            SpeechRecognitionEngine SpeechEngine = new SpeechRecognitionEngine(); //create new speech engine
            Grammar Grm = new DictationGrammar(); //Create grammer

            SpeechEngine.LoadGrammar(Grm);

            SpeechEngine.SetInputToDefaultAudioDevice(); //Capture from micin

            StringBuilder sb = new StringBuilder();
            int TimeControl = 0; //Counter for TenSecIntervals tracking
            int TenSecIntervals = Interval;

            while (true)
            {
                try
                {
                    TimeControl = TimeControl + 1;
                    var recText = SpeechEngine.Recognize();
                    sb.Append(recText.Text);

                    if (TimeControl == TenSecIntervals)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                    break;
                }
            }
            SpeechEngine.Dispose();
            string TranslatedValue = CheckStringConditionByPresenceOfWords(sb.ToString(), 
                                                new List<string>() { "A" }, 
                                                new List<string>() { "c", "cr", "English", "start", "started" }); // TODO; create dic

            AddOrUpdateReturnParamActual("RecordedText", TranslatedValue);

        }

        /// <summary>
        /// Determine which of 3 conditions is the case based on words recognized in a string. 
        /// Looks for 2 conditions & returns 3rd condition if neither is recognized.
        /// If a failure word is found, it returns the failing condition. 
        /// If a success word is returned, the success condition is returned. 
        /// If nothing at all is matched, it returns the NoMatches condition.
        /// Messages are optional.
        /// </summary>
        /// <param name="TextExtracted"></param>
        /// <param name="FailureWords"></param>
        /// <param name="SuccessWords"></param>
        /// <param name="FailureMessage"></param>
        /// <param name="SuccessMessage"></param>
        /// <param name="NoMatchMessage"></param>
        /// <returns></returns>
        private string CheckStringConditionByPresenceOfWords(string TextExtracted, 
                List<string> FailureWords, 
                List<string> SuccessWords, 
                string FailureMessage="Call did not reach C", 
                string SuccessMessage="Call reached C", 
                string NoMatchMessage="No words were matched for this call")
        {
            //TODO: use a dictinary from file or alike -need to externalize
            // if elminating words are used return fail & exit
            foreach(string word in FailureWords)    {if (TextExtracted.IndexOf(word) > -1) return FailureMessage;   }

            // if any passing words are used return pass & exit
            foreach (string word in SuccessWords)   {if (TextExtracted.IndexOf(word) > -1) return SuccessMessage;   }

            // if we're still here that means nothing was recognized, so we should return no matches
            return NoMatchMessage;
        }


        private void ReadAudio() //Speech(wave file) to Text 
        {


            SpeechRecognitionEngine SpeechEngine = new SpeechRecognitionEngine(); //create new speech engine
            Grammar Grm = new DictationGrammar(); //create grammer



            SpeechEngine.LoadGrammar(Grm);



            SpeechEngine.SetInputToWaveFile(WaveLocation);
            

            //Add waits to sort out any audio issues
            SpeechEngine.BabbleTimeout = new TimeSpan(Int32.MaxValue); //Background noise
            SpeechEngine.InitialSilenceTimeout = new TimeSpan(Int32.MaxValue); //Initial silence
            SpeechEngine.EndSilenceTimeout = new TimeSpan(100000000); //unrecognizeable silence
            SpeechEngine.EndSilenceTimeoutAmbiguous = new TimeSpan(100000000); //additional check


            StringBuilder sb = new StringBuilder();
            while (true)
            {
                try
                {
                    var recText = SpeechEngine.Recognize();
                    if (recText == null)
                    {
                        break;
                    }

                    sb.Append(recText.Text);
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                    break;
                }
            }
            SpeechEngine.Dispose();
            AddOrUpdateReturnParamActual("RetrievedText", sb.ToString());
        }


        private void Speech()
        {
            try
            {
                SpeechSynthesizer SpeechReaderloud = new SpeechSynthesizer();
                SpeechReaderloud.Rate = (int)-2;
                SpeechReaderloud.Speak(TextToSayLoud);

                SpeechReaderloud.Dispose();
            }
            catch { }

        }
    }
}



