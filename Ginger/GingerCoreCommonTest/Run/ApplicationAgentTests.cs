using Amdocs.Ginger.Common.Enums;
using GingerCore.Platforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerCoreCommonTest.Run
{
    [TestClass]
    public class ApplicationAgentTests
    {
        [TestMethod]
        public void AgentProperty_SetNewAgent_DirtyStatusModified()
        {
            ApplicationAgent applicationAgent = new()
            {
                Agent = new GingerCore.Agent() { Name = "Agent_1" }
            };
            applicationAgent.StartDirtyTracking();

            applicationAgent.Agent = new GingerCore.Agent() { Name = "Agent_2" };

            Assert.AreEqual(expected: eDirtyStatus.Modified, actual: applicationAgent.DirtyStatus);
        }
    }
}
