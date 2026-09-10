using System.Windows;
using System.Windows.Input;

namespace Woodic.Vistas
{
    public partial class DialogoEntrada : Window
    {
        public string ValorIngresado { get; private set; } = string.Empty;

        public DialogoEntrada(string titulo, string mensaje, string valorInicial = "")
        {
            InitializeComponent();
            Title = titulo;
            lblMensaje.Text = mensaje;
            txtValor.Text = valorInicial;
            txtValor.SelectAll();
            Loaded += (s, e) => txtValor.Focus();
        }

        public static string? Mostrar(Window? owner, string titulo, string mensaje, string valorInicial = "")
        {
            var dlg = new DialogoEntrada(titulo, mensaje, valorInicial);
            if (owner != null) dlg.Owner = owner;
            if (dlg.ShowDialog() == true)
            {
                return dlg.ValorIngresado;
            }
            return null;
        }

        private void btnAceptar_Click(object sender, RoutedEventArgs e)
        {
            ValorIngresado = txtValor.Text.Trim();
            DialogResult = true;
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void txtValor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnAceptar_Click(sender, e);
            }
            else if (e.Key == Key.Escape)
            {
                btnCancelar_Click(sender, e);
            }
        }
    }
}
