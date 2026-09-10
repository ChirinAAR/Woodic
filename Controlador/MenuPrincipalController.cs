using System;
using System.Windows;
using Woodic.Vistas;

namespace Woodic.Controlador
{
    public class MenuPrincipalController
    {
        private readonly Window? vista;

        public MenuPrincipalController(Window? vista)
        {
            this.vista = vista;
        }

        public void AbrirCrearPedido()
        {
            var main = new MainWindow();
            main.Show();
            main.Controller.MostrarCrearPedido();
            vista?.Close();
        }

        public void AbrirListaPedidos()
        {
            var main = new MainWindow();
            main.Show();
            main.Controller.MostrarListaPedidos();
            vista?.Close();
        }

        public void AbrirCortes()
        {
            var main = new MainWindow();
            main.Show();
            main.Controller.MostrarListaPedidos();
            vista?.Close();
        }

        public void AbrirAnadirPlaca()
        {
            var main = new MainWindow();
            main.Show();
            main.Controller.MostrarConfiguracion();
            vista?.Close();
        }

        public void SalirAplicacion()
        {
            Application.Current.Shutdown();
        }
    }
}
