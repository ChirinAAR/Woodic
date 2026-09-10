using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Woodic.Modelo;

namespace Woodic.Services
{
    public static class ReportPrintService
    {
        public static bool ImprimirPresupuesto(
            Cliente? cliente,
            Placa? placa,
            int cantidadModulos,
            int cantidadPlacas,
            decimal precioTotal,
            List<Dictionary<string, List<double[]>>> despieceModulos,
            int pedidoId = 0)
        {
            try
            {
                var printDialog = new PrintDialog();
                if (printDialog.ShowDialog() != true)
                {
                    return false;
                }

                var doc = CrearDocumentoPresupuesto(cliente, placa, cantidadModulos, cantidadPlacas, precioTotal, despieceModulos, pedidoId);
                doc.PageHeight = printDialog.PrintableAreaHeight;
                doc.PageWidth = printDialog.PrintableAreaWidth;
                doc.PagePadding = new Thickness(40);
                doc.ColumnWidth = printDialog.PrintableAreaWidth;

                var paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                printDialog.PrintDocument(paginator, $"Presupuesto Woodic #{pedidoId}");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}", "Error de Impresión", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static FlowDocument CrearDocumentoPresupuesto(
            Cliente? cliente,
            Placa? placa,
            int cantidadModulos,
            int cantidadPlacas,
            decimal precioTotal,
            List<Dictionary<string, List<double[]>>> despieceModulos,
            int pedidoId)
        {
            var doc = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI, Arial"),
                FontSize = 12,
                Foreground = Brushes.Black,
                Background = Brushes.White
            };

