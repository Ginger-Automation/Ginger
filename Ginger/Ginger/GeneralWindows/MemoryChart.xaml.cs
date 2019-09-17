#region License
/*
Copyright © 2014-2019 European Support Limited

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
using System.ComponentModel;
using System.Management;
using System.Windows.Threading;

namespace Ginger    
{
    /// <summary>
    /// Interaction logic for MemoryChart.xaml
    /// </summary>
    public partial class MemoryChart : INotifyPropertyChanged
    {
        DispatcherTimer dispatcherTimer;

        public MemoryChart()
        {
            InitializeComponent();
            InitMemoryWatch();       
            DataContext = this;
        }

        public void InitMemoryWatch()
        {
            // keep 60 samples worth of memory by default
            const int memorySamples = 60;
            MemoryStats = new RingBuffer <MemorySample> (memorySamples);

            var dateTime = DateTime.Now - TimeSpan.FromSeconds(memorySamples);
            
            // create blank past memory samples
            for (var i = 0; i < memorySamples - 1; i++)
            {
                dateTime = dateTime + TimeSpan.FromSeconds(1);
                MemoryStats.Add(new MemorySample { Timestamp = dateTime });
            }

            //TODO: make it running only when run tab is visible
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimerTick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 5);  // 5secs            
        }

        public void Start()
        {
            dispatcherTimer.Start();
        }

        public void Stop()
        {
            dispatcherTimer.Stop();
        }

        private void DispatcherTimerTick(object sender, EventArgs e)
        {
            LatestMemorySample = MemorySample.Generate();
            _memoryStats.Add(LatestMemorySample);
        }

        private RingBuffer<MemorySample> _memoryStats;
        public RingBuffer<MemorySample> MemoryStats
        {
            get { return _memoryStats; }
            set
            {
                _memoryStats = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("MemoryStats"));
                }
            }
        }
        private MemorySample _latestMemorySample;
        public MemorySample LatestMemorySample
        {
            get { return _latestMemorySample; }
            set
            {
                _latestMemorySample = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("LatestMemorySample"));
                }
            }
        }

        public class MemorySample
        {
            public static MemorySample Generate()
            {
                return new MemorySample
                {
                    ByteCount = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64,
                    Timestamp = DateTime.Now
                };
            }

            public string HumanReadableByteCount
            {
                get
                {
                    long workingSet = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64;
                    var megaBytes = workingSet/(1024*1024);
                    return string.Format("{0}MB", megaBytes);
                }
            }
            public string HumanReadableFreeRam
            {
                get
                {
                    ObjectQuery wql = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher(wql);
                    ManagementObjectCollection results = searcher.Get();

                    string st="?";
                    foreach (ManagementObject result in results)
                    {
                        long freePhizMemory = long.Parse(result["FreePhysicalMemory"].ToString());
                        st = freePhizMemory / (1024) + " MB Free Physical Memory";
                    }
                    return st;
                }
            }
            public long ByteCount { get; set; }
            public DateTime Timestamp { get; set; }
        }
        
        #region INotifyPropertyChanged Interface

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
