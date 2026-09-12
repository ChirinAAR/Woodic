using System;
using System.Windows;
using System.Windows.Controls;
using Woodic.Controlador;
using Woodic.Modelo;

namespace Woodic.Vistas
{
    public partial class CrearPedidoView : UserControl
    {
        private readonly MainWindow _mainWindow;
        private readonly CrearPedidoController _controller;
        private bool _isInitializing = true;

        public CrearPedidoView(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            _controller = new CrearPedidoController(mainWindow);

            InitializeComponent();
            _isInitializing = false;

            CargarLineas();
        }

        private void CargarLineas()
        {
            if (_isInitializing || _controller == null || cmbLinea == null || chkAglomerado == null) return;

            string compuesto = chkAglomerado.IsChecked == true ? "Aglomerado" : "MDF";
            var lineas = _controller.CargarLineas(compuesto);

            cmbLinea.Items.Clear();
            foreach (var l in lineas)
            {
                cmbLinea.Items.Add(l);
            }

            if (cmbLinea.Items.Count > 0)
            {
                cmbLinea.SelectedIndex = 0;
            }
            else if (cmbColor != null)
            {
                cmbColor.Items.Clear();
            }
        }

        private void chkAglomerado_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            if (chkMDF != null && chkMDF.IsChecked == true)
            {
                chkMDF.IsChecked = false;
            }
            CargarLineas();
        }

        private void chkAglomerado_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            if (chkMDF != null && chkMDF.IsChecked != true)
            {
                chkMDF.IsChecked = true;
            }
        }

        private void chkMDF_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            if (chkAglomerado != null && chkAglomerado.IsChecked == true)
            {
                chkAglomerado.IsChecked = false;
            }
            CargarLineas();
        }

        private void chkMDF_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            if (chkAglomerado != null && chkAglomerado.IsChecked != true)
            {
                chkAglomerado.IsChecked = true;
            }
        }

        private void cmbLinea_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing || _controller == null || cmbLinea == null || cmbColor == null) return;

            if (cmbLinea.SelectedItem == null)
            {
                cmbColor.Items.Clear();
                return;
            }

            string compuesto = chkAglomerado.IsChecked == true ? "Aglomerado" : "MDF";
            string linea = cmbLinea.SelectedItem.ToString() ?? "";
            var colores = _controller.CargarColores(compuesto, linea);

            cmbColor.Items.Clear();
            foreach (var c in colores)
            {
                cmbColor.Items.Add(c);
            }

            if (cmbColor.Items.Count > 0)
            {
                cmbColor.SelectedIndex = 0;
            }
        }

        private void txtNombre_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lblErrorNombre != null) lblErrorNombre.Visibility = Visibility.Collapsed;
        }

        private void txtContacto_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lblErrorContacto != null) lblErrorContacto.Visibility = Visibility.Collapsed;
        }

        private void txtDireccion_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lblErrorDireccion != null) lblErrorDireccion.Visibility = Visibility.Collapsed;
        }

        private void txtCantidadModulos_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lblErrorCantidadModulos != null) lblErrorCantidadModulos.Visibility = Visibility.Collapsed;
        }

        private void btnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            string nombre = txtNombre.Text.Trim();
            string contactoStr = txtContacto.Text.Trim();
            string direccion = txtDireccion.Text.Trim();
            string cantidadModulosStr = txtCantidadModulos.Text.Trim();

            bool tieneError = false;

            if (string.IsNullOrWhiteSpace(nombre))
            {
                lblErrorNombre.Text = "*Debe llenar este campo";
                lblErrorNombre.Visibility = Visibility.Visible;
                tieneError = true;
            }
            else
            {
                lblErrorNombre.Visibility = Visibility.Collapsed;
            }

            if (string.IsNullOrWhiteSpace(contactoStr))
            {
                lblErrorContacto.Text = "*Debe llenar este campo";
                lblErrorContacto.Visibility = Visibility.Visible;
                tieneError = true;
            }
            else if (!long.TryParse(contactoStr, out long _) || !EsSoloDigitos(contactoStr))
            {
                lblErrorContacto.Text = "*Solamente Numeros";
                lblErrorContacto.Visibility = Visibility.Visible;
                tieneError = true;
            }
            else
            {
                lblErrorContacto.Visibility = Visibility.Collapsed;
            }

            if (string.IsNullOrWhiteSpace(direccion))
            {
                lblErrorDireccion.Text = "*Debe llenar este campo";
                lblErrorDireccion.Visibility = Visibility.Visible;
                tieneError = true;
            }
            else
            {
                lblErrorDireccion.Visibility = Visibility.Collapsed;
            }

            if (!int.TryParse(cantidadModulosStr, out int cantidadModulos) || cantidadModulos <= 0)
            {
                lblErrorCantidadModulos.Text = "*Debe ser mayor a 0";
                lblErrorCantidadModulos.Visibility = Visibility.Visible;
                tieneError = true;
            }
            else
            {
                lblErrorCantidadModulos.Visibility = Visibility.Collapsed;
            }

            if (tieneError)
            {
                return;
            }

            long contacto = long.Parse(contactoStr);

            string compuesto = chkAglomerado.IsChecked == true ? "Aglomerado" : "MDF";
            string linea = cmbLinea.SelectedItem?.ToString() ?? "";
            string color = cmbColor.SelectedItem?.ToString() ?? "";

            int idPlaca = _controller.ObtenerIdPlaca(compuesto, linea, color);
            if (idPlaca <= 0)
            {
                MessageBox.Show("Debe seleccionar una línea y color de madera válidos.", "Material no seleccionado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var cliente = new Cliente(contacto, nombre, direccion);
            int idPedido = _controller.GuardarPedido(cliente, idPlaca, cantidadModulos);

            if (idPedido <= 0)
            {
                MessageBox.Show("No se pudo registrar el pedido en la base de datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Iniciar diseño de módulos
            _controller.IniciarDisenoModulos(idPedido, cantidadModulos);
        }

        private static bool EsSoloDigitos(string texto)
        {
            foreach (char c in texto)
            {
                if (!char.IsDigit(c)) return false;
            }
            return true;
        }
    }
}
