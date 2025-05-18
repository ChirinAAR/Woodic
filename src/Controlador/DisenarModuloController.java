
package Controlador;
import Vistas.DisenarModuloView;
import javax.swing.*;
import java.awt.*;
import java.util.List;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Map;

public class DisenarModuloController {
    DisenarModuloView vista;
    private int alturaModulo = 1;
    private int anchoModulo = 1;
    private int profundidadModulo;
    private static final int MAX_ALTO = 2600;
    private static final int MAX_ANCHO = 2600;
    private static final int MAX_PROFUNDIDAD = 1830;

    private int moduloX;
    private int moduloY;
    private float escalaX = 1;
    private float escalaY = 1;
    private List<Point> divisoriosHorizontales = new ArrayList<>();
    private List<Point> divisoriosVerticales = new ArrayList<>();

    private boolean dibujarPuerta = false;
    private List<Rectangle> puertas = new ArrayList<>();

    private boolean dibujarCajon = false;
    private int cantidadCajones = 0;
    private List<Rectangle> cajones = new ArrayList<>();

    private Map<String, List<double[]>> medidasElementos = new HashMap<>(); // Para guardar las medidas
    private JPanel panelDibujo; // Referencia al JPanel de la vista

    public DisenarModuloController() {
    }

    

    public DisenarModuloController(DisenarModuloView view) { // Constructor que recibe la vista
        this.vista = view;
        this.panelDibujo = vista.getjPanel2(); // Obtiene la referencia al JPanel
    }
    public void mostrar()
    {
        vista.setVisible(true);
    }
    public void ocultar()
    {
        vista.setVisible(false);
    }
    
    public boolean hayElementoEnEspacio(Rectangle espacio, List<Rectangle> elementos) {
        for (Rectangle elemento : elementos) {
            if (espacio.intersects(elemento)) {
                return true;
            }
        }
        return false;
    }

    public void colocarPuertas(Rectangle espacio, int numPuertas) {
        if (numPuertas == 1) {
            puertas.add(espacio);
        } else if (numPuertas == 2) {
            // Dividir el espacio en dos y colocar dos puertas
            int anchoMitad = espacio.width / 2;
            puertas.add(new Rectangle(espacio.x, espacio.y, anchoMitad, espacio.height));
            puertas.add(new Rectangle(espacio.x + anchoMitad, espacio.y, anchoMitad, espacio.height));
        }
    }

    public void colocarCajones(Rectangle espacio, int cantidad) {
        if (cantidad > 0) {
            int espacioCajon = espacio.height / cantidad;
            for (int i = 0; i < cantidad; i++) {
                cajones.add(new Rectangle(espacio.x, espacio.y + i * espacioCajon, espacio.width, espacioCajon));
            }
        }
    }

    public Rectangle encontrarSubespacio(int x, int y) {
        if (divisoriosHorizontales.isEmpty() && divisoriosVerticales.isEmpty()) {
            return new Rectangle(0, 0, anchoModulo, alturaModulo);
        }

        List<Integer> puntosX = new ArrayList<>();
        puntosX.add(0);
        for (Point p : divisoriosVerticales) {
            puntosX.add(p.x);
        }
        puntosX.add(anchoModulo);
        puntosX.sort(Integer::compareTo);

        List<Integer> puntosY = new ArrayList<>();
        puntosY.add(0);
        for (Point p : divisoriosHorizontales) {
            puntosY.add(p.y);
        }
        puntosY.add(alturaModulo);
        puntosY.sort(Integer::compareTo);

        for (int i = 0; i < puntosX.size() - 1; i++) {
            for (int j = 0; j < puntosY.size() - 1; j++) {
                int x1 = puntosX.get(i);
                int y1 = puntosY.get(j);
                int ancho = puntosX.get(i + 1) - x1;
                int alto = puntosY.get(j + 1) - y1;
                if (x >= x1 && x < x1 + ancho && y >= y1 && y < y1 + alto) {
                    return new Rectangle(x1, y1, ancho, alto);
                }
            }
        }
        return null;
    }

