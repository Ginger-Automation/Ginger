#region License
/*
Copyright © 2014-2026 European Support Limited
 
Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
 
http://www.apache.org/licenses/LICENSE-2.0
 
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Amdocs.Ginger.Common.WorkSpaceLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GingerTestHelper;

namespace GingerCoreCommonTest.WorkSpaceLib
{
    [TestClass]
    [Level1]
    public class RepoFolderManagerTests
    {
        private string _tempRoot;
        private string _originalAppDataPathBackup;

        [TestInitialize]
        public void Setup()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), "RepoFolderManagerTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempRoot);

            // We need to override General.LocalUserApplicationDataFolderPath backing field so the manager writes into our temp folder
            // The LocalUserApplicationDataFolderPath implementation caches the value in a private static field 'mAppDataFolder'.
            var generalType = Type.GetType("Amdocs.Ginger.Common.GeneralLib.General, GingerCoreCommon");
            var field = generalType.GetField("mAppDataFolder", BindingFlags.Static | BindingFlags.NonPublic);
            _originalAppDataPathBackup = (string)field.GetValue(null);
            field.SetValue(null, _tempRoot); // force usage of temp folder
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Restore cached path (avoid leaking temp path into other tests)
            var generalType = Type.GetType("Amdocs.Ginger.Common.GeneralLib.General, GingerCoreCommon");
            var field = generalType.GetField("mAppDataFolder", BindingFlags.Static | BindingFlags.NonPublic);
            field.SetValue(null, _originalAppDataPathBackup);

            try { Directory.Delete(_tempRoot, true); } catch { }
        }

        private RepoFolderManager CreateManager(string processId = null)
        {
            return new RepoFolderManager(processId ?? Guid.NewGuid().ToString("N"), Path.Combine(_tempRoot, "Repos"));
        }

        [TestMethod]
        [Timeout(60000)]
        public void AssignFolder_AssignsUniqueFolder()
        {
            var manager = CreateManager("proc1");
            string folder = manager.AssignFolder("repoA");

            Assert.IsTrue(Directory.Exists(folder), "Assigned folder exists");
            StringAssert.Contains(folder, Path.Combine(_tempRoot, "Repos", "repoA"));
        }

        [TestMethod]
        [Timeout(60000)]
        public void AssignFolder_TwoProcessesGetDifferentNumbers()
        {
            var m1 = CreateManager("p1");
            var m2 = CreateManager("p2");

            string f1 = m1.AssignFolder("repoZ");
            string f2 = m2.AssignFolder("repoZ");

            Assert.AreNotEqual(f1, f2, "Different processes should receive distinct folders");
        }

        [TestMethod]
        [Timeout(60000)]
        public void ReleaseFolder_AllowsReuse()
        {
            var m1 = CreateManager("r1");
            var f1 = m1.AssignFolder("repoR");
            m1.ReleaseFolder();

            // New process should be able to get the same folder number again (1)
            var m2 = CreateManager("r2");
            var f2 = m2.AssignFolder("repoR");

            Assert.AreEqual(f1, f2, "Released folder should be reusable");
        }

        [TestMethod]
        [Timeout(60000)]
        [Ignore]
        public void UpdateHeartbeat_RefreshesTimestamp()
        {
            var procId = "hb1";
            var manager = CreateManager(procId);
            var folder = manager.AssignFolder("repoHB");

            // Capture assignment file content pre-update
            string assignmentsPath = Path.Combine(_tempRoot, "RepoFolderManagerData", "repo_folder_pool_assignments.json");
            DateTime before = DateTime.UtcNow;
            manager.UpdateHeartbeat();

            string json = File.ReadAllText(assignmentsPath);
            Assert.IsTrue(json.Contains("\"hb1\""), "Assignment record exists");
            // Not parsing full JSON (simplicity) but ensure file timestamp updated
            Assert.IsTrue(File.GetLastWriteTimeUtc(assignmentsPath) >= before, "Heartbeat caused rewrite");
        }

        [TestMethod]
        [Timeout(60000)]
        public void CleanupStaleAssignments_RemovesOld()
        {
            // Use very short stale timeout by constructing with parameter
            var manager = new RepoFolderManager("stale1", Path.Combine(_tempRoot, "Repos"), staleAssignmentTimeout: TimeSpan.FromMilliseconds(10));
            var folder = manager.AssignFolder("repoS");
            // Wait for staleness
            Task.Delay(30).Wait();
            // Force new manager to trigger cleanup
            var manager2 = new RepoFolderManager("newProc", Path.Combine(_tempRoot, "Repos"), staleAssignmentTimeout: TimeSpan.FromMilliseconds(10));
            manager2.AssignFolder("repoS");

            string assignmentsPath = Path.Combine(_tempRoot, "RepoFolderManagerData", "repo_folder_pool_assignments.json");
            string json = File.ReadAllText(assignmentsPath);
            Assert.IsFalse(json.Contains("stale1"), "Stale assignment removed");
        }

        [TestMethod]
        [Timeout(60000)]
        public void ConcurrentAssignments_NoCollision()
        {
            int count = 5;
            string repo = "repoC";
            var folders = new string[count];

            Parallel.For(0, count, i =>
            {
                var m = CreateManager($"pc{i}");
                folders[i] = m.AssignFolder(repo);
            });

            // Ensure all unique
            Assert.AreEqual(count, folders.Distinct().Count(), "Each process got a unique folder");
        }

        [TestMethod]
        [Timeout(60000)]
        public void AcquireLock_TimesOutUnderContention()
        {
            // Arrange: create a lock file and hold it with exclusive access
            string lockPath = Path.Combine(_tempRoot, "RepoFolderManagerData", "repo_folder_pool.lock");
            Directory.CreateDirectory(Path.GetDirectoryName(lockPath));
            using var holder = new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

            // Act: attempt to assign with very short timeout and few retries
            var manager = new RepoFolderManager( "timeoutProc", Path.Combine(_tempRoot, "Repos"), lockAcquisitionTimeout: TimeSpan.FromMilliseconds(200), maxLockRetryAttempts: 5, initialRetryDelay: TimeSpan.FromMilliseconds(20));

            var ex = Assert.ThrowsException<TimeoutException>(() => manager.AssignFolder("repoT"));
            StringAssert.Contains(ex.Message, "Failed to acquire lock", "Should indicate lock acquisition failure");
        }

        [TestMethod]
        [Timeout(60000)]
        public void CorruptedJson_IsIgnoredAndNewAssignmentSucceeds()
        {
            // Prepare corrupted JSON file
            string assignmentFile = Path.Combine(_tempRoot, "RepoFolderManagerData", "repo_folder_pool_assignments.json");
            Directory.CreateDirectory(Path.GetDirectoryName(assignmentFile));
            File.WriteAllText(assignmentFile, "{ this is not valid json !!!");

            var manager = CreateManager("cj1");
            string folder = manager.AssignFolder("repoCJ");

            Assert.IsTrue(Directory.Exists(folder), "Assignment still succeeded despite corruption");
            string newJson = File.ReadAllText(assignmentFile);
            Assert.IsTrue(newJson.Contains("cj1"), "New assignments file created");
        }
    }
}
