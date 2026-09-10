using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Woodic.Controlador;

namespace Woodic.Vistas
{
    public partial class ListaPedidosView : UserControl
    {
        private readonly MainWindow? _mainWindow;
        private readonly ListaPedidosController? _controller;

        public ListaPedidosView()
        {
            InitializeComponent();
        }

        public ListaPedidosView(MainWindow mainWindow) : this()
        {
            _mainWindow = mainWindow;
            _controller = new ListaPedidosController(mainWindow);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CargarLista();
        }

        private void CargarLista()
        {
            if (_controller != null)
            {
                dgPedidos.ItemsSource = _controller.CargarPedidos();
            }
        }

        private PedidoFila? ObtenerPedidoSeleccionado()
        {
            return dgPedidos.SelectedItem as PedidoFila;
        }

        private void btnVerCortes_Click(object sender, RoutedEventArgs e)
        {
            var ped = ObtenerPedidoSeleccionado();
            if (ped == null)
            {
                MessageBox.Show("Por favor, seleccione un pedido de la lista.", "Seleccionar Pedido", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _controller?.VerCortes(ped.IdPedido);
        }

        private void dgPedidos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnVerCortes_Click(sender, e);
        }

        private void btnImprimirPresupuesto_Click(object sender, RoutedEventArgs e)
        {
            var ped = ObtenerPedidoSeleccionado();
            if (ped == null)
            {
                MessageBox.Show("Por favor, seleccione un pedido de la lista.", "Seleccionar Pedido", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _controller?.ImprimirPedido(ped.IdPedido);
        }

        private void btnEliminarPedido_Click(object sender, RoutedEventArgs e)
        {
            var ped = ObtenerPedidoSeleccionado();
            if (ped == null)
            {
                MessageBox.Show("Por favor, seleccione un pedido de la lista.", "Seleccionar Pedido", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"¿Está seguro de que desea eliminar el pedido #{ped.IdPedido} de '{ped.Nombre}'?",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (confirm == MessageBoxResult.Yes)
            {
                if (_controller != null && _controller.EliminarPedido(ped.IdPedido, ped.Contacto))
                {
                    MessageBox.Show("Pedido eliminado con éxito.", "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                    CargarLista();
                }
            }
        }
    }
}
