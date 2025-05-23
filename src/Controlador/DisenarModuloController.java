package Controlador;
import Vistas.DisenarModuloView;
import javax.swing.*;

import Modelo.Modulo;
import Modelo.Pedido;

import java.awt.*;
import java.awt.event.MouseEvent;
import java.sql.*;
import java.util.List;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Map;

public class DisenarModuloController {
    private DisenarModuloView vista;
    private Modulo mod;
    private Pedido ped = new Pedido();
    int idPed = ped.getId_pedido();
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

    private List<Rectangle> puertas = new ArrayList<>();
    private List<Rectangle> cajones = new ArrayList<>();
    private Map<String, List<double[]>> medidasElementos = new HashMap<>();
    private JPanel panelDibujo;

    private int cantidadModulos = 1;
    private int moduloActual = 1;
    private int idPedido = -1;

    public DisenarModuloController(DisenarModuloView vista, int idPedido, int cantidadModulos) {
        this.vista = vista;
        this.idPedido = idPedido;
        this.cantidadModulos = cantidadModulos;
        this.mod = new Modulo();
        this.panelDibujo = vista.getjPanel2();
    }

    public void actualizarDimensiones(int altura, int ancho, int profundidad) {
        if (mod == null) {
            mod = new Modulo();
        }
        this.alturaModulo = altura;
        this.anchoModulo = ancho;
        this.profundidadModulo = profundidad;
        mod.setAlto(altura);
        mod.setAncho(ancho);
        mod.setProfundo(profundidad);
        divisoriosHorizontales.clear();
        divisoriosVerticales.clear();
        puertas.clear();
        cajones.clear();
        calcularMedidas();
    }

    public int getModuloActual() {
    return moduloActual;
}
public int getCantidadModulos() {
    return cantidadModulos;
}

    public void agregarDivisorio(boolean horizontal, int posicion) {
        if (horizontal) {
            if (posicion > 0 && posicion < alturaModulo) {
                // Evitar duplicados
                for (Point p : divisoriosHorizontales) {
                    if (p.y == posicion) return;
                }
                divisoriosHorizontales.add(new Point(0, posicion));
            }
        } else {
            if (posicion > 0 && posicion < anchoModulo) {
                for (Point p : divisoriosVerticales) {
                    if (p.x == posicion) return;
                }
                divisoriosVerticales.add(new Point(posicion, 0));
            }
        }
        calcularMedidas();
    }

    // Encuentra el subespacio donde se hizo click
    public Rectangle encontrarSubespacio(int mouseX, int mouseY) {
        int x = (int) ((mouseX - moduloX) / escalaX);
        int y = (int) ((mouseY - moduloY) / escalaY);

        List<Integer> puntosX = new ArrayList<>();
        puntosX.add(0);
        for (Point p : divisoriosVerticales) puntosX.add(p.x);
        puntosX.add(anchoModulo);

        List<Integer> puntosY = new ArrayList<>();
        puntosY.add(0);
        for (Point p : divisoriosHorizontales) puntosY.add(p.y);
        puntosY.add(alturaModulo);

        int x1 = 0, y1 = 0, x2 = anchoModulo, y2 = alturaModulo;
        for (int i = 0; i < puntosX.size() - 1; i++) {
            if (x >= puntosX.get(i) && x < puntosX.get(i + 1)) {
                x1 = puntosX.get(i);
                x2 = puntosX.get(i + 1);
                break;
            }
        }
        for (int i = 0; i < puntosY.size() - 1; i++) {
            if (y >= puntosY.get(i) && y < puntosY.get(i + 1)) {
                y1 = puntosY.get(i);
                y2 = puntosY.get(i + 1);
                break;
            }
        }
        return new Rectangle(x1, y1, x2 - x1, y2 - y1);
    }

    public boolean hayElementoEnEspacio(Rectangle espacio, List<Rectangle> elementos) {
        for (Rectangle elemento : elementos) {
            if (espacio.equals(elemento)) return true;
        }
        return false;
    }

    public void manejarClickPuerta(int mouseX, int mouseY, int boton) {
        Rectangle subespacio = encontrarSubespacio(mouseX, mouseY);
        if (subespacio == null || hayElementoEnEspacio(subespacio, puertas)) return;

        if (boton == MouseEvent.BUTTON1) {
            puertas.add(subespacio);
        } else if (boton == MouseEvent.BUTTON3) {
            // Dos puertas: dividir el subespacio en dos verticalmente
            int mitad = subespacio.width / 2;
            puertas.add(new Rectangle(subespacio.x, subespacio.y, mitad, subespacio.height));
            puertas.add(new Rectangle(subespacio.x + mitad, subespacio.y, subespacio.width - mitad, subespacio.height));
        }
        calcularMedidas();
        vista.actualizarLabelPuertas(puertas.size());
        dibujarModuloConElementos();
    }

