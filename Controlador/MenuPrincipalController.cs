using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Woodic.Vistas;

namespace Woodic.Controlador
{
    public class MenuPrincipalController
    {
        private readonly MenuPrincipal vista;

        public MenuPrincipalController(MenuPrincipal vista)
        {
            this.vista = vista;
        }

        /// <summary>
        /// Abre la ventana para crear un nuevo pedido.
        /// </summary>
        public void AbrirCrearPedido()
        {
            CrearPedidoView crearPedido = new CrearPedidoView();
            crearPedido.Show();

            // Si deseas que el menú se oculte al abrir otra ventana, descomenta la siguiente línea:
            // vista.Hide(); 
        }

        /// <summary>
        /// Abre la ventana con la lista de pedidos.
        /// </summary>
        public void AbrirListaPedidos()
        {
            // Asegúrate de tener migrada la clase ListaPedidosView
            // ListaPedidosView lista = new ListaPedidosView();
            // lista.Show();
            MessageBox.Show("Abriendo Lista de Pedidos...", "Navegación", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Abre la ventana de cortes.
        /// </summary>
        public void AbrirCortes()
        {
            // CortesView cortes = new CortesView();
            // cortes.Show();
            MessageBox.Show("Abriendo módulo de Cortes...", "Navegación", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Abre la ventana para añadir una placa a la base de datos.
        /// </summary>
        public void AbrirAnadirPlaca()
        {
            // AnadirPlacaView anadirPlaca = new AnadirPlacaView();
            // anadirPlaca.Show();
            MessageBox.Show("Abriendo Añadir Placa...", "Navegación", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Cierra la aplicación completamente.
        /// </summary>
        public void SalirAplicacion()
        {
            Application.Current.Shutdown();
        }
    }
}
