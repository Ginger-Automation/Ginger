using GingerCore;
using GingerCore.ALM;
using GingerCore.Environments;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerCore.ALM
{
    public class PublishToALMConfig
    {
        public bool IsVariableInTCRunUsed { get; set; }

        private string mVariableForTCRunName;        
        public string VariableForTCRunName
        {
            get
            {
                return mVariableForTCRunName;
            }
            set
            {
                if (mVariableForTCRunName != value)
                {
                    mVariableForTCRunName = value;
                    OnPropertyChanged(nameof(VariableForTCRunName));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
      
        public string VariableForTCRunNameCalculated { get; set; }   
                       
        public bool ToAttachActivitiesGroupReport { get; set; }
     
        public FilterByStatus FilterStatus { get; set; }
                       
        public void CalculateTCRunName(ValueExpression ve)
        {          
            if (IsVariableInTCRunUsed && (VariableForTCRunName != null) && (VariableForTCRunName != string.Empty))
            {
                ve.Value = VariableForTCRunName;
                VariableForTCRunNameCalculated = ve.ValueCalculated;
            }
            else
            {
               ve.Value = "GingerRun_{VBS Eval=now()}";
               VariableForTCRunNameCalculated = ve.ValueCalculated;
            }
        }
    }
}
