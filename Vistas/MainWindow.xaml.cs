using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Woodic.Controlador;
using Woodic.Modelo;
using Woodic.Services;

namespace Woodic.Vistas
{
    public partial class MainWindow : Window
    {
        public MainController Controller { get; }

        public MainWindow()
        {
            InitializeComponent();
            Controller = new MainController(this);

            // Cargar el tema guardado previamente (o tema oscuro por defecto) y pantalla de bienvenida
            Controller.CargarTemaGuardado();
            Controller.MostrarInicio();

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;
            await VerificarYConfigurarBaseDeDatosAsync();
        }

        private async Task VerificarYConfigurarBaseDeDatosAsync()
        {
            if (!LocalDbManager.IsLocalDbInstalled())
            {
                bool installed = await LocalDbManager.PromptAndInstallLocalDbAsync(this);
                if (!installed)
                {
                    MessageBox.Show(
                        this,
                        "Woodic requiere Microsoft SQL Server LocalDB para almacenar pedidos, placas y clientes.\n" +
                        "Puede instalar 'SqlLocalDB.msi' en cualquier momento ejecutando el instalador incluido.",
                        "Aviso de Base de Datos",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }
            }

            await Task.Run(() => LocalDbManager.EnsureInstanceStarted());

            bool tablesExist = await Task.Run(() => DatabaseHelper.TablesExist());
            if (!tablesExist)
            {
                var dialogo = new DialogoConfigurando
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                IsEnabled = false;
                dialogo.Show();

                await Task.Run(() =>
                {
                    try
                    {
                        DatabaseHelper.InitializeSchema();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al inicializar base de datos: {ex.Message}");
                    }
                });

                await Task.Delay(1000);
                dialogo.Close();
                IsEnabled = true;
            }
        }

        public void ActualizarTextoBotonTema(bool esOscuro)
        {
            btnCambiarTema.Content = esOscuro ? "🌙 Oscuro" : "☀️ Claro";
            btnCambiarTema.ToolTip = esOscuro ? "Cambiar a tema claro" : "Cambiar a tema oscuro";
        }

        public void NavegarA(UserControl vista)
        {
            MainContent.Content = vista;
        }

        public void ActualizarBotonActivo(Button? activo)
        {
            var navButtons = new[] { btnCrearPedido, btnPresupuestoRapido, btnListaPedidos, btnConfiguracion };
            var activeStyle = (Style)FindResource("ActiveNavButtonStyle");
            var defaultStyle = (Style)FindResource("NavButtonStyle");

            foreach (var btn in navButtons)
            {
                btn.Style = (btn == activo) ? activeStyle : defaultStyle;
            }
        }

        private void btnCrearPedido_Click(object sender, RoutedEventArgs e)
        {
            Controller.MostrarCrearPedido();
        }

        private void btnPresupuestoRapido_Click(object sender, RoutedEventArgs e)
        {
            Controller.MostrarPresupuestoRapido();
        }

        private void btnListaPedidos_Click(object sender, RoutedEventArgs e)
        {
            Controller.MostrarListaPedidos();
        }

        private void btnConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            Controller.MostrarConfiguracion();
        }

        private void btnCambiarTema_Click(object sender, RoutedEventArgs e)
        {
            Controller.ToggleTheme();
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Controller.Salir();
        }
    }
}
