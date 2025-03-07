using Amdocs.Ginger.Repository;
using GingerCoreNET.SourceControl;
using System;
namespace Amdocs.Ginger.Common.SourceControlLib
{
    public class GingerSolution : RepositoryItemBase
    {
        [IsSerializedForLocalRepository]
        public  Guid SolutionGuid { get; set; }
        [IsSerializedForLocalRepository]
        public SourceControlInfo SourceControlInfo { get; set; }
        public override string ItemName { get => nameof(GingerSolution); set { } }
    }

    public class SourceControlInfo : RepositoryItemBase
    {
        [IsSerializedForLocalRepository]
        public string Url { get; set; }


        [IsSerializedForLocalRepository]
        public string Username { get; set; }


        [IsSerializedForLocalRepository]
        public SourceControlBase.eSourceControlType Type { get; set; }


        [IsSerializedForLocalRepository]
        public string AuthorName { get; set; }


        [IsSerializedForLocalRepository]
        public string AuthorEmail { get; set; }


        [IsSerializedForLocalRepository]
        public string Branch { get; set; }


        [IsSerializedForLocalRepository]
        public string LocalFolderPath { get; set; }


        [IsSerializedForLocalRepository]
        public new Guid Guid { get; set; }


        [IsSerializedForLocalRepository]
        public bool IsProxyConfigured { get; set; }


        [IsSerializedForLocalRepository]
        public string ProxyAddress { get; set; }


        [IsSerializedForLocalRepository]
        public string ProxyPort { get; set; }


        [IsSerializedForLocalRepository(80)]
        public int Timeout { get; set; }


        public string Password{get; set;}


        public string EncryptedPassword { get; set; }


        public bool UseShellClient { get; internal set; }

        public bool IgnoreCertificate { get; internal set; }


        public override string ItemName { get => nameof(SourceControlInfo); set { } }
    }


}




