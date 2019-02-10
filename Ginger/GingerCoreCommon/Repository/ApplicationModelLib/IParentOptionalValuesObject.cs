using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.Repository
{    
     /// <summary>
     /// This interface is used to handle the generic implementation of OptionValues List
     /// </summary>
    public interface IParentOptionalValuesObject
    {
        string ElementName { get; }

        ObservableList<OptionalValue> OptionalValuesList { get; set; }

        void PropertyChangedEventHandler();
    }
}
