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
using System.Text;
using Amdocs.Ginger.Repository;

namespace GingerCore.Actions
{
    public abstract class ActImageCaptureSupport : ActWithoutDriver
    {
        public abstract int ClickX { get; set; }
        public abstract int ClickY { get; set; }
        public abstract int StartX { get; set; }
        public abstract int StartY { get; set; }
        public abstract int EndX { get; set; }
        public abstract int EndY { get; set; }
        public abstract string LocatorImgFile { get; set; }
        public abstract string ImagePath { get; }

        public string Coordinates { get { return StartX + ", " + StartY; } }
    }
}
