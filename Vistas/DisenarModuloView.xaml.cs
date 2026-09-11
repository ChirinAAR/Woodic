using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Woodic.Controlador;
using Woodic.Modelo;
using Woodic.Services;

namespace Woodic.Vistas
{
    public partial class DisenarModuloView : UserControl
    {
        private enum ModoSeleccion
        {
            Ninguno,
            DivisorioHorizontal,
            DivisorioVertical,
            Puertas,
            Cajones
        }

        private readonly MainWindow _mainWindow;
        private readonly int _idPedido;
        private readonly int _cantidadModulos;
        private int _moduloActual = 1;
        private readonly List<string>? _descripciones;
        private readonly bool _esPresupuestoRapido;

        public DisenarModuloController Controller { get; }

        private ModoSeleccion _modoSeleccion = ModoSeleccion.Ninguno;
        private int _posicionDivisorioTemporal = 0;
        private int _cantidadCajonesTemporal = 2;
        private bool _doblePuertaTemporal = false;

        // Parámetros de Cámara 3D
        private double _cameraYaw = 25.0;     // Grados horizontal
        private double _cameraPitch = 15.0;   // Grados vertical
        private double _cameraDistance = 3200;
        private Point _lastMousePos;
        private bool _isOrbiting = false;

        private bool _isInitializing = true;

        public DisenarModuloView(
            MainWindow mainWindow,
            int idPedido,
            int cantidadModulos,
            List<string>? descripcionesModulos = null,
            bool esPresupuestoRapido = false)
        {
            _isInitializing = true;
            _mainWindow = mainWindow;
            _idPedido = idPedido;
            _cantidadModulos = Math.Max(1, cantidadModulos);
            _descripciones = descripcionesModulos;
            _esPresupuestoRapido = esPresupuestoRapido;

            Controller = new DisenarModuloController();

            InitializeComponent();
            _isInitializing = false;

            if (cmbFormato != null && cmbFormato.SelectedIndex < 0)
            {
                cmbFormato.SelectedIndex = 0;
            }

            ActualizarTitulo();
            AplicarMedidasDesdeUI();
            SetCameraView(30, 20);
        }

        private void ActualizarTitulo()
        {
            string desc = "";
            if (_descripciones != null && _moduloActual - 1 < _descripciones.Count)
            {
                desc = _descripciones[_moduloActual - 1];
            }
            if (string.IsNullOrWhiteSpace(desc))
            {
                desc = _esPresupuestoRapido ? "Presupuesto Rápido" : $"Módulo {_moduloActual}";
            }

            if (lblTituloVistaPrevia != null)
            {
                lblTituloVistaPrevia.Text = $"Vista Previa: {desc} ({_moduloActual}/{_cantidadModulos})";
            }
        }

        private void AplicarMedidasDesdeUI()
        {
            if (_isInitializing || Controller == null || txtAltura == null || txtAncho == null || txtProfundidad == null || cmbFormato == null) return;

            if (!int.TryParse(txtAltura.Text, out int alto) || alto <= 0) alto = 1800;
            if (!int.TryParse(txtAncho.Text, out int ancho) || ancho <= 0) ancho = 900;
            if (!int.TryParse(txtProfundidad.Text, out int prof) || prof <= 0) prof = 500;

            int formato = cmbFormato.SelectedIndex >= 0 ? cmbFormato.SelectedIndex : 0;

            Controller.ActualizarDimensiones(alto, ancho, prof, formato);
            ActualizarVisualizacion3D();
        }

        private void ActualizarVisualizacion3D()
        {
            if (_isInitializing || furnitureVisual == null || mainCamera == null || Controller == null) return;

            furnitureVisual.Content = Controller.GenerarModelo3D();
            ActualizarCamara();
        }

        #region Controles de Cámara 3D

