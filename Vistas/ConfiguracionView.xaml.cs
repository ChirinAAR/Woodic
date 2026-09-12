using System;
using System.Windows;
using System.Windows.Controls;
using Woodic.Controlador;
using Woodic.Modelo;

namespace Woodic.Vistas
{
    public partial class ConfiguracionView : UserControl
    {
        private readonly MainWindow? _mainWindow;
        private readonly ConfiguracionController? _controller;

        public ConfiguracionView()
        {
            InitializeComponent();
        }

        public ConfiguracionView(MainWindow mainWindow) : this()
        {
            _mainWindow = mainWindow;
            _controller = new ConfiguracionController(mainWindow.Controller);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CargarPlacas();
        }

        private void CargarPlacas()
        {
            if (_controller != null)
            {
                dgPlacas.ItemsSource = _controller.CargarPlacas();
            }
        }

        private void btnEliminarPlaca_Click(object sender, RoutedEventArgs e)
        {
            if (dgPlacas.SelectedItem is not Placa placaSeleccionada)
            {
                MessageBox.Show("Seleccione una placa de la tabla para eliminar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_controller == null) return;

            int pedidosAsociados = _controller.ContarPedidosAsociados(placaSeleccionada.IdPlaca);

            if (pedidosAsociados > 0)
            {
                var result = MessageBox.Show(
                    $"La placa '{placaSeleccionada.Linea} {placaSeleccionada.Color}' no se puede eliminar directamente porque está en uso en {pedidosAsociados} pedido(s) registrado(s).\n\n" +
                    "¿Desea eliminar la placa junto con todos los pedidos asociados (incluyendo sus módulos y piezas de corte)?\n\n" +
                    "• Presione 'Sí' para eliminar la placa y sus {pedidosAsociados} pedido(s) vinculados.\n" +
                    "• Presione 'No' para cancelar y conservar los datos.",
                    "Placa en Uso por Pedidos",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    if (_controller.BorrarPlacaYPedidos(placaSeleccionada.IdPlaca))
                    {
                        lblEstadoPlaca.Text = $"🗑️ Placa y {pedidosAsociados} pedido(s) asociados eliminados correctamente.";
                        CargarPlacas();
                    }
                    else
                    {
                        MessageBox.Show("No se pudo eliminar la placa y sus pedidos asociados.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                return;
            }

            var confirm = MessageBox.Show(
                $"¿Está seguro de eliminar la placa '{placaSeleccionada.Linea} {placaSeleccionada.Color}' (ID: {placaSeleccionada.IdPlaca}) del catálogo?",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (confirm == MessageBoxResult.Yes)
            {
                if (_controller.BorrarPlaca(placaSeleccionada.IdPlaca))
                {
                    lblEstadoPlaca.Text = $"🗑️ Placa '{placaSeleccionada.Linea} {placaSeleccionada.Color}' eliminada.";
                    CargarPlacas();
                }
                else
                {
                    MessageBox.Show("No se pudo eliminar la placa.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnGuardarPlaca_Click(object sender, RoutedEventArgs e)
        {
            string linea = txtLinea.Text.Trim();
            string color = txtColor.Text.Trim();
            string proveedor = txtProveedor.Text.Trim();
            string precioStr = txtPrecio.Text.Trim().Replace("$", "").Replace(",", ".");

            if (string.IsNullOrWhiteSpace(linea) || string.IsNullOrWhiteSpace(color) ||
                string.IsNullOrWhiteSpace(proveedor) || string.IsNullOrWhiteSpace(precioStr))
            {
                MessageBox.Show("Por favor complete todos los campos de la placa.", "Campos Requeridos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(precioStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double precio) || precio <= 0)
            {
                MessageBox.Show("Ingrese un valor numérico válido para el precio.", "Precio Inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int.TryParse(txtAnchoPlaca.Text.Trim(), out int ancho);
            int.TryParse(txtLargoPlaca.Text.Trim(), out int largo);
            if (ancho <= 0) ancho = 1830;
            if (largo <= 0) largo = 2400;

            var nuevaPlaca = new Placa
            {
                Linea = linea,
                Color = color,
                Proveedor = proveedor,
                Precio = precio,
                Compuesto = rbAglomerado.IsChecked == true ? "Aglomerado" : "MDF",
                Beta = rbConVeta.IsChecked == true,
                Ancho = ancho,
                Largo = largo
            };

            if (_controller != null && _controller.AnadirPlaca(nuevaPlaca))
            {
                lblEstadoPlaca.Text = $"✅ Placa '{nuevaPlaca.Linea} {nuevaPlaca.Color}' agregada exitosamente.";

                txtLinea.Clear();
                txtColor.Clear();
                txtProveedor.Clear();
                txtPrecio.Clear();
                txtAnchoPlaca.Text = "1830";
                txtLargoPlaca.Text = "2400";
                rbAglomerado.IsChecked = true;
                rbConVeta.IsChecked = true;

                CargarPlacas();
            }
            else
            {
                MessageBox.Show("Ocurrió un error al guardar la placa en la base de datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void rbConVeta_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
