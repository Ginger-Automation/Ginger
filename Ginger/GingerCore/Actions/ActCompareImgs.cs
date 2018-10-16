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
using System.IO;
using System.Runtime.InteropServices;
using GingerCore.Helpers;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Automation;
using GingerCore.Drivers;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace GingerCore.Actions
{
    public class ActCompareImgs : ActWithoutDriver
    {
        public override string ActionDescription { get { return "Compare Screen Areas Action"; } }
        public override string ActionUserDescription { get { return "Compares screen areas"; } }

        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
        {
            TBH.AddText("Use this action in case you want to compare the screen area.");
        }

        public override string ActionEditPage { get { return "ActCompareImgsEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return true; } }
        public override bool ValueConfigsNeeded { get { return true; } }

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

        [IsSerializedForLocalRepository]
        public int StartX { get; set; }
        [IsSerializedForLocalRepository]
        public int StartY { get; set; }
        [IsSerializedForLocalRepository]
        public int EndX { get; set; }
        [IsSerializedForLocalRepository]
        public int EndY { get; set; }
        [IsSerializedForLocalRepository]
        public string ExpectedImgFile { get; set; }
        [IsSerializedForLocalRepository]
        public string Coordinates { get { return StartX + ", " + StartY + ", " + EndX + ", " + EndY; } }
        [IsSerializedForLocalRepository]
        public string WindowName { get; set; }

        public override String ActionType
        {
            get
            {
                return "Image Compare Action";
            }
        }
        public new static partial class Fields
        {
            public static string ExpectedImgFile = "ExpectedImgFile";
            public static string Coordinates = "Coordinates";
            public static string WindowName = "WindowName";
        }


        public override System.Drawing.Image Image { get { return Resources.Vbs16x16; } }

        public override void Execute()
        {
            Bitmap MainWinImage;
            AutomationElement targetWin = null;
            AutomationElement gingerWin = null;
            List<System.Drawing.Point> result = new List<System.Drawing.Point>();
            //System.Drawing.Point(0,0);
            if (!string.IsNullOrWhiteSpace(WindowName))
                targetWin = UIAutomationGetWindowByTitle(WindowName);

            string ExpectedImgFile1 = ExpectedImgFile;
            if (ExpectedImgFile1.StartsWith("~"))
            {
                string SolutionFolder1 = SolutionFolder.ToString();
                ExpectedImgFile1 = ExpectedImgFile1.Replace("~\\", "");
                ExpectedImgFile1 = System.IO.Path.Combine(SolutionFolder1, ExpectedImgFile1);
            }
            if (WindowName == "FULLSCREEN")
            {
                List<AutomationElement> wins = UIAutomationGetFirstLevelWindows();
                targetWin = AutomationElement.RootElement;

                foreach (AutomationElement w in wins)
                {
                    if (w == UIAutomationGetWindowByTitle("Amdocs Ginger Automation"))
                    {
                        gingerWin = w;
                        WinAPIAutomation.MinimizeWindow(w.Current.ProcessId);
                        break;
                    }
                }
                System.Drawing.Rectangle SelectionRectangle = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                using (MainWinImage = new Bitmap(SelectionRectangle.Width, SelectionRectangle.Height))
                {

                    using (Graphics g = Graphics.FromImage(MainWinImage))
                    {

                        g.CopyFromScreen(new System.Drawing.Point(0, 0), System.Drawing.Point.Empty, SelectionRectangle.Size);

                    }

                    result = GetSubPositions(MainWinImage, Image.FromFile(ExpectedImgFile1));


                    if (result.Count <= 0)
                    {
                        this.Error = "Image is not found in the current window";
                        return;
                    }
                }
            }
            else if (string.IsNullOrWhiteSpace(WindowName) || targetWin == null)
            {
                List<AutomationElement> wins = UIAutomationGetFirstLevelWindows();
                targetWin = AutomationElement.RootElement;

                foreach (AutomationElement w in wins)
                {
                    if (w == UIAutomationGetWindowByTitle("Amdocs Ginger Automation"))
                        continue;
                    MainWinImage = GetWindowBitmap(w);
                    if (File.Exists(ExpectedImgFile1) && MainWinImage != null)
                        result = GetSubPositions(MainWinImage, Image.FromFile(ExpectedImgFile1));
                    else
                        continue;
                    if (result.Count > 0)
                    {
                        targetWin = w;
                        break;
                    }
                }

                if (result.Count <= 0)
                {
                    this.Error = "Image is not found in the current screen";
                    return;
                }
            }
            else
            {
                if (targetWin != null)
                {
                    WinAPIAutomation.ShowWindow(targetWin);
                    MainWinImage = GetWindowBitmap(targetWin);
                    result = GetSubPositions(MainWinImage, Image.FromFile(ExpectedImgFile1));

                    if (result.Count <= 0)
                    {
                        this.Error = "Image is not found in the current window";
                        return;
                    }
                }
                else
                {
                    this.Error = "The main searching is not existing";
                    return;
                }
            }
        }

        public AutomationElement UIAutomationGetWindowByTitle(string WindowTitle)
        {
            TreeWalker walker = TreeWalker.ControlViewWalker;
            AutomationElement win = walker.GetFirstChild(AutomationElement.RootElement);

            while (win != null)
            {
                string WinTitle = (string)win.GetCurrentPropertyValue(AutomationElement.NameProperty);
                if (WinTitle.Contains(WindowTitle))
                {
                    return win;
                }
                win = walker.GetNextSibling(win);
            }
            return null;
        }

        public List<AutomationElement> UIAutomationGetFirstLevelWindows()
        {
            TreeWalker walker = TreeWalker.ControlViewWalker;
            List<AutomationElement> winList = new List<AutomationElement>();
            winList.Add(AutomationElement.RootElement);
            AutomationElement win = walker.GetFirstChild(AutomationElement.RootElement);

            while (win != null)
            {
                winList.Add(win);
                win = walker.GetNextSibling(win);
            }
            return winList;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);


        public Bitmap GetWindowBitmap(AutomationElement window)
        {
            Bitmap bmp = new Bitmap((int)window.Current.BoundingRectangle.Width, (int)window.Current.BoundingRectangle.Height);
            Graphics memoryGraphics = Graphics.FromImage(bmp);
            IntPtr dc = memoryGraphics.GetHdc();
            bool success = PrintWindow((IntPtr)window.Current.NativeWindowHandle, dc, 0);
            memoryGraphics.ReleaseHdc(dc);
            return bmp;
        }

        private static Bitmap GrayScale(Bitmap Bmp)
        {
            int rgb;
            Color c;

            for (int y = 0; y < Bmp.Height; y++)
                for (int x = 0; x < Bmp.Width; x++)
                {
                    c = Bmp.GetPixel(x, y);
                    rgb = (int)((c.R + c.G + c.B) / 3);
                    Bmp.SetPixel(x, y, Color.FromArgb(rgb, rgb, rgb));
                }
            return Bmp;
        }

        public static List<System.Drawing.Point> GetSubPositions(Image main, Image sub)
        {
            List<System.Drawing.Point> possiblepos = new List<System.Drawing.Point>();
            Bitmap mainBitmap = main as Bitmap;
            Bitmap subBitmap = sub as Bitmap;
            mainBitmap = GrayScale(mainBitmap);
            subBitmap = GrayScale(subBitmap);
            int mainwidth = mainBitmap.Width;
            int mainheight = mainBitmap.Height;

            int subwidth = subBitmap.Width;
            int subheight = subBitmap.Height;

            int movewidth = mainwidth - subwidth;
            int moveheight = mainheight - subheight;

            BitmapData bmMainData = mainBitmap.LockBits(new System.Drawing.Rectangle(0, 0, mainwidth, mainheight), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData bmSubData = subBitmap.LockBits(new System.Drawing.Rectangle(0, 0, subwidth, subheight), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            int bytesMain = Math.Abs(bmMainData.Stride) * mainheight;
            int strideMain = bmMainData.Stride;
            System.IntPtr Scan0Main = bmMainData.Scan0;
            byte[] dataMain = new byte[bytesMain];
            System.Runtime.InteropServices.Marshal.Copy(Scan0Main, dataMain, 0, bytesMain);

            int bytesSub = Math.Abs(bmSubData.Stride) * subheight;
            int strideSub = bmSubData.Stride;
            System.IntPtr Scan0Sub = bmSubData.Scan0;
            byte[] dataSub = new byte[bytesSub];
            System.Runtime.InteropServices.Marshal.Copy(Scan0Sub, dataSub, 0, bytesSub);

            for (int y = 0; y < moveheight; ++y)
            {
                for (int x = 0; x < movewidth; ++x)
                {
                    MyColor curcolor = GetColor(x, y, strideMain, dataMain);

                    foreach (var item in possiblepos.ToArray())
                    {
                        int xsub = x - item.X;
                        int ysub = y - item.Y;
                        if (xsub >= subwidth || ysub >= subheight || xsub < 0)
                            continue;

                        MyColor subcolor = GetColor(xsub, ysub, strideSub, dataSub);

                        if (!curcolor.Equals(subcolor))
                        {
                            possiblepos.Remove(item);
                        }
                    }

                    if (curcolor.Equals(GetColor(0, 0, strideSub, dataSub)))
                        possiblepos.Add(new System.Drawing.Point(x, y));
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(dataSub, 0, Scan0Sub, bytesSub);
            subBitmap.UnlockBits(bmSubData);

            System.Runtime.InteropServices.Marshal.Copy(dataMain, 0, Scan0Main, bytesMain);
            mainBitmap.UnlockBits(bmMainData);

            return possiblepos;
        }


        private static MyColor GetColor(int x, int y, int stride, byte[] data)
        {
            int pos = y * stride + x * 4;
            byte a = data[pos + 3];
            byte r = data[pos + 2];
            byte g = data[pos + 1];
            byte b = data[pos + 0];
            return MyColor.FromARGB(a, r, g, b);
        }

        struct MyColor
        {
            byte A;
            byte R;
            byte G;
            byte B;

            public static MyColor FromARGB(byte a, byte r, byte g, byte b)
            {
                MyColor mc = new MyColor();
                mc.A = a;
                mc.R = r;
                mc.G = g;
                mc.B = b;
                return mc;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is MyColor))
                    return false;
                MyColor color = (MyColor)obj;
                if (color.A == this.A && color.R == this.R && color.G == this.G && color.B == this.B)
                    return true;
                return false;
            }
        }

    }

}