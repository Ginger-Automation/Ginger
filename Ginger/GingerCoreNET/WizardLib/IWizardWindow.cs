using System;
using System.Collections.Generic;
using System.Text;

namespace GingerWPF.WizardLib
{
    public interface IWizardWindow
    {
        void ProcessStarted();
        void ProcessEnded();
        void Close();
        // void ShowDialog(int width = 800);
        void NextButton(bool isEnabled);
    }
}