    public void manejarClickCajon(int mouseX, int mouseY, int cantidad) {
    Rectangle subespacio = encontrarSubespacio(mouseX, mouseY);
    if (subespacio == null || hayElementoEnEspacio(subespacio, cajones)) return;

    int altoCajon = subespacio.height / cantidad;
    for (int i = 0; i < cantidad; i++) {
        Rectangle cajon = new Rectangle(subespacio.x, subespacio.y + i * altoCajon, subespacio.width, altoCajon);
        cajones.add(cajon);
    }
    calcularMedidas();
    vista.actualizarLabelCajones(cajones.size());
    dibujarModuloConElementos();
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

    private void calcularMedidas() {
        medidasElementos.clear();
        // Módulo base
        List<double[]> moduloMedidas = new ArrayList<>();
        moduloMedidas.add(new double[]{0, 0, anchoModulo, alturaModulo, profundidadModulo});
        medidasElementos.put("modulo", moduloMedidas);

        // Divisorios horizontales
        List<double[]> divH = new ArrayList<>();
        for (Point p : divisoriosHorizontales) {
            divH.add(new double[]{0, p.y, anchoModulo - 36, profundidadModulo});
        }
        medidasElementos.put("divisoriosHorizontales", divH);

        // Divisorios verticales
        List<double[]> divV = new ArrayList<>();
        for (Point p : divisoriosVerticales) {
            divV.add(new double[]{p.x, 0, alturaModulo - 36, profundidadModulo});
        }
        medidasElementos.put("divisoriosVerticales", divV);

        // Puertas
        List<double[]> puertasMedidas = new ArrayList<>();
        for (Rectangle puerta : puertas) {
            double anchoP = puerta.width - 6;
            double altoP = puerta.height - 6;
            puertasMedidas.add(new double[]{puerta.x, puerta.y, anchoP, altoP});
        }
        medidasElementos.put("puertas", puertasMedidas);

        // Cajones
        List<double[]> cajonesMedidas = new ArrayList<>();
        for (Rectangle cajon : cajones) {
            double baseAncho = cajon.width - 62;
            double baseProf = profundidadModulo - 40;
            double frenteAlto = (cajon.height) - 30;
            double lateralLargo = baseAncho - 36;
            double tapaAncho = cajon.width - (cajon.height) - 5;
            cajonesMedidas.add(new double[]{cajon.x, cajon.y, baseAncho, baseProf, frenteAlto, lateralLargo, tapaAncho});
        }
        medidasElementos.put("cajones", cajonesMedidas);
    }

    public Map<String, List<double[]>> getMedidasElementos() {
        return medidasElementos;
    }

public void iniciarCicloDiseno() {
        moduloActual = 1;
        vista.mostrarMensajeModuloActual(moduloActual, cantidadModulos);
    }

public void terminarModuloYContinuar() {
    // Guardar módulo y componentes en la BD
    int idModulo = guardarModuloEnBD();
    if (idModulo != -1) {
        guardarComponentesEnBD(idModulo);
    }

    if (moduloActual < cantidadModulos) {
        moduloActual++;
        vista.mostrarMensajeModuloActual(moduloActual, cantidadModulos);
        limpiarVistaParaNuevoModulo();
    } else {
        vista.finalizarCicloDiseno();
    }
}

private int guardarModuloEnBD() {
    int idModulo = -1;
    try (Connection conn = DriverManager.getConnection("jdbc:mysql://localhost/woodicbase", "root", "")) {
        String sql = "INSERT INTO Modulo (ANCHO, ALTO, PROFUNDO, PEDIDO_idPEDIDO) VALUES (?, ?, ?, ?)";
        PreparedStatement ps = conn.prepareStatement(sql, Statement.RETURN_GENERATED_KEYS);
        ps.setInt(1, mod.getAncho());
        ps.setInt(2, mod.getAlto());
        ps.setInt(3, mod.getProfundo());
        ps.setInt(4, idPedido);
        ps.executeUpdate();
        ResultSet rs = ps.getGeneratedKeys();
        if (rs.next()) {
            idModulo = rs.getInt(1);
        }
        rs.close();
        ps.close();
    } catch (SQLException e) {
        vista.mostrarError("Error al guardar módulo: " + e.getMessage());
    }
    return idModulo;
}

private void guardarComponentesEnBD(int idModulo) {
    Map<String, List<double[]>> medidas = getMedidasElementos();
    try (Connection conn = DriverManager.getConnection("jdbc:mysql://localhost/woodicbase", "root", "")) {
        String sql = "INSERT INTO Componente (NOMBRECOMPONENTE, ANCHOCOMP, LARGOCOMP, CANTIDADCOMP, MODULO_idMODULO) VALUES (?, ?, ?, ?, ?)";
        PreparedStatement ps = conn.prepareStatement(sql);
        for (Map.Entry<String, List<double[]>> entry : medidas.entrySet()) {
            String nombre = entry.getKey();
            List<double[]> lista = entry.getValue();
            for (double[] datos : lista) {
                int ancho = (int) Math.round(datos.length > 2 ? datos[2] : 0);
                int largo = (int) Math.round(datos.length > 3 ? datos[3] : 0);
                int cantidad = 1; // Ajusta según lógica de tu app
                ps.setString(1, nombre);
                ps.setInt(2, ancho);
                ps.setInt(3, largo);
                ps.setInt(4, cantidad);
                ps.setInt(5, idModulo);
                ps.addBatch();
            }
        }
        ps.executeBatch();
        ps.close();
    } catch (SQLException e) {
        vista.mostrarError("Error al guardar componentes: " + e.getMessage());
    }
}

private void limpiarVistaParaNuevoModulo() {
    actualizarDimensiones(1, 1, 1);
    dibujarModuloConElementos();
    vista.limpiarCamposModulo();
    vista.actualizarLabelPuertas(0);
    vista.actualizarLabelCajones(0);
}
}
