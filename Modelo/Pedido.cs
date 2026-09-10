using System;

namespace Woodic.Modelo
{
    public class Pedido
    {
        public int IdPedido { get; set; }
        public int PlacaId { get; set; }
        public int CantidadModulos { get; set; } = 1;
        public decimal Precio { get; set; }
        public Cliente? Cliente { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;

        public Pedido() { }

        // Compatibilidad con código original
        public int getId_pedido() => IdPedido;
        public void setId_pedido(int id) => IdPedido = id;

        public int getPlacaId() => PlacaId;
        public void setPlacaId(int id) => PlacaId = id;

        public int getCantidadModulos() => CantidadModulos;
        public void setCantidadModulos(int cant) => CantidadModulos = cant;

        public int getPrecio() => (int)Precio;
        public void setPrecio(int p) => Precio = p;
        public void setPrecio(decimal p) => Precio = p;
        public void setPrecio(double p) => Precio = Convert.ToDecimal(p);

        public Cliente? getCliente() => Cliente;
        public void setCliente(Cliente? c) => Cliente = c;
        public Cliente? GetCliente() => Cliente;
    }
}
