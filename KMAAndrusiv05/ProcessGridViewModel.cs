using KMAAndrusiv05.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace KMAAndrusiv05
{
    internal class ProcessGridViewModel: BaseViewModel
    {
        class UpdateThread
        {
            private readonly object _locker;
            private readonly ObservableCollection<ProcessEntry> _processes;

            public event EventHandler Updated; 

            public void Run()
            {
                Stopwatch st = new Stopwatch();
                while (true)
                {
                    st.Restart();
                    var forRemoval = new List<ProcessEntry>();

                    foreach (var p in _processes)
                    {
                        try
                        {
                            if (p.HasExited)
                                forRemoval.Add(p);
                        }
                        catch (Exception )
                        {
                            // ignored
                        }
                    }
                   
                    HashSet<ProcessEntry> currentProcesses = new HashSet<ProcessEntry>(from p in Process.GetProcesses() select new ProcessEntry(p));
                    currentProcesses.ExceptWith(_processes);

                    lock (_locker)
                    {
                        foreach (var p in forRemoval)
                            _processes.Remove(p);

                        foreach (var p in currentProcesses)
                        {
                            _processes.Add(p);
                        }
                    }

                    foreach (var p in _processes)
                        p.Update();

                    Updated?.Invoke(this, null);

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

        private ObservableCollection<ProcessEntry> _processes;

        public ObservableCollection<ProcessEntry> Processes
        {
            get => _processes;

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
            var locker = new object();
            BindingOperations.EnableCollectionSynchronization(_processes, locker);
            var updateThread = new UpdateThread(Processes, locker);
            updateThread.Updated += UpdateThread_Updated;

            var backgroundThread = new Thread(new ThreadStart(updateThread.Run));
            backgroundThread.IsBackground = true;
            backgroundThread.Start();
        }

        private void UpdateThread_Updated(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background, 
                new Action(() => {
                    OnPropertyChanged($"Processes");
                })
            );
        }

        private RelayCommand<object> _modulesCommand;

        public RelayCommand<object> ModulesCommand =>
            _modulesCommand ?? (_modulesCommand = new RelayCommand<object>(
                ModulesImplementation, CanExecute));

        private static void ModulesImplementation(object obj)
        {
            NavigationManager.Instance.Navigate(ViewType.Modules);
        }

        private RelayCommand<object> _threadsCommand;

        public RelayCommand<object> ThreadsCommand =>
            _threadsCommand ?? (_threadsCommand = new RelayCommand<object>(
                ThreadsImplementation, CanExecute));

        private static bool CanExecute(object obj)
        {
            return SelectedProcess != null;
        }

        private static void ThreadsImplementation(object obj)
        {
            NavigationManager.Instance.Navigate(ViewType.Threads);
        }

        private RelayCommand<object> _killCommand;

        public RelayCommand<object> KillCommand =>
            _killCommand ?? (_killCommand = new RelayCommand<object>(
                KillImplementation, CanExecute));

        private static void KillImplementation(object obj)
        {
            try
            {
                SelectedProcess.Process.Kill();
            }
            catch
            {
                // ignored
            }
        }

        private RelayCommand<object> _folderCommand;

        public RelayCommand<object> FolderCommand =>
            _folderCommand ?? (_folderCommand = new RelayCommand<object>(
                FolderImplementation, CanExecute));

        private static void FolderImplementation(object obj)
        {
            try
            {
                if (!SelectedProcess.Filepath.Equals("N/A"))
                    Process.Start(Path.GetDirectoryName(SelectedProcess.Filepath) ?? throw new InvalidOperationException());
            }
            catch
            {
                // ignored
            }
        }
    }
}
