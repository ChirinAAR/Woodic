using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Printing;
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

                if (despieceModulos.Count > 0 && despieceModulos.Count != cantidadModulos)
                {
                    cantidadModulos = despieceModulos.Count;
                }

                double pageWidth = printDialog.PrintableAreaWidth > 0 ? printDialog.PrintableAreaWidth : 793.7;
                double pageHeight = printDialog.PrintableAreaHeight > 0 ? printDialog.PrintableAreaHeight : 1122.5;

                // Si hay 5 o más módulos, cambiar orientación a horizontal (Landscape)
                if (cantidadModulos >= 5)
                {
                    try
                    {
                        if (printDialog.PrintTicket != null)
                        {
                            printDialog.PrintTicket.PageOrientation = PageOrientation.Landscape;
                        }
                    }
                    catch { }

                    if (pageWidth < pageHeight)
                    {
                        (pageWidth, pageHeight) = (pageHeight, pageWidth);
                    }
                }

                var doc = CrearDocumentoPresupuesto(cliente, placa, cantidadModulos, cantidadPlacas, precioTotal, despieceModulos, pedidoId, pageWidth);
                doc.PageHeight = pageHeight;
                doc.PageWidth = pageWidth;
                doc.PagePadding = new Thickness(40);
                doc.ColumnWidth = pageWidth;

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
            int pedidoId,
            double pageWidth = 793.7)
        {
            if (despieceModulos.Count > 0 && despieceModulos.Count != cantidadModulos)
            {
                cantidadModulos = despieceModulos.Count;
            }

            var doc = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI, Arial"),
                FontSize = 11,
                Foreground = Brushes.Black,
                Background = Brushes.White
            };

            // Encabezado
            var header = new Paragraph();
            header.Inlines.Add(new Run("WOODIC") { FontSize = 24, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(15, 23, 42)) });
            header.Inlines.Add(new LineBreak());
            header.Inlines.Add(new Run("PRESUPUESTO DE CORTE") { FontSize = 15, FontWeight = FontWeights.SemiBold, Foreground = new SolidColorBrush(Color.FromRgb(71, 85, 105)) });
            header.Inlines.Add(new LineBreak());
            header.Inlines.Add(new Run($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}   |   Presupuesto #{(pedidoId > 0 ? pedidoId.ToString() : "RÁPIDO")}") { FontSize = 10, FontStyle = FontStyles.Italic, Foreground = Brushes.Gray });
            header.BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240));
            header.BorderThickness = new Thickness(0, 0, 0, 1.5);
            header.Padding = new Thickness(0, 0, 0, 8);
            doc.Blocks.Add(header);

            // Datos del Cliente y Material
            var infoSection = new Paragraph { Margin = new Thickness(0, 12, 0, 10), LineHeight = 18 };
            infoSection.Inlines.Add(new Run("DATOS DEL CLIENTE") { FontWeight = FontWeights.Bold, FontSize = 12, Foreground = new SolidColorBrush(Color.FromRgb(30, 58, 138)) });
            infoSection.Inlines.Add(new LineBreak());
            infoSection.Inlines.Add(new Run($"Cliente: {(string.IsNullOrWhiteSpace(cliente?.Nombre) ? "Consumidor Final / Presupuesto Rápido" : cliente.Nombre)}"));
            if (!string.IsNullOrWhiteSpace(cliente?.Direccion))
            {
                infoSection.Inlines.Add(new Run($"   |   Dirección: {cliente.Direccion}"));
            }
            if (cliente?.Contacto > 0)
            {
                infoSection.Inlines.Add(new Run($"   |   Contacto: {cliente.Contacto}"));
            }
            infoSection.Inlines.Add(new LineBreak());
            infoSection.Inlines.Add(new LineBreak());

            infoSection.Inlines.Add(new Run("MATERIAL Y MÓDULOS") { FontWeight = FontWeights.Bold, FontSize = 12, Foreground = new SolidColorBrush(Color.FromRgb(30, 58, 138)) });
            infoSection.Inlines.Add(new LineBreak());
            infoSection.Inlines.Add(new Run($"Madera seleccionada: {placa?.Compuesto ?? "Aglomerado"} - {placa?.Linea ?? "Clásica"} {placa?.Color ?? "Blanco"} (Placa estándar {placa?.Ancho ?? 1830} x {placa?.Largo ?? 2400} mm)"));
            infoSection.Inlines.Add(new LineBreak());
            infoSection.Inlines.Add(new Run($"Cantidad de módulos a fabricar: {cantidadModulos}"));
            doc.Blocks.Add(infoSection);

            // Resumen de Despiece
            var despieceTitle = new Paragraph(new Run("DESGLOSE DE PIEZAS") { FontWeight = FontWeights.Bold, FontSize = 12, Foreground = new SolidColorBrush(Color.FromRgb(30, 58, 138)) })
            {
                Margin = new Thickness(0, 8, 0, 6)
            };
            doc.Blocks.Add(despieceTitle);

            string[] nombresComponentes = {
                "Zocalo", "Cabezal", "Laterales", "Div Horizontal", "Div Vertical", "Banq Horizontal", "Banq Vertical",
                "Base Cajon", "Frente Cajon", "Lateral Cajon", "Tapa Cajon", "Puertas"
            };

            // Anchos explícitos en píxeles para evitar que WPF comprima o desborde las columnas
            double printableWidth = Math.Max(400, pageWidth - 80); // 40px padding en cada lateral
            double colComponenteWidth = cantidadModulos >= 4 ? 130.0 : 150.0;
            double colModuloWidth = Math.Max(90.0, (printableWidth - colComponenteWidth) / Math.Max(1, cantidadModulos));

            var table = new Table { CellSpacing = 0, Margin = new Thickness(0, 0, 0, 15) };
            table.Columns.Add(new TableColumn { Width = new GridLength(colComponenteWidth) });
            for (int i = 0; i < cantidadModulos; i++)
            {
                table.Columns.Add(new TableColumn { Width = new GridLength(colModuloWidth) });
            }

            var rowGroup = new TableRowGroup();
            var headerRow = new TableRow { Background = new SolidColorBrush(Color.FromRgb(241, 245, 249)) };
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Componente") { FontWeight = FontWeights.Bold, FontSize = 11 }))
            {
                Padding = new Thickness(8, 6, 8, 6),
                BorderBrush = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
                BorderThickness = new Thickness(0, 0, 0, 2)
            });
            for (int i = 1; i <= cantidadModulos; i++)
            {
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run($"Módulo {i}") { FontWeight = FontWeights.Bold, FontSize = 11 }))
                {
                    Padding = new Thickness(8, 6, 8, 6),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
                    BorderThickness = new Thickness(0, 0, 0, 2)
                });
            }
            rowGroup.Rows.Add(headerRow);

            bool alt = false;
            foreach (var nombre in nombresComponentes)
            {
                var row = new TableRow { Background = alt ? new SolidColorBrush(Color.FromRgb(248, 250, 252)) : Brushes.White };
                alt = !alt;
                row.Cells.Add(new TableCell(new Paragraph(new Run(nombre) { FontWeight = FontWeights.SemiBold }))
                {
                    Padding = new Thickness(8, 5, 8, 5),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                    BorderThickness = new Thickness(0, 0, 0, 1)
                });

                for (int m = 0; m < cantidadModulos; m++)
                {
                    string textoPiezas = "-";
                    if (m < despieceModulos.Count && despieceModulos[m].TryGetValue(nombre, out var lista) && lista.Count > 0)
                    {
                        var agrupadas = new Dictionary<(int Ancho, int Largo), int>();
                        foreach (var item in lista)
                        {
                            int ancho = (int)Math.Round(item[0]);
                            int largo = (int)Math.Round(item[1]);
                            int cant = item.Length > 2 ? (int)Math.Round(item[2]) : 1;
                            var key = (ancho, largo);
                            agrupadas[key] = agrupadas.GetValueOrDefault(key, 0) + cant;
                        }
                        var parts = agrupadas.Select(kv => $"{kv.Key.Ancho}x{kv.Key.Largo} ({kv.Value})");
                        textoPiezas = string.Join(", ", parts);
                    }

                    var p = new Paragraph(new Run(textoPiezas)
                    {
                        Foreground = textoPiezas == "-" ? Brushes.Silver : Brushes.Black
                    });

                    row.Cells.Add(new TableCell(p)
                    {
                        Padding = new Thickness(8, 5, 8, 5),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                        BorderThickness = new Thickness(0, 0, 0, 1)
                    });
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
                Margin = new Thickness(0, 10, 0, 0),
                BorderBrush = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
                BorderThickness = new Thickness(0, 1.5, 0, 0),
                Padding = new Thickness(0, 10, 0, 0),
                LineHeight = 22
            };
            totals.Inlines.Add(new Run($"Placas de {placa?.Ancho ?? 1830} x {placa?.Largo ?? 2400} mm necesarias: ") { FontSize = 12 });
            totals.Inlines.Add(new Run($"{cantidadPlacas} unidad(es)") { FontSize = 12, FontWeight = FontWeights.Bold });
            totals.Inlines.Add(new LineBreak());
            totals.Inlines.Add(new Run($"Costo Material Placas: ${precioMaterial:N2}   +   Herrajes estimados (30%): ${precioHerrajes:N2}") { FontSize = 11, Foreground = Brushes.DarkSlateGray });
            totals.Inlines.Add(new LineBreak());
            totals.Inlines.Add(new Run($"PRECIO TOTAL APROXIMADO: ${precioTotal:N2}") { FontSize = 17, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(21, 128, 61)) });
            doc.Blocks.Add(totals);

            return doc;
        }

        public static string ObtenerCarpetaDocumentos()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var current = new DirectoryInfo(baseDir);
            DirectoryInfo? projectRootDir = null;

            while (current != null)
            {
                bool isInBinOrObj = current.FullName.IndexOf(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    current.FullName.IndexOf(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    current.Name.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
                                    current.Name.Equals("obj", StringComparison.OrdinalIgnoreCase);

                if (!isInBinOrObj)
                {
                    string candidateDoc = Path.Combine(current.FullName, "documentos");
                    if (Directory.Exists(candidateDoc))
                    {
                        return candidateDoc;
                    }

                    bool hasProjectIndicators = File.Exists(Path.Combine(current.FullName, "Woodic.csproj")) ||
                                                File.Exists(Path.Combine(current.FullName, "Woodic.sln")) ||
                                                Directory.Exists(Path.Combine(current.FullName, "Themes"));

                    if (hasProjectIndicators && projectRootDir == null)
                    {
                        projectRootDir = current;
                    }
                }

                current = current.Parent;
            }

            if (projectRootDir != null)
            {
                string folder = Path.Combine(projectRootDir.FullName, "documentos");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                return folder;
            }

            string fallbackFolder = Path.Combine(baseDir, "documentos");
            if (!Directory.Exists(fallbackFolder))
            {
                Directory.CreateDirectory(fallbackFolder);
            }
            return fallbackFolder;
        }

        public static string ExportarCortesPDF(int pedidoId, decimal precioTotal, List<Dictionary<string, List<double[]>>> despieceModulos)
        {
            string folder = ObtenerCarpetaDocumentos();
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
            gfx.DrawString($"WOODIC - Listado de Cortes (Pedido #{pedidoId})", fontTitle, XBrushes.DarkBlue, new XPoint(40, y));
            y += 20;
            gfx.DrawString($"Fecha : {DateTime.Now:dd/MM/yyyy HH:mm}", fontSub, XBrushes.Gray, new XPoint(40, y));
            y += 25;

            for (int m = 0; m < despieceModulos.Count; m++)
            {
                gfx.DrawString($"Módulo {m + 1}:", fontBold, XBrushes.Black, new XPoint(40, y));
                y += 16;

                // Tabla de componentes
                gfx.DrawRectangle(XPens.LightGray, XBrushes.AliceBlue, 40, y, 515, 18);
                gfx.DrawString("Componente", fontBold, XBrushes.Black, new XPoint(50, y + 13));
                gfx.DrawString("Medida", fontBold, XBrushes.Black, new XPoint(220, y + 13));
                gfx.DrawString("Cantidad", fontBold, XBrushes.Black, new XPoint(450, y + 13));
                y += 20;

                var mod = despieceModulos[m];
                foreach (var entry in mod)
                {
                    if (entry.Value.Count == 0) continue;

                    // Agrupar cortes del mismo tipo y misma medida sumando la cantidad total
                    var agrupados = new Dictionary<(int Ancho, int Largo), int>();
                    foreach (var item in entry.Value)
                    {
                        int ancho = (int)Math.Round(item[0]);
                        int largo = (int)Math.Round(item[1]);
                        int cant = item.Length > 2 ? (int)Math.Round(item[2]) : 1;

                        var key = (ancho, largo);
                        agrupados[key] = agrupados.GetValueOrDefault(key, 0) + cant;
                    }

                    foreach (var kvp in agrupados)
                    {
                        gfx.DrawString(entry.Key, fontRegular, XBrushes.Black, new XPoint(50, y + 12));
                        gfx.DrawString($"{kvp.Key.Ancho} mm x {kvp.Key.Largo} mm", fontRegular, XBrushes.Black, new XPoint(220, y + 12));
                        gfx.DrawString(kvp.Value.ToString(), fontRegular, XBrushes.Black, new XPoint(460, y + 12));
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
