using KMAAndrusiv05.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KMAAndrusiv05
{
    /// <summary>
    /// Interaction logic for ProcessModulesControl.xaml
    /// </summary>
    public partial class ProcessModulesControl : UserControl, INavigatable
    {
        public ProcessModulesControl()
        {
            InitializeComponent();
            DataContext = new ProcessModulesViewModel();
        }
    }
}
