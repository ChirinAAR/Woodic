using System;
using System.Threading.Tasks;
using System.Windows;
using Woodic.Modelo;

namespace Woodic
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Capturar cualquier excepción no controlada en el despachador de WPF para evitar cierres abruptos
            DispatcherUnhandledException += (sender, args) =>
            {
                MessageBox.Show(
                    $"Se ha producido un error inesperado en la aplicación:\n\n{args.Exception.Message}\n\nDetalle:\n{args.Exception.StackTrace}",
                    "Error Inesperado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                args.Handled = true;
            };


            // Pre-calentar la base de datos en segundo plano para que el motor SQL Server LocalDB
            // esté listo de inmediato y no bloquee el hilo de la interfaz al abrir las vistas
            Task.Run(() =>
            {
                try
                {
                    DatabaseHelper.InitializeSchema();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Inicialización de base de datos en segundo plano: {ex.Message}");
                }
            });
        }
    }
}
