using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace KMAAndrusiv05
{
    internal enum ProcessState
    {
        Active, NotResponding
    }

    internal class ProcessEntry : INotifyPropertyChanged
    {
        private static readonly long TotalRam;
        private readonly PerformanceCounter _processCounter;

        private readonly Process _process;

        private ProcessState _state;
        private long _memory;
        private int _threadCount;
        private string _username;
        private string _filePath;
        private double _memoryPercent;
        private DateTime _startDate;
        private float _cpuUsage;

        private bool _system=false;

        public void Update()
        {
            _state = _process.Responding ? ProcessState.Active : ProcessState.NotResponding;
            _memory = _process.PagedMemorySize64 / 1024;

            _memoryPercent = Math.Round((double)_memory / TotalRam * 1000) / 10;

            _threadCount = _process.Threads.Count;

            if (_username == null)
                _username = GetProcessUser(_process);

            if (!_system)
            {
                try
                {
                    if (_filePath == null)
                    {
                        _filePath = _process.MainModule.FileName;
                    }
                }
                catch (Exception)
                {
                    _filePath = "N/A";
                }

                try
                {
                    _startDate = _process.StartTime;
                }
                catch (Exception)
                {
                    _system = false;
                }
            }

            try
            {
                float f1 = _processCounter.NextValue();
                _cpuUsage = f1 / Environment.ProcessorCount;
            }
            catch (Exception)
            {
                // ignored
            }

            OnPropertyChanged($"CPULoad");
            OnPropertyChanged($"State");
            OnPropertyChanged($"MemoryPercent");
            OnPropertyChanged($"Memory");
            OnPropertyChanged($"ThreadCount");
        }

        public string ProcessName => _process.ProcessName;
        public int Id => _process.Id;
        public ProcessState State => _state;
        public float CPULoad => _cpuUsage;
        public double MemoryPercent => _memoryPercent;
        public long Memory => _memory;
        public int ThreadCount => _threadCount;
        public string Username => _username;
        public string Filepath => _filePath;
        public DateTime LaunchTime => _startDate;

        public Process Process => _process;

        public bool HasExited
        {
            get
            {
                if (_system)
                    return false;
                try
                {
                    return _process.HasExited;
                }
                catch (Exception)
                {
                    _system = true;
                    return false;
                }
            }
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ProcessEntry entry))
                return false;

            return entry.Id == Id;
        }

        static ProcessEntry()
        {
            const string query = "SELECT MaxCapacity FROM Win32_PhysicalMemoryArray";
            var searcher = new ManagementObjectSearcher(query);
            foreach (ManagementObject WniPART in searcher.Get())
            {
                TotalRam = Convert.ToUInt32(WniPART.Properties["MaxCapacity"].Value);
            }
        }
        public ProcessEntry(Process process)
        {
            _process = process;
            _processCounter = new PerformanceCounter("Process", "% Processor Time", _process.ProcessName);
        }

        private static string GetProcessUser(Process process)
        {
            var processHandle = IntPtr.Zero;
            try
            {
                OpenProcessToken(process.Handle, 8, out processHandle);
                var wi = new WindowsIdentity(processHandle);
                var user = wi.Name;
                return user.Contains(@"\") ? user.Substring(user.IndexOf(@"\", StringComparison.Ordinal) + 1) : user;
            }
            catch
            {
                return "N/A";
            }
            finally
            {
                if (processHandle != IntPtr.Zero)
                {
                    CloseHandle(processHandle);
                }
            }
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;


        internal virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
