using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Woodic.Modelo
{
    public class Cliente
    {
        int contacto;
        string nombre;
        string direccion;

        public Cliente(){
        }
        public int getContacto()
        {
            return contacto;
        }
        public void setContacto(int contacto)
        {
            this.contacto = contacto;
        }
        public string getNombre()
        {
            return nombre;
        }
        public void setNombre(string nombre)
        {
            this.nombre = nombre;
        }
        public string getDireccion()
        {
            return direccion;
        }
        public void setDireccion(string direccion)
        {
            this.direccion = direccion;
        }
    }
}
