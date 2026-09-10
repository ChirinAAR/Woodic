using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Data.SqlClient;
using Woodic.Modelo;
using Woodic.Vistas;

namespace Woodic.Controlador
{
    public class CrearPedidoController
    {
        private readonly MainWindow _mainWindow;

        public CrearPedidoController(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public List<string> CargarLineas(string compuesto)
        {
            var lineas = new List<string>();
            try
            {
                using var conn = DatabaseHelper.CreateConnection();
                conn.Open();
                string sql = "SELECT DISTINCT LINEA FROM placa WHERE COMPUESTO = @compuesto ORDER BY LINEA";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@compuesto", compuesto);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lineas.Add(reader.GetString(0));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar líneas: {ex.Message}", "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return lineas;
        }

        public List<string> CargarColores(string compuesto, string linea)
        {
            var colores = new List<string>();
            if (string.IsNullOrEmpty(linea)) return colores;

            try
            {
                using var conn = DatabaseHelper.CreateConnection();
                conn.Open();
                string sql = "SELECT DISTINCT COLOR FROM placa WHERE COMPUESTO = @compuesto AND LINEA = @linea ORDER BY COLOR";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@compuesto", compuesto);
                cmd.Parameters.AddWithValue("@linea", linea);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    colores.Add(reader.GetString(0));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar colores: {ex.Message}", "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return colores;
        }

        public int ObtenerIdPlaca(string compuesto, string linea, string color)
        {
            int idPlaca = -1;
            try
            {
                using var conn = DatabaseHelper.CreateConnection();
                conn.Open();
                string sql = "SELECT TOP 1 idPLACA FROM placa WHERE COMPUESTO = @compuesto AND LINEA = @linea AND COLOR = @color";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@compuesto", compuesto);
                cmd.Parameters.AddWithValue("@linea", linea);
                cmd.Parameters.AddWithValue("@color", color);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    idPlaca = Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar placa: {ex.Message}");
            }
            return idPlaca;
        }

        public int GuardarPedido(Cliente cliente, int idPlaca, int cantidadModulos)
        {
            int idPedido = -1;
            try
            {
                using var conn = DatabaseHelper.CreateConnection();
                conn.Open();

                // 1. Guardar cliente (si no existe)
                string sqlCliente = @"
                    IF NOT EXISTS (SELECT 1 FROM cliente WHERE CONTACTO = @contacto)
                    BEGIN
                        INSERT INTO cliente (CONTACTO, NOMBRE, DIRECCION) VALUES (@contacto, @nombre, @direccion);
                    END
                    ELSE
                    BEGIN
                        UPDATE cliente SET NOMBRE = @nombre, DIRECCION = @direccion WHERE CONTACTO = @contacto;
                    END";

                using (var cmdCliente = new SqlCommand(sqlCliente, conn))
                {
                    cmdCliente.Parameters.AddWithValue("@contacto", cliente.Contacto);
                    cmdCliente.Parameters.AddWithValue("@nombre", cliente.Nombre ?? "");
                    cmdCliente.Parameters.AddWithValue("@direccion", cliente.Direccion ?? "");
                    cmdCliente.ExecuteNonQuery();
                }

                // 2. Guardar pedido
                string sqlPedido = @"
                    INSERT INTO pedido (CANTIDADMODULOS, PRECIO, cliente_CONTACTO, placa_idPLACA)
                    OUTPUT INSERTED.idPEDIDO
                    VALUES (@cantidad, 0, @contacto, @idPlaca);";

                using (var cmdPedido = new SqlCommand(sqlPedido, conn))
                {
                    cmdPedido.Parameters.AddWithValue("@cantidad", cantidadModulos);
                    cmdPedido.Parameters.AddWithValue("@contacto", cliente.Contacto);
                    cmdPedido.Parameters.AddWithValue("@idPlaca", idPlaca);
                    idPedido = Convert.ToInt32(cmdPedido.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar pedido: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return idPedido;
        }

        public void IniciarDisenoModulos(int idPedido, int cantidadModulos, List<string>? descripciones = null)
        {
            var disenoView = new DisenarModuloView(_mainWindow, idPedido, cantidadModulos, descripciones, esPresupuestoRapido: false);
            _mainWindow.NavegarA(disenoView);
        }
    }
}
