
package Modelo;
import java.time.LocalDateTime;

public class Pedido
{
   private int id_pedido;
   private int precio;
   private int cantidadmod;
   private LocalDateTime fecha = LocalDateTime.now();
   
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

    public LocalDateTime getFecha() {
        return fecha;
    }




    }
