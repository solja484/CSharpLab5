using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace KMAAndrusiv05
{
    enum ProcessState
    {
        Active, NotResponding
    }

    class ProcessEntry : INotifyPropertyChanged
    {
        private static long _totalRam;
        private PerformanceCounter _processCounter;

        private Process _process;

        private ProcessState _state;
        private long _memory;
        private int _threadCount;
        private string _username;
        private string _filePath;
        private double _memoryPercent;
        private DateTime _startDate;
        private float _cpuUsage;

        private bool _system = false;

        public void Update()
        {
            _state = _process.Responding ? ProcessState.Active : ProcessState.NotResponding;
            _memory = _process.PagedMemorySize64 / 1024;

            _memoryPercent = Math.Round((double)_memory / _totalRam * 1000) / 10;

            _threadCount = _process.Threads.Count;

            if (_username == null)
                _username = GetProcessUser(_process);

            if (!_system)
            {
                try
                {

                    if (_filePath == null)
                    {
                        _startDate = _process.StartTime;
                    }

                    if (_filePath == null)
                    {
                        _filePath = _process.MainModule.FileName;
                    }
                }
                catch (Exception e)
                {
                    _filePath = "N/A";
                    _system = true;
                }
            }


            float f1 = _processCounter.NextValue();
            _cpuUsage = f1 / Environment.ProcessorCount;

            OnPropertyChanged("CPULoad");
            OnPropertyChanged("State");
            OnPropertyChanged("MemoryPercent");
            OnPropertyChanged("Memory");
            OnPropertyChanged("ThreadCount");
        }

        public string ProcessName { get => _process.ProcessName; }
        public int Id { get => _process.Id; }
        public ProcessState State { get => _state; }
        public float CPULoad { get => _cpuUsage; }
        public double MemoryPercent { get => _memoryPercent; }
        public long Memory { get => _memory; }
        public int ThreadCount { get => _threadCount; }
        public string Username { get => _username; }
        public string Filepath { get => _filePath; }
        public DateTime LaunchTime { get => _startDate; }

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
                catch (Exception e)
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
            var entry = obj as ProcessEntry;

            if (entry == null)
                return false;

            return entry.Id == this.Id;
        }

        static ProcessEntry()
        {
            string Query = "SELECT MaxCapacity FROM Win32_PhysicalMemoryArray";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(Query);
            foreach (ManagementObject WniPART in searcher.Get())
            {
                _totalRam = Convert.ToUInt32(WniPART.Properties["MaxCapacity"].Value);
            }
        }
        public ProcessEntry(Process process)
        {
            _process = process;
            _processCounter = new PerformanceCounter("Process", "% Processor Time", _process.ProcessName);
        }

        private static string GetProcessUser(Process process)
        {
            IntPtr processHandle = IntPtr.Zero;
            try
            {
                OpenProcessToken(process.Handle, 8, out processHandle);
                WindowsIdentity wi = new WindowsIdentity(processHandle);
                string user = wi.Name;
                return user.Contains(@"\") ? user.Substring(user.IndexOf(@"\") + 1) : user;
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
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);
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
