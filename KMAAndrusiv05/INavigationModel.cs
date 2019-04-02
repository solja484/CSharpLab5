using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMAAndrusiv05
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