            // Encabezado
            var header = new Paragraph();
            header.Inlines.Add(new Run("WOODIC") { FontSize = 24, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(15, 23, 42)) });
            header.Inlines.Add(new LineBreak());
            header.Inlines.Add(new Run("WOOD INTELLIGENT CUTTER - PRESUPUESTO DE CORTE") { FontSize = 12, Foreground = Brushes.Gray });
            header.Inlines.Add(new LineBreak());
            header.Inlines.Add(new Run($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}   |   Presupuesto #{ (pedidoId > 0 ? pedidoId.ToString() : "RÁPIDO") }") { FontSize = 11, FontStyle = FontStyles.Italic });
            header.BorderBrush = Brushes.LightGray;
            header.BorderThickness = new Thickness(0, 0, 0, 1.5);
            header.Padding = new Thickness(0, 0, 0, 10);
            doc.Blocks.Add(header);

            // Datos del Cliente y Material
            var infoSection = new Paragraph { Margin = new Thickness(0, 15, 0, 10) };
            infoSection.Inlines.Add(new Run("DATOS DEL CLIENTE") { FontWeight = FontWeights.Bold, FontSize = 13 });
            infoSection.Inlines.Add(new LineBreak());
            infoSection.Inlines.Add(new Run($"Cliente: {(string.IsNullOrWhiteSpace(cliente?.Nombre) ? "Consumidor Final / Presupuesto Rápido" : cliente.Nombre)}") );
            infoSection.Inlines.Add(new LineBreak());
            if (!string.IsNullOrWhiteSpace(cliente?.Direccion))
            {
                infoSection.Inlines.Add(new Run($"Dirección: {cliente.Direccion}   |   "));
            }
            if (cliente?.Contacto > 0)
            {
                infoSection.Inlines.Add(new Run($"Contacto: {cliente.Contacto}"));
                infoSection.Inlines.Add(new LineBreak());
            }

            infoSection.Inlines.Add(new LineBreak());
            infoSection.Inlines.Add(new Run("MATERIAL Y MÓDULOS") { FontWeight = FontWeights.Bold, FontSize = 13 });
            infoSection.Inlines.Add(new LineBreak());
            infoSection.Inlines.Add(new Run($"Madera seleccionada: {placa?.Compuesto ?? "Aglomerado"} - {placa?.Linea ?? "Clásica"} {placa?.Color ?? "Blanco"}") );
            infoSection.Inlines.Add(new LineBreak());
            infoSection.Inlines.Add(new Run($"Cantidad de módulos a fabricar: {cantidadModulos}") );
            doc.Blocks.Add(infoSection);

            // Resumen de Despiece
            var despieceTitle = new Paragraph(new Run("DESGLOSE DE PIEZAS") { FontWeight = FontWeights.Bold, FontSize = 13 })
            {
                Margin = new Thickness(0, 10, 0, 5)
            };
            doc.Blocks.Add(despieceTitle);

            string[] nombresComponentes = {
                "Zocalo", "Cabezal", "Laterales", "Div Horizontal", "Div Vertical", "Banq Horizontal", "Banq Vertical",
                "Base Cajon", "Frente Cajon", "Lateral Cajon", "Tapa Cajon", "Puertas"
            };

            var table = new Table { CellSpacing = 0, Margin = new Thickness(0, 0, 0, 15) };
            table.Columns.Add(new TableColumn { Width = new GridLength(140) });
            for (int i = 0; i < cantidadModulos; i++)
            {
                table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            }

            var rowGroup = new TableRowGroup();
            var headerRow = new TableRow { Background = new SolidColorBrush(Color.FromRgb(240, 243, 248)) };
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Componente") { FontWeight = FontWeights.Bold })) { Padding = new Thickness(6) });
            for (int i = 1; i <= cantidadModulos; i++)
            {
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run($"Módulo {i}") { FontWeight = FontWeights.Bold })) { Padding = new Thickness(6) });
            }
            rowGroup.Rows.Add(headerRow);

            bool alt = false;
            foreach (var nombre in nombresComponentes)
            {
                var row = new TableRow { Background = alt ? new SolidColorBrush(Color.FromRgb(250, 250, 250)) : Brushes.White };
                alt = !alt;
                row.Cells.Add(new TableCell(new Paragraph(new Run(nombre))) { Padding = new Thickness(4) });

                for (int m = 0; m < cantidadModulos; m++)
                {
                    string textoPiezas = "-";
                    if (m < despieceModulos.Count && despieceModulos[m].TryGetValue(nombre, out var lista) && lista.Count > 0)
                    {
                        var parts = new List<string>();
                        foreach (var item in lista)
                        {
                            int ancho = (int)Math.Round(item[0]);
                            int largo = (int)Math.Round(item[1]);
                            int cant = item.Length > 2 ? (int)Math.Round(item[2]) : 1;
                            parts.Add($"{ancho}x{largo} ({cant})");
                        }
                        textoPiezas = string.Join(", ", parts);
                    }
                    row.Cells.Add(new TableCell(new Paragraph(new Run(textoPiezas))) { Padding = new Thickness(4) });
                }
                rowGroup.Rows.Add(row);
            }
            table.RowGroups.Add(rowGroup);
            doc.Blocks.Add(table);

            // Totales
            decimal precioPlaca = placa?.Precio > 0 ? Convert.ToDecimal(placa.Precio) : (precioTotal / Math.Max(1, cantidadPlacas) / 1.30m);
            decimal precioMaterial = precioPlaca * cantidadPlacas;
            decimal precioHerrajes = precioMaterial * 0.30m;

            var totals = new Paragraph
            {
                Margin = new Thickness(0, 15, 0, 0),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(0, 1.5, 0, 0),
                Padding = new Thickness(0, 10, 0, 0)
            };
            totals.Inlines.Add(new Run($"Placas de 1830 x 2400 mm necesarias: {cantidadPlacas} unidad(es)") { FontSize = 12 });
            totals.Inlines.Add(new LineBreak());
            totals.Inlines.Add(new Run($"Costo Material Placas: ${precioMaterial:N2}  +  Herrajes (30%): ${precioHerrajes:N2}") { FontSize = 11, Foreground = Brushes.DarkSlateGray });
            totals.Inlines.Add(new LineBreak());
            totals.Inlines.Add(new Run($"PRECIO TOTAL APROXIMADO: ${precioTotal:N2}") { FontSize = 18, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(21, 128, 61)) });
            doc.Blocks.Add(totals);

            return doc;
        }

        public static string ExportarCortesPDF(int pedidoId, decimal precioTotal, List<Dictionary<string, List<double[]>>> despieceModulos)
        {
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pedidos");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string filePath = Path.Combine(folder, $"cortes_pedido_{pedidoId}.pdf");

            var pdfDoc = new PdfDocument();
            pdfDoc.Info.Title = $"Cortes Pedido #{pedidoId}";

            var page = pdfDoc.AddPage();
            page.Size = PdfSharp.PageSize.A4;
            var gfx = XGraphics.FromPdfPage(page);

            var fontTitle = new XFont("Arial", 16, XFontStyleEx.Bold);
            var fontSub = new XFont("Arial", 11, XFontStyleEx.Regular);
            var fontBold = new XFont("Arial", 10, XFontStyleEx.Bold);
            var fontRegular = new XFont("Arial", 9, XFontStyleEx.Regular);

            double y = 40;
            gfx.DrawString($"WOODIC - Cortes por Módulo (Pedido #{pedidoId})", fontTitle, XBrushes.DarkBlue, new XPoint(40, y));
            y += 20;
            gfx.DrawString($"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}", fontSub, XBrushes.Gray, new XPoint(40, y));
            y += 25;

            for (int m = 0; m < despieceModulos.Count; m++)
            {
                gfx.DrawString($"Módulo {m + 1}:", fontBold, XBrushes.Black, new XPoint(40, y));
                y += 16;

                // Tabla de componentes
                gfx.DrawRectangle(XPens.LightGray, XBrushes.AliceBlue, 40, y, 515, 18);
                gfx.DrawString("Componente", fontBold, XBrushes.Black, new XPoint(50, y + 13));
                gfx.DrawString("Medidas (Ancho x Largo)", fontBold, XBrushes.Black, new XPoint(220, y + 13));
                gfx.DrawString("Cantidad", fontBold, XBrushes.Black, new XPoint(450, y + 13));
                y += 20;

                var mod = despieceModulos[m];
                foreach (var entry in mod)
                {
                    if (entry.Value.Count == 0) continue;

                    foreach (var item in entry.Value)
                    {
                        int ancho = (int)Math.Round(item[0]);
                        int largo = (int)Math.Round(item[1]);
                        int cant = item.Length > 2 ? (int)Math.Round(item[2]) : 1;

                        gfx.DrawString(entry.Key, fontRegular, XBrushes.Black, new XPoint(50, y + 12));
                        gfx.DrawString($"{ancho} mm x {largo} mm", fontRegular, XBrushes.Black, new XPoint(220, y + 12));
                        gfx.DrawString(cant.ToString(), fontRegular, XBrushes.Black, new XPoint(460, y + 12));
                        y += 16;

                        if (y > page.Height.Point - 60)
                        {
                            page = pdfDoc.AddPage();
                            gfx = XGraphics.FromPdfPage(page);
                            y = 40;
                        }
                    }
                }

                y += 15;
            }

            gfx.DrawLine(XPens.DarkBlue, 40, y, 555, y);
            y += 20;
            gfx.DrawString($"Precio Total Aproximado: ${precioTotal:N2}", fontTitle, XBrushes.DarkGreen, new XPoint(40, y));

            pdfDoc.Save(filePath);
            return filePath;
        }
    }
}
