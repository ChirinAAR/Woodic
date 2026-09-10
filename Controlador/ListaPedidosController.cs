using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Data.SqlClient;
using Woodic.Modelo;
using Woodic.Services;
using Woodic.Vistas;

namespace Woodic.Controlador
{
    public class PedidoFila
    {
        public string Nombre { get; set; } = string.Empty;
        public long Contacto { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public int IdPedido { get; set; }
        public int CantidadModulos { get; set; }
        public decimal Precio { get; set; }
        public string PrecioFormateado => $"${Precio:N2}";
        public DateTime Fecha { get; set; }
    }

    public class ListaPedidosController
    {
        private readonly MainWindow _mainWindow;

        public ListaPedidosController(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public List<PedidoFila> CargarPedidos()
        {
            var pedidos = new List<PedidoFila>();
            try
            {
                using var conn = DatabaseHelper.CreateConnection();
                conn.Open();

                string sql = @"
                    SELECT c.NOMBRE, c.CONTACTO, c.DIRECCION, p.idPEDIDO, p.CANTIDADMODULOS, p.PRECIO, p.FECHA
                    FROM pedido p
                    JOIN cliente c ON p.cliente_CONTACTO = c.CONTACTO
                    ORDER BY p.idPEDIDO DESC";

                using var cmd = new SqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    pedidos.Add(new PedidoFila
                    {
                        Nombre = reader.IsDBNull(0) ? "" : reader.GetString(0),
                        Contacto = reader.GetInt64(1),
                        Direccion = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        IdPedido = reader.GetInt32(3),
                        CantidadModulos = reader.GetInt32(4),
                        Precio = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
                        Fecha = reader.IsDBNull(6) ? DateTime.Now : reader.GetDateTime(6)
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar pedidos: {ex.Message}");
            }
            return pedidos;
        }

        public bool EliminarPedido(int idPedido, long contacto)
        {
            try
            {
                using var conn = DatabaseHelper.CreateConnection();
                conn.Open();

                // Eliminar pedido (cascada eliminará modulos y componentes)
                string sqlPedido = "DELETE FROM pedido WHERE idPEDIDO = @idPedido";
                using (var cmdP = new SqlCommand(sqlPedido, conn))
                {
                    cmdP.Parameters.AddWithValue("@idPedido", idPedido);
                    cmdP.ExecuteNonQuery();
                }

                // Si el cliente no tiene más pedidos, opcionalmente eliminar cliente
                string sqlCliente = @"
                    IF NOT EXISTS (SELECT 1 FROM pedido WHERE cliente_CONTACTO = @contacto)
                    BEGIN
                        DELETE FROM cliente WHERE CONTACTO = @contacto;
                    END";
                using (var cmdC = new SqlCommand(sqlCliente, conn))
                {
                    cmdC.Parameters.AddWithValue("@contacto", contacto);
                    cmdC.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar pedido: {ex.Message}");
                return false;
            }
        }

        public void VerCortes(int idPedido)
        {
            var cortesView = new CortesView(_mainWindow, idPedido);
            _mainWindow.NavegarA(cortesView);
        }

        public void ImprimirPedido(int idPedido)
        {
            var finalController = new FinalViewController();
            var cliente = finalController.ObtenerClienteDePedido(idPedido);
            var presupuesto = finalController.CalcularPresupuesto(idPedido, 1);

            ReportPrintService.ImprimirPresupuesto(
                cliente,
                presupuesto.PlacaSeleccionada,
                presupuesto.CantidadModulos,
                presupuesto.CantidadPlacas,
                presupuesto.PrecioTotal,
                presupuesto.DespieceModulos,
                idPedido
            );
        }
    }
}
