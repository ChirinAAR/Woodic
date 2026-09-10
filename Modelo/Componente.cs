using System;

namespace Woodic.Modelo
{
    public class Componente
    {
        public int IdComponente { get; set; }
        public string NombreComponente { get; set; } = string.Empty;
        public int Ancho { get; set; }
        public int Largo { get; set; }
        public int Cantidad { get; set; } = 1;
        public int ModuloId { get; set; }

        public Componente() { }

        public Componente(string nombre, int ancho, int largo, int cantidad = 1, int moduloId = 0)
        {
            NombreComponente = nombre;
            Ancho = ancho;
            Largo = largo;
            Cantidad = cantidad;
            ModuloId = moduloId;
        }

        public class Pieza
        {
            public int Ancho { get; set; }
            public int Alto { get; set; }
            public bool Colocada { get; set; } = false;

            public Pieza(int ancho, int alto)
            {
                Ancho = ancho;
                Alto = alto;
            }

            public Pieza Rotada()
            {
                return new Pieza(Alto, Ancho);
            }
        }
    }
}
