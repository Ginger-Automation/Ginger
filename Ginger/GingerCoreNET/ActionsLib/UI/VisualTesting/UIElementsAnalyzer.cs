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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using GingerCore.Drivers;
using OpenQA.Selenium;

namespace GingerCore.Actions.VisualTesting
{
    public class UIElementsAnalyzer : IVisualAnalyzer
    {
        ActVisualTesting mAct;
        IVisualTestingDriver mDriver;

        bool IVisualAnalyzer.SupportUniqueExecution()
        {
            return false;
        }

        void IVisualAnalyzer.SetAction(IVisualTestingDriver driver, ActVisualTesting act)
        {
            mAct = act;
            mDriver = driver;
        }

        void IVisualAnalyzer.Compare()
        {
            //TODO: use interface on the driver to get elements
            if (mDriver is IVisualTestingDriver)
            {
                IVisualTestingDriver d = (IVisualTestingDriver)mDriver;

                string filename = mAct.GetFullFilePath(mAct.BaselineInfoFile);
                VisualElementsInfo VE1 = VisualElementsInfo.Load(filename);
                VE1.Bitmap = mAct.baseImage;

                VisualElementsInfo VE2 = d.GetVisualElementsInfo();
                VE1.Compare(VE2);
                IEnumerable<VisualElement> listwithnomatch = from x in VE1.Elements where x.Text != "" && x.MatchingElement == null select x;

                // Create compare bitmap for VE1
                Bitmap bmp = new Bitmap(VE1.Bitmap);
                // mark element with no match
                foreach (VisualElement VE in listwithnomatch)
                {
                    using (Graphics gr = Graphics.FromImage(bmp))
                    {
                        gr.SmoothingMode = SmoothingMode.AntiAlias;

                        //TODO: check the -3 or + 6 will not go out of bitmap
                        Rectangle rect = new Rectangle(VE.X - 3, VE.Y - 3, VE.Width + 6, VE.Height + 6);

                        gr.FillRectangle(Brushes.Transparent, rect);
                        using (Pen thick_pen = new Pen(Color.HotPink, 2))
                        {
                            gr.DrawRectangle(thick_pen, rect);
                        }
                    }
                }

                mAct.CompareResult = bmp;

                // Add output of mismatch
                mAct.AddOrUpdateReturnParamActual("Mismatch elements in target", listwithnomatch.Count() + "");

                //TODO: if output each mismatch then do output

                
                mAct.AddScreenShot(bmp, "Compare Result");

                //TODO: add small bitmap of mismatch elem
            }
        }

        public void CreateBaseline()
        {
            VisualElementsInfo VEI = mDriver.GetVisualElementsInfo();
            if (string.IsNullOrEmpty(mAct.BaselineInfoFile))
            {
                // Create default file name for info file
                mAct.BaselineInfoFile = @"~\Documents\ScreenShots\" + mAct.Description + " - Baseline.txt";
            }
            string filename = mAct.GetFullFilePath(mAct.BaselineInfoFile);
            VEI.Save(filename);
        }

        void IVisualAnalyzer.Execute()
        {
            throw new NotImplementedException();
        }

        
    }
}
