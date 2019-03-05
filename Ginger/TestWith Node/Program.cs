using System;
using Amdocs.Ginger.Common.NodeJs;
namespace TestWith_Node
{
    class Program
    {
        static void Main(string[] args)
        {
            NodeClient NC = new NodeClient();

            NC.SendMessage("Ambsr");

            Console.ReadLine();
        }
    }
}
