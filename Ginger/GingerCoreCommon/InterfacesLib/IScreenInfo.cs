using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public interface IScreenInfo
    {
        public int ScreenCount();
        public int PrimaryScreenIndex();
        public string ScreenName(int screenIndex);
        public Size ScreenSize(int screenIndex);
        public Point ScreenPosition(int screenIndex);
        public Point TaskbarPosition(int screenIndex);
        public Size TaskbarSize(int screenIndex);
    }
}
