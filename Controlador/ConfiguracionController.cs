using System;
using System.Collections.Generic;
using Woodic.Modelo;

namespace Woodic.Controlador
{
    public class ConfiguracionController
    {
        private readonly MainController _mainController;

        public ConfiguracionController(MainController mainController)
        {
            _mainController = mainController;
        }

        public List<Placa> CargarPlacas()
        {
            return DatabaseHelper.GetAllPlacas();
        }

        public bool AnadirPlaca(Placa placa)
        {
            return DatabaseHelper.InsertPlaca(placa);
        }

        public bool BorrarPlaca(int idPlaca)
        {
            return DatabaseHelper.DeletePlaca(idPlaca);
        }

        public void InicializarBaseDatos()
        {
            DatabaseHelper.InitializeSchema();
        }

        public void ReiniciarBaseDatos()
        {
            DatabaseHelper.ResetDatabase();
        }

        public void CambiarTema(bool dark)
        {
            _mainController.SetTheme(dark);
        }

        public bool EsTemaOscuro => _mainController.IsDarkTheme;
    }
}
