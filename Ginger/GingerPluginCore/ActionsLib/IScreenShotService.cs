using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core.ActionsLib
{
   public interface IScreenShotService
    {
       Bitmap GetActiveScreenImage();

       List<Bitmap> GetAllScreensImages();
    }
}
