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

public class HttpLogAppender : AppenderSkeleton
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private readonly BlockingCollection<LoggingEvent> _queue = new BlockingCollection<LoggingEvent>(new ConcurrentQueue<LoggingEvent>());
    private CancellationTokenSource _cts;
    private Task _workerTask;

    // not set from config, injected at runtime
    private string _apiUrl;
    public string ApiUrl
    {
        get => _apiUrl;
        set
        {
            _apiUrl = value;
        }
    }

    private int batchSize = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["HttpLogBatchSize"]?.ToString()) ? Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["HttpLogBatchSize"]?.ToString()) : 20;

    public int GetBatchSize()
    {
        return batchSize;
    }

    public void SetBatchSize(int value)
    {
        batchSize = value;
    }

    private int FlushIntervalSeconds = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["HttpLogFlushIntervalSeconds"]?.ToString()) ? Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["HttpLogFlushIntervalSeconds"]?.ToString()) : 5;

    public int GetFlushIntervalSeconds()
    {
        return FlushIntervalSeconds;
    }

    public void SetFlushIntervalSeconds(int value)
    {
        FlushIntervalSeconds = value;
    }

    private int MaxRetryDelaySeconds = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["HttpLogMaxRetryDelaySeconds"]?.ToString()) ? Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["HttpLogMaxRetryDelaySeconds"]?.ToString()) : 60;

    public int GetMaxRetryDelaySeconds()
    {
        return MaxRetryDelaySeconds;
    }

    public void SetMaxRetryDelaySeconds(int value)
    {
        MaxRetryDelaySeconds = value;
    }

    public Guid? ExecutionId { get; set; }

    public Guid? InstanceId { get; set; }

    public Guid LogId { get; set; }

    public string LogData { get; set; }

    private AccountReportApiHandler _accountReportApiHandler;

    public AccountReportApiHandler AccountReportApiHandler
    {
        get => _accountReportApiHandler;
        set => _accountReportApiHandler = value;
    }

    public override void ActivateOptions()
    {
        base.ActivateOptions();
        _cts = new CancellationTokenSource();
        _workerTask = Task.Run(() => ProcessQueue(_cts.Token));
    }

    protected override void OnClose()
    {
        try
        {
            if (_workerTask != null)
            {
                _workerTask.Wait(2000); // Wait for the task to complete or timeout
            }
            _cts?.Cancel();
        }
        catch (AggregateException ex)
        {
            // Handle TaskCanceledException explicitly
            ex.Handle(e => e is TaskCanceledException);
        }
        finally
        {
            base.OnClose();
        }
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
                if(!string.IsNullOrEmpty(ApiUrl))
                {
                    LoggingEvent log;
                    if (_queue.TryTake(out log, TimeSpan.FromSeconds(FlushIntervalSeconds)))
                    {
                        buffer.Add(log);
                    }

                    if (buffer.Count >= GetBatchSize() || (buffer.Count > 0 && log == null))
                    {
                        string LogData = string.Empty;
                        //var payload = new List<object>();
                        foreach (var evt in buffer)
                        {
                            LogData = $"{LogData}[{evt.Level.DisplayName} | {evt.TimeStamp.ToString("HH:mm:ss:fff_dd-MMM")}]{evt.RenderedMessage}";
                            Exception ex = evt.ExceptionObject;

                            if (ex != null)
                            {
                                string excFullInfo = "Error:" + ex.Message + Environment.NewLine;
                                excFullInfo += "Source:" + ex.Source + Environment.NewLine;
                                excFullInfo += "Stack Trace: " + ex.StackTrace;

                                LogData = $"{LogData}{Environment.NewLine}Exception Details:{Environment.NewLine}{excFullInfo}";
                            }
                            LogData = $"{LogData}{Environment.NewLine}{Environment.NewLine}";
                        }

                        if (AccountReportApiHandler != null && !string.IsNullOrEmpty(ApiUrl))
                        {
                            await AccountReportApiHandler.SendExecutionLogToCentralDBAsync(ApiUrl, ExecutionId, InstanceId, LogData);
                        }
                        else
                        {
                            Console.WriteLine("[HttpLogAppender] AccountReportApiHandler or ApiUrl is not set. Cannot send logs.");
                        }

                        buffer.Clear();
                        retryDelay = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HttpLogAppender] Failed to send logs: {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(retryDelay), token);
                retryDelay = Math.Min(retryDelay * 2, MaxRetryDelaySeconds);
            }
        }
    }
}