    public void agregarDivisorio(boolean horizontal) {
        try {
            String input = JOptionPane.showInputDialog(vista, "Ingrese la posición del divisorio:");
            if (input != null) { // Para evitar NullPointerException si el usuario cancela.
                int posicion = Integer.parseInt(input);
                if (horizontal) {
                    if (posicion > 0 && posicion < alturaModulo) {
                        divisoriosHorizontales.add(new Point(0, posicion));
                    } else {
                        JOptionPane.showMessageDialog(vista, "La posición del divisorio horizontal debe estar entre 0 y " + alturaModulo + ".");
                    }
                } else {
                    if (posicion > 0 && posicion < anchoModulo) {
                        divisoriosVerticales.add(new Point(posicion, 0));
                    } else {
                        JOptionPane.showMessageDialog(vista, "La posición del divisorio vertical debe estar entre 0 y " + anchoModulo + ".");
                    }
                }
            }
        } catch (NumberFormatException e) {
            JOptionPane.showMessageDialog(vista, "Ingrese un número válido para la posición del divisorio.");
        }
    }

    public void dibujarModuloConElementos() {
        Graphics2D g2d = (Graphics2D) panelDibujo.getGraphics();
        if (g2d == null) {
            return;
        }
        g2d.clearRect(0, 0, panelDibujo.getWidth(), panelDibujo.getHeight()); // Limpiar el jPanel2 antes de dibujar

    // Calcular escala
        escalaX = (float) panelDibujo.getWidth() / 2600;
        escalaY = (float) panelDibujo.getHeight() / 2600;

        int anchoDibujado = Math.round(anchoModulo * escalaX);
        int altoDibujado = Math.round(alturaModulo * escalaY);

        moduloX = (panelDibujo.getWidth() - anchoDibujado) / 2;
        moduloY = (panelDibujo.getHeight() - altoDibujado) / 2;

        g2d.setColor(Color.BLACK);
        g2d.setStroke(new BasicStroke(5));
        g2d.drawRect(moduloX, moduloY, anchoDibujado, altoDibujado);

    // Dibujar divisorios horizontales
    g2d.setColor(Color.BLACK);
    for (Point p : divisoriosHorizontales) {
        g2d.drawLine(moduloX, moduloY + (int) (p.y * escalaY), moduloX + (int) (anchoModulo * escalaX), moduloY + (int) (p.y * escalaY));
    }

    // Dibujar divisorios verticales
    for (Point p : divisoriosVerticales) {
        g2d.drawLine(moduloX + (int) (p.x * escalaX), moduloY, moduloX + (int) (p.x * escalaX), moduloY + (int) (alturaModulo * escalaY));
    }

    g2d.setColor(new Color(100, 100, 100));
        g2d.setStroke(new BasicStroke(2));
        for (Rectangle puerta : puertas) {
            int px = moduloX + Math.round(puerta.x * escalaX);
            int py = moduloY + Math.round(puerta.y * escalaY);
            int pw = Math.round(puerta.width * escalaX);
            int ph = Math.round(puerta.height * escalaY);
            g2d.drawRect(px, py, pw, ph);
             g2d.drawLine(px, py, px + pw, py + ph);
        }

        // Dibujar cajones
        g2d.setColor(new Color(150, 150, 200));
        g2d.setStroke(new BasicStroke(1));
        for (Rectangle cajon : cajones) {
            int cx = moduloX + Math.round(cajon.x * escalaX);
            int cy = moduloY + Math.round(cajon.y * escalaY);
            int cw = Math.round(cajon.width * escalaX);
            int ch = Math.round(cajon.height * escalaY);
            g2d.drawRect(cx, cy, cw, ch);
        }

        g2d.dispose();
    }

