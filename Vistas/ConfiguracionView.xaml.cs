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
        private bool _isInitializing = true;

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
            _isInitializing = true;

            // Reflejar estado del tema
            if (_controller != null)
            {
                if (_controller.EsTemaOscuro)
                    rbTemaOscuro.IsChecked = true;
                else
                    rbTemaClaro.IsChecked = true;
            }

            _isInitializing = false;
            CargarPlacas();
        }

        private void CargarPlacas()
        {
            if (_controller != null)
            {
                dgPlacas.ItemsSource = _controller.CargarPlacas();
            }
        }

        private void rbTemaOscuro_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing || _controller == null) return;
            _controller.CambiarTema(true);
        }

        private void rbTemaClaro_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing || _controller == null) return;
            _controller.CambiarTema(false);
        }

        private void btnInicializarDb_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _controller?.InicializarBaseDatos();
                MessageBox.Show("Base de datos local inicializada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CargarPlacas();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al inicializar la base de datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnReiniciarDb_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "ADVERTENCIA: Esta acción eliminará todos los pedidos, clientes y placas personalizadas de la base de datos local y restaurará el catálogo de fábrica.\n\n¿Desea continuar?",
                "Confirmar Reinicio de BD",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    _controller?.ReiniciarBaseDatos();
                    MessageBox.Show("Base de datos reiniciada con éxito a valores de fábrica.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    CargarPlacas();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al reiniciar la base de datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnEliminarPlaca_Click(object sender, RoutedEventArgs e)
        {
            if (dgPlacas.SelectedItem is not Placa placaSeleccionada)
            {
                MessageBox.Show("Seleccione una placa de la tabla para eliminar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"¿Está seguro de eliminar la placa '{placaSeleccionada.Linea} {placaSeleccionada.Color}' (ID: {placaSeleccionada.IdPlaca})?",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (confirm == MessageBoxResult.Yes)
            {
                if (_controller != null && _controller.BorrarPlaca(placaSeleccionada.IdPlaca))
                {
                    MessageBox.Show("Placa eliminada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBox.Show("Placa agregada exitosamente al catálogo.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

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
    }
}
