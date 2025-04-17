
package Modelo;


public class Cliente {
    private String nombreapellido;
    private String direccion;
    private int contacto;

    public Cliente(String nombreapellido, String direccion, int contacto) {
        this.nombreapellido = nombreapellido;
        this.direccion = direccion;
        this.contacto = contacto;
    }

    public int getContacto() {
        return contacto;
    }

    public void setContacto(int contacto) {
        this.contacto = contacto;
    }

    public String getNombreapellido() {
        return nombreapellido;
    }

    public void setNombreapellido(String nombreapellido) {
        this.nombreapellido = nombreapellido;
    }

    public String getDireccion() {
        return direccion;
    }

    public void setDireccion(String direccion) {
        this.direccion = direccion;
    }

    @Override
    public String toString() {
        return "Cliente{" + "nombreapellido=" + nombreapellido + ", direccion=" + direccion + ", contacto=" + contacto + '}';
    }
    
    
}