    public void dibujarModulo(int alto, int ancho, int profundo) {
        try {
            actualizarDimensiones(alto,ancho,profundo);
        } catch (NumberFormatException e) {
            JOptionPane.showMessageDialog(vista, "Por favor ingrese valores numéricos válidos.");
            return;
        }

        if (alto <= 0 || alto > MAX_ALTO || ancho <= 0 || ancho > MAX_ALTO || profundo <= 0 || profundo > MAX_PROFUNDIDAD) {
            JOptionPane.showMessageDialog(vista, "Las dimensiones superan los límites permitidos.\nAltura máxima: MAX_ALTO mm\nAncho máximo: MAX_ALTO mm");
            return;
        }

        Graphics2D g2d = (Graphics2D) panelDibujo.getGraphics();
        if (g2d == null) {
            return;
        }
        g2d.clearRect(0, 0, panelDibujo.getWidth(), panelDibujo.getHeight());

        escalaX = (float) panelDibujo.getWidth() / 2600;
        escalaY = (float) panelDibujo.getHeight() / 2600;

        int anchoDibujado = Math.round(ancho * escalaX);
        int altoDibujado = Math.round(alto * escalaY);

        moduloX = (panelDibujo.getWidth() - anchoDibujado) / 2;
        moduloY = (panelDibujo.getHeight() - altoDibujado) / 2;

        g2d.setColor(Color.BLACK);
        g2d.setStroke(new BasicStroke(5));
        g2d.drawRect(moduloX, moduloY, anchoDibujado, altoDibujado);

        g2d.dispose();
    }

    public void actualizarDimensiones(int altura, int ancho, int profundidad) {
        alturaModulo = altura;
        anchoModulo = ancho;
        profundidadModulo = profundidad;
    }

    private int contarCajonesEnSubespacio(Rectangle espacio) {
        int count = 0;
        for (Rectangle cajon : cajones) {
            if (espacio.intersects(cajon)) {
                count++;
            }
        }
        return count;
    }

    private void calcularMedidas() {
        medidasElementos.clear();

        // Medidas del módulo base
        List<double[]> moduloMedidas = new ArrayList<>();
        moduloMedidas.add(new double[]{0, 0, anchoModulo, alturaModulo, profundidadModulo}); // x, y, ancho, alto, profundidad
        medidasElementos.put("modulo", moduloMedidas);

        // Medidas de los divisorios
        List<double[]> divisoriosHMedidas = new ArrayList<>();
        for (Point p : divisoriosHorizontales) {
            divisoriosHMedidas.add(new double[]{0, p.y, anchoModulo, 0, profundidadModulo}); // x, y, ancho, alto, profundidad
        }
        medidasElementos.put("divisoriosHorizontales", divisoriosHMedidas);

        List<double[]> divisoriosVMedidas = new ArrayList<>();
        for (Point p : divisoriosVerticales) {
            divisoriosVMedidas.add(new double[]{p.x, 0, 0, alturaModulo, profundidadModulo}); // x, y, ancho, alto, profundidad
        }
        medidasElementos.put("divisoriosVerticales", divisoriosVMedidas);

        // Medidas de las puertas
        List<double[]> puertasMedidas = new ArrayList<>();
        for (Rectangle puerta : puertas) {
            puertasMedidas.add(new double[]{puerta.x, puerta.y, puerta.width, puerta.height, profundidadModulo}); // x, y, ancho, alto, profundidad
        }
        medidasElementos.put("puertas", puertasMedidas);

        // Medidas de los cajones
        List<double[]> cajonesMedidas = new ArrayList<>();
        for (Rectangle cajon : cajones) {
            cajonesMedidas.add(new double[]{cajon.x, cajon.y, cajon.width, cajon.height, profundidadModulo}); // x, y, ancho, alto, profundidad
        }
        medidasElementos.put("cajones", cajonesMedidas);
    }
}
