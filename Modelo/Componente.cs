using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Woodic.Modelo
{
    public class Componente
    {
        int idComponente;
        string nombreComponente;
        int ancho;
        int largo;
        int cantidad;
        internal class Pieza
        {
            public int ancho, alto;
            public bool colocada = false;
            public Pieza(int ancho, int alto)
            {
                this.ancho = ancho;
                this.alto = alto;
            }
            public Pieza rotada()
            {
                return new Pieza(this.alto, this.ancho);
            }
        }
    }
}