        private void ActualizarCamara()
        {
            if (_isInitializing || mainCamera == null || Controller == null) return;

            // Centro geométrico del mueble
            double cx = Controller.AnchoModulo / 2.0;
            double cy = Controller.AlturaModulo / 2.0;
            double cz = Controller.ProfundidadModulo / 2.0;

            double radYaw = _cameraYaw * Math.PI / 180.0;
            double radPitch = _cameraPitch * Math.PI / 180.0;

            double x = cx + (_cameraDistance * Math.Sin(radYaw) * Math.Cos(radPitch));
            double y = cy + (_cameraDistance * Math.Sin(radPitch));
            double z = cz + (_cameraDistance * Math.Cos(radYaw) * Math.Cos(radPitch));

            mainCamera.Position = new Point3D(x, y, z);
            mainCamera.LookDirection = new Vector3D(cx - x, cy - y, cz - z);
            mainCamera.UpDirection = new Vector3D(0, 1, 0);
        }

        private void SetCameraView(double yaw, double pitch)
        {
            _cameraYaw = yaw;
            _cameraPitch = pitch;
            _cameraDistance = Math.Max(Controller.AnchoModulo, Controller.AlturaModulo) * 2.2;
            ActualizarCamara();
        }

        private void btnCamIsometrica_Click(object sender, RoutedEventArgs e) => SetCameraView(30, 20);
        private void btnCamFrente_Click(object sender, RoutedEventArgs e) => SetCameraView(0, 0);

        private void viewport3D_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (_modoSeleccion != ModoSeleccion.Ninguno)
                {
                    // En modo selección, procesar clic en el subespacio
                    ManejarClicEnSubespacio(e.GetPosition(viewport3D));
                    return;
                }

