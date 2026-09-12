using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Woodic.Modelo;
using Woodic.Services;

namespace Woodic.Controlador
{
    public class FinalViewController
    {
        public List<Dictionary<string, List<double[]>>> ObtenerListaDespiece(int idPedido)
        {
            var listaDespiece = new List<Dictionary<string, List<double[]>>>();
            string[] nombresComponentes = {
                "Zocalo", "Cabezal", "Laterales", "Div Horizontal", "Div Vertical", "Banq Horizontal", "Banq Vertical",
                "Base Cajon", "Frente Cajon", "Lateral Cajon", "Tapa Cajon", "Puertas"
            };

            try
            {
                using var conn = DatabaseHelper.CreateConnection();
                conn.Open();

                string sqlModulos = "SELECT idMODULO FROM modulo WHERE pedido_idPEDIDO = @idPedido ORDER BY idMODULO ASC";
                var modulosIds = new List<int>();
                using (var cmdMod = new SqlCommand(sqlModulos, conn))
                {
                    cmdMod.Parameters.AddWithValue("@idPedido", idPedido);
                    using var rMod = cmdMod.ExecuteReader();
                    while (rMod.Read())
                    {
                        modulosIds.Add(rMod.GetInt32(0));
                    }
                }

                foreach (int idModulo in modulosIds)
                {
                    var despieceModulo = new Dictionary<string, List<double[]>>();
                    foreach (var nombre in nombresComponentes)
                    {
                        despieceModulo[nombre] = new List<double[]>();
                    }

                    string sqlComp = "SELECT NOMBRECOMPONENTE, ANCHOCOMP, LARGOCOMP, CANTIDADCOMP FROM componente WHERE modulo_idMODULO = @idMod";
                    using (var cmdComp = new SqlCommand(sqlComp, conn))
                    {
                        cmdComp.Parameters.AddWithValue("@idMod", idModulo);
                        using var rComp = cmdComp.ExecuteReader();
                        while (rComp.Read())
                        {
                            string nom = rComp.GetString(0);
                            int ancho = rComp.GetInt32(1);
                            int largo = rComp.GetInt32(2);
                            int cant = rComp.GetInt32(3);

                            if (despieceModulo.ContainsKey(nom))
                            {
                                despieceModulo[nom].Add(new double[] { ancho, largo, cant });
                            }
                        }
                    }

                    listaDespiece.Add(despieceModulo);
                }
            }
            catch (Exception)
            {
                // Manejo de error
            }

            return listaDespiece;
        }

        public List<Componente.Pieza> ObtenerTodasLasPiezas(int idPedido)
        {
            var piezas = new List<Componente.Pieza>();
            var despiece = ObtenerListaDespiece(idPedido);

            foreach (var mod in despiece)
            {
                foreach (var lista in mod.Values)
                {
                    foreach (var d in lista)
                    {
                        int ancho = (int)Math.Round(d[0]);
                        int largo = (int)Math.Round(d[1]);
                        int cant = d.Length > 2 ? (int)Math.Round(d[2]) : 1;
                        for (int i = 0; i < cant; i++)
                        {
                            piezas.Add(new Componente.Pieza(ancho, largo));
                        }
                    }
                }
            }

            return piezas;
        }

        public PresupuestoModel CalcularPresupuesto(int idPedido, int cantidadModulos)
        {
            var despiece = ObtenerListaDespiece(idPedido);
            var piezas = ObtenerTodasLasPiezas(idPedido);

            Placa? placa = null;
            try
            {
                using var conn = DatabaseHelper.CreateConnection();
                conn.Open();

                string sql = @"
                    SELECT p.idPLACA, p.LINEA, p.COMPUESTO, p.COLOR, p.BETA, p.PRECIOPLAC, p.PROVEEDOR, p.ANCHO, p.LARGO
                    FROM placa p
                    INNER JOIN pedido ped ON ped.placa_idPLACA = p.idPLACA
                    WHERE ped.idPEDIDO = @idPedido";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@idPedido", idPedido);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    placa = new Placa
                    {
                        IdPlaca = reader.GetInt32(0),
                        Linea = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        Compuesto = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Color = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        Beta = !reader.IsDBNull(4) && reader.GetBoolean(4),
                        Precio = reader.IsDBNull(5) ? 0 : Convert.ToDouble(reader.GetDecimal(5)),
                        Proveedor = reader.IsDBNull(6) ? "" : reader.GetString(6),
                        Ancho = reader.IsDBNull(7) ? 1830 : reader.GetInt32(7),
                        Largo = reader.IsDBNull(8) ? 2400 : reader.GetInt32(8)
                    };
                }
            }
            catch (Exception)
            {
                // Fallback
            }

            int anchoPlaca = placa?.Ancho > 0 ? placa.Ancho : 1830;
            int altoPlaca = placa?.Largo > 0 ? placa.Largo : 2400;

            int cantidadPlacas = CorteOptimizer.CalcularCantidadPlacas(piezas, anchoPlaca, altoPlaca, permitirRotacion: true);

            decimal precioPlaca = placa?.Precio > 0 ? Convert.ToDecimal(placa.Precio) : 25000.00m;
            decimal precioMaterial = precioPlaca * cantidadPlacas;
            decimal precioHerrajes = precioMaterial * 0.30m;
            decimal precioTotal = precioMaterial + precioHerrajes;

            // Actualizar precio en BD
            if (idPedido > 0)
            {
                try
                {
                    using var conn = DatabaseHelper.CreateConnection();
                    conn.Open();
                    string sqlUpdate = "UPDATE pedido SET PRECIO = @precio WHERE idPEDIDO = @id";
                    using var cmdUpdate = new SqlCommand(sqlUpdate, conn);
                    cmdUpdate.Parameters.AddWithValue("@precio", precioTotal);
                    cmdUpdate.Parameters.AddWithValue("@id", idPedido);
                    cmdUpdate.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // Ignorar
                }
            }

            return new PresupuestoModel
            {
                PedidoId = idPedido,
                PlacaSeleccionada = placa,
                CantidadModulos = despiece.Count > 0 ? despiece.Count : cantidadModulos,
                CantidadPlacas = cantidadPlacas,
                PrecioPlaca = precioPlaca,
                PrecioMaterial = precioMaterial,
                PrecioHerrajes = precioHerrajes,
                PrecioTotal = precioTotal,
                EsPresupuestoRapido = false,
                DespieceModulos = despiece,
                TodasLasPiezas = piezas
            };
        }

        public Cliente? ObtenerClienteDePedido(int idPedido)
        {
            try
            {
                using var conn = DatabaseHelper.CreateConnection();
                conn.Open();

                string sql = @"
                    SELECT c.CONTACTO, c.NOMBRE, c.DIRECCION
                    FROM cliente c
                    INNER JOIN pedido p ON p.cliente_CONTACTO = c.CONTACTO
                    WHERE p.idPEDIDO = @id";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", idPedido);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new Cliente
                    {
                        Contacto = reader.GetInt64(0),
                        Nombre = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        Direccion = reader.IsDBNull(2) ? "" : reader.GetString(2)
                    };
                }
            }
            catch (Exception)
            {
                // Ignorar
            }
            return null;
        }
    }
}
