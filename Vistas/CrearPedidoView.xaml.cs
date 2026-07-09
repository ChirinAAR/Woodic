using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
using MySql.Data.MySqlClient;
using Woodic.Controlador;
using Woodic.Modelo;

namespace Woodic.Vistas
{
    /// <summary>
    /// Lógica de interacción para CrearPedidoView.xaml
    /// </summary>
    public partial class CrearPedidoView : Window
    {
        private Placa placa = new Placa();
        private Pedido pedido = new Pedido();
        private CrearPedidoController controlador;

        // Cadena de conexión para los métodos locales de inicialización
        private readonly string connectionString = "Server=localhost;Database=woodicbase;Uid=root;Pwd=;";

        public CrearPedidoView()
        {
            InitializeComponent();
            controlador = new CrearPedidoController(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarLineasInicial();
            chkMDF.IsChecked = true; // Activa MDF por defecto
            placa.setCompuesto("MDF");
            controlador.CargarLinea();

            if (cmbLinea.Items.Count > 0)
            {
                cmbLinea.SelectedIndex = 0;
                CargarColoresInicial(cmbLinea.SelectedItem.ToString());
            }
        }

        private void btnAceptar_Click(object sender, RoutedEventArgs e)
        {
            int cantidadModulos;
            if (!int.TryParse(txtCantidadModulos.Text, out cantidadModulos) || cantidadModulos <= 0)
            {
                MessageBox.Show("Ingrese un número válido y mayor a cero para la cantidad de módulos.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbLinea.SelectedItem == null || cmbColor.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar una línea y un color.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string linea = cmbLinea.SelectedItem.ToString();
            string color = cmbColor.SelectedItem.ToString();
            int idPlaca = controlador.ObtenerIdPlacaSeleccionada(linea, color);

            if (idPlaca == -1)
            {
                MessageBox.Show("Debe seleccionar una placa válida.");
                return;
            }

            // Poblar modelo Cliente
            Cliente cliente = new Cliente();
            cliente.setNombre(txtNombreCliente.Text);

            if (!int.TryParse(txtContacto.Text, out int contacto))
            {
                MessageBox.Show("Ingrese un número válido para el contacto.");
                return;
            }
            cliente.setContacto(contacto);
            cliente.setDireccion(txtDireccion.Text);

            // Poblar modelo Pedido
            pedido.setCliente(cliente);
            pedido.setCantidadModulos(cantidadModulos);
            pedido.setPrecio(0); // O el valor calculado
            pedido.setPlacaId(idPlaca);

            int idPedido = controlador.Guardar(pedido);
            if (idPedido == -1)
            {
                MessageBox.Show("No se pudo guardar el pedido. Intente nuevamente.");
                return;
            }

            // Aquí instanciarías tu siguiente vista (Asegúrate de migrar DescripcionView también)
            // DescripcionView dialog = new DescripcionView(cantidadModulos, idPedido);
            // dialog.ShowDialog();

            MessageBox.Show("Pedido guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void chkAglomerado_Checked(object sender, RoutedEventArgs e)
        {
            if (chkAglomerado.IsChecked == true)
            {
                placa.setCompuesto("Aglomerado");
                controlador.CargarLinea();
                chkMDF.IsChecked = false; // Desmarcar el otro
            }
            else if (chkMDF.IsChecked == false)
            {
                cmbLinea.Items.Clear();
            }
        }

        private void chkMDF_Checked(object sender, RoutedEventArgs e)
        {
            if (chkMDF.IsChecked == true)
            {
                placa.setCompuesto("MDF");
                controlador.CargarLinea();
                chkAglomerado.IsChecked = false; // Desmarcar el otro
            }
            else if (chkAglomerado.IsChecked == false)
            {
                cmbLinea.Items.Clear();
            }
        }

        private void cmbLinea_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbLinea.SelectedItem != null)
            {
                controlador.CargarColor();
            }
        }

        private void cmbColor_DropDownOpened(object sender, EventArgs e)
        {
            controlador.CargarColor();
        }

        private void CargarLineasInicial()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT DISTINCT LINEA FROM PLACA";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        cmbLinea.Items.Clear();
                        while (reader.Read())
                        {
                            cmbLinea.Items.Add(reader.GetString("LINEA"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar líneas: " + ex.Message);
            }
        }

        private void CargarColoresInicial(string lineaSeleccionada)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT DISTINCT COLOR FROM PLACA WHERE LINEA = @linea";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@linea", lineaSeleccionada);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            cmbColor.Items.Clear();
                            while (reader.Read())
                            {
                                cmbColor.Items.Add(reader.GetString("COLOR"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar colores: " + ex.Message);
            }
        }
    }
}
