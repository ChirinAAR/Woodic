using System;
using System.Collections.Generic;

namespace Woodic.Modelo
{
    public static class CorteOptimizer
    {
        public static int CalcularCantidadPlacas(List<Componente.Pieza> piezas, int anchoPlaca = 1830, int altoPlaca = 2400, bool permitirRotacion = true)
        {
            var placasUsadas = new List<PlacaVirtual>();

            // Ordenar piezas de mayor a menor área para mejorar el empaquetado (Best Fit Decreasing)
            var piezasOrdenadas = new List<Componente.Pieza>(piezas);
            piezasOrdenadas.Sort((a, b) => (b.Ancho * b.Alto).CompareTo(a.Ancho * a.Alto));

            foreach (var p in piezasOrdenadas)
            {
                bool colocada = false;
                foreach (var placa in placasUsadas)
                {
                    if (IntentarColocarEnPlaca(placa, p, permitirRotacion))
                    {
                        colocada = true;
                        break;
                    }
                }

                if (!colocada)
                {
                    var nueva = new PlacaVirtual(anchoPlaca, altoPlaca);
                    IntentarColocarEnPlaca(nueva, p, permitirRotacion);
                    placasUsadas.Add(nueva);
                }
            }

            return Math.Max(1, placasUsadas.Count);
        }

        private class PlacaVirtual
        {
            public int Ancho { get; }
            public int Alto { get; }
            public List<Rect> EspaciosLibres { get; } = new();

            public PlacaVirtual(int ancho, int alto)
            {
                Ancho = ancho;
                Alto = alto;
                EspaciosLibres.Add(new Rect(0, 0, ancho, alto));
            }
        }

        private class Rect
        {
            public int X { get; }
            public int Y { get; }
            public int Ancho { get; }
            public int Alto { get; }

            public Rect(int x, int y, int ancho, int alto)
            {
                X = x;
                Y = y;
                Ancho = ancho;
                Alto = alto;
            }

            public bool PuedeContener(Componente.Pieza p)
            {
                return p.Ancho <= Ancho && p.Alto <= Alto;
            }
        }

        private static bool IntentarColocarEnPlaca(PlacaVirtual placa, Componente.Pieza p, bool permitirRotacion)
        {
            for (int i = 0; i < placa.EspaciosLibres.Count; i++)
            {
                var espacio = placa.EspaciosLibres[i];
                if (espacio.PuedeContener(p))
                {
                    ColocarPiezaEnPlaca(placa, p, espacio, i);
                    return true;
                }

                if (permitirRotacion)
                {
                    var rotada = p.Rotada();
                    if (espacio.PuedeContener(rotada))
                    {
                        ColocarPiezaEnPlaca(placa, rotada, espacio, i);
                        return true;
                    }
                }
            }

            return false;
        }

        private static void ColocarPiezaEnPlaca(PlacaVirtual placa, Componente.Pieza p, Rect espacio, int indexEspacio)
        {
            int anchoRestante = espacio.Ancho - p.Ancho;
            int altoRestante = espacio.Alto - p.Alto;

            placa.EspaciosLibres.RemoveAt(indexEspacio);

            if (anchoRestante > 0)
            {
                placa.EspaciosLibres.Add(new Rect(espacio.X + p.Ancho, espacio.Y, anchoRestante, p.Alto));
            }

            if (altoRestante > 0)
            {
                placa.EspaciosLibres.Add(new Rect(espacio.X, espacio.Y + p.Alto, espacio.Ancho, altoRestante));
            }

            p.Colocada = true;
        }
    }
}
