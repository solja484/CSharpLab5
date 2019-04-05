
namespace KMAAndrusiv05.Navigation
{
    internal enum ViewType
    {
        List
    }

    internal interface INavigationModel
    {
        void Navigate(ViewType viewType);
    }
}
