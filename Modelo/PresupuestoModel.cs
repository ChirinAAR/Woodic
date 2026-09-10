using System;
using System.Collections.Generic;

namespace Woodic.Modelo
{
    public class PresupuestoModel
    {
        public int PedidoId { get; set; }
        public Placa? PlacaSeleccionada { get; set; }
        public int CantidadModulos { get; set; } = 1;
        public int CantidadPlacas { get; set; } = 1;
        public decimal PrecioPlaca { get; set; }
        public decimal PrecioMaterial { get; set; }
        public decimal PrecioHerrajes { get; set; }
        public decimal PrecioTotal { get; set; }
        public bool EsPresupuestoRapido { get; set; } = false;

        public List<Dictionary<string, List<double[]>>> DespieceModulos { get; set; } = new();
        public List<Componente.Pieza> TodasLasPiezas { get; set; } = new();
    }
}
