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

        public int ContarPedidosAsociados(int idPlaca)
        {
            return DatabaseHelper.ContarPedidosConPlaca(idPlaca);
        }

        public bool BorrarPlaca(int idPlaca)
        {
            return DatabaseHelper.DeletePlaca(idPlaca);
        }

        public bool BorrarPlacaYPedidos(int idPlaca)
        {
            return DatabaseHelper.DeletePlacaYPedidos(idPlaca);
        }
    }
}
