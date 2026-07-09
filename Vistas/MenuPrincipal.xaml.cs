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
using System.Windows.Shapes;
using Woodic.Controlador;

namespace Woodic.Vistas
{
    /// <summary>
    /// Lógica de interacción para MenuPrincipal.xaml
    /// </summary>
    public partial class MenuPrincipal : Window
    {
        private MenuPrincipalController controlador;

        public MenuPrincipal()
        {
            InitializeComponent();
            // Inicializamos el controlador y le pasamos la instancia de esta vista
            controlador = new MenuPrincipalController(this);
        }

        private void btnNuevoPedido_Click(object sender, RoutedEventArgs e)
        {
            controlador.AbrirCrearPedido();
        }

        private void btnListaPedidos_Click(object sender, RoutedEventArgs e)
        {
            controlador.AbrirListaPedidos();
        }

        private void btnCortes_Click(object sender, RoutedEventArgs e)
        {
            controlador.AbrirCortes();
        }

        private void btnAnadirPlaca_Click(object sender, RoutedEventArgs e)
        {
            controlador.AbrirAnadirPlaca();
        }

        private void btnBaseDatos_Click(object sender, RoutedEventArgs e)
        {
            // Si tienes un panel de base de datos o configuraciones, puedes llamarlo aquí
            MessageBox.Show("Módulo de Base de Datos en desarrollo.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnSalir_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("¿Está seguro de que desea salir de la aplicación?",
                                                      "Confirmar Salida",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                controlador.SalirAplicacion();
            }
        }
    }
}
