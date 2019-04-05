
namespace KMAAndrusiv05.Navigation
{
    internal enum ViewType
    {
        List,
        Modules,
        Threads
    }

    delegate void NavigationEventHandler(ViewType to);

    internal interface INavigationModel
    {
        void Navigate(ViewType viewType);
        event NavigationEventHandler Navigated;
    }
}
