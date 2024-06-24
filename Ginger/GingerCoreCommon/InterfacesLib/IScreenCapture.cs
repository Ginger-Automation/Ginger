using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public interface IScreenCapture
    {
        public byte[] Capture(Point position, Size size, ImageFormat format);
    }
}
