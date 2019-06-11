using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web.Actions
{
   public interface ISelect
    {

        void ClearValue();
        List<string> GetValidValue();

        bool IsValuePopulated();
        void Select(string Value);
        void SelectByIndex(int index);
        void SelectByText(string Text);

        string GetSelectedValue();
    }
}
