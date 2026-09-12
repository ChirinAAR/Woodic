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
        }
    }
}
