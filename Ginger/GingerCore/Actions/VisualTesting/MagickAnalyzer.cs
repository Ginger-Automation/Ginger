#region License
/*
Copyright © 2014-2019 European Support Limited

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
using ImageMagick;
using System;
using System.Drawing;

namespace GingerCore.Actions.VisualTesting
{
    class MagickAnalyzer : IVisualAnalyzer
    {
        ActVisualTesting mAct;
        bool IVisualAnalyzer.SupportUniqueExecution()
        {
            return false;
        }

        void IVisualAnalyzer.SetAction(IVisualTestingDriver driver, ActVisualTesting act)
        {
            mAct = act;
        }

        void IVisualAnalyzer.Compare()
        {
            MagickImage magickBaseImg = new MagickImage(mAct.baseImage);
            MagickImage magickTargetImg = new MagickImage(mAct.targetImage);

            var diffImg = new MagickImage();

            double percentageDifference;

            // TODO: add combo with list of options for user to choose the Error Matic and Cahnnels
            ActInputValue AIV = mAct.GetOrCreateInputParam(ActVisualTesting.Fields.ErrorMetric);
            
            //TODO: fix me - removed hard code
            //caused build problem on build machine so temp fix for now
            ErrorMetric EM = ErrorMetric.Fuzz;
             percentageDifference = magickBaseImg.Compare(magickTargetImg, EM, diffImg, Channels.Red);
             percentageDifference = percentageDifference * 100;
             percentageDifference = Math.Round(percentageDifference, 2);

            Bitmap ImgToSave = diffImg.ToBitmap();
            mAct.CompareResult = ImgToSave;

            mAct.AddOrUpdateReturnParamActual("Percentage Difference", percentageDifference + "");

            mAct.AddScreenShot(ImgToSave, "Compare Result");
        }

        public void CreateBaseline()
        {
        }

        void IVisualAnalyzer.Execute()
        {
            throw new NotImplementedException();
        }
    }
}
