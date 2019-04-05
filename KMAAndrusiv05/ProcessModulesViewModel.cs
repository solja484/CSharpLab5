using KMAAndrusiv05.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMAAndrusiv05
{
    class ProcessModulesViewModel : BaseViewModel
    {
        public ProcessEntry CurrentProcess
        {
            get
            {
                return ProcessGridViewModel.SelectedProcess;
            }
        }
        
        public ProcessModulesViewModel()
        {
            NavigationManager.Instance.Navigated += Instance_Navigated;
        }

        private void Instance_Navigated(ViewType to)
        {
            if (to == ViewType.Modules)
                OnPropertyChanged("CurrentProcess");
        }

        private RelayCommand<object> _backCommand;

        public RelayCommand<object> BackCommand
        {
            get
            {
                return _backCommand ?? (_backCommand = new RelayCommand<object>(
                           BackImplementation));
            }
        }

        private void BackImplementation(object obj)
        {
            NavigationManager.Instance.Navigate(ViewType.List);
        }
    }
}
