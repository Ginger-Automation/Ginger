using System;
using System.Collections.Generic;

namespace GingerCoreNET.SolutionRepositoryLib.SolutionTreeViewLib
{
    // List<>
    public class ObjectContextMenu
    {
        public List<ObjectContextMenuItem> Items = new List<ObjectContextMenuItem>();

        public void AddItem(string Text, Action Action)
        {
            Items.Add(new ObjectContextMenuItem() { Text = Text, Action = Action });
        }
    }
}