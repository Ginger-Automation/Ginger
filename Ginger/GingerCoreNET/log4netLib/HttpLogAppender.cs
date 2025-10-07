using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.External.Configurations;
using Amdocs.Ginger.CoreNET.External.GingerPlay;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib.CenteralizedExecutionLogger;
using log4net.Appender;
using log4net.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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

        // not set from config, injected at runtime
        private string _apiUrl;
        public string ApiUrl
        {
            get
            {
                if(string.IsNullOrEmpty(_apiUrl))
                {
                    if( !string.IsNullOrWhiteSpace(GingerPlayEndPointManager.GetAccountReportServiceUrlByGateWay()))
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
                    _apiUrl = $"{value}{GingerPlayEndPointManager.GetAccoutReportServiceGateWay()}"; //Final Url with gateway
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
                Reporter.ToLog(eLogLevel.ERROR, "[HttpLogAppender] Invalid BatchSize configuration, defaulting to 20");
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
                if(_accountReportApiHandler == null)
                {
                    if(!string.IsNullOrEmpty(_apiUrl))
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
                Reporter.ToLog(eLogLevel.ERROR, "[HttpLogAppender] Invalid BatchSize configuration, using default 20");
                BatchSize = "20";
            }
            if (string.IsNullOrEmpty(FlushIntervalSeconds) || !int.TryParse(FlushIntervalSeconds, out _))
            {
                Reporter.ToLog(eLogLevel.ERROR, "[HttpLogAppender] Invalid FlushIntervalSeconds configuration, using default 5");
                FlushIntervalSeconds = "5";
            }
            if (string.IsNullOrEmpty(MaxRetryDelaySeconds) || !int.TryParse(MaxRetryDelaySeconds, out _))
            {
                Reporter.ToLog(eLogLevel.ERROR, "[HttpLogAppender] Invalid MaxRetryDelaySeconds configuration, using default 60");
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

        protected override void Append(LoggingEvent loggingEvent)
        {
            // Assume 'originalEvent' is your LoggingEvent instance
            var newEvent = new LoggingEvent(loggingEvent.GetLoggingEventData());

            // Create a snapshot of the Properties collection
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
            // clone so async worker has its own copy
            _queue.Add(newEvent);
        }


        private async Task ProcessQueue(CancellationToken token)
        {
            var buffer = new List<LoggingEvent>();
            int retryDelay = 1;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (WorkSpace.Instance is not null && WorkSpace.Instance.RunningInExecutionMode && !string.IsNullOrEmpty(ApiUrl))
                    {
                        LoggingEvent log;
                        if (_queue.TryTake(out log, TimeSpan.FromSeconds(int.Parse(FlushIntervalSeconds))))
                        {
                            buffer.Add(log);
                        }

                        if (buffer.Count >= GetBatchSize() || (buffer.Count > 0 && log == null))
                        {
                            var logDataBuilder = new StringBuilder();
                            foreach (var evt in buffer)
                            {
                                logDataBuilder.Append($"[{evt.Level.DisplayName} | {evt.TimeStamp.ToString("HH:mm:ss:fff_dd-MMM")}]{evt.RenderedMessage}");
                                Exception ex = evt.ExceptionObject;

                                if (ex != null)
                                {
                                    string excFullInfo = "Error:" + ex.Message + Environment.NewLine;
                                    excFullInfo += "Source:" + ex.Source + Environment.NewLine;
                                    excFullInfo += "Stack Trace: " + ex.StackTrace;

                                    logDataBuilder.Append($"{Environment.NewLine}Exception Details:{Environment.NewLine}{excFullInfo}");
                                }
                                logDataBuilder.Append($"{Environment.NewLine}");
                            }
                            string LogData = logDataBuilder.ToString();
                            if (AccountReportApiHandler != null)
                            {
                                int exceptionCount = 0;
                                try
                                {
                                    bool isSuccess = await AccountReportApiHandler.SendExecutionLogToCentralDBAsync(ApiUrl, ExecutionId, InstanceId, LogData);
                                    if (isSuccess)
                                    {
                                        buffer.Clear();
                                        retryDelay = 1;
                                        exceptionCount = 0;
                                    }
                                    else
                                    {
                                        Reporter.ToLog(eLogLevel.ERROR, "[HttpLogAppender] Failed to send logs, will retry.");
                                        await Task.Delay(TimeSpan.FromSeconds(retryDelay), token);
                                        retryDelay = Math.Min(retryDelay * 2, int.Parse(MaxRetryDelaySeconds));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    exceptionCount++;
                                    if (exceptionCount > 3)
                                    {
                                        // after 3 exceptions give up and drop the logs
                                        Reporter.ToLog(eLogLevel.ERROR, "[HttpLogAppender] Failed to send logs after 3 attempts, dropping logs.", ex);
                                        buffer.Clear();
                                        retryDelay = 1;
                                    }
                                    else
                                    {
                                        Reporter.ToLog(eLogLevel.ERROR, $"[HttpLogAppender] Exception occurred while sending logs. Will retry.", ex);
                                        await Task.Delay(TimeSpan.FromSeconds(retryDelay), token);
                                        retryDelay = Math.Min(retryDelay * 2, int.Parse(MaxRetryDelaySeconds));
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("[HttpLogAppender] AccountReportApiHandler or ApiUrl is not set. Cannot send logs.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[HttpLogAppender] Failed to send logs: {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(retryDelay), token);
                    retryDelay = Math.Min(retryDelay * 2, int.Parse(MaxRetryDelaySeconds));
                }
            }
        }
    }
}
