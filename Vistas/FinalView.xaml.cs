using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Woodic.Controlador;
using Woodic.Modelo;
using Woodic.Services;

namespace Woodic.Vistas
{
    public partial class FinalView : UserControl
    {
        private readonly MainWindow? _mainWindow;
        private PresupuestoModel? _presupuesto;
        private bool _promptShown = false;

        public FinalView()
        {
            InitializeComponent();
        }

        public FinalView(MainWindow mainWindow, int idPedido, int cantidadModulos) : this()
        {
            _mainWindow = mainWindow;
            var controller = new FinalViewController();
            _presupuesto = controller.CalcularPresupuesto(idPedido, cantidadModulos);
            CargarDatos();
        }

        public FinalView(MainWindow mainWindow, PresupuestoModel presupuesto) : this()
        {
            _mainWindow = mainWindow;
            _presupuesto = presupuesto;
            CargarDatos();
        }

        private void CargarDatos()
        {
            if (_presupuesto == null) return;

            // Header & badges
            if (_presupuesto.EsPresupuestoRapido)
            {
                badgePresupuestoRapido.Visibility = Visibility.Visible;
                lblDetalleSubtitulo.Text = $"Presupuesto calculado con la placa más económica: {_presupuesto.PlacaSeleccionada?.Linea} {_presupuesto.PlacaSeleccionada?.Color}";
                btnVerCortes.Visibility = Visibility.Collapsed;
            }
            else
            {
                badgePresupuestoRapido.Visibility = Visibility.Collapsed;
                lblDetalleSubtitulo.Text = $"Pedido #{_presupuesto.PedidoId} - {_presupuesto.CantidadModulos} módulo(s)";
            }

            lblCantidadPlacas.Text = _presupuesto.CantidadPlacas.ToString();
            if (_presupuesto.PlacaSeleccionada != null)
            {
                lblInfoPlaca.Text = $"({_presupuesto.PlacaSeleccionada.Linea} {_presupuesto.PlacaSeleccionada.Color} - {_presupuesto.PlacaSeleccionada.Ancho}x{_presupuesto.PlacaSeleccionada.Largo} mm a ${_presupuesto.PrecioPlaca:N2}/u)";
            }

            lblDesgloseCostos.Text = $"Material ({_presupuesto.CantidadPlacas} placas): ${_presupuesto.PrecioMaterial:N2} | Herrajes (30%): ${_presupuesto.PrecioHerrajes:N2}";
            lblPrecioTotal.Text = $"${_presupuesto.PrecioTotal:N2}";

            ConstruirTablaDespiece();
        }

        private void ConstruirTablaDespiece()
        {
            if (_presupuesto == null || _presupuesto.DespieceModulos == null) return;

            string[] nombresComponentes = {
                "Zocalo", "Cabezal", "Laterales", "Div Horizontal", "Div Vertical", "Banq Horizontal", "Banq Vertical",
                "Base Cajon", "Frente Cajon", "Lateral Cajon", "Tapa Cajon", "Puertas"
            };

            int cantModulos = _presupuesto.DespieceModulos.Count;
            var dataTable = new DataTable();

            // Definir columnas
            dataTable.Columns.Add("Cortes", typeof(string));
            for (int i = 1; i <= cantModulos; i++)
            {
                dataTable.Columns.Add($"Modulo {i}", typeof(string));
            }

            // Llenar filas
            foreach (var nombre in nombresComponentes)
            {
                var row = dataTable.NewRow();
                row["Cortes"] = nombre;

                for (int m = 0; m < cantModulos; m++)
                {
                    var dicc = _presupuesto.DespieceModulos[m];
                    if (dicc.TryGetValue(nombre, out var lista) && lista.Count > 0)
                    {
                        var agrupadas = new Dictionary<string, int>();
                        foreach (var d in lista)
                        {
                            int ancho = (int)Math.Round(d[0]);
                            int largo = (int)Math.Round(d[1]);
                            int cant = d.Length > 2 ? (int)Math.Round(d[2]) : 1;
                            string clave = $"{ancho}x{largo}";
                            agrupadas[clave] = agrupadas.GetValueOrDefault(clave, 0) + cant;
                        }

                        var partes = agrupadas.Select(kv => $"{kv.Key} ({kv.Value})");
                        row[$"Modulo {m + 1}"] = string.Join(", ", partes);
                    }
                    else
                    {
                        row[$"Modulo {m + 1}"] = "-";
                    }
                }
                dataTable.Rows.Add(row);
            }

            // Generar columnas de DataGrid
            dgDespiece.Columns.Clear();
            dgDespiece.Columns.Add(new DataGridTextColumn
            {
                Header = "Cortes",
                Binding = new Binding("[Cortes]"),
                FontWeight = FontWeights.SemiBold,
                Width = new DataGridLength(140)
            });

            for (int i = 1; i <= cantModulos; i++)
            {
                string colName = $"Modulo {i}";
                dgDespiece.Columns.Add(new DataGridTextColumn
                {
                    Header = colName,
                    Binding = new Binding($"[{colName}]"),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                });
            }

            dgDespiece.ItemsSource = dataTable.DefaultView;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_promptShown && _presupuesto != null)
            {
                _promptShown = true;
                var res = MessageBox.Show(
                    "El presupuesto ha sido calculado con éxito.\n\n¿Desea imprimir el presupuesto ahora?",
                    "Imprimir Presupuesto",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (res == MessageBoxResult.Yes)
                {
                    btnImprimir_Click(this, new RoutedEventArgs());
                }
            }
        }

        private void btnImprimir_Click(object sender, RoutedEventArgs e)
        {
            if (_presupuesto == null) return;

            Cliente? cliente = null;
            if (_presupuesto.PedidoId > 0)
            {
                var controller = new FinalViewController();
                cliente = controller.ObtenerClienteDePedido(_presupuesto.PedidoId);
            }

            ReportPrintService.ImprimirPresupuesto(
                cliente,
                _presupuesto.PlacaSeleccionada,
                _presupuesto.CantidadModulos,
                _presupuesto.CantidadPlacas,
                _presupuesto.PrecioTotal,
                _presupuesto.DespieceModulos,
                _presupuesto.PedidoId
            );
        }

        private void btnVerCortes_Click(object sender, RoutedEventArgs e)
        {
            if (_mainWindow == null || _presupuesto == null) return;

            if (_presupuesto.PedidoId > 0)
            {
                var cortesView = new CortesView(_mainWindow, _presupuesto.PedidoId);
                _mainWindow.NavegarA(cortesView);
            }
            else
            {
                MessageBox.Show("El presupuesto rápido no tiene número de pedido asociado.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnFinalizar_Click(object sender, RoutedEventArgs e)
        {
            if (_mainWindow != null)
            {
                _mainWindow.NavegarA(new InicioView(_mainWindow));
            }
        }
    }
}
