using System;
using System.Windows;
using System.Windows.Controls;
using Woodic.Vistas;

namespace Woodic.Controlador
{
    public class MainController
    {
        private readonly MainWindow _mainWindow;
        public bool IsDarkTheme { get; private set; } = true;

        public MainController(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void MostrarInicio()
        {
            _mainWindow.NavegarA(new InicioView(_mainWindow));
            _mainWindow.ActualizarBotonActivo(null);
        }

        public void MostrarCrearPedido()
        {
            _mainWindow.NavegarA(new CrearPedidoView(_mainWindow));
            _mainWindow.ActualizarBotonActivo(_mainWindow.btnCrearPedido);
        }

        public void MostrarPresupuestoRapido()
        {
            var controller = new PresupuestoRapidoController(_mainWindow);
            controller.IniciarPresupuestoRapido();
            _mainWindow.ActualizarBotonActivo(_mainWindow.btnPresupuestoRapido);
        }

        public void MostrarListaPedidos()
        {
            _mainWindow.NavegarA(new ListaPedidosView(_mainWindow));
            _mainWindow.ActualizarBotonActivo(_mainWindow.btnListaPedidos);
        }

        public void MostrarConfiguracion()
        {
            _mainWindow.NavegarA(new ConfiguracionView(_mainWindow));
            _mainWindow.ActualizarBotonActivo(_mainWindow.btnConfiguracion);
        }

        public void ToggleTheme()
        {
            SetTheme(!IsDarkTheme);
        }

        public void SetTheme(bool dark)
        {
            IsDarkTheme = dark;
            string themeUri = dark ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml";

            try
            {
                var dict = new ResourceDictionary
                {
                    Source = new Uri(themeUri, UriKind.Relative)
                };

                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(dict);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cambiar tema: {ex.Message}");
            }
        }

        public void Salir()
        {
            Application.Current.Shutdown();
        }
    }
}
