using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Media3D;
using Microsoft.Data.SqlClient;
using Woodic.Modelo;
using Woodic.Services;

namespace Woodic.Controlador
{
    public class DisenarModuloController
    {
        public const int MAX_ALTO = 2600;
        public const int MAX_ANCHO = 2600;
        public const int MAX_PROFUNDIDAD = 1830;

        public int AlturaModulo { get; private set; } = 1800;
        public int AnchoModulo { get; private set; } = 900;
        public int ProfundidadModulo { get; private set; } = 500;
        public int Formato { get; private set; } = 0; // 0: Completo, 1: Escritorio, 2: Bajomesada

        public List<Segmento3D> DivisoriosHorizontales { get; } = new();
        public List<Segmento3D> DivisoriosVerticales { get; } = new();
        public List<Point> BanquinasHorizontales { get; } = new();
        public List<Point> BanquinasVerticales { get; } = new();
        public List<SubespacioRect> Puertas { get; } = new();
        public List<SubespacioRect> Cajones { get; } = new();

        public SubespacioRect? SubespacioResaltado { get; set; }

        private readonly Dictionary<string, List<double[]>> _medidasElementos = new();

        public DisenarModuloController()
        {
            CalcularMedidas();
        }

        public void ActualizarDimensiones(int alto, int ancho, int profundo, int formato)
        {
            AlturaModulo = Math.Clamp(alto, 100, MAX_ALTO);
            AnchoModulo = Math.Clamp(ancho, 100, MAX_ANCHO);
            ProfundidadModulo = Math.Clamp(profundo, 100, MAX_PROFUNDIDAD);
            Formato = formato;

            DivisoriosHorizontales.Clear();
            DivisoriosVerticales.Clear();
            BanquinasHorizontales.Clear();
            BanquinasVerticales.Clear();
            Puertas.Clear();
            Cajones.Clear();
            SubespacioResaltado = null;

            CalcularMedidas();
        }

        public void CambiarFormato(int formato)
        {
            Formato = formato;
            CalcularMedidas();
        }

        public bool RequiereSeleccionSubespacioHorizontal(int posicion)
        {
            if (posicion <= 0 || posicion >= AlturaModulo) return false;
            return DivisoriosVerticales.Count > 0;
        }

        public bool RequiereSeleccionSubespacioVertical(int posicion)
        {
            if (posicion <= 0 || posicion >= AnchoModulo) return false;
            return DivisoriosHorizontales.Count > 0;
        }

        public void AgregarDivisorioHorizontalCompleto(int posicion)
        {
            if (posicion <= 0 || posicion >= AlturaModulo) return;
            DivisoriosHorizontales.Add(new Segmento3D(0, posicion, AnchoModulo, posicion));
            CalcularMedidas();
        }

        public void AgregarDivisorioVerticalCompleto(int posicion)
        {
            if (posicion <= 0 || posicion >= AnchoModulo) return;
            DivisoriosVerticales.Add(new Segmento3D(posicion, 0, posicion, AlturaModulo));
            CalcularMedidas();
        }

        public void AgregarDivisorioHorizontalEnSubespacio(int posicionY, SubespacioRect subespacio)
        {
            DivisoriosHorizontales.Add(new Segmento3D(subespacio.X, posicionY, subespacio.X + subespacio.Width, posicionY));
            CalcularMedidas();
        }

        public void AgregarDivisorioVerticalEnSubespacio(int posicionX, SubespacioRect subespacio)
        {
            DivisoriosVerticales.Add(new Segmento3D(posicionX, subespacio.Y, posicionX, subespacio.Y + subespacio.Height));
            CalcularMedidas();
        }

        public void AgregarBanquina(bool horizontal, int posicion)
        {
            if (horizontal)
            {
                if (posicion > 0 && posicion < AlturaModulo)
                {
                    BanquinasHorizontales.Add(new Point(0, posicion));
                }
            }
            else
            {
                if (posicion > 0 && posicion < AnchoModulo)
                {
                    BanquinasVerticales.Add(new Point(posicion, 0));
                }
            }
            CalcularMedidas();
        }

        public SubespacioRect? EncontrarSubespacio(double x, double y)
        {
            if (x < 0 || x > AnchoModulo || y < 0 || y > AlturaModulo)
            {
                return null;
            }

            var puntosX = new List<int> { 0, AnchoModulo };
            foreach (var s in DivisoriosVerticales)
            {
                if (s.Y1 <= y && s.Y2 >= y)
                {
                    puntosX.Add(s.X1);
                }
            }

            var puntosY = new List<int> { 0, AlturaModulo };
            foreach (var s in DivisoriosHorizontales)
            {
                if (s.X1 <= x && s.X2 >= x)
                {
                    puntosY.Add(s.Y1);
                }
            }

            puntosX.Sort();
            puntosY.Sort();

            int x1 = 0, x2 = AnchoModulo;
            for (int i = 0; i < puntosX.Count - 1; i++)
            {
                if (x >= puntosX[i] && x <= puntosX[i + 1])
                {
                    x1 = puntosX[i];
                    x2 = puntosX[i + 1];
                    break;
                }
            }

            int y1 = 0, y2 = AlturaModulo;
            for (int i = 0; i < puntosY.Count - 1; i++)
            {
                if (y >= puntosY[i] && y <= puntosY[i + 1])
                {
                    y1 = puntosY[i];
                    y2 = puntosY[i + 1];
                    break;
                }
            }

            return new SubespacioRect(x1, y1, x2 - x1, y2 - y1);
        }

