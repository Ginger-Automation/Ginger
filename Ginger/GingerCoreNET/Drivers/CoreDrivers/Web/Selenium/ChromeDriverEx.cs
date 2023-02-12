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
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chromium;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GingerCore.Drivers
{
    public static class ChromeDriverEx 
    {
        public static Screenshot GetFullPageScreenshot(this ChromiumDriver driver)
        {
            //Dictionary will contain the parameters needed to get the full page screen shot
            Dictionary<string, Object> metrics = new Dictionary<string, Object>();
            metrics["width"] = driver.ExecuteScript("return Math.max(window.innerWidth,document.body.scrollWidth,document.documentElement.scrollWidth)");
            metrics["height"] = driver.ExecuteScript("return Math.max(window.innerHeight,document.body.scrollHeight,document.documentElement.scrollHeight)");
            object devicePixelRatio = driver.ExecuteScript("return window.devicePixelRatio");
            if (devicePixelRatio != null)
            {
                double doubleValue = 0;
                if (double.TryParse(devicePixelRatio.ToString(), out doubleValue))
                {
                    metrics["deviceScaleFactor"] = doubleValue;
                }
                else
                {
                    long longValue = 0;
                    if (long.TryParse(devicePixelRatio.ToString(), out longValue))
                    {
                        metrics["deviceScaleFactor"] = longValue;
                    }
                }
            }
            metrics["mobile"] = driver.ExecuteScript("return typeof window.orientation !== 'undefined'");
            //Execute the emulation Chrome Command to change browser to a custom device that is the size of the entire page
            driver.ExecuteChromeCommand("Emulation.setDeviceMetricsOverride", metrics);
            //You can then just screenshot it as it thinks everything is visible
            Screenshot screenshot = driver.GetScreenshot();
            //This command will return your browser back to a normal, usable form if you need to do anything else with it.
            driver.ExecuteChromeCommand("Emulation.clearDeviceMetricsOverride", new Dictionary<string, Object>());

            return screenshot;
        }
    }
}
