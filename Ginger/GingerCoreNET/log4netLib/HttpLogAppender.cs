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

using AccountReport.Contracts;
using AccountReport.Contracts.Enum;
using AccountReport.Contracts.RequestModels;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.External.GingerPlay;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib.CenteralizedExecutionLogger;
using log4net.Appender;
using log4net.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.log4netLib
{
    public class HttpLogAppender : AppenderSkeleton
    {
        private readonly BlockingCollection<LoggingEvent> _queue = new BlockingCollection<LoggingEvent>(new ConcurrentQueue<LoggingEvent>());
        private CancellationTokenSource _cts;
        private Task _workerTask;
        private bool _disposed = false; // To detect redundant calls to Dispose
        private bool isExecutionStarted = false;
        private bool isRunSetStarted = false;
        private bool isPreRunSetOperation = false;
        private bool isPostRunSetOperation = false;

        // not set from config, injected at runtime
        private string _apiUrl;
        public string ApiUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_apiUrl))
                {
                    if (!string.IsNullOrWhiteSpace(GingerPlayEndPointManager.GetAccountReportServiceUrlByGateWay()))
                    {
                        _apiUrl = GingerPlayEndPointManager.GetAccountReportServiceUrlByGateWay();
                    }
                }
                return _apiUrl;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(_apiUrl) || (!string.IsNullOrWhiteSpace(value) && value != _apiUrl))
                {
                    _apiUrl = value;
                    _accountReportApiHandler = string.IsNullOrWhiteSpace(_apiUrl) ? null : new AccountReportApiHandler(_apiUrl);
                    _apiUrl = $"{value}{GingerPlayEndPointManager.GetAccountReportServiceGateWay()}"; //Final Url with gateway
                }
            }
        }


        private static string GetConfigString(string key, string defaultValue)
        {
            string value = System.Configuration.ConfigurationManager.AppSettings[key];
            return !string.IsNullOrEmpty(value) ? value : defaultValue;
        }


        public string BatchSize { get; set; } = GetConfigString("HttpLogBatchSize", "20");

        public int GetBatchSize()
        {
            if (!int.TryParse(BatchSize, out var result) || result <= 0)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "[HttpLogAppender] Invalid BatchSize configuration, defaulting to 20");
                result = 20;
            }
            return result;
        }

        public string FlushIntervalSeconds { get; set; } = GetConfigString("HttpLogFlushIntervalSeconds", "5");
        public string MaxRetryDelaySeconds { get; set; } = GetConfigString("HttpLogMaxRetryDelaySeconds", "60");

        public Guid? ExecutionId { get; set; }

        public long? InstanceId { get; set; }

        private AccountReportApiHandler _accountReportApiHandler;

        public AccountReportApiHandler AccountReportApiHandler
        {
            get
            {
                if (_accountReportApiHandler == null)
                {
                    if (!string.IsNullOrEmpty(_apiUrl))
                    {
                        _accountReportApiHandler = new AccountReportApiHandler(_apiUrl);
                    }
                }
                return _accountReportApiHandler;
            }
        }

        public override void ActivateOptions()
        {
            base.ActivateOptions();
            // Validate configuration
            if (string.IsNullOrEmpty(BatchSize) || !int.TryParse(BatchSize, out _))
            {
                Reporter.ToLog(eLogLevel.DEBUG, "[HttpLogAppender] Invalid BatchSize configuration, using default 20");
                BatchSize = "20";
            }
            if (string.IsNullOrEmpty(FlushIntervalSeconds) || !int.TryParse(FlushIntervalSeconds, out _))
            {
                Reporter.ToLog(eLogLevel.DEBUG, "[HttpLogAppender] Invalid FlushIntervalSeconds configuration, using default 5");
                FlushIntervalSeconds = "5";
            }
            if (string.IsNullOrEmpty(MaxRetryDelaySeconds) || !int.TryParse(MaxRetryDelaySeconds, out _))
            {
                Reporter.ToLog(eLogLevel.DEBUG, "[HttpLogAppender] Invalid MaxRetryDelaySeconds configuration, using default 60");
                MaxRetryDelaySeconds = "60";
            }
            _cts = new CancellationTokenSource();
            _workerTask = Task.Run(() => ProcessQueue(_cts.Token));
        }

        protected override void OnClose()
        {
            Dispose(); // Ensure Dispose is called when the appender is closed
            base.OnClose();
        }
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            try
            {                
                _queue?.CompleteAdding();   
                
                // Process any remaining items in the queue before shutdown
                ProcessRemainingItems();
                      
                _cts?.Cancel();
                if (_workerTask != null)
                {
                    _workerTask.Wait(2000); // Wait for the task to complete or timeout
                }              
                _cts?.Dispose(); // Dispose of the CancellationTokenSource                
            }
            catch (AggregateException ex)
            {
                // Handle TaskCanceledException explicitly               
                Reporter.ToLog(eLogLevel.DEBUG, "Dispose Error disposing HttpLogAppenders", ex);
                ex.Handle(e => e is TaskCanceledException);
            }
            finally
            {
                _disposed = true;
            }
        }

        ~HttpLogAppender()
        {
            Dispose(); // Finalizer to ensure resources are cleaned up
        }

        private async void ProcessRemainingItems()
        {
            try
            {                
                var remainingItems = new List<LoggingEvent>();

                // Collect all remaining items
                while (_queue.TryTake(out LoggingEvent item, TimeSpan.FromMilliseconds(100)))
                {                    
                    remainingItems.Add(item);
                }                

                if (remainingItems.Count > 0)
                {
                    // Process the remaining items synchronously                    
                    await ProcessBatch(remainingItems);                    
                }                
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "[HttpLogAppender] Error processing remaining items during disposal", ex);
            }           
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            try
            {
                LoggingEvent newEvent;

                // If there's an exception, use the constructor that accepts the exception
                if (loggingEvent.ExceptionObject != null)
                {
                    newEvent = new LoggingEvent(
                       typeof(LoggingEvent),
                        loggingEvent.Repository,
                        loggingEvent.LoggerName,
                        loggingEvent.Level,
                        loggingEvent.MessageObject,
                         loggingEvent.ExceptionObject  // Pass the exception directly
                    );
                }
                else
                {
                    // Use the existing approach for events without exceptions
                    var loggingData = loggingEvent.GetLoggingEventData();
                    newEvent = new LoggingEvent(loggingData);
                }

                // Create a snapshot of the Properties collection to avoid collection modification exceptions
                var propertiesSnapshot = new Dictionary<string, object>();
                foreach (System.Collections.DictionaryEntry entry in loggingEvent.Properties)
                {
                    string key = entry.Key.ToString();
                    propertiesSnapshot[key] = entry.Value;
                }

                // Copy custom properties from the snapshot
                foreach (var kvp in propertiesSnapshot)
                {
                    newEvent.Properties[kvp.Key] = kvp.Value;
                }

                // Add to queue for async processing
                if (!_disposed && !_queue.IsAddingCompleted)
                {
                    try
                    {
                        _queue.Add(newEvent);
                    }
                    catch (InvalidOperationException)
                    {
                        // Queue is completed for adding, ignore
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't break the logging chain
                Reporter.ToLog(eLogLevel.DEBUG, $"[HttpLogAppender] Error in Append: {ex.Message}");
            }
        }


        // Updated ProcessQueue method
        private async Task ProcessQueue(CancellationToken token)
        {
            var buffer = new List<LoggingEvent>();
            int retryDelay = 1;

            while (!token.IsCancellationRequested || !_queue.IsCompleted)
            {
                try
                {
                    if (WorkSpace.Instance is not null && WorkSpace.Instance.RunningInExecutionMode && !string.IsNullOrEmpty(ApiUrl))
                    {
                        LoggingEvent log;
                        bool hasItem = _queue.TryTake(out log, TimeSpan.FromSeconds(int.Parse(FlushIntervalSeconds)));

                        if (hasItem)
                        {
                            buffer.Add(log);
                        }

                        // Process batch if:
                        // 1. Buffer is full (reached batch size)
                        // 2. No new items and buffer has items (timeout occurred)
                        // 3. Queue is completed (shutdown) and buffer has items
                        bool shouldProcess = buffer.Count >= GetBatchSize() ||
                                           (buffer.Count > 0 && !hasItem) ||
                                           (buffer.Count > 0 && _queue.IsCompleted);

                        if (shouldProcess)
                        {
                            int exceptionCount = 0;
                            bool processed = false;

                            while (!processed && exceptionCount <= 3)
                            {
                                try
                                {
                                    await ProcessBatch(buffer);
                                    buffer.Clear();
                                    retryDelay = 1;
                                    processed = true;
                                }
                                catch (Exception ex)
                                {
                                    exceptionCount++;
                                    if (exceptionCount > 3)
                                    {
                                        Reporter.ToLog(eLogLevel.DEBUG, "[HttpLogAppender] Failed to send logs after 3 retries, dropping logs.", ex);
                                        buffer.Clear();
                                        processed = true;
                                    }
                                    else
                                    {
                                        Reporter.ToLog(eLogLevel.DEBUG, $"[HttpLogAppender] Exception occurred while sending logs. Will retry.", ex);
                                        if (!token.IsCancellationRequested)
                                        {
                                            await Task.Delay(TimeSpan.FromSeconds(retryDelay), token);
                                            retryDelay = Math.Min(retryDelay * 2, int.Parse(MaxRetryDelaySeconds));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Avoid tight loop when not in execution mode or API URL is not set
                        if (!token.IsCancellationRequested)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1), token);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, $"[HttpLogAppender] Failed to process queue: {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(retryDelay), token);
                        retryDelay = Math.Min(retryDelay * 2, int.Parse(MaxRetryDelaySeconds));
                    }
                }
            }

            // Final flush - process any remaining items
            if (buffer.Count > 0)
            {
                try
                {
                    await ProcessBatch(buffer);
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "[HttpLogAppender] Error in final flush", ex);
                }
            }
        }

        private Guid? EntityId = Guid.Empty;
        private Guid? solutionId = Guid.Empty;
        private string runsetName = string.Empty;

        // Extract the batch processing logic into a separate method
        private async Task ProcessBatch(List<LoggingEvent> buffer)
        {
            if (buffer == null || buffer.Count == 0)
            {
                return;
            }
            var logDataBuilder = new StringBuilder();
            List<AccountReport.Contracts.RequestModels.ExecutionErrorRequest> ExecutionErrorRequestsList = new List<ExecutionErrorRequest>();

            try
            {
                if (EntityId == Guid.Empty && WorkSpace.Instance?.RunsetExecutor?.RunSetConfig?.Guid != Guid.Empty)
                {
                    EntityId = WorkSpace.Instance?.RunsetExecutor?.RunSetConfig?.Guid;
                }
                if (solutionId == Guid.Empty && WorkSpace.Instance?.Solution?.Guid != Guid.Empty)
                {
                    solutionId = WorkSpace.Instance?.Solution?.Guid;
                }
                if (string.IsNullOrEmpty(runsetName) && !string.IsNullOrEmpty(WorkSpace.Instance?.RunsetExecutor?.RunSetConfig?.Name))
                {
                    runsetName = WorkSpace.Instance?.RunsetExecutor?.RunSetConfig?.Name;
                }
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "exeception occured on fetching Runset/Solution details in HttpLogAppender", ex);
            }

            foreach (var evt in buffer)
            {
                AccountReport.Contracts.RequestModels.ExecutionErrorRequest ExecutionErrorRequests = new AccountReport.Contracts.RequestModels.ExecutionErrorRequest();
                StringBuilder currentLog = new StringBuilder();

                if (evt.RenderedMessage.IndexOf("Run Set Execution Started", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    isExecutionStarted = true;
                    isRunSetStarted = true;
                }

                if (evt.RenderedMessage.IndexOf("Running Pre-Execution Run Set Operations", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    isPreRunSetOperation = true;
                }

                if (evt.RenderedMessage.IndexOf("Running Post-Execution Run Set Operations", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    isPostRunSetOperation = true;
                    isPreRunSetOperation = false;
                }

                if (evt.RenderedMessage.IndexOf("Run Set Execution Ended", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    isExecutionStarted = false;
                }

                if (!isRunSetStarted && evt.Level.DisplayName.Equals("ERROR", StringComparison.Ordinal) && evt.RenderedMessage.IndexOf("Error(s) occurred process exit code", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    try
                    {
                        if (ExecutionId.HasValue && AccountReportApiHandler != null && !string.IsNullOrEmpty(ApiUrl))
                        {
                            AccountReportRunSet accountReportRunSet = new AccountReportRunSet
                            {
                                Id = ExecutionId.Value,
                                ExecutionId = ExecutionId.Value,
                                EntityId = EntityId,
                                GingerSolutionGuid = solutionId ?? Guid.Empty,
                                Name = runsetName,
                                RunStatus = eExecutionStatus.Failed,
                            };

                            bool response = await AccountReportApiHandler.SendRunsetExecutionDataToCentralDBAsync(accountReportRunSet, isUpdate: true).ConfigureAwait(false);
                            if (!response)
                            {
                                Reporter.ToLog(eLogLevel.DEBUG, "[HttpLogAppender] Failed to send Runset Execution data to Central DB.");
                            }
                        }
                    }
                    catch (Exception ex1)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "[HttpLogAppender] Failed to send Runset Execution data to Central DB.", ex1);
                    }
                }
                else
                {
                    currentLog.Append($"[{evt.Level.DisplayName} | {evt.TimeStamp.ToString("HH:mm:ss:fff_dd-MMM")}]{evt.RenderedMessage}");
                }
                string exceptiosource = string.Empty;
                Exception ex = evt.ExceptionObject;
                if (ex != null)
                {
                    string excFullInfo = "Error:" + ex.Message + Environment.NewLine;
                    excFullInfo += "Source:" + ex.Source + Environment.NewLine;
                    excFullInfo += "Stack Trace: " + ex.StackTrace;
                    exceptiosource = ex.Source;
                    currentLog.Append($"{Environment.NewLine}Exception Details:{Environment.NewLine}{excFullInfo}");
                }
                currentLog.Append($"{Environment.NewLine}{Environment.NewLine}");

                if ((evt.Level.DisplayName.Equals("ERROR", StringComparison.Ordinal) || evt.Level.DisplayName.Equals("FATAL", StringComparison.Ordinal)) && !isExecutionStarted)
                {
                    if (!string.IsNullOrEmpty(exceptiosource))
                    {
                        ExecutionErrorRequests.ErrorSource = exceptiosource;
                    }
                    if (evt.RenderedMessage.IndexOf("Error(s) occurred process exit code", StringComparison.OrdinalIgnoreCase) < 0) //Error(s) occurred process exit code should not get add in ExecutionError
                    {
                        SetExecutionError(evt, ExecutionErrorRequests, isExecutionStarted, false);
                    }
                }
                else if (isExecutionStarted)
                {
                    if (evt.Level.DisplayName.Equals("INFO", StringComparison.Ordinal) && evt.RenderedMessage.IndexOf("Action Execution Ended", StringComparison.OrdinalIgnoreCase) >= 0 && evt.RenderedMessage.IndexOf("Execution Status= Failed", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var match = Regex.Match(evt.RenderedMessage, @"SourcePath= (.*?)\n");
                        if (match.Success)
                        {
                            string sourcePath = match.Groups[1].Value;
                            ExecutionErrorRequests.ErrorOriginPath = sourcePath;
                        }
                        var ActionNamematch = Regex.Match(evt.RenderedMessage, @"Description= (.*?)\n");
                        if (ActionNamematch.Success)
                        {
                            string ActionName = ActionNamematch.Groups[1].Value;
                            ExecutionErrorRequests.ErrorSource = ActionName;
                        }
                        int TotalRetries = 0;
                        var TotalRetriesmatch = Regex.Match(evt.RenderedMessage, @"Total Retries Configured= (.*?)\n");
                        if (TotalRetriesmatch.Success && int.TryParse(TotalRetriesmatch.Groups[1].Value, out var tr)) { TotalRetries = tr; }
                        int CurrentRetry = 0;
                        var CurrentRetrymatch = Regex.Match(evt.RenderedMessage, @"Current Retry Iteration= (.*?)\n");
                        if (CurrentRetrymatch.Success && int.TryParse(CurrentRetrymatch.Groups[1].Value, out var cr)) { CurrentRetry = cr; }
                        var ActionIdmatch = Regex.Match(evt.RenderedMessage, @"ID:([^,]+)");
                        if (ActionIdmatch.Success)
                        {
                            string ActionId = ActionIdmatch.Groups[1].Value;
                            ExecutionErrorRequests.ErrorOriginID = ActionId;
                        }

                        if (TotalRetries == CurrentRetry)
                        {
                            SetExecutionError(evt, ExecutionErrorRequests, isExecutionStarted, false);
                        }

                    }
                }
                else if (isPreRunSetOperation)
                {
                    if (Regex.IsMatch(evt.Level.DisplayName, @"^INFO$") && Regex.IsMatch(evt.RenderedMessage, @"Execution Ended for Run Set Operation.*Status= Failed", RegexOptions.Singleline))
                    {
                        var OperationNameMatch = Regex.Match(evt.RenderedMessage, @"\band Name\s+([^,\r\n]+)");
                        if (OperationNameMatch.Success)
                        {
                            string OPerationName = OperationNameMatch.Groups[1].Value;
                            ExecutionErrorRequests.ErrorOriginPath = OPerationName;
                        }
                        ExecutionErrorRequests.ErrorSource = "Pre Runset Operation Type";
                        SetExecutionError(evt, ExecutionErrorRequests, isExecutionStarted, true);
                    }
                }
                else if (isPostRunSetOperation)
                {
                    if (Regex.IsMatch(evt.Level.DisplayName, @"^INFO$") && Regex.IsMatch(evt.RenderedMessage, @"Execution Ended for Run Set Operation.*Status= Failed", RegexOptions.Singleline))
                    {
                        var OperationNameMatch = Regex.Match(evt.RenderedMessage, @"\band Name\s+([^,\r\n]+)");
                        if (OperationNameMatch.Success)
                        {
                            string OPerationName = OperationNameMatch.Groups[1].Value;
                            ExecutionErrorRequests.ErrorOriginPath = OPerationName;
                        }
                        ExecutionErrorRequests.ErrorSource = "Post Runset Operation Type";
                        SetExecutionError(evt, ExecutionErrorRequests, isExecutionStarted, true);
                    }
                }

                logDataBuilder.Append(currentLog);
                if (!string.IsNullOrEmpty(ExecutionErrorRequests?.ErrorMessage))
                {
                    ExecutionErrorRequestsList.Add(ExecutionErrorRequests);
                }
            }

            // Send the batch
            string LogData = logDataBuilder.ToString();
            if (AccountReportApiHandler != null && !string.IsNullOrEmpty(ApiUrl))
            {
                AccountReport.Contracts.RequestModels.ExecutionLogRequest ExecutionLogRequest = new AccountReport.Contracts.RequestModels.ExecutionLogRequest();
                ExecutionLogRequest.ExecutionId = ExecutionId;
                ExecutionLogRequest.LogData = LogData;
                ExecutionLogRequest.InstanceId = InstanceId;
                ExecutionLogRequest.ExecutionErrorRequests = ExecutionErrorRequestsList;

                bool isSuccess = await AccountReportApiHandler.SendExecutionLogToCentralDBAsync(ApiUrl, ExecutionLogRequest);
                if (!isSuccess)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "[HttpLogAppender] Failed to send logs during batch processing.");
                }
            }
        }

        private static void SetExecutionError(LoggingEvent evt, ExecutionErrorRequest ExecutionErrorRequests, bool isExecutionStarted, bool isRunsetOperation)
        {
            ExecutionErrorRequests.ErrorOccurrenceTime = evt.TimeStamp;
            ExecutionErrorRequests.ErrorLevel = isExecutionStarted ? eExecutionErrorLevel.Execution : eExecutionErrorLevel.Setup;

            if (isRunsetOperation)
            {
                Match match = Regex.Match(evt.RenderedMessage, @"Errors= (.*?)\n");
                if (match.Success)
                {
                    ExecutionErrorRequests.ErrorMessage = match.Groups[1].Value;
                }
                else
                {
                    ExecutionErrorRequests.ErrorMessage = evt.RenderedMessage;
                }
            }
            else
            {
                Match match = Regex.Match(evt.RenderedMessage, @"Error Details=([\s\S]*?)\r?\nExtra Details=");
                if (match.Success)
                {
                    ExecutionErrorRequests.ErrorMessage = match.Groups[1].Value;
                }
                else
                {
                    ExecutionErrorRequests.ErrorMessage = evt.RenderedMessage;
                }
            }
        }
    }
}
