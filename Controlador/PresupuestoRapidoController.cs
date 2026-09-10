using System;
using System.Collections.Generic;
using System.Windows;
using Woodic.Modelo;
using Woodic.Vistas;

namespace Woodic.Controlador
{
    public class PresupuestoRapidoController
    {
        private readonly MainWindow _mainWindow;

        public PresupuestoRapidoController(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void IniciarPresupuestoRapido()
        {
            // Abre directamente el diseñador para 1 módulo en modo rápido
            var disenoView = new DisenarModuloView(
                _mainWindow,
                idPedido: 0,
                cantidadModulos: 1,
                descripcionesModulos: new List<string> { "Módulo Presupuesto Rápido" },
                esPresupuestoRapido: true
            );

            _mainWindow.NavegarA(disenoView);
        }

        public void FinalizarPresupuestoRapido(
            Dictionary<string, List<double[]>> despieceModulo,
            List<Componente.Pieza> piezas)
        {
            // 1. Obtener la placa más económica de la base de datos
            Placa? placaEconomica = DatabaseHelper.GetCheapestPlaca();
            if (placaEconomica == null)
            {
                // Fallback por defecto si la base estuviera vacía
                placaEconomica = new Placa
                {
                    Linea = "Económica",
                    Compuesto = "Aglomerado",
                    Color = "Blanco Estándar",
                    Precio = 22000.00,
                    Ancho = 1830,
                    Largo = 2400
                };
            }

            // 2. Calcular placas necesarias usando el optimizador de corte
            int cantidadPlacas = CorteOptimizer.CalcularCantidadPlacas(piezas, placaEconomica.Ancho, placaEconomica.Largo, permitirRotacion: true);

            // 3. Calcular precios
            decimal precioPlaca = Convert.ToDecimal(placaEconomica.Precio);
            decimal precioMaterial = precioPlaca * cantidadPlacas;
            decimal precioHerrajes = precioMaterial * 0.30m;
            decimal precioTotal = precioMaterial + precioHerrajes;

            var presupuesto = new PresupuestoModel
            {
                PedidoId = 0,
                PlacaSeleccionada = placaEconomica,
                CantidadModulos = 1,
                CantidadPlacas = cantidadPlacas,
                PrecioPlaca = precioPlaca,
                PrecioMaterial = precioMaterial,
                PrecioHerrajes = precioHerrajes,
                PrecioTotal = precioTotal,
                EsPresupuestoRapido = true,
                DespieceModulos = new List<Dictionary<string, List<double[]>>> { despieceModulo },
                TodasLasPiezas = piezas
            };

            // 4. Navegar a FinalView con los resultados
            var finalView = new FinalView(_mainWindow, presupuesto);
            _mainWindow.NavegarA(finalView);
        }
    }
}