        public void AgregarPuertasEnSubespacio(SubespacioRect subespacio, bool doblePuerta)
        {
            if (doblePuerta)
            {
                int mitad = subespacio.Width / 2;
                Puertas.Add(new SubespacioRect(subespacio.X, subespacio.Y, mitad, subespacio.Height));
                Puertas.Add(new SubespacioRect(subespacio.X + mitad, subespacio.Y, subespacio.Width - mitad, subespacio.Height));
            }
            else
            {
                Puertas.Add(new SubespacioRect(subespacio.X, subespacio.Y, subespacio.Width, subespacio.Height));
            }
            CalcularMedidas();
        }

        public void AgregarCajonesEnSubespacio(SubespacioRect subespacio, int cantidad)
        {
            if (cantidad <= 0) cantidad = 1;
            int altoCajon = subespacio.Height / cantidad;
            for (int i = 0; i < cantidad; i++)
            {
                Cajones.Add(new SubespacioRect(subespacio.X, subespacio.Y + (i * altoCajon), subespacio.Width, altoCajon));
            }
            CalcularMedidas();
        }

        public void DeshacerDivisorios()
        {
            DivisoriosHorizontales.Clear();
            DivisoriosVerticales.Clear();
            CalcularMedidas();
        }

        public void DeshacerBanquinas()
        {
            BanquinasHorizontales.Clear();
            BanquinasVerticales.Clear();
            CalcularMedidas();
        }

        public void DeshacerCajones()
        {
            Cajones.Clear();
            CalcularMedidas();
        }

        public void DeshacerPuertas()
        {
            Puertas.Clear();
            CalcularMedidas();
        }

        public void CalcularMedidas()
        {
            _medidasElementos.Clear();

            // Zócalo y Cabezal
            var zocalo = new List<double[]>();
            var cabezal = new List<double[]>();
            if (Formato != 1) // No es escritorio (tiene zócalo)
            {
                zocalo.Add(new double[] { AnchoModulo, ProfundidadModulo, 1 });
            }
            if (Formato != 2) // No es bajomesada (tiene cabezal)
            {
                cabezal.Add(new double[] { AnchoModulo, ProfundidadModulo, 1 });
            }
            _medidasElementos["Zocalo"] = zocalo;
            _medidasElementos["Cabezal"] = cabezal;

            // Laterales (2 piezas de profundidad x (altura - 36mm))
            var laterales = new List<double[]>
            {
                new double[] { ProfundidadModulo, Math.Max(10, AlturaModulo - 36), 2 }
            };
            _medidasElementos["Laterales"] = laterales;

            // Divisorios Horizontales
            var divH = new List<double[]>();
            foreach (var s in DivisoriosHorizontales)
            {
                double largo = s.X2 - s.X1;
                divH.Add(new double[] { Math.Max(10, largo - 36), ProfundidadModulo, 1 });
            }
            _medidasElementos["Div Horizontal"] = divH;

            // Divisorios Verticales
            var divV = new List<double[]>();
            foreach (var s in DivisoriosVerticales)
            {
                double alto = s.Y2 - s.Y1;
                divV.Add(new double[] { Math.Max(10, alto - 36), ProfundidadModulo, 1 });
            }
            _medidasElementos["Div Vertical"] = divV;

            // Banquinas
            var banH = new List<double[]>();
            foreach (var _ in BanquinasHorizontales)
            {
                banH.Add(new double[] { Math.Max(10, AnchoModulo - 36), 100, 1 });
            }
            _medidasElementos["Banq Horizontal"] = banH;

            var banV = new List<double[]>();
            foreach (var _ in BanquinasVerticales)
            {
                banV.Add(new double[] { Math.Max(10, AlturaModulo - 36), 100, 1 });
            }
            _medidasElementos["Banq Vertical"] = banV;

            // Puertas
            var puertasMedidas = new List<double[]>();
            foreach (var p in Puertas)
            {
                puertasMedidas.Add(new double[] { Math.Max(10, p.Width - 6), Math.Max(10, p.Height - 6), 1 });
            }
            _medidasElementos["Puertas"] = puertasMedidas;

            // Cajones
            var baseCajon = new List<double[]>();
            var tapaCajon = new List<double[]>();
            var frenteCajon = new List<double[]>();
            var lateralCajon = new List<double[]>();

            foreach (var c in Cajones)
            {
                double anchoEspacio = c.Width;
                double alturaEspacio = c.Height;

                double anchoBase = Math.Max(10, anchoEspacio - 62);
                double largoBase = Math.Max(10, ProfundidadModulo - 30);
                baseCajon.Add(new double[] { anchoBase, largoBase, 1 });

                double anchoTapa = anchoEspacio;
                double altoTapa = alturaEspacio;
                tapaCajon.Add(new double[] { anchoTapa, altoTapa, 1 });

                double anchoFrente = anchoBase;
                double altoFrente = Math.Max(10, altoTapa - 48);
                frenteCajon.Add(new double[] { anchoFrente, altoFrente, 2 });

                double largoLateral = Math.Max(10, largoBase - 36);
                double altoLateral = Math.Max(10, altoTapa - 48);
                lateralCajon.Add(new double[] { largoLateral, altoLateral, 2 });
            }

            _medidasElementos["Base Cajon"] = baseCajon;
            _medidasElementos["Tapa Cajon"] = tapaCajon;
            _medidasElementos["Frente Cajon"] = frenteCajon;
            _medidasElementos["Lateral Cajon"] = lateralCajon;
        }

