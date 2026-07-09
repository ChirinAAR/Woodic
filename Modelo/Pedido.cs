using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Woodic.Modelo
{
    public class Pedido
    {
        int id_pedido;
        int placaId;
        int cantidadModulos;
        int precio;
        public Pedido() {
        }
        public int getId_pedido()
        {
            return id_pedido;
        }
        public void setId_pedido(int id_pedido)
        {
            this.id_pedido = id_pedido;
        }
        public void setPlacaId(int placaId)
        {
            this.placaId = placaId;
        }
        public int getPlacaId()
        {
            return placaId;
        }
        public int getCantidadModulos()
        {
            return cantidadModulos;
        }
        public void setCantidadModulos(int cantidadModulos)
        {
            this.cantidadModulos = cantidadModulos;
        }
        public int getPrecio()
        {
            return precio;
        }
        public void setPrecio(int precio)
        {
            this.precio = precio;
        }
        private Cliente cliente;
        public void setCliente(Cliente cliente)
        {
            this.cliente = cliente;
        }

        public Cliente getCliente()
        {
            return cliente;
        }

        internal Cliente Cliente()
        {
            throw new NotImplementedException();
        }
    }
}
