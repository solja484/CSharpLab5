using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace KMAAndrusiv05
{
    class ProcessGridViewModel: BaseViewModel
    {
        class UpdateThread
        {
            private object _locker;
            private ObservableCollection<ProcessEntry> _processes;

            public event EventHandler Updated; 

            public void Run()
            {
                Stopwatch st = new Stopwatch();
                while (true)
                {
                    st.Restart();
                    List<ProcessEntry> forRemoval = new List<ProcessEntry>();

                    foreach (ProcessEntry p in _processes)
                    {
                        try
                        {
                            if (p.HasExited)
                                forRemoval.Add(p);
                        }
                        catch (Exception e)
                        {
                        }
                    }
                   
                    HashSet<ProcessEntry> currentProcesses = new HashSet<ProcessEntry>(from p in Process.GetProcesses() select new ProcessEntry(p));
                    currentProcesses.ExceptWith(_processes);

                    lock (_locker)
                    {
                        foreach (ProcessEntry p in forRemoval)
                            _processes.Remove(p);

                        foreach (ProcessEntry p in currentProcesses)
                        {
                            _processes.Add(p);
                        }
                    }

                    foreach (ProcessEntry p in _processes)
                        p.Update();

                    Updated(this, null);

                    st.Stop();

                    Console.WriteLine(st.ElapsedMilliseconds);

                    if (st.ElapsedMilliseconds < 3000)
                        Thread.Sleep(3000 - (int)st.ElapsedMilliseconds);
                }
            }

            public UpdateThread(ObservableCollection<ProcessEntry> processes, object locker)
            {
                _processes = processes;
                _locker = locker;
            }
        }

        private UpdateThread updateThread;
        private ObservableCollection<ProcessEntry> _processes;

        public ObservableCollection<ProcessEntry> Processes
        {
            get
            {
                return _processes;
            }

            set
            {
                _processes = value;
                OnPropertyChanged();
            }
        }

        public ProcessGridViewModel()
        {
            _processes = new ObservableCollection<ProcessEntry>();
            object locker = new object();
            BindingOperations.EnableCollectionSynchronization(_processes, locker);
            updateThread = new UpdateThread(Processes, locker);
            updateThread.Updated += UpdateThread_Updated;

            Thread backgroundThread = new Thread(new ThreadStart(updateThread.Run));
            backgroundThread.IsBackground = true;
            backgroundThread.Start();
        }

        private void UpdateThread_Updated(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background, 
                new Action(() => {
                    OnPropertyChanged("Processes");
                })
            );
        }
    }
}
