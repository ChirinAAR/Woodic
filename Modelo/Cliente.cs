using System;

namespace Woodic.Modelo
{
    public class Cliente
    {
        public long Contacto { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;

        public Cliente() { }

        public Cliente(long contacto, string nombre, string direccion)
        {
            Contacto = contacto;
            Nombre = nombre;
            Direccion = direccion;
        }

        // Métodos de compatibilidad
        public long getContacto() => Contacto;
        public void setContacto(long contacto) => Contacto = contacto;
        public void setContacto(int contacto) => Contacto = contacto;

        public string getNombre() => Nombre;
        public void setNombre(string nombre) => Nombre = nombre;

        public string getDireccion() => Direccion;
        public void setDireccion(string direccion) => Direccion = direccion;
    }
}
