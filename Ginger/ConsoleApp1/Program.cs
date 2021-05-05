using Amdocs.Ginger.CoreNET.SourceControl;
using System;
using System.Collections.Generic;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            SVNSourceControlShellWrapper svn = new SVNSourceControlShellWrapper();
            //svn.Init();
            string error = "";
            List<string> conflictsPaths = new List<string>();
            svn.GetLatest(@"C:\temp\Ginger-TDC",ref error,ref conflictsPaths);
            svn.SourceControlUser = "NatishaKadam";
            svn.SourceControlPass = "Ginger1234";
            svn.SourceControlURL= "http://cmitechint1srv.corp.amdocs.com:81/svn/Ginger-TDC/";
       
            svn.GetProject(@"c:\temp", "http://cmitechint1srv.corp.amdocs.com:81/svn/Ginger-TDC/",ref error);

        }
    }
}
