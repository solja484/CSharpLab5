namespace KMAAndrusiv05.Navigation
{

    internal class NavigationManager
    {
        public event NavigationEventHandler Navigated;

        private static readonly object Locker = new object();
        private static NavigationManager _instance;

        internal static NavigationManager Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                lock (Locker)
                {
                    return _instance ?? (_instance = new NavigationManager());
                }
            }
        }

        private INavigationModel _navigationModel;

        private NavigationManager()
        {

        }

        internal void Initialize(INavigationModel navigationModel)
        {
            _navigationModel = navigationModel;
            _navigationModel.Navigated += _navigationModel_Navigated;
        }

        private void _navigationModel_Navigated(ViewType to)
        {
            Navigated?.Invoke(to);
        }

        internal void Navigate(ViewType viewType)
        {
            _navigationModel.Navigate(viewType);
        }

    }
}
