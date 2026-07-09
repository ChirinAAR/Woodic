using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MySql.Data.MySqlClient; // Requiere instalar el paquete NuGet MySql.Data
using Woodic.Modelo; // Asegúrate de tener tu namespace de Modelos
using Woodic.Vistas;

namespace Woodic.Controlador
{
    public class CrearPedidoController
    {
        private readonly CrearPedidoView vista;

        // Cadena de conexión para MySQL en C#
        private readonly string connectionString = "Server=localhost;Database=woodicbase;Uid=root;Pwd=;";

        public CrearPedidoController(CrearPedidoView vista)
        {
            this.vista = vista;
        }

        /// <summary>
        /// Guarda el pedido y el cliente en la base de datos.
        /// </summary>
        public int Guardar(Pedido pedido)
        {
            int idPedido = -1;
            Cliente cliente = pedido.Cliente();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Guardar cliente (si no existe)
                    string sqlCliente = "INSERT IGNORE INTO Cliente (NOMBRE, DIRECCION, CONTACTO) VALUES (@nombre, @direccion, @contacto)";
                    using (MySqlCommand cmdCliente = new MySqlCommand(sqlCliente, conn))
                    {
                        cmdCliente.Parameters.AddWithValue("@nombre", cliente.getNombre());
                        cmdCliente.Parameters.AddWithValue("@direccion", cliente.getDireccion());
                        cmdCliente.Parameters.AddWithValue("@contacto", cliente.getContacto());
                        cmdCliente.ExecuteNonQuery();
                    }

                    // Guardar pedido
                    string sqlPedido = "INSERT INTO Pedido (CANTIDADMODULOS, PRECIO, cliente_CONTACTO, placa_idPLACA) VALUES (@cantidad, @precio, @contacto, @idPlaca)";
                    using (MySqlCommand cmdPedido = new MySqlCommand(sqlPedido, conn))
                    {
                        cmdPedido.Parameters.AddWithValue("@cantidad", pedido.getCantidadModulos());
                        cmdPedido.Parameters.AddWithValue("@precio", pedido.getPrecio());
                        cmdPedido.Parameters.AddWithValue("@contacto", cliente.getContacto());
                        cmdPedido.Parameters.AddWithValue("@idPlaca", pedido.getPlacaId());

                        cmdPedido.ExecuteNonQuery();

                        // Obtener el ID generado
                        idPedido = Convert.ToInt32(cmdPedido.LastInsertedId);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error al guardar pedido o cliente: " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return idPedido;
        }

        public void CargarLinea()
        {
            string compuesto = vista.chkAglomerado.IsChecked == true ? "Aglomerado" : "MDF";
            vista.cmbLinea.Items.Clear();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT DISTINCT LINEA FROM PLACA WHERE COMPUESTO = @compuesto";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@compuesto", compuesto);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                vista.cmbLinea.Items.Add(reader.GetString("LINEA"));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error al cargar líneas: " + e.Message);
            }
        }

        /// <summary>
        /// Carga los colores disponibles según la línea seleccionada en la vista.
        /// </summary>
        public void CargarColor()
        {
            if (vista.cmbLinea.SelectedItem == null) return;
            string lineaSeleccionada = vista.cmbLinea.SelectedItem.ToString();
            vista.cmbColor.Items.Clear();

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
                            while (reader.Read())
                            {
                                vista.cmbColor.Items.Add(reader.GetString("COLOR"));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error al cargar colores: " + e.Message);
            }
        }

        public int ObtenerIdPlacaSeleccionada(string linea, string color)
        {
            int idPlaca = -1;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT idPLACA FROM placa WHERE LINEA = @linea AND COLOR = @color";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@linea", linea);
                        cmd.Parameters.AddWithValue("@color", color);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                idPlaca = reader.GetInt32("idPLACA");
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Manejo de error
            }
            return idPlaca;
        }
    }
}

