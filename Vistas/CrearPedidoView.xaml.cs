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

        private void btnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            string nombre = txtNombre.Text.Trim();
            string contactoStr = txtContacto.Text.Trim();
            string direccion = txtDireccion.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(contactoStr) || string.IsNullOrWhiteSpace(direccion))
            {
                MessageBox.Show("Por favor complete todos los datos del cliente.", "Campos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!long.TryParse(contactoStr, out long contacto) || contacto <= 0)
            {
                MessageBox.Show("Por favor ingrese un número de contacto válido.", "Dato inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtCantidadModulos.Text.Trim(), out int cantidadModulos) || cantidadModulos <= 0)
            {
                MessageBox.Show("La cantidad de módulos debe ser un número entero mayor a 0.", "Dato inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
    }
}
