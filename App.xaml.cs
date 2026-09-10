using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Woodic.Modelo;
using Woodic.Services;

namespace Woodic
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Contains("--test-db"))
            {
                try
                {
                    DatabaseHelper.InitializeSchema();
                    var placas = DatabaseHelper.GetAllPlacas();
                    Console.WriteLine($"DB_TEST_OK: Placas count = {placas.Count}");
                    var cheapest = DatabaseHelper.GetCheapestPlaca();
                    Console.WriteLine($"DB_TEST_CHEAPEST: {cheapest?.Linea} {cheapest?.Color} ${cheapest?.Precio}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DB_TEST_ERROR: {ex.Message}");
                }
                Shutdown(0);
                return;
            }

            if (e.Args.Contains("--test-calc"))
            {
                try
                {
                    DatabaseHelper.InitializeSchema();
                    var piezas = new List<Componente.Pieza>
                    {
                        new Componente.Pieza(600, 800),
                        new Componente.Pieza(600, 800),
                        new Componente.Pieza(500, 700),
                        new Componente.Pieza(500, 700),
                        new Componente.Pieza(400, 300),
                        new Componente.Pieza(400, 300)
                    };

                    int placasNec = CorteOptimizer.CalcularCantidadPlacas(piezas, 1830, 2400, true);
                    Console.WriteLine($"CALC_TEST_OK: Placas necesarias = {placasNec}");

                    var despiece = new List<Dictionary<string, List<double[]>>>
                    {
                        new Dictionary<string, List<double[]>>
                        {
                            { "Laterales", new List<double[]> { new double[] { 600, 800, 2 } } },
                            { "Div Horizontal", new List<double[]> { new double[] { 500, 700, 2 } } },
                            { "Puertas", new List<double[]> { new double[] { 400, 300, 2 } } }
                        }
                    };

                    string pdfPath = ReportPrintService.ExportarCortesPDF(999, 45000m, despiece);
                    Console.WriteLine($"PDF_TEST_OK: PDF creado en {pdfPath} (Existe: {File.Exists(pdfPath)})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CALC_TEST_ERROR: {ex.Message}");
                }
                Shutdown(0);
                return;
            }

            try
            {
                DatabaseHelper.InitializeSchema();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Inicialización de base de datos: {ex.Message}");
            }
        }
    }
}
