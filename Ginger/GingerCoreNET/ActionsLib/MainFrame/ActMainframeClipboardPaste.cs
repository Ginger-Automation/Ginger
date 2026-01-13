using Amdocs.Ginger.Common.InterfacesLib;
using GingerCore.Actions;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Open3270;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.ActionsLib.MainFrame
{
    public class ActMainframeClipboardPaste : Act
    {
        public override string ActionDescription { get { return "Paste Value to MainFrame"; } }
        public override string ActionUserDescription { get { return "Paste Value to MainFrame"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this to Paste Values to Mainframe");
        }

        public override string ActionEditPage { get { return "Mainframe.ActMainFrameClipboardPasteEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

        public new static partial class Fields
        {
            public static string ValueToPaste = "ValueToPaste";
        }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                List<ePlatformType> mPf = [ePlatformType.MainFrame];
                return mPf;
            }
        }

        public override String ActionType
        {
            get
            {
                return "Paste Value to MainFrame";
            }
        }

    }
}
