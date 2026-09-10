using System.Windows;
using System.Windows.Controls;
using Woodic.Controlador;

namespace Woodic.Vistas
{
    public partial class MainWindow : Window
    {
        public MainController Controller { get; }

        public MainWindow()
        {
            InitializeComponent();
            Controller = new MainController(this);

            // Iniciar con tema oscuro y pantalla de bienvenida
            Controller.SetTheme(true);
            Controller.MostrarInicio();
        }

        public void NavegarA(UserControl vista)
        {
            MainContent.Content = vista;
        }

        public void ActualizarBotonActivo(Button? activo)
        {
            var navButtons = new[] { btnCrearPedido, btnPresupuestoRapido, btnListaPedidos, btnConfiguracion };
            var activeStyle = (Style)FindResource("ActiveNavButtonStyle");
            var defaultStyle = (Style)FindResource("NavButtonStyle");

            foreach (var btn in navButtons)
            {
                btn.Style = (btn == activo) ? activeStyle : defaultStyle;
            }
        }

        private void btnCrearPedido_Click(object sender, RoutedEventArgs e)
        {
            Controller.MostrarCrearPedido();
        }

        private void btnPresupuestoRapido_Click(object sender, RoutedEventArgs e)
        {
            Controller.MostrarPresupuestoRapido();
        }

        private void btnListaPedidos_Click(object sender, RoutedEventArgs e)
        {
            Controller.MostrarListaPedidos();
        }

        private void btnConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            Controller.MostrarConfiguracion();
        }

        private void btnCambiarTema_Click(object sender, RoutedEventArgs e)
        {
            Controller.ToggleTheme();
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Controller.Salir();
        }
    }
}
