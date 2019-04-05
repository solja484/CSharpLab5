using KMAAndrusiv05.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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

                    if (st.ElapsedMilliseconds < 2000)
                        Thread.Sleep(2000 - (int)st.ElapsedMilliseconds);
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

        public static ProcessEntry SelectedProcess { get; set; }

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

        private RelayCommand<object> _modulesCommand;

        public RelayCommand<object> ModulesCommand
        {
            get
            {
                return _modulesCommand ?? (_modulesCommand = new RelayCommand<object>(
                           ModulesImplementation, CanExecute));
            }
        }

        private void ModulesImplementation(object obj)
        {
            NavigationManager.Instance.Navigate(ViewType.Modules);
        }

        private RelayCommand<object> _threadsCommand;

        public RelayCommand<object> ThreadsCommand
        {
            get
            {
                return _threadsCommand ?? (_threadsCommand = new RelayCommand<object>(
                           ThreadsImplementation, CanExecute));
            }
        }

        private bool CanExecute(object obj)
        {
            return SelectedProcess != null;
        }

        private void ThreadsImplementation(object obj)
        {
            NavigationManager.Instance.Navigate(ViewType.Threads);
        }

        private RelayCommand<object> _killCommand;

        public RelayCommand<object> KillCommand
        {
            get
            {
                return _killCommand ?? (_killCommand = new RelayCommand<object>(
                           KillImplementation, CanExecute));
            }
        }

        private void KillImplementation(object obj)
        {
            try
            {
                SelectedProcess.Process.Kill();
            }
            catch
            {

            }
        }

        private RelayCommand<object> _folderCommand;

        public RelayCommand<object> FolderCommand
        {
            get
            {
                return _folderCommand ?? (_folderCommand = new RelayCommand<object>(
                           FolderImplementation, CanExecute));
            }
        }

        private void FolderImplementation(object obj)
        {
            try
            {
                if (!SelectedProcess.Filepath.Equals("N/A"))
                    Process.Start(Path.GetDirectoryName(SelectedProcess.Filepath));
            }
            catch
            {

            }
        }
    }
}
