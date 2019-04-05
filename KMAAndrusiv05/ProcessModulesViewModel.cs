using KMAAndrusiv05.Navigation;

namespace KMAAndrusiv05
{
    internal class ProcessModulesViewModel : BaseViewModel
    {
        public ProcessEntry CurrentProcess => ProcessGridViewModel.SelectedProcess;

        public ProcessModulesViewModel()
        {
            NavigationManager.Instance.Navigated += Instance_Navigated;
        }

        private void Instance_Navigated(ViewType to)
        {
            if (to == ViewType.Modules)
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