        public Dictionary<string, List<double[]>> ObtenerMedidasElementos()
        {
            return new Dictionary<string, List<double[]>>(_medidasElementos);
        }

        public List<Componente.Pieza> ObtenerPiezasDelModulo()
        {
            var piezas = new List<Componente.Pieza>();
            foreach (var lista in _medidasElementos.Values)
            {
                foreach (var datos in lista)
                {
                    int ancho = (int)Math.Round(datos[0]);
                    int alto = (int)Math.Round(datos[1]);
                    int cantidad = datos.Length > 2 ? (int)Math.Round(datos[2]) : 1;
                    for (int i = 0; i < cantidad; i++)
                    {
                        piezas.Add(new Componente.Pieza(ancho, alto));
                    }
                }
            }
            return piezas;
        }

        public int GuardarModuloYComponentesEnBD(int idPedido, string descripcion)
        {
            int idModulo = -1;
            try
            {
                using var conn = DatabaseHelper.CreateConnection();
                conn.Open();

                string sqlModulo = @"
                    INSERT INTO modulo (ANCHO, ALTO, PROFUNDO, pedido_idPEDIDO, DESCRIPCION)
                    OUTPUT INSERTED.idMODULO
                    VALUES (@ancho, @alto, @profundo, @idPedido, @desc);";

                using (var cmdMod = new SqlCommand(sqlModulo, conn))
                {
                    cmdMod.Parameters.AddWithValue("@ancho", AnchoModulo);
                    cmdMod.Parameters.AddWithValue("@alto", AlturaModulo);
                    cmdMod.Parameters.AddWithValue("@profundo", ProfundidadModulo);
                    cmdMod.Parameters.AddWithValue("@idPedido", idPedido);
                    cmdMod.Parameters.AddWithValue("@desc", descripcion ?? "");
                    idModulo = Convert.ToInt32(cmdMod.ExecuteScalar());
                }

                if (idModulo > 0)
                {
                    string sqlComp = @"
                        INSERT INTO componente (NOMBRECOMPONENTE, ANCHOCOMP, LARGOCOMP, CANTIDADCOMP, modulo_idMODULO)
                        VALUES (@nombre, @ancho, @largo, @cantidad, @idModulo);";

                    foreach (var entry in _medidasElementos)
                    {
                        string nombre = entry.Key;
                        foreach (var datos in entry.Value)
                        {
                            int ancho = (int)Math.Round(datos[0]);
                            int largo = (int)Math.Round(datos[1]);
                            int cantidad = datos.Length > 2 ? (int)Math.Round(datos[2]) : 1;

                            using var cmdComp = new SqlCommand(sqlComp, conn);
                            cmdComp.Parameters.AddWithValue("@nombre", nombre);
                            cmdComp.Parameters.AddWithValue("@ancho", ancho);
                            cmdComp.Parameters.AddWithValue("@largo", largo);
                            cmdComp.Parameters.AddWithValue("@cantidad", cantidad);
                            cmdComp.Parameters.AddWithValue("@idModulo", idModulo);
                            cmdComp.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar módulo en base de datos: {ex.Message}");
            }

            return idModulo;
        }

        public Model3DGroup GenerarModelo3D()
        {
            return Furniture3DBuilder.BuildFurnitureModel(
                AlturaModulo,
                AnchoModulo,
                ProfundidadModulo,
                Formato,
                DivisoriosHorizontales,
                DivisoriosVerticales,
                BanquinasHorizontales,
                BanquinasVerticales,
                Cajones,
                Puertas,
                SubespacioResaltado
            );
        }
    }
}
