#region License
/*
Copyright © 2014-2024 European Support Limited

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
using OpenQA.Selenium.Chromium;
using System;
using System.Collections.Generic;

namespace GingerCore.Drivers
{
    public static class ChromeDriverEx
    {
        public static Screenshot GetFullPageScreenshot(this ChromiumDriver driver)
        {
            // Capture the original scroll position
            Dictionary<string, object> originalScrollPosition = (Dictionary<string, object>)driver.ExecuteScript("return { x: window.pageXOffset, y: window.pageYOffset };");

            // Capture page dimensions and device metrics
            Dictionary<string, Object> metrics = new Dictionary<string, Object>
            {
                ["width"] = driver.ExecuteScript("return Math.max(window.innerWidth, document.body.scrollWidth, document.documentElement.scrollWidth)"),
                ["height"] = driver.ExecuteScript("return Math.max(window.innerHeight, document.body.scrollHeight, document.documentElement.scrollHeight)"),
                ["mobile"] = driver.ExecuteScript("return typeof window.orientation !== 'undefined'")
            };

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

            // Execute the emulation Chrome command to change browser to a custom device that is the size of the entire page
            driver.ExecuteCdpCommand("Emulation.setDeviceMetricsOverride", metrics);

            // Take screenshot as everything is now visible
            Screenshot screenshot = driver.GetScreenshot();

            // Reset the device metrics and scroll position to original state 
            driver.ExecuteCdpCommand("Emulation.clearDeviceMetricsOverride", new Dictionary<string, object>());
            driver.ExecuteScript($"window.scrollTo({{ top: {originalScrollPosition["y"]}, left: {originalScrollPosition["x"]}, behavior: 'instant' }});");

            return screenshot;
        }
    }
}
