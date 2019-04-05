using KMAAndrusiv05.Navigation;

namespace KMAAndrusiv05
{
    /// <inheritdoc>
    ///     <cref></cref>
    /// </inheritdoc>
    /// <summary>
    /// Interaction logic for ProcessGridControl.xaml
    /// </summary>
    public partial class ProcessGridControl : INavigatable
    {
        public ProcessGridControl()
        {
            InitializeComponent();
            DataContext = new ProcessGridViewModel();
        }
    }
}
