#region License
/*
Copyright © Property of Amdocs Quality Engineering 
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Configuration;
using System.Collections;
using System.Threading;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;

namespace Ginger
{
   public class CustomTraceListener : TextWriterTraceListener
    {
        //The traceActivateSwitch determine if to Trace or not
        public Boolean _traceActivateSwitch = false;

        //The traceLevelSwitchs determine which Traces to include in the log file
        public Boolean _traceLevelSwitch_Error = false;
        public Boolean _traceLevelSwitch_Info = false;

        //File streams for each sub App which able to create backup log files according to the 
        //definitions in the config file
        public long _maxFileLength;
        public int _maxFileCount;
        public string _logFilesPath;

        //_customFileStreamsList contains all the file streams
        private ArrayList _customFileStreamsList;

        //_pointingFS Pointing on the File Stream to write in
        private FileStreamWithBackup _pointingFS = null;
        //_traceLevel determine the trace message level
        private string _traceLevel = string.Empty;
        //_sessionID indicate what is the message session ID
        private string _sessionID = string.Empty;
        //_subAppName indicate what is the message sub Application source
        
        //_isVerifiedToWrite determine if to write or not according to the Trace switches
        private Boolean _isVerifiedToWrite = false;

        //Set the App.config change watcher
        ConfigFileWatcher _AppConfigWatcher = null;

        //creating a single CustomTraceListener instance
        public static CustomTraceListener _instance = new CustomTraceListener();

        private CustomTraceListener()
        {
            //Removing all current Listeners
            Trace.Listeners.Clear();

            //getting switchs values from config file
            bool tracelog = WorkSpace.Instance.UserProfile.AppLogLevel == eAppReporterLoggingLevel.Debug ? true : false;
            _traceActivateSwitch = tracelog; 
            _traceLevelSwitch_Error = tracelog;
            _traceLevelSwitch_Info = tracelog;

            //getting log files definitions from config file
            _maxFileLength = (Int64.Parse(System.Configuration.ConfigurationManager.AppSettings["TRACE_MAX_LOG_FILE_SIZE_(MB)"].ToString()) * 1000000);
            _maxFileCount = int.Parse(System.Configuration.ConfigurationManager.AppSettings["TRACE_MAX_NUMBER_OF_LOG_FILES_BACKUPS"].ToString());
            _logFilesPath = WorkSpace.Instance.DefualtUserLocalWorkingFolder;

            //creating the default file stream
            _customFileStreamsList = new ArrayList();
            AddFileStreamToList("DEFAULT");

            //Set the App.config watcher- update switches and files definitions if config file changed
            _AppConfigWatcher = new ConfigFileWatcher();
        }

        public static CustomTraceListener Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Gets one or more subapplication names separated by ',' and create file stream
        /// to each one
        /// </summary>
        /// <param name="subAppName"></param>
        private void AddFileStreamToList(string subAppName)
        {
            //disable log files creation if traceActivateSwitch == false
            if (!_traceActivateSwitch)
            {
                return;
            }
            if ((subAppName != null) && (subAppName != string.Empty))
            {
                //Split subapps name (if there is more then one)
                ArrayList subAppNamesList = new ArrayList(subAppName.ToUpper().Split(new char[] { ',' }));

                foreach (string subAppNameFromList in subAppNamesList)
                {
                    Boolean isAlreadyExist = false;

                    foreach (CustomFileStream CustomFileStreamFromList in _customFileStreamsList)
                    {
                        if (CustomFileStreamFromList.subApplicationName == subAppNameFromList.ToUpper())
                        {
                            isAlreadyExist = true;
                            break;
                        }
                    }

                    if (isAlreadyExist == false)
                    {
                        //Check the log folder exist and if not create it
                        System.IO.DirectoryInfo logsDirectory = new System.IO.DirectoryInfo(_logFilesPath);
                        if (!logsDirectory.Exists)
                        {
                            logsDirectory.Create();
                        }

                        CustomFileStream newFileStream = new CustomFileStream();
                        newFileStream.subApplicationName = subAppNameFromList.Trim().ToUpper();
                        DateTime date = DateTime.Now;
                        if (newFileStream.subApplicationName == "DEFAULT")
                        {
                            newFileStream.fileStream = new FileStreamWithBackup(
                                     (logsDirectory.FullName + '\\' +
                                      System.Configuration.ConfigurationManager.AppSettings["TRACE_DEFUALT_LOG_FILE_NAME"].ToString().ToUpper() + "_" + date.Year.ToString()+"_"+date.Month.ToString()+"_"+date.Day.ToString()+"_"+date.Millisecond.ToString() +  " -Client.log"),
                                      _maxFileLength, _maxFileCount, FileMode.Append);
                        }
                        else
                        {
                            newFileStream.fileStream = new FileStreamWithBackup(
                                     (logsDirectory.FullName + '\\' +
                                      newFileStream.subApplicationName +"_"+ date.Year.ToString() + "_" + date.Month.ToString() + "_" + date.Day.ToString() + "_" + date.Millisecond.ToString() + " -Client.log"),
                                      _maxFileLength, _maxFileCount, FileMode.Append);
                        }

                        newFileStream.isActive = true;
                        _customFileStreamsList.Add(newFileStream);
                    }
                }
            }
        }

        private void SetupTraceMessagDefinitions(string traceMessagDefinitions)
        {
            string _subAppName = string.Empty;
            _pointingFS = null;
            _traceLevel = string.Empty;
            _sessionID = string.Empty;
            _subAppName = string.Empty;
            Boolean isConfigreAble = true;

            if ((traceMessagDefinitions != null) && (traceMessagDefinitions != string.Empty))
            {
                //Adding the Session ID details 
                if (Thread.CurrentPrincipal != null)
                {
                    if (Thread.CurrentPrincipal.Identity != null)
                    {
                        if (Thread.CurrentPrincipal.Identity.Name != null)
                        {
                            traceMessagDefinitions += "#" + Thread.CurrentPrincipal.Identity.Name.ToString() + "#Default";
                        }
                    }
                }
                    

                //split definitions
                ArrayList MessagDefinitions = new ArrayList(traceMessagDefinitions.ToString().ToUpper().Split(new Char[] { '#' }));

                if (MessagDefinitions.Count > 0)
                {
                    //check if it setup message or regular message
                    //case setup message
                    if (MessagDefinitions[0].ToString() == "SETUP_FILE_STREAM")
                    {
                        //creat new file stream to a new sub application
                        AddFileStreamToList(MessagDefinitions[1].ToString());
                    }
                    //case normal message
                    //check if all 3 definitions were sent before start
                    else if (MessagDefinitions.Count >= 3)
                    {
                        //setup definitions
                        _traceLevel = MessagDefinitions[0].ToString();
                        _sessionID = MessagDefinitions[1].ToString();
                        _subAppName = MessagDefinitions[2].ToString();

                        //set the file stream pointer
                        foreach (CustomFileStream CustomFileStreamFromList in _customFileStreamsList)
                        {
                            if (CustomFileStreamFromList.subApplicationName.ToString() == _subAppName)
                            {
                                _pointingFS = CustomFileStreamFromList.fileStream;
                                break;
                            }
                        }

                        //write to default file in case the sub application is unknown 
                        if (_pointingFS == null)
                        {
                            CustomFileStream tempFS = (CustomFileStream)_customFileStreamsList[0]; //Default FS
                            _pointingFS = tempFS.fileStream;
                        }
                    }
                    else
                    {
                        isConfigreAble = false;
                    }
                        
                }
                else
                {
                    isConfigreAble = false;
                }
                    
            }
            else
            {
                isConfigreAble = false;
            }
                

            //Default Setup
            if (!isConfigreAble)
            {
                CustomFileStream tempFS = (CustomFileStream)_customFileStreamsList[0]; //Default FS
                _pointingFS = tempFS.fileStream;
                _traceLevel = "INFO";
                _sessionID = "?/?";
                _subAppName = "";
            }
        }

        private void CheckIfVerifiedToWrite()
        {
            if (_traceActivateSwitch)
            {
                if ((_pointingFS != null) && (_traceLevel != null) && (_sessionID != null))
                {
                    if ((_traceLevel.ToUpper() == "ERROR") && ((_traceLevelSwitch_Error) || (_traceLevelSwitch_Info)))
                    {
                        _isVerifiedToWrite = true;
                    }
                    else if ((_traceLevel.ToUpper() == "INFO") && (_traceLevelSwitch_Info))
                    {
                        _isVerifiedToWrite = true;
                    }
                }
            }
        }

        public override void Write(string message)
        {
            //The encoding is used to convert the string to byte array for the File Stream
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            try
            {
                if (_isVerifiedToWrite == true)
                {
                    //write Trace message to text file
                    lock (this)// lock to make sure only one thread writes each time
                    {
                        _pointingFS.Write(encoding.GetBytes(message), 0, encoding.GetByteCount(message));
                    }

                }
            }
            catch (Exception ex)
            {

            }
        }

        public override void Write(object value)
        {
            this.WriteLine(value, "Info");
        }

        public override void Write(string message, string traceMessagDefinitions)
        {
            this.WriteLine(message, traceMessagDefinitions);
        }

        public override void Write(object value, string traceMessagDefinitions)
        {
            this.WriteLine(value, traceMessagDefinitions);
        }

        public override void WriteLine(string message)
        {
            this.WriteLine(message, "Info");
        }

        public override void WriteLine(object value)
        {
            this.WriteLine(value, "Info");
        }

        public override void WriteLine(string message, string traceMessagDefinitions)
        {
            string _subAppName = string.Empty;
            try
            {
                SetupTraceMessagDefinitions(traceMessagDefinitions);

                CheckIfVerifiedToWrite();

                if (_isVerifiedToWrite == true)
                {
                    if ((message != null) && (message != string.Empty))
                    {
                        this.Write(string.Format("{0} |Session ID: {1} |Level: {2}\r\n{3}\r\n\r\n", DateTime.Now.ToString("dd-MMM-yy HH:mm:ss"), _sessionID, _traceLevel, message));

                        //init definitions
                        _pointingFS = null;
                        _traceLevel = string.Empty;
                        _sessionID = string.Empty;
                        _subAppName = string.Empty;
                        _isVerifiedToWrite = false;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public override void WriteLine(object value, string traceMessagDefinitions)
        {
            try
            {
                string _subAppName = string.Empty;
                SetupTraceMessagDefinitions(traceMessagDefinitions);

                CheckIfVerifiedToWrite();

                if (_isVerifiedToWrite && (value != null))
                {
                    if (value is Exception)
                    {
                        //expected "value" to be an Exception object
                        Exception ex = (Exception)value;

                        //Write Exception object details
                        this.Write(string.Format("{0} |Session ID: {1} |Level: {2}\r\n{3}\r\n", DateTime.Now.ToString("dd-MMM-yy HH:mm:ss"), _sessionID, _traceLevel, "****** EXCEPTION ******"));
                        this.Write(string.Format("{0}: {1}\r\n", "Type", ex.GetType().ToString()));
                        if (ex.Message.Contains("Parameter name:"))
                        {
                            string[] exMessageParts = ex.Message.Split('\n');
                            this.Write(string.Format("{0}: {1}\r\n", "Message", exMessageParts[0]));
                            this.Write(string.Format("{0} \r\n", exMessageParts[1]));
                        }
                        else
                        {
                            this.Write(string.Format("{0}: {1}\r\n", "Message", ex.Message));
                        }
                        this.Write(string.Format("{0}: {1}\r\n", "Source", ex.Source));
                        this.Write(string.Format("{0}: {1}\r\n", "TargetSite", ex.TargetSite));
                        this.Write(string.Format("{0}: {1}r\n", "Stack Trace", ex.StackTrace));
                        this.Write(string.Format("{0}: {1}\r\n", "InnerException", ex.InnerException));
                        this.Write(string.Format("***********************\r\n\r\n"));
                    }
                    else
                    {
                        //Write general object string
                        this.Write(string.Format("{0} |Session ID: {1} |Level: {2}\r\n{3}\r\n\r\n", DateTime.Now.ToString("dd-MMM-yy HH:mm:ss"), _sessionID, _traceLevel, value.ToString()));
                    }

                    //init definitions
                    _pointingFS = null;
                    _traceLevel = string.Empty;
                    _sessionID = string.Empty;
                    _subAppName = string.Empty;
                    _isVerifiedToWrite = false;
                }
            }
            catch (Exception ex)
            {

            }
        }

        struct CustomFileStream
        {
            public FileStreamWithBackup fileStream;
            public string subApplicationName;
            public Boolean isActive;
        }

        sealed class FileStreamWithBackup : FileStream
        {
            private long m_maxFileLength;
            private int m_maxFileCount;
            private string m_fileDir;
            private string m_fileBase;
            private string m_fileExt;
            private int m_fileDecimals;
            private int m_nextFileIndex;

            public long MaxFileLength { get { return m_maxFileLength; } }
            public int MaxFileCount { get { return m_maxFileCount; } }

            public FileStreamWithBackup(string path, long maxFileLength, int maxFileCount, FileMode mode)
                : base(path, BaseFileMode(mode), FileAccess.Write)
            {
                Init(path, maxFileLength, maxFileCount, mode);
            }

            public FileStreamWithBackup(string path, long maxFileLength, int maxFileCount, FileMode mode, FileShare share)
                : base(path, BaseFileMode(mode), FileAccess.Write, share)
            {
                Init(path, maxFileLength, maxFileCount, mode);
            }

            public FileStreamWithBackup(string path, long maxFileLength, int maxFileCount, FileMode mode, FileShare share, int bufferSize)
                : base(path, BaseFileMode(mode), FileAccess.Write, share, bufferSize)
            {
                Init(path, maxFileLength, maxFileCount, mode);
            }

            public FileStreamWithBackup(string path, long maxFileLength, int maxFileCount, FileMode mode, FileShare share, int bufferSize, bool isAsync)
                : base(path, BaseFileMode(mode), FileAccess.Write, share, bufferSize, isAsync)
            {
                Init(path, maxFileLength, maxFileCount, mode);
            }

            public override bool CanRead { get { return false; } }

            public override void Write(byte[] array, int offset, int count)
            {
                int actualCount = System.Math.Min(count, array.GetLength(0));
                if (Position + actualCount <= m_maxFileLength)
                {
                    base.Write(array, offset, count);
                }
                else
                {
                    BackupAndResetStream();
                    Write(array, offset, count);
                }

                base.Flush();
            }

            private void Init(string path, long maxFileLength, int maxFileCount, FileMode mode)
            {
                if (maxFileLength <= 0)
                {
                    throw new ArgumentOutOfRangeException("Invalid maximum file length");
                }
                    
                if (maxFileCount <= 0)
                {
                    throw new ArgumentOutOfRangeException("Invalid maximum file count");
                }
                  

                m_maxFileLength = maxFileLength;
                m_maxFileCount = maxFileCount;

                string fullPath = Path.GetFullPath(path);
                m_fileDir = Path.GetDirectoryName(fullPath);
                m_fileBase = Path.GetFileNameWithoutExtension(fullPath);
                m_fileExt = Path.GetExtension(fullPath);

                m_fileDecimals = 1;
                int decimalBase = 10;
                while (decimalBase < m_maxFileCount)
                {
                    ++m_fileDecimals;
                    decimalBase *= 10;
                }

                switch (mode)
                {
                    case FileMode.Create:
                    case FileMode.CreateNew:
                    case FileMode.Truncate:
                        // Delete old files
                        for (int iFile = 0; iFile < m_maxFileCount; ++iFile)
                        {
                            string file = GetBackupFileName(iFile);
                            if (System.IO.File.Exists(file))
                            {
                                System.IO.File.Delete(file);
                            }
                                
                        }
                        break;

                    default:
                        // Position file pointer to the last backup file
                        for (int iFile = 0; iFile < m_maxFileCount; ++iFile)
                        {
                            if (System.IO.File.Exists(GetBackupFileName(iFile)))
                            {
                                m_nextFileIndex = iFile + 1;
                            }
                                
                        }
                        if (m_nextFileIndex == m_maxFileCount)
                        {
                            m_nextFileIndex = 0;
                        }
                            
                        Seek(0, SeekOrigin.End);
                        break;
                }
            }

            private void BackupAndResetStream()
            {
                Flush();
                System.IO.File.Copy(Name, GetBackupFileName(m_nextFileIndex), true);
                SetLength(0);

                ++m_nextFileIndex;
                if (m_nextFileIndex >= m_maxFileCount)
                {
                    m_nextFileIndex = 0;
                }
                    
            }

            private string GetBackupFileName(int index)
            {
                StringBuilder format = new StringBuilder();
                format.AppendFormat("D{0}", m_fileDecimals);
                StringBuilder sb = new StringBuilder();
                if (m_fileExt.Length > 0)
                {
                    sb.AppendFormat("{0}_Old_{1}{2}", m_fileBase, index.ToString(format.ToString()), m_fileExt);
                }
                else
                {
                    sb.AppendFormat("{0}{1}", m_fileBase, index.ToString(format.ToString()));
                }
                    
                return Path.Combine(m_fileDir, sb.ToString());
            }

            private static FileMode BaseFileMode(FileMode mode)
            {
                return mode == FileMode.Append ? FileMode.OpenOrCreate : mode;
            }

        }

        public class ConfigFileWatcher
        {
            private AutoResetEvent _trigger = new AutoResetEvent(false);
            private FileSystemWatcher _fileWatcher;
            private DateTime _lastEventFired = DateTime.MinValue;

            public ConfigFileWatcher()
            {
                //setup the file to watch on
                string dir = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
                _fileWatcher = new FileSystemWatcher(dir, "Web.config");
                _fileWatcher.Changed += new FileSystemEventHandler(_fileWatcher_Changed);
                _fileWatcher.EnableRaisingEvents = true;
            }

            private void _fileWatcher_Changed(object sender, FileSystemEventArgs e)
            {
                if (e.ChangeType == WatcherChangeTypes.Changed && e.Name == _fileWatcher.Filter && (DateTime.Now - _lastEventFired).TotalSeconds > 1)
                {
                    Trace.WriteLine("Config fIle was changed, File Watcher Event Fired", "Info");
                    try
                    {
                        //Refreshing config file and reading the new values
                        Thread.Sleep(1000);//Thread wait until the App.config file is been released
                        System.Configuration.ConfigurationManager.RefreshSection("appSettings");
                        var updatedAppConfig = System.Configuration.ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);

                        //updateing the switches values
                        if ((CustomTraceListener._instance._traceActivateSwitch == true) &&
                            (ReadBoolValueFromConfig(updatedAppConfig.AppSettings.Settings["TraceActivateSwitch"].Value.ToString()) == false))
                        {
                            Trace.WriteLine("The Traces writing is turning OFF", "Info");
                        }

                        CustomTraceListener._instance._traceActivateSwitch = ReadBoolValueFromConfig(updatedAppConfig.AppSettings.Settings["TraceActivateSwitch"].Value.ToString());
                        CustomTraceListener._instance._traceLevelSwitch_Error = ReadBoolValueFromConfig(updatedAppConfig.AppSettings.Settings["TraceLevelSwitch_Error"].Value.ToString());
                        CustomTraceListener._instance._traceLevelSwitch_Info = ReadBoolValueFromConfig(updatedAppConfig.AppSettings.Settings["TraceLevelSwitch_Info"].Value.ToString());
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex, "Error");
                    }

                    _trigger.Set();
                    _lastEventFired = DateTime.Now;
                    Trace.WriteLine("New Trace configurations were loaded from the config file", "Info");
                }
            }
        }

        public static Boolean ReadBoolValueFromConfig(string value)
        {
            if (value.ToUpper() == "TRUE")
            {
                return (true);
            }
            else
            {
                return (false);
            }
                
        }
    }
}