#region License
/*
Copyright © 2014-2025 European Support Limited

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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Common.WorkSpaceLib
{
    public class AssignmentInfo
    {
        public string FolderPath { get; set; } = string.Empty;
        public DateTime LastHeartbeatUtc { get; set; }
    }

    public class RepoFolderManager : IDisposable
    {
        private readonly string _lockFilePath;
        private readonly string _assignmentFilePath;
        private readonly string _baseWorkingFolder;

        private FileStream _lockFileStream;

        // Holds the assignments loaded in memory during lock
        private Dictionary<string, AssignmentInfo> _assignments = [];

        // Process identifier (should be unique per process, e.g. GUID or PID)
        private readonly string _processId;

        // Configuration for timeouts
        private readonly TimeSpan _lockAcquisitionTimeout;
        private readonly TimeSpan _staleAssignmentTimeout;
        private readonly int _maxLockRetryAttempts;
        private readonly TimeSpan _initialRetryDelay;

        public RepoFolderManager(string processId, string baseWorkingFolder = null,
            TimeSpan? lockAcquisitionTimeout = null,
            TimeSpan? staleAssignmentTimeout = null,
            int maxLockRetryAttempts = 100,
            TimeSpan? initialRetryDelay = null)
        {
            _baseWorkingFolder = baseWorkingFolder ?? GeneralLib.General.DefaultGingerReposFolder;
            _processId = processId ?? throw new ArgumentNullException(nameof(processId));

            // Set default timeouts if not provided
            _lockAcquisitionTimeout = lockAcquisitionTimeout ?? TimeSpan.FromMinutes(5);
            _staleAssignmentTimeout = staleAssignmentTimeout ?? TimeSpan.FromHours(12);
            _maxLockRetryAttempts = maxLockRetryAttempts;
            _initialRetryDelay = initialRetryDelay ?? TimeSpan.FromMilliseconds(100);

            // Use dedicated subfolder for coordination to avoid clutter and allow permissions handling.
            string dataFolder = Path.Combine(GeneralLib.General.LocalUserApplicationDataFolderPath, "RepoFolderManagerData");
            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }

            _lockFilePath = Path.Combine(dataFolder, "repo_folder_pool.lock");
            _assignmentFilePath = Path.Combine(dataFolder, "repo_folder_pool_assignments.json");
        }


        /// <summary>
        /// Assigns a unique numbered folder inside the repo folder for this process.
        /// Blocks and locks the assignment store during operation.
        /// </summary>
        /// <param name="repoName">The git repo name - used for parent folder name</param>
        /// <returns>Full path to the assigned numbered folder for this process</returns>
        public string AssignFolder(string repoName)
        {
            if (string.IsNullOrWhiteSpace(repoName))
            {
                throw new ArgumentException("repoName cannot be empty.", nameof(repoName));
            }

            AcquireLockWithRetry();

            try
            {
                LoadAssignments();

                // Clean up stale assignments
                CleanupStaleAssignments();

                // Remove any previous assignment for this process if exists
                RemovePreviousAssignment();

                string repoBaseFolder = Path.Combine(_baseWorkingFolder, repoName);

                if (!Directory.Exists(repoBaseFolder))
                {
                    Directory.CreateDirectory(repoBaseFolder);
                }

                int folderNum = 1;
                string assignedFolder = null;
                while (folderNum < 1000)
                {
                    string subFolderPath = Path.Combine(repoBaseFolder, folderNum.ToString());

                    bool inUse = _assignments.Values.Any(assignment =>
                        string.Equals(assignment.FolderPath, subFolderPath, StringComparison.OrdinalIgnoreCase));

                    if (!inUse)
                    {
                        if (!Directory.Exists(subFolderPath))
                        {
                            Directory.CreateDirectory(subFolderPath);
                        }

                        // Assign to this process with current timestamp
                        _assignments[_processId] = new AssignmentInfo
                        {
                            FolderPath = subFolderPath,
                            LastHeartbeatUtc = DateTime.UtcNow
                        };
                        WriteAssignments();

                        assignedFolder = subFolderPath;
                        break;
                    }

                    folderNum++;
                }

                if (assignedFolder == null)
                {
                    throw new InvalidOperationException($"Could not find available folder in {repoBaseFolder} after checking 1000 options.");
                }

                return assignedFolder;
            }
            finally
            {
                ReleaseLock();
            }
        }

        /// <summary>
        /// Releases the folder assigned to this process.
        /// </summary>
        public void ReleaseFolder()
        {
            AcquireLockWithRetry();

            try
            {
                LoadAssignments();

                // Remove assignment for this process if any
                if (_assignments.Remove(_processId))
                {
                    WriteAssignments();
                }
            }
            finally
            {
                ReleaseLock();
            }
        }

        /// <summary>
        /// Updates the heartbeat timestamp for this process's assignment.
        /// Call this periodically to prevent the assignment from being considered stale.
        /// </summary>
        public void UpdateHeartbeat()
        {
            AcquireLockWithRetry();

            try
            {
                LoadAssignments();

                if (_assignments.TryGetValue(_processId, out var assignment))
                {
                    assignment.LastHeartbeatUtc = DateTime.UtcNow;
                    WriteAssignments();
                }
            }
            finally
            {
                ReleaseLock();
            }
        }

        #region Private Helpers

        private void AcquireLockWithRetry()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var currentDelay = _initialRetryDelay;
            int attempt = 0;

            while (stopwatch.Elapsed < _lockAcquisitionTimeout && attempt < _maxLockRetryAttempts)
            {
                try
                {
                    // Try to acquire the lock
                    _lockFileStream = new FileStream(_lockFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

                    // Use a platform-safe locking mechanism
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        _lockFileStream.Lock(0, 0);
                    }

                    return; // Successfully acquired lock
                }
                catch (IOException)
                {
                    // Lock is held by another process, dispose the stream and retry
                    _lockFileStream?.Dispose();
                    _lockFileStream = null;

                    attempt++;
                    if (attempt < _maxLockRetryAttempts && stopwatch.Elapsed < _lockAcquisitionTimeout)
                    {
                        Thread.Sleep(currentDelay);
                        // Exponential backoff with jitter
                        currentDelay = TimeSpan.FromMilliseconds(Math.Min(currentDelay.TotalMilliseconds * 2, 5000));
                    }
                }
            }

            throw new TimeoutException($"Failed to acquire lock within {_lockAcquisitionTimeout} after {attempt} attempts.");
        }

        private void ReleaseLock()
        {
            if (_lockFileStream != null)
            {
                try
                {
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        _lockFileStream.Unlock(0, 0);
                    }
                }
                catch (Exception) { /* Ignore unlocking errors */ }

                _lockFileStream.Close();
                _lockFileStream.Dispose();
                _lockFileStream = null;
            }
        }

        private void CleanupStaleAssignments()
        {
            var cutoffTime = DateTime.UtcNow.Subtract(_staleAssignmentTimeout);
            var staleKeys = _assignments
                .Where(kvp => kvp.Value.LastHeartbeatUtc < cutoffTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var staleKey in staleKeys)
            {
                _assignments.Remove(staleKey);
            }

            if (staleKeys.Count > 0)
            {
                WriteAssignments();
            }
        }

        private void LoadAssignments()
        {
            if (!File.Exists(_assignmentFilePath))
            {
                _assignments = [];
                return;
            }

            try
            {
                string json = File.ReadAllText(_assignmentFilePath);
                _assignments = JsonSerializer.Deserialize<Dictionary<string, AssignmentInfo>>(json) ?? [];
            }
            catch (Exception ex) when (ex is IOException || ex is JsonException || ex is UnauthorizedAccessException)
            {
                _assignments = [];
            }

        }

        private void WriteAssignments()
        {
            var json = JsonSerializer.Serialize(_assignments, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_assignmentFilePath, json);
        }

        private void RemovePreviousAssignment()
        {
            _assignments.Remove(_processId);
        }

        #endregion

        public void Dispose()
        {
            // Release any folder assignment before disposing
            try
            {
                ReleaseFolder();
            }
            catch (Exception)
            {
                // Ignore errors during cleanup
            }

            ReleaseLock();
        }
    }
}