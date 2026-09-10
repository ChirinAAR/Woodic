using System.Windows.Controls;

namespace Woodic.Vistas
{
    public partial class InicioView : UserControl
    {
        private readonly MainWindow _mainWindow;

        public InicioView(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }
    }
}