                _isOrbiting = true;
                _lastMousePos = e.GetPosition(viewport3D);
                viewport3D.CaptureMouse();
            }
        }

        private void viewport3D_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentPos = e.GetPosition(viewport3D);

            if (_isOrbiting && e.LeftButton == MouseButtonState.Pressed)
            {
                double dx = currentPos.X - _lastMousePos.X;
                double dy = currentPos.Y - _lastMousePos.Y;

                _cameraYaw += dx * 0.5;
                _cameraPitch = Math.Clamp(_cameraPitch - (dy * 0.5), -89, 89);

                _lastMousePos = currentPos;
                ActualizarCamara();
            }
            else if (_modoSeleccion != ModoSeleccion.Ninguno)
            {
                // Resaltar subespacio bajo el cursor si se proyecta sobre el frente
                var subespacio = MapearPuntoPantallaASubespacio(currentPos);
                if (subespacio != Controller.SubespacioResaltado)
                {
                    Controller.SubespacioResaltado = subespacio;
                    ActualizarVisualizacion3D();
                }
            }
        }

        private void viewport3D_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isOrbiting)
            {
                _isOrbiting = false;
                viewport3D.ReleaseMouseCapture();
            }
        }

        private void viewport3D_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _cameraDistance = Math.Clamp(_cameraDistance - (e.Delta * 2.0), 500, 10000);
            ActualizarCamara();
        }

        #endregion

        #region Mapeo y Detección de Subespacios

        private SubespacioRect? MapearPuntoPantallaASubespacio(Point screenPoint)
        {
            // Usar Ray hit-testing en Viewport3D
            var hitParams = new PointHitTestParameters(screenPoint);
            SubespacioRect? subespacioDetectado = null;

            VisualTreeHelper.HitTest(viewport3D, null, result =>
            {
                if (result is RayMeshGeometry3DHitTestResult rayResult)
                {
                    Point3D p = rayResult.PointHit;
                    // p.X y p.Y corresponden a las coordenadas en mm del mueble
                    subespacioDetectado = Controller.EncontrarSubespacio(p.X, p.Y);
                    return HitTestResultBehavior.Stop;
                }
                return HitTestResultBehavior.Continue;
            }, hitParams);

            if (subespacioDetectado == null)
            {
                // Mapeo proporcional de respaldo si la cámara está de frente
                double vw = viewport3D.ActualWidth;
                double vh = viewport3D.ActualHeight;
                if (vw > 0 && vh > 0)
                {
                    double nx = (screenPoint.X / vw) * Controller.AnchoModulo;
                    double ny = (1.0 - (screenPoint.Y / vh)) * Controller.AlturaModulo;
                    subespacioDetectado = Controller.EncontrarSubespacio(nx, ny);
                }
            }

            return subespacioDetectado;
        }

        private void ManejarClicEnSubespacio(Point screenPoint)
        {
            var subespacio = MapearPuntoPantallaASubespacio(screenPoint);
            if (subespacio == null)
            {
                lblInstruccion.Text = "Haga clic dentro del módulo para colocar el elemento.";
                return;
            }

            switch (_modoSeleccion)
            {
                case ModoSeleccion.DivisorioHorizontal:
                    Controller.AgregarDivisorioHorizontalEnSubespacio(_posicionDivisorioTemporal, subespacio);
                    lblInstruccion.Text = "Divisorio horizontal colocado.";
                    break;

                case ModoSeleccion.DivisorioVertical:
                    Controller.AgregarDivisorioVerticalEnSubespacio(_posicionDivisorioTemporal, subespacio);
                    lblInstruccion.Text = "Divisorio vertical colocado.";
                    break;

                case ModoSeleccion.Puertas:
                    Controller.AgregarPuertasEnSubespacio(subespacio, _doblePuertaTemporal);
                    lblInstruccion.Text = "Puerta(s) colocada(s).";
                    break;

                case ModoSeleccion.Cajones:
                    Controller.AgregarCajonesEnSubespacio(subespacio, _cantidadCajonesTemporal);
                    lblInstruccion.Text = $"{_cantidadCajonesTemporal} cajones colocados.";
                    break;
            }

            // Salir del modo selección
            _modoSeleccion = ModoSeleccion.Ninguno;
            Controller.SubespacioResaltado = null;
            ActualizarVisualizacion3D();
        }

        #endregion

        #region Acciones de los Controles

        private void btnAplicarMedidas_Click(object sender, RoutedEventArgs e)
        {
            AplicarMedidasDesdeUI();
        }

        private void cmbFormato_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing || Controller == null || cmbFormato == null) return;

            if (cmbFormato.SelectedIndex >= 0)
            {
                Controller.CambiarFormato(cmbFormato.SelectedIndex);
                ActualizarVisualizacion3D();
            }
        }

        private void btnDivisorioHorizontal_Click(object sender, RoutedEventArgs e)
        {
            string? input = DialogoEntrada.Mostrar(
                Window.GetWindow(this),
                "Añadir Divisorio Horizontal",
                $"Ingrese la altura (Y) en mm (1 a {Controller.AlturaModulo - 1}):",
                (Controller.AlturaModulo / 2).ToString());

            if (int.TryParse(input, out int pos) && pos > 0 && pos < Controller.AlturaModulo)
            {
                if (Controller.RequiereSeleccionSubespacioHorizontal(pos))
                {
                    _modoSeleccion = ModoSeleccion.DivisorioHorizontal;
                    _posicionDivisorioTemporal = pos;
                    lblInstruccion.Text = "Haga clic en el compartimiento donde irá el divisorio horizontal.";
                    SetCameraView(0, 0); // Vista frontal para mayor precisión
                }
                else
                {
                    Controller.AgregarDivisorioHorizontalCompleto(pos);
                    ActualizarVisualizacion3D();
                    lblInstruccion.Text = "Divisorio horizontal agregado.";
                }
            }
        }

        private void btnDivisorioVertical_Click(object sender, RoutedEventArgs e)
        {
            string? input = DialogoEntrada.Mostrar(
                Window.GetWindow(this),
                "Añadir Divisorio Vertical",
                $"Ingrese la posición (X) en mm (1 a {Controller.AnchoModulo - 1}):",
                (Controller.AnchoModulo / 2).ToString());

            if (int.TryParse(input, out int pos) && pos > 0 && pos < Controller.AnchoModulo)
            {
                if (Controller.RequiereSeleccionSubespacioVertical(pos))
                {
                    _modoSeleccion = ModoSeleccion.DivisorioVertical;
                    _posicionDivisorioTemporal = pos;
                    lblInstruccion.Text = "Haga clic en el compartimiento donde irá el divisorio vertical.";
                    SetCameraView(0, 0);
                }
                else
                {
                    Controller.AgregarDivisorioVerticalCompleto(pos);
                    ActualizarVisualizacion3D();
                    lblInstruccion.Text = "Divisorio vertical agregado.";
                }
            }
        }

        private void btnBanquinaHorizontal_Click(object sender, RoutedEventArgs e)
        {
            string? input = DialogoEntrada.Mostrar(Window.GetWindow(this), "Banquina Horizontal", "Posición de banquina horizontal (mm):", "100");
            if (int.TryParse(input, out int pos))
            {
                Controller.AgregarBanquina(true, pos);
                ActualizarVisualizacion3D();
            }
        }

        private void btnBanquinaVertical_Click(object sender, RoutedEventArgs e)
        {
            string? input = DialogoEntrada.Mostrar(Window.GetWindow(this), "Banquina Vertical", "Posición de banquina vertical (mm):", "100");
            if (int.TryParse(input, out int pos))
            {
                Controller.AgregarBanquina(false, pos);
                ActualizarVisualizacion3D();
            }
        }

        private void btnSeleccionarCajones_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtCantidadCajones.Text, out int cant) || cant <= 0)
            {
                MessageBox.Show("Ingrese una cantidad válida de cajones.");
                return;
            }

            _modoSeleccion = ModoSeleccion.Cajones;
            _cantidadCajonesTemporal = cant;
            lblInstruccion.Text = $"Haga clic en el compartimiento para colocar los {cant} cajones.";
            SetCameraView(0, 0);
        }

        private void btnSeleccionarPuertas_Click(object sender, RoutedEventArgs e)
        {
            _modoSeleccion = ModoSeleccion.Puertas;
            _doblePuertaTemporal = rbPuertaDoble.IsChecked == true;
            lblInstruccion.Text = $"Haga clic en el compartimiento para colocar la(s) puerta(s) {(_doblePuertaTemporal ? "dobles" : "simple")}.";
            SetCameraView(0, 0);
        }

        private void btnDeshacerDivisorios_Click(object sender, RoutedEventArgs e)
        {
            Controller.DeshacerDivisorios();
            ActualizarVisualizacion3D();
        }

        private void btnDeshacerBanquinas_Click(object sender, RoutedEventArgs e)
        {
            Controller.DeshacerBanquinas();
            ActualizarVisualizacion3D();
        }

        private void btnDeshacerCajones_Click(object sender, RoutedEventArgs e)
        {
            Controller.DeshacerCajones();
            ActualizarVisualizacion3D();
        }

        private void btnDeshacerPuertas_Click(object sender, RoutedEventArgs e)
        {
            Controller.DeshacerPuertas();
            ActualizarVisualizacion3D();
        }

        private void btnTerminar_Click(object sender, RoutedEventArgs e)
        {
            if (_esPresupuestoRapido)
            {
                var presRapido = new PresupuestoRapidoController(_mainWindow);
                presRapido.FinalizarPresupuestoRapido(
                    Controller.ObtenerMedidasElementos(),
                    Controller.ObtenerPiezasDelModulo()
                );
            }
            else
            {
                // Guardar módulo actual en base de datos
                string desc = _descripciones != null && _moduloActual - 1 < _descripciones.Count
                    ? _descripciones[_moduloActual - 1]
                    : $"Módulo {_moduloActual}";

                Controller.GuardarModuloYComponentesEnBD(_idPedido, desc);

                if (_moduloActual < _cantidadModulos)
                {
                    _moduloActual++;
                    ActualizarTitulo();
                    // Limpiar y resetear para siguiente módulo
                    AplicarMedidasDesdeUI();
                }
                else
                {
                    var finalView = new FinalView(_mainWindow, _idPedido, _cantidadModulos);
                    _mainWindow.NavegarA(finalView);
                }
            }
        }

        #endregion
    }
}
