using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public interface IBitmapOperations
    {
        public byte[] MergeVertically(IEnumerable<byte[]> images, ImageFormat format);
        public void Save(string filepath, byte[] image, ImageFormat format);
    }
}
