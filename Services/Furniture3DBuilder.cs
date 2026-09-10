using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Woodic.Services
{
    public class Segmento3D
    {
        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }

        public Segmento3D(int x1, int y1, int x2, int y2)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
        }
    }

    public class SubespacioRect
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public SubespacioRect(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    public static class Furniture3DBuilder
    {
        private const double Thickness = 18.0;

        public static Model3DGroup BuildFurnitureModel(
            int alto,
            int ancho,
            int profundo,
            int formato, // 0: Completo, 1: Escritorio (sin zócalo inf), 2: Bajomesada (sin cabezal sup)
            List<Segmento3D> divisoriosH,
            List<Segmento3D> divisoriosV,
            List<System.Windows.Point> banquinasH,
            List<System.Windows.Point> banquinasV,
            List<SubespacioRect> cajones,
            List<SubespacioRect> puertas,
            SubespacioRect? highlightedSubspace = null,
            Color? woodColor = null)
        {
            var group = new Model3DGroup();

            if (alto <= 0 || ancho <= 0 || profundo <= 0)
            {
                return group;
            }

            // Materiales
            Color baseColor = woodColor ?? Color.FromRgb(215, 185, 145); // Tono madera natural
            Color darkerBorder = Color.FromRgb((byte)Math.Max(0, baseColor.R - 35),
                                              (byte)Math.Max(0, baseColor.G - 35),
                                              (byte)Math.Max(0, baseColor.B - 35));
            Color metalColor = Color.FromRgb(180, 185, 195);
            Color backPanelColor = Color.FromRgb(240, 235, 225);

            var woodMaterial = new DiffuseMaterial(new SolidColorBrush(baseColor));
            var borderMaterial = new DiffuseMaterial(new SolidColorBrush(darkerBorder));
            var handleMaterial = new DiffuseMaterial(new SolidColorBrush(metalColor));
            var backMaterial = new DiffuseMaterial(new SolidColorBrush(backPanelColor));
            var drawerFrontMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(230, 205, 170)));
            var doorMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(225, 198, 160)));

            // 1. Laterales (Izquierdo y Derecho)
            // Lateral Izquierdo
            AddBox(group, 0, 0, 0, Thickness, alto, profundo, woodMaterial);
            // Lateral Derecho
            AddBox(group, ancho - Thickness, 0, 0, Thickness, alto, profundo, woodMaterial);

            // 2. Cabezal superior (solo si formato != 2 / no es bajomesada)
            if (formato != 2)
            {
                AddBox(group, Thickness, alto - Thickness, 0, ancho - (Thickness * 2), Thickness, profundo, woodMaterial);
            }

            // 3. Zócalo inferior (solo si formato != 1 / no es escritorio)
            if (formato != 1)
            {
                AddBox(group, Thickness, 0, 0, ancho - (Thickness * 2), Thickness, profundo, woodMaterial);
            }

            // 4. Fondo trasero (fina lámina en el fondo Z=0)
            double fondoY = (formato == 1) ? 0 : Thickness;
            double fondoAlto = alto - fondoY - ((formato == 2) ? 0 : Thickness);
            AddBox(group, Thickness, fondoY, 0, ancho - (Thickness * 2), fondoAlto, 3, backMaterial);

            // 5. Divisorios Horizontales
            foreach (var s in divisoriosH)
            {
                double x = Math.Max(Thickness, s.X1);
                double w = Math.Min(ancho - Thickness, s.X2) - x;
                double y = s.Y1 - (Thickness / 2);
                AddBox(group, x, y, 0, w, Thickness, profundo - 10, woodMaterial);
            }

            // 6. Divisorios Verticales
            foreach (var s in divisoriosV)
            {
                double x = s.X1 - (Thickness / 2);
                double y = Math.Max(Thickness, s.Y1);
                double h = Math.Min(alto - Thickness, s.Y2) - y;
                AddBox(group, x, y, 0, Thickness, h, profundo - 10, woodMaterial);
            }

            // 7. Banquinas
            foreach (var p in banquinasH)
            {
                AddBox(group, Thickness, p.Y, profundo - 30, ancho - (Thickness * 2), 100, Thickness, borderMaterial);
            }

            foreach (var p in banquinasV)
            {
                AddBox(group, p.X, Thickness, profundo - 30, Thickness, alto - (Thickness * 2), 100, borderMaterial);
            }

            // 8. Cajones
            foreach (var c in cajones)
            {
                double cx = c.X + 3;
                double cy = c.Y + 3;
                double cw = Math.Max(10, c.Width - 6);
                double ch = Math.Max(10, c.Height - 6);
                double cz = profundo - Thickness;

                // Tapa / Frente del cajón
                AddBox(group, cx, cy, cz, cw, ch, Thickness, drawerFrontMaterial);

                // Tirador / manija metálica en el centro del frente
                double hx = cx + (cw / 2) - 35;
                double hy = cy + (ch / 2) - 4;
                AddBox(group, hx, hy, cz + Thickness, 70, 8, 12, handleMaterial);
            }

            // 9. Puertas
            foreach (var p in puertas)
            {
                double px = p.X + 3;
                double py = p.Y + 3;
                double pw = Math.Max(10, p.Width - 6);
                double ph = Math.Max(10, p.Height - 6);
                double pz = profundo - Thickness;

                // Hoja de la puerta
                AddBox(group, px, py, pz, pw, ph, Thickness, doorMaterial);

                // Tirador vertical
                double hx = px + pw - 25;
                double hy = py + (ph / 2) - 45;
                AddBox(group, hx, hy, pz + Thickness, 8, 90, 12, handleMaterial);
            }

            // 10. Subespacio resaltado (si el usuario está seleccionando ubicación)
            if (highlightedSubspace != null)
            {
                var highlightMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(120, 56, 189, 248)));
                AddBox(group,
                    highlightedSubspace.X,
                    highlightedSubspace.Y,
                    5,
                    highlightedSubspace.Width,
                    highlightedSubspace.Height,
                    profundo - 10,
                    highlightMaterial);
            }

            return group;
        }

        public static void AddBox(
            Model3DGroup group,
            double x, double y, double z,
            double width, double height, double depth,
            Material material)
        {
            var mesh = new MeshGeometry3D();

            // 8 vértices del paralelepípedo
            Point3D p0 = new(x, y, z);
            Point3D p1 = new(x + width, y, z);
            Point3D p2 = new(x + width, y + height, z);
            Point3D p3 = new(x, y + height, z);
            Point3D p4 = new(x, y, z + depth);
            Point3D p5 = new(x + width, y, z + depth);
            Point3D p6 = new(x + width, y + height, z + depth);
            Point3D p7 = new(x, y + height, z + depth);

            // Caras (2 triángulos por cara, en sentido antihorario)
            // Frontal (Z + depth)
            AddQuad(mesh, p4, p5, p6, p7);
            // Trasera (Z)
            AddQuad(mesh, p1, p0, p3, p2);
            // Izquierda (X)
            AddQuad(mesh, p0, p4, p7, p3);
            // Derecha (X + width)
            AddQuad(mesh, p5, p1, p2, p6);
            // Superior (Y + height)
            AddQuad(mesh, p7, p6, p2, p3);
            // Inferior (Y)
            AddQuad(mesh, p0, p1, p5, p4);

            mesh.Freeze();
            var model = new GeometryModel3D(mesh, material)
            {
                BackMaterial = material
            };
            group.Children.Add(model);
        }

        private static void AddQuad(MeshGeometry3D mesh, Point3D p0, Point3D p1, Point3D p2, Point3D p3)
        {
            int baseIndex = mesh.Positions.Count;

            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.Positions.Add(p3);

            // Triángulo 1
            mesh.TriangleIndices.Add(baseIndex);
            mesh.TriangleIndices.Add(baseIndex + 1);
            mesh.TriangleIndices.Add(baseIndex + 2);

            // Triángulo 2
            mesh.TriangleIndices.Add(baseIndex);
            mesh.TriangleIndices.Add(baseIndex + 2);
            mesh.TriangleIndices.Add(baseIndex + 3);
        }
    }
}
