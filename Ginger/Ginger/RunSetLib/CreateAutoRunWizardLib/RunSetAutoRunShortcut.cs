using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.RunSetLib
{
    public class RunSetAutoRunShortcut
    {
        public bool CreateShortcut;

        public string ShortcutFileName { get; set; }


        string mShortcutFolderPath = null;
        public string ShortcutFolderPath
        {
            get
            {
                if (mShortcutFolderPath == null)
                {
                    //defualt folder
                    mShortcutFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                }
                return mShortcutFolderPath;
            }
            set
            {
                mShortcutFolderPath = value;
            }
        }
    }
}
