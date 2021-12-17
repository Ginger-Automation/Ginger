using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Repository;

namespace GingerCore.Actions
{
    public abstract class ActImageCaptureSupport : ActWithoutDriver
    {
        [IsSerializedForLocalRepository]
        public abstract int ClickX { get; set; }
        [IsSerializedForLocalRepository]
        public abstract int ClickY { get; set; }
        [IsSerializedForLocalRepository]
        public abstract int StartX { get; set; }
        [IsSerializedForLocalRepository]
        public abstract int StartY { get; set; }
        [IsSerializedForLocalRepository]
        public abstract int EndX { get; set; }
        [IsSerializedForLocalRepository]
        public abstract int EndY { get; set; }
        [IsSerializedForLocalRepository]
        public abstract string LocatorImgFile { get; set; }

        public string Coordinates { get { return StartX + ", " + StartY; } }
    }
}
