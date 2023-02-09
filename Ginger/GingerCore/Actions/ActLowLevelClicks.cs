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

extern alias UIAComWrapperNetstandard;
using UIAuto = UIAComWrapperNetstandard::System.Windows.Automation;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common.Repository;
using GingerCore.Properties;
using GingerCore.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using GingerCore.Platforms;
using System.Runtime.InteropServices;
using GingerCore.Helpers;
using System.Drawing.Imaging;
using System.Drawing;

using GingerCore.Drivers;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.CoreNET;

namespace GingerCore.Actions
{
    public class ActLowLevelClicks : ActImageCaptureSupport, IObsoleteAction
    {
        public override string ActionDescription { get { return "Image search and click on screen"; } }
        public override string ActionUserDescription { get { return "Image search and click on screen"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
        }

        public override string ActionEditPage { get { return "ActLowLevelClicksEditPage"; } }
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

        public enum eActLowLevelClicksAction
        {

            MouseRightClick = 1,
            MouseLeftClick = 0,
            MouseLeftDoubleClick = 2,
            InputValue = 3,

        }

        public string WindowTitle
        {
            get
            {
                return GetOrCreateInputParam(nameof(WindowTitle)).Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(WindowTitle), value);
            }
        }
        public eActLowLevelClicksAction ActLowLevelClicksAction
        {
            get
            {
                return (eActLowLevelClicksAction)GetOrCreateInputParam<eActLowLevelClicksAction>(nameof(ActLowLevelClicksAction), eActLowLevelClicksAction.MouseLeftClick);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ActLowLevelClicksAction), value.ToString());
            }
        }
        public override int ClickX
        {
            get
            {
                int value;
                int.TryParse(GetOrCreateInputParam(nameof(ClickX)).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ClickX), value.ToString());
            }
        }
        public override int ClickY
        {
            get
            {
                int value;
                int.TryParse(GetOrCreateInputParam(nameof(ClickY)).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ClickY), value.ToString());
            }
        }
        public override int StartX
        {
            get
            {
                int value;
                int.TryParse(GetOrCreateInputParam(nameof(StartX)).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(StartX), value.ToString());
            }
        }
        public override int StartY
        {
            get
            {
                int value;
                int.TryParse(GetOrCreateInputParam(nameof(StartY)).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(StartY), value.ToString());
            }
        }
        public override int EndX
        {
            get
            {
                int value;
                int.TryParse(GetOrCreateInputParam(nameof(EndX)).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(EndX), value.ToString());
            }
        }
        public override int EndY
        {
            get
            {
                int value;
                int.TryParse(GetOrCreateInputParam(nameof(EndY)).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(EndY), value.ToString());
            }
        }
        public override string LocatorImgFile
        {
            get
            {
                return GetOrCreateInputParam(nameof(LocatorImgFile)).Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(LocatorImgFile), value);
            }
        }

        public override string ImagePath
        {
            get
            {
                return @"Documents\ExpectedImages\";
            }
        }

        public override String ActionType
        {
            get
            {
                return "Low level screen search and click Action";
            }
        }
        public new static partial class Fields
        {
            public static string Coordinates = "Coordinates";
            public static string LocatorImgFile = "LocatorImgFile";
            public static string WindowTitle = "WindowTitle";

        }

        public override eImageType Image { get { return eImageType.MousePointer; } }
        public override List<ePlatformType> LegacyActionPlatformsList { get { return Platforms; } }

        public override void Execute()
        {
            Bitmap MainWinImage;
            UIAuto.AutomationElement targetWin = null;
            UIAuto.AutomationElement gingerWin = null;
            WinAPIAutomation winAPI = new WinAPIAutomation();
            List<System.Drawing.Point> result = new List<System.Drawing.Point>(); //System.Drawing.Point(0,0);
            if (!string.IsNullOrWhiteSpace(WindowTitle))
                targetWin = UIAutomationGetWindowByTitle(WindowTitle);
            string locatorImgFilePath;

            //locatorImgFilePath = LocatorImgFile.Replace("~\\", SolutionFolder);
            locatorImgFilePath = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(LocatorImgFile);

            if (!File.Exists(LocatorImgFile))
            {
                if (LocatorImgFile.Contains("Documents"))
                {
                    int getLengthFilePath = LocatorImgFile.Length;
                    int getDocumentLastIndex = LocatorImgFile.LastIndexOf("Documents");
                    locatorImgFilePath = System.IO.Path.Combine(SolutionFolder, LocatorImgFile.Substring(getDocumentLastIndex, getLengthFilePath - getDocumentLastIndex));
                    if (!File.Exists(locatorImgFilePath))
                    {
                        Error = "Failed to find the image file at: " + LocatorImgFile;
                        return;
                    }
                }
                else
                {
                    Error = "Failed to find the image file at: " + LocatorImgFile;
                    return;
                }
            }

            if (WindowTitle == "FULLSCREEN")
            {
                List<UIAuto.AutomationElement> wins = UIAutomationGetFirstLevelWindows();
                targetWin = UIAuto.AutomationElement.RootElement;

                foreach (UIAuto.AutomationElement w in wins)
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

                    result = GetSubPositions(MainWinImage, System.Drawing.Image.FromFile(locatorImgFilePath));


                    if (result.Count <= 0)
                    {
                        this.Error = "Image is not found in the current window";
                        return;
                    }
                }
            }
            else if (string.IsNullOrWhiteSpace(WindowTitle) || targetWin == null)
            {
                List<UIAuto.AutomationElement> wins = UIAutomationGetFirstLevelWindows();
                targetWin = UIAuto.AutomationElement.RootElement;

                foreach (UIAuto.AutomationElement w in wins)
                {
                    if (w == UIAutomationGetWindowByTitle("Amdocs Ginger Automation"))
                        continue;
                    MainWinImage = GetWindowBitmap(w);
                    if (File.Exists(locatorImgFilePath) && MainWinImage != null)
                        result = GetSubPositions(MainWinImage, System.Drawing.Image.FromFile(locatorImgFilePath));
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
                    result = GetSubPositions(MainWinImage, System.Drawing.Image.FromFile(locatorImgFilePath));
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

            switch (ActLowLevelClicksAction)
            {
                case eActLowLevelClicksAction.MouseLeftClick:
                    winAPI.SendClickOnWinXYPoint(targetWin, result[0].X + ClickX, result[0].Y + ClickY);
                    break;
                case eActLowLevelClicksAction.MouseRightClick:
                    winAPI.SendRightClickOnWinXYPoint(targetWin, result[0].X + ClickX, result[0].Y + ClickY);
                    break;
                case eActLowLevelClicksAction.MouseLeftDoubleClick:
                    winAPI.SendDoubleClickOnWinXYPoint(targetWin, result[0].X + ClickX, result[0].Y + ClickY);
                    break;
                case eActLowLevelClicksAction.InputValue:
                    winAPI.SetElementTextOnWinXYPoint(targetWin, GetInputParamCalculatedValue("Value"), result[0].X + ClickX, result[0].Y + ClickY);
                    break;
            }
            if (WindowTitle == "FULLSCREEN")
                WinAPIAutomation.NormalizeWindow(gingerWin.Current.ProcessId);
        }

        private UIAuto.AutomationElement UIAutomationGetWindowByTitle(string WindowTitle)
        {
            UIAuto.TreeWalker walker = UIAuto.TreeWalker.ControlViewWalker;
            UIAuto.AutomationElement win = walker.GetFirstChild(UIAuto.AutomationElement.RootElement);

            while (win != null)
            {
                string WinTitle = (string)win.GetCurrentPropertyValue(UIAuto.AutomationElement.NameProperty);
                if (WinTitle.Contains(WindowTitle))
                {
                    return win;
                }
                win = walker.GetNextSibling(win);
            }
            return null;
        }
        private List<UIAuto.AutomationElement> UIAutomationGetFirstLevelWindows()
        {
            UIAuto.TreeWalker walker = UIAuto.TreeWalker.ControlViewWalker;
            List<UIAuto.AutomationElement> winList = new List<UIAuto.AutomationElement>();
            winList.Add(UIAuto.AutomationElement.RootElement);
            UIAuto.AutomationElement win = walker.GetFirstChild(UIAuto.AutomationElement.RootElement);

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

        private Bitmap GetWindowBitmap(UIAuto.AutomationElement window)
        {
            Bitmap bmp = new Bitmap((int)window.Current.BoundingRectangle.Width, (int)window.Current.BoundingRectangle.Height);
            Graphics memoryGraphics = Graphics.FromImage(bmp);
            IntPtr dc = memoryGraphics.GetHdc();
            bool success = PrintWindow((IntPtr)window.Current.NativeWindowHandle, dc, 0);
            memoryGraphics.ReleaseHdc(dc);
            return bmp;
        }

        private void ClickWindowXY(UIAuto.AutomationElement win, int x, int y)
        {
            WinAPIAutomation winAPI = new WinAPIAutomation();
            winAPI.SendClickOnWinXYPoint(win, x, y);
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

        private static MyColor GetColor(System.Drawing.Point point, int stride, byte[] data)
        {
            return GetColor(point.X, point.Y, stride, data);
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

        public bool IsObsoleteForPlatform(ePlatformType platform)
        {
            return true;
        }

        public Act GetNewAction()
        {
            AutoMapper.MapperConfiguration mapConfigBrowserElementt = new AutoMapper.MapperConfiguration(cfg => { cfg.CreateMap<Act, ActSikuli>(); });
            ActSikuli newActSikuli = mapConfigBrowserElementt.CreateMapper().Map<Act, ActSikuli>(this);
            return newActSikuli;
        }

        public Type TargetAction()
        {
            return typeof(ActSikuli);
        }

        public string TargetActionTypeName()
        {
            return new ActSikuli().ActionDescription;
        }

        public ePlatformType GetTargetPlatform()
        {
            return ePlatformType.Web;
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
