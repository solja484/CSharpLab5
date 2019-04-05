using KMAAndrusiv05;
using System;

namespace KMAAndrusiv05.Navigation
{
    internal class InitializationNavigationModel : BaseNavigationModel
    {
        public InitializationNavigationModel(IContentOwner contentOwner) : base(contentOwner)
        {

        }

        protected override void InitializeView(ViewType viewType)
        {
            switch (viewType)
            {
                case ViewType.List:
                    ViewsDictionary.Add(viewType, new ProcessGridControl());
                    break;
                case ViewType.Modules:
                    ViewsDictionary.Add(viewType, new ProcessModulesControl());
                    break;
                case ViewType.Threads:
                    ViewsDictionary.Add(viewType, new ProcessThreadsControl());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(viewType), viewType, null);
            }
        }
    }
}