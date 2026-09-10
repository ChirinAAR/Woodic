using System;

namespace Woodic.Modelo
{
    public class Placa
    {
        public int IdPlaca { get; set; }
        public string Linea { get; set; } = string.Empty;
        public string Compuesto { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public double Precio { get; set; }
        public bool Beta { get; set; }
        public string Proveedor { get; set; } = string.Empty;
        public int Ancho { get; set; } = 1830;
        public int Largo { get; set; } = 2400;

        public string BetaTexto => Beta ? "Con Veta" : "Sin Veta";
        public string DimensionesTexto => $"{Ancho} x {Largo}";
        public string PrecioFormateado => $"${Precio:N2}";

        public Placa() { }

        // Compatibilidad con código original
        public int getId_placa() => IdPlaca;
        public void setId_placa(int id) => IdPlaca = id;

        public string getLinea() => Linea;
        public void setLinea(string l) => Linea = l;

        public string getColor() => Color;
        public void setColor(string c) => Color = c;

        public string getCompuesto() => Compuesto;
        public void setCompuesto(string c) => Compuesto = c;

        public int getPrecio() => (int)Precio;
        public void setPrecio(int p) => Precio = p;

        public bool isBeta() => Beta;
        public void setBeta(bool b) => Beta = b;

        public int getAncho() => Ancho;
        public void setAncho(int a) => Ancho = a;

        public int getLargo() => Largo;
        public void setLargo(int l) => Largo = l;
    }
}
