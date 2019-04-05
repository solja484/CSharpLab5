using KMAAndrusiv05.Navigation;

namespace KMAAndrusiv05
{
    internal class ProcessThreadsViewModel : BaseViewModel
    {
        public ProcessEntry CurrentProcess => ProcessGridViewModel.SelectedProcess;

        public ProcessThreadsViewModel()
        {
            NavigationManager.Instance.Navigated += Instance_Navigated;
        }

        private void Instance_Navigated(ViewType to)
        {
            if (to == ViewType.Threads)
                OnPropertyChanged($"CurrentProcess");
        }

        private RelayCommand<object> _backCommand;

        public RelayCommand<object> BackCommand =>
            _backCommand ?? (_backCommand = new RelayCommand<object>(
                BackImplementation));

        private static void BackImplementation(object obj)
        {
            NavigationManager.Instance.Navigate(ViewType.List);
        }
    }
}
