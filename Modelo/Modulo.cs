using System;

namespace Woodic.Modelo
{
    public class Modulo
    {
        public int IdModulo { get; set; }
        public int Ancho { get; set; } = 1;
        public int Alto { get; set; } = 1;
        public int Profundo { get; set; } = 1;
        public int PedidoId { get; set; }
        public string Descripcion { get; set; } = string.Empty;

        public Modulo() { }

        public Modulo(int ancho, int alto, int profundo, int pedidoId = 0, string descripcion = "")
        {
            Ancho = ancho;
            Alto = alto;
            Profundo = profundo;
            PedidoId = pedidoId;
            Descripcion = descripcion;
        }

        // Métodos de compatibilidad
        public int getId_modulo() => IdModulo;
        public void setId_modulo(int id) => IdModulo = id;

        public int getAncho() => Ancho;
        public void setAncho(int a) => Ancho = a;

        public int getAlto() => Alto;
        public void setAlto(int a) => Alto = a;

        public int getProfundo() => Profundo;
        public void setProfundo(int p) => Profundo = p;

        public string getDescripcion() => Descripcion;
        public void setDescripcion(string d) => Descripcion = d;
    }
}
