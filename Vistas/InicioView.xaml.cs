using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Woodic.Vistas
{
    public partial class InicioView : UserControl
    {
        private readonly MainWindow? _mainWindow;

        public InicioView(MainWindow? mainWindow = null)
        {
            InitializeComponent();
            _mainWindow = mainWindow;

            if (_mainWindow?.Controller != null)
            {
                ActualizarLogo(_mainWindow.Controller.IsDarkTheme);
            }
        }

        public void ActualizarLogo(bool esOscuro)
        {
            try
            {
                string packUri = esOscuro ? "pack://application:,,,/Vistas/logo.png" : "pack://application:,,,/Vistas/logo2.png";
                imgLogo.Source = new BitmapImage(new Uri(packUri, UriKind.Absolute));
            }
            catch
            {
                // Fallback a URI relativa
                try
                {
                    string relUri = esOscuro ? "/Vistas/logo.png" : "/Vistas/logo2.png";
                    imgLogo.Source = new BitmapImage(new Uri(relUri, UriKind.Relative));
                }
                catch { }
            }
        }
    }
}
