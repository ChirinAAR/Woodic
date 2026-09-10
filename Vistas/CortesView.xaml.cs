using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Data.SqlClient;
using Woodic.Controlador;
using Woodic.Modelo;
using Woodic.Services;

namespace Woodic.Vistas
{
    public partial class CortesView : UserControl
    {
        private readonly MainWindow? _mainWindow;
        private readonly int _idPedido;

        public CortesView()
        {
            InitializeComponent();
        }

        public CortesView(MainWindow mainWindow, int idPedido) : this()
        {
            _mainWindow = mainWindow;
            _idPedido = idPedido;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CargarCortes();
        }

        private void CargarCortes()
        {
            lblTitulo.Text = $"Listado de Cortes - Pedido #{_idPedido}";

            string[] columnas = {
                "idModulo", "Zocalo", "Cabezal", "Laterales", "Div Horizontal", "Div Vertical",
                "Banq Horizontal", "Banq Vertical", "Base Cajon", "Frente Cajon", "Lateral Cajon",
                "Tapa Cajon", "Puertas"
            };

            var mapCol = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "zocalo", "Zocalo" },
                { "cabezal", "Cabezal" },
                { "laterales", "Laterales" },
                { "div horizontal", "Div Horizontal" },
                { "div vertical", "Div Vertical" },
                { "banq horizontal", "Banq Horizontal" },
                { "banq vertical", "Banq Vertical" },
                { "base cajon", "Base Cajon" },
                { "frente cajon", "Frente Cajon" },
                { "lateral cajon", "Lateral Cajon" },
                { "tapa cajon", "Tapa Cajon" },
                { "puertas", "Puertas" }
            };

            var dataTable = new DataTable();
            foreach (var col in columnas)
            {
                dataTable.Columns.Add(col, typeof(string));
            }

            try
            {
                using var conn = DatabaseHelper.CreateConnection();
                conn.Open();

                var modulosIds = new List<int>();
                string sqlMod = "SELECT idMODULO FROM modulo WHERE pedido_idPEDIDO = @idPed ORDER BY idMODULO ASC";
                using (var cmdM = new SqlCommand(sqlMod, conn))
                {
                    cmdM.Parameters.AddWithValue("@idPed", _idPedido);
                    using var rM = cmdM.ExecuteReader();
                    while (rM.Read())
                    {
                        modulosIds.Add(rM.GetInt32(0));
                    }
                }

                foreach (var idModulo in modulosIds)
                {
                    var row = dataTable.NewRow();
                    row["idModulo"] = $"Módulo {idModulo}";

                    for (int c = 1; c < columnas.Length; c++)
                    {
                        row[columnas[c]] = "-";
                    }

                    string sqlComp = "SELECT NOMBRECOMPONENTE, ANCHOCOMP, LARGOCOMP, CANTIDADCOMP FROM componente WHERE modulo_idMODULO = @idMod";
                    using (var cmdC = new SqlCommand(sqlComp, conn))
                    {
                        cmdC.Parameters.AddWithValue("@idMod", idModulo);
                        using var rC = cmdC.ExecuteReader();

                        var medidasPorCol = new Dictionary<string, Dictionary<string, int>>();

                        while (rC.Read())
                        {
                            string nombre = rC.GetString(0);
                            int ancho = rC.GetInt32(1);
                            int largo = rC.GetInt32(2);
                            int cant = rC.GetInt32(3);
                            string medida = $"{ancho}x{largo}";

                            if (mapCol.TryGetValue(nombre.Trim(), out var targetCol))
                            {
                                if (!medidasPorCol.ContainsKey(targetCol))
                                    medidasPorCol[targetCol] = new Dictionary<string, int>();

                                medidasPorCol[targetCol][medida] = medidasPorCol[targetCol].GetValueOrDefault(medida, 0) + cant;
                            }
                        }

                        foreach (var kv in medidasPorCol)
                        {
                            var partes = kv.Value.Select(m => $"{m.Key} ({m.Value})");
                            row[kv.Key] = string.Join(", ", partes);
                        }
                    }

                    dataTable.Rows.Add(row);
                }

                dgCortes.Columns.Clear();
                dgCortes.Columns.Add(new DataGridTextColumn
                {
                    Header = "Módulo",
                    Binding = new Binding("[idModulo]"),
                    FontWeight = FontWeights.Bold,
                    Width = new DataGridLength(100)
                });

                for (int c = 1; c < columnas.Length; c++)
                {
                    dgCortes.Columns.Add(new DataGridTextColumn
                    {
                        Header = columnas[c],
                        Binding = new Binding($"[{columnas[c]}]"),
                        Width = new DataGridLength(120)
                    });
                }

                dgCortes.ItemsSource = dataTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar cortes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnExportarPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var controller = new FinalViewController();
                var cliente = controller.ObtenerClienteDePedido(_idPedido);
                var presupuesto = controller.CalcularPresupuesto(_idPedido, 1);

                string rutaPdf = ReportPrintService.ExportarCortesPDF(_idPedido, presupuesto.PrecioTotal, presupuesto.DespieceModulos);

                lblEstadoExportacion.Text = $"✅ PDF generado con éxito: {Path.GetFileName(rutaPdf)}";

                var res = MessageBox.Show(
                    $"PDF generado correctamente en:\n{rutaPdf}\n\n¿Desea abrir el archivo ahora?",
                    "PDF Exportado",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information
                );

                if (res == MessageBoxResult.Yes)
                {
                    Process.Start(new ProcessStartInfo(rutaPdf) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnImprimir_Click(object sender, RoutedEventArgs e)
        {
            var controller = new FinalViewController();
            var cliente = controller.ObtenerClienteDePedido(_idPedido);
            var presupuesto = controller.CalcularPresupuesto(_idPedido, 1);

            ReportPrintService.ImprimirPresupuesto(
                cliente,
                presupuesto.PlacaSeleccionada,
                presupuesto.CantidadModulos,
                presupuesto.CantidadPlacas,
                presupuesto.PrecioTotal,
                presupuesto.DespieceModulos,
                _idPedido
            );
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            if (_mainWindow != null)
            {
                _mainWindow.NavegarA(new ListaPedidosView(_mainWindow));
            }
        }
    }
}
