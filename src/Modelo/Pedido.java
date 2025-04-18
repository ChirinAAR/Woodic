
package Modelo;


public class Pedido extends Cliente
{
   private int id_pedido;
   private int precio;
   private int cantidadmod;

   private Fecha fecha;

       public Pedido(int id_pedido, int precio, int cantidadmod, Fecha fecha, String nombreapellido, String direccion, int contacto) {
        super(nombreapellido, direccion, contacto); // constructor de Cliente
        this.id_pedido = id_pedido;
        this.precio = precio;
        this.cantidadmod = cantidadmod;
        this.fecha = fecha;
    }

    public int getId_pedido() {
        return id_pedido;
    }

    public void setId_pedido(int id_pedido) {
        this.id_pedido = id_pedido;
    }

    public int getPrecio() {
        return precio;
    }

    public void setPrecio(int precio) {
        this.precio = precio;
    }

    public int getCantidadmod() {
        return cantidadmod;
    }

    public void setCantidadmod(int cantidadmod) {
        this.cantidadmod = cantidadmod;
    }

    public Fecha getFecha() {
        return fecha;
    }

    public void setFecha(Fecha fecha) {
        this.fecha = fecha;
    }

    @Override
    public String toString() {
        return "Pedido{" + "id_pedido=" + id_pedido + ", precio=" + precio + ", cantidadmod=" + cantidadmod + ", fecha=" + fecha + '}';
    }

   
   
    
    
}
