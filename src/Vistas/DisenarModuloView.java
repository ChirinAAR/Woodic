/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/GUIForms/JFrame.java to edit this template
 */
package Vistas;
import Controlador.*;
import javax.swing.*;
import java.awt.*;
import java.awt.event.MouseAdapter;
import java.awt.event.MouseEvent;
import java.util.ArrayList;
import java.util.List;
import java.util.HashMap;
import java.util.Map;
/**
 *
 * @author Alumno
 */
public class DisenarModuloView extends javax.swing.JFrame {

    private DisenarModuloController diseno;
    private FinalViewController lista;
    private CrearPedidoController pedilo;
    private MenuPrincipalController menu;

    private int alturaModulo = 1;
    private int anchoModulo = 1;
    private int profundidadModulo;
    private static final int MAX_ALTO = 2600;
    private static final int MAX_ANCHO = 2600;
    private static final int MAX_PROFUNDIDAD = 1830;

    private int moduloX;
    private int moduloY;
    private float escalaX = 1;
    private float escalaY = 1;private List<Point> divisoriosHorizontales = new ArrayList<>();
    private List<Point> divisoriosVerticales = new ArrayList<>();

    private boolean dibujarPuerta = false;
    private List<Rectangle> puertas = new ArrayList<>();

    private boolean dibujarCajon = false;
    private int cantidadCajones = 0;
    private List<Rectangle> cajones = new ArrayList<>();

    private Map<String, List<double[]>> medidasElementos = new HashMap<>(); // Para guardar las medidas


    
    public DisenarModuloView() {
    initComponents();

   // ActionListener para el botón Aceptar (dibujar módulo base)
        jButton9.addActionListener(e -> {
            try {
                alturaModulo = Integer.parseInt(jTextField1.getText());
                anchoModulo = Integer.parseInt(jTextField2.getText());
                profundidadModulo = Integer.parseInt(jTextField3.getText());
                if (alturaModulo <= 0 || alturaModulo > MAX_ALTO || anchoModulo <= 0 || anchoModulo > MAX_ALTO || profundidadModulo <= 0 || profundidadModulo > MAX_PROFUNDIDAD) {
                    JOptionPane.showMessageDialog(this, "Las dimensiones superan los límites permitidos.");
                    return;
                }
                dibujarModuloConElementos();
            } catch (NumberFormatException ex) {
                JOptionPane.showMessageDialog(this, "Ingrese valores numéricos válidos.");
            }
        });

        jButton1.addActionListener(e -> {
            agregarDivisorio(true); // Llama al método agregarDivisorio
            dibujarModuloConElementos(); // Redibuja el módulo con el divisorio
        });

        // Modificado para dibujar automáticamente el divisorio
        jButton2.addActionListener(e -> {
            agregarDivisorio(false); // Llama al método agregarDivisorio
            dibujarModuloConElementos(); // Redibuja el módulo con el divisorio
        });

        jButton5.addActionListener(e -> {
            if (jCheckBox3.isSelected()) {
                dibujarPuerta = true;
                jPanel2.setCursor(new Cursor(Cursor.HAND_CURSOR));
            } else {
                JOptionPane.showMessageDialog(this, "Debe seleccionar la opción 'Puertas'.");
                dibujarPuerta = false;
                jPanel2.setCursor(new Cursor(Cursor.DEFAULT_CURSOR));
            }
        });

        jButton6.addActionListener(e -> {
            if (jCheckBox4.isSelected()) {
                try {
                    cantidadCajones = Integer.parseInt(jTextField4.getText());
                    if (cantidadCajones > 0) {
                        dibujarCajon = true;
                        jPanel2.setCursor(new Cursor(Cursor.HAND_CURSOR));
                    } else {
                        JOptionPane.showMessageDialog(this, "Ingrese una cantidad válida de cajones.");
                    }
                } catch (NumberFormatException ex) {
                    JOptionPane.showMessageDialog(this, "Ingrese un número entero para la cantidad de cajones.");
                }
            } else {
                JOptionPane.showMessageDialog(this, "Debe seleccionar la opción 'Cajones'.");
                dibujarCajon = false;
                jPanel2.setCursor(new Cursor(Cursor.DEFAULT_CURSOR));
            }
        });

        jPanel2.addMouseListener(new MouseAdapter() {
            @Override
            public void mouseClicked(MouseEvent e) {
                int clickX = e.getX();
                int clickY = e.getY();
                int moduloRelX = clickX - moduloX;
                int moduloRelY = clickY - moduloY;

                if (dibujarPuerta) {
                    Rectangle espacio = encontrarSubespacio(moduloRelX, moduloRelY);
                    if (espacio != null && !hayElementoEnEspacio(espacio, puertas)) {
                        int numPuertas = (e.getButton() == MouseEvent.BUTTON1) ? 1 : (e.getButton() == MouseEvent.BUTTON3) ? 2 : 0;
                        colocarPuertas(espacio, numPuertas);
                        dibujarModuloConElementos();
                        jLabel11.setText("Total: " + puertas.size());
                    } else if (espacio == null) {
                        JOptionPane.showMessageDialog(DisenarModuloView.this, "Debe hacer clic dentro de un subespacio para colocar la puerta.");
                    } else {
                        JOptionPane.showMessageDialog(DisenarModuloView.this, "Ya hay una puerta en este subespacio.");
                    }
                    dibujarPuerta = false;
                    jPanel2.setCursor(new Cursor(Cursor.DEFAULT_CURSOR));
                } else if (dibujarCajon) {
                    Rectangle espacio = encontrarSubespacio(moduloRelX, moduloRelY);
                    if (espacio != null && !hayElementoEnEspacio(espacio, cajones)) {
                        colocarCajones(espacio, cantidadCajones);
                        dibujarModuloConElementos();
                        jLabel10.setText("Total: " + cajones.size());
                    } else if (espacio == null) {
                        JOptionPane.showMessageDialog(DisenarModuloView.this, "Debe hacer clic dentro de un subespacio para colocar los cajones.");
                    } else {
                        JOptionPane.showMessageDialog(DisenarModuloView.this, "Ya hay cajones en este subespacio.");
                    }
                    dibujarCajon = false;
                    cantidadCajones = 0;
                    jTextField4.setText("");
                    jPanel2.setCursor(new Cursor(Cursor.DEFAULT_CURSOR));
                }
            }  
        });
}
    

    /**
     * This method is called from within the constructor to initialize the form.
     * WARNING: Do NOT modify this code. The content of this method is always
     * regenerated by the Form Editor.
     */
    @SuppressWarnings("unchecked")
    // <editor-fold defaultstate="collapsed" desc="Generated Code">//GEN-BEGIN:initComponents
    private void initComponents() {

        jPanel1 = new javax.swing.JPanel();
        jLabel1 = new javax.swing.JLabel();
        jLabel2 = new javax.swing.JLabel();
        jLabel3 = new javax.swing.JLabel();
        jLabel4 = new javax.swing.JLabel();
        jTextField1 = new javax.swing.JTextField();
        jTextField2 = new javax.swing.JTextField();
        jTextField3 = new javax.swing.JTextField();
        jLabel5 = new javax.swing.JLabel();
        jLabel6 = new javax.swing.JLabel();
        jLabel7 = new javax.swing.JLabel();
        jLabel8 = new javax.swing.JLabel();
        jCheckBox1 = new javax.swing.JCheckBox();
        jButton1 = new javax.swing.JButton();
        jButton2 = new javax.swing.JButton();
        jCheckBox2 = new javax.swing.JCheckBox();
        jButton3 = new javax.swing.JButton();
        jButton4 = new javax.swing.JButton();
        jCheckBox3 = new javax.swing.JCheckBox();
        jButton5 = new javax.swing.JButton();
        jCheckBox4 = new javax.swing.JCheckBox();
        jButton6 = new javax.swing.JButton();
        jLabel9 = new javax.swing.JLabel();
        jTextField4 = new javax.swing.JTextField();
        jLabel10 = new javax.swing.JLabel();
        jLabel11 = new javax.swing.JLabel();
        jButton7 = new javax.swing.JButton();
        jButton8 = new javax.swing.JButton();
        jPanel2 = new javax.swing.JPanel();
        jLabel12 = new javax.swing.JLabel();
        jButton9 = new javax.swing.JButton();

        setDefaultCloseOperation(javax.swing.WindowConstants.EXIT_ON_CLOSE);

        jPanel1.setBackground(new java.awt.Color(0, 51, 102));
        jPanel1.setForeground(new java.awt.Color(0, 51, 102));

        jLabel1.setBackground(new java.awt.Color(255, 255, 255));
        jLabel1.setFont(new java.awt.Font("Segoe UI", 3, 18)); // NOI18N
        jLabel1.setForeground(new java.awt.Color(255, 255, 255));
        jLabel1.setText("Ingrese Medidas");

        jLabel2.setForeground(new java.awt.Color(255, 255, 255));
        jLabel2.setText("Altura");

        jLabel3.setForeground(new java.awt.Color(255, 255, 255));
        jLabel3.setText("Ancho");

        jLabel4.setForeground(new java.awt.Color(255, 255, 255));
        jLabel4.setText("Profundidad");

        jLabel5.setForeground(new java.awt.Color(255, 255, 255));
        jLabel5.setText("mm");

        jLabel6.setForeground(new java.awt.Color(255, 255, 255));
        jLabel6.setText("mm");

        jLabel7.setForeground(new java.awt.Color(255, 255, 255));
        jLabel7.setText("mm");

        jLabel8.setBackground(new java.awt.Color(255, 255, 255));
        jLabel8.setFont(new java.awt.Font("Segoe UI", 3, 18)); // NOI18N
        jLabel8.setForeground(new java.awt.Color(255, 255, 255));
        jLabel8.setText("Opciones");

        jCheckBox1.setForeground(new java.awt.Color(255, 255, 255));
        jCheckBox1.setText("Divisorios");

        jButton1.setText("Añadir Divisorio Horizontal");
        jButton1.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                jButton1ActionPerformed(evt);
            }
        });

        jButton2.setText("Añadir Divisorio Vertical");
        jButton2.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                jButton2ActionPerformed(evt);
            }
        });

        jCheckBox2.setForeground(new java.awt.Color(255, 255, 255));
        jCheckBox2.setText("Banquinas");

        jButton3.setText("Añadir Banquina Horizontal");

        jButton4.setText("Añadir Banquina Vertical");

        jCheckBox3.setForeground(new java.awt.Color(255, 255, 255));
        jCheckBox3.setText("Puertas");

        jButton5.setText("Seleccionar Ubicacion");
        jButton5.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                jButton5ActionPerformed(evt);
            }
        });

        jCheckBox4.setForeground(new java.awt.Color(255, 255, 255));
        jCheckBox4.setText("Cajones");

        jButton6.setText("Seleccionar Ubicacion");

        jLabel9.setForeground(new java.awt.Color(255, 255, 255));
        jLabel9.setText("Cantidad:");

        jLabel10.setForeground(new java.awt.Color(255, 255, 255));
        jLabel10.setText("Total:");

        jLabel11.setForeground(new java.awt.Color(255, 255, 255));
        jLabel11.setText("Total:");

        jButton7.setBackground(new java.awt.Color(153, 0, 0));
        jButton7.setForeground(new java.awt.Color(255, 255, 255));
        jButton7.setText("Salir");
        jButton7.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                jButton7ActionPerformed(evt);
            }
        });

        jButton8.setText("Terminar");
        jButton8.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                jButton8ActionPerformed(evt);
            }
        });

        javax.swing.GroupLayout jPanel2Layout = new javax.swing.GroupLayout(jPanel2);
        jPanel2.setLayout(jPanel2Layout);
        jPanel2Layout.setHorizontalGroup(
            jPanel2Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGap(0, 0, Short.MAX_VALUE)
        );
        jPanel2Layout.setVerticalGroup(
            jPanel2Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGap(0, 0, Short.MAX_VALUE)
        );

        jLabel12.setForeground(new java.awt.Color(255, 255, 255));
        jLabel12.setText("Vista Previa Modulo");

        jButton9.setText("Aceptar");
        jButton9.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                jButton9ActionPerformed(evt);
            }
        });

        javax.swing.GroupLayout jPanel1Layout = new javax.swing.GroupLayout(jPanel1);
        jPanel1.setLayout(jPanel1Layout);
        jPanel1Layout.setHorizontalGroup(
            jPanel1Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(jPanel1Layout.createSequentialGroup()
                .addContainerGap()
                .addGroup(jPanel1Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
                    .addGroup(jPanel1Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING, false)
                        .addComponent(jLabel1, javax.swing.GroupLayout.PREFERRED_SIZE, 159, javax.swing.GroupLayout.PREFERRED_SIZE)
                        .addGroup(jPanel1Layout.createSequentialGroup()
                            .addComponent(jLabel4)
                            .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                            .addComponent(jTextField3)
                            .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                            .addComponent(jLabel7))
                        .addGroup(jPanel1Layout.createSequentialGroup()
                            .addComponent(jLabel2)
                            .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED)
                            .addComponent(jTextField1, javax.swing.GroupLayout.PREFERRED_SIZE, 67, javax.swing.GroupLayout.PREFERRED_SIZE)
                            .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                            .addComponent(jLabel6))
                        .addGroup(jPanel1Layout.createSequentialGroup()
                            .addComponent(jLabel3)
                            .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                            .addComponent(jTextField2, javax.swing.GroupLayout.PREFERRED_SIZE, 70, javax.swing.GroupLayout.PREFERRED_SIZE)
                            .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                            .addComponent(jLabel5))
                        .addComponent(jCheckBox1)
                        .addComponent(jButton1, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                        .addComponent(jLabel8, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                        .addComponent(jButton2, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                        .addComponent(jCheckBox2)
                        .addComponent(jButton3, javax.swing.GroupLayout.DEFAULT_SIZE, 183, Short.MAX_VALUE)
                        .addComponent(jButton4, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                        .addComponent(jButton5, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                        .addComponent(jButton6, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                        .addGroup(jPanel1Layout.createSequentialGroup()
                            .addComponent(jButton7)
                            .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                            .addComponent(jButton8, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE))
                        .addComponent(jButton9)
                        .addGroup(javax.swing.GroupLayout.Alignment.TRAILING, jPanel1Layout.createSequentialGroup()
                            .addComponent(jLabel9)
                            .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                            .addComponent(jTextField4, javax.swing.GroupLayout.PREFERRED_SIZE, 81, javax.swing.GroupLayout.PREFERRED_SIZE)
                            .addGap(41, 41, 41)))
                    .addGroup(jPanel1Layout.createSequentialGroup()
                        .addComponent(jCheckBox4)
                        .addGap(18, 18, 18)
                        .addComponent(jLabel10))
                    .addGroup(jPanel1Layout.createSequentialGroup()
                        .addComponent(jCheckBox3)
                        .addGap(18, 18, 18)
                        .addComponent(jLabel11)))
                .addGroup(jPanel1Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
                    .addGroup(jPanel1Layout.createSequentialGroup()
                        .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED, 211, Short.MAX_VALUE)
                        .addComponent(jLabel12)
                        .addGap(203, 203, 203))
                    .addGroup(jPanel1Layout.createSequentialGroup()
                        .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED)
                        .addComponent(jPanel2, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                        .addContainerGap())))
        );
        jPanel1Layout.setVerticalGroup(
            jPanel1Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addGroup(jPanel1Layout.createSequentialGroup()
                .addContainerGap()
                .addGroup(jPanel1Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
                    .addComponent(jLabel1)
                    .addComponent(jLabel12, javax.swing.GroupLayout.Alignment.TRAILING))
                .addGroup(jPanel1Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
                    .addGroup(jPanel1Layout.createSequentialGroup()
                        .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED)
                        .addGroup(jPanel1Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE)
                            .addComponent(jLabel2)
                            .addComponent(jTextField1, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)
                            .addComponent(jLabel6))
                        .addGap(18, 18, 18)
                        .addGroup(jPanel1Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE)
                            .addComponent(jLabel3)
                            .addComponent(jTextField2, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)
                            .addComponent(jLabel5))
                        .addGap(18, 18, 18)
                        .addGroup(jPanel1Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE)
                            .addComponent(jLabel4)
                            .addComponent(jTextField3, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)
                            .addComponent(jLabel7))
                        .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED, 22, Short.MAX_VALUE)
                        .addComponent(jButton9)
                        .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                        .addComponent(jLabel8)
                        .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                        .addComponent(jCheckBox1)
                        .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                        .addComponent(jButton1)
                        .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                        .addComponent(jButton2)
                        .addGap(32, 32, 32)
                        .addComponent(jCheckBox2)
                        .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                        .addComponent(jButton3)
                        .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                        .addComponent(jButton4)
                        .addGap(36, 36, 36)
                        .addGroup(jPanel1Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE)
                            .addComponent(jCheckBox3)
                            .addComponent(jLabel11))
                        .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                        .addComponent(jButton5)
                        .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                        .addGroup(jPanel1Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE)
                            .addComponent(jCheckBox4)
                            .addComponent(jLabel10))
                        .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                        .addGroup(jPanel1Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE)
                            .addComponent(jTextField4, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)
                            .addComponent(jLabel9))
                        .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                        .addComponent(jButton6))
                    .addGroup(jPanel1Layout.createSequentialGroup()
                        .addGap(3, 3, 3)
                        .addComponent(jPanel2, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)))
                .addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
                .addGroup(jPanel1Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE)
                    .addComponent(jButton7)
                    .addComponent(jButton8))
                .addContainerGap())
        );

        javax.swing.GroupLayout layout = new javax.swing.GroupLayout(getContentPane());
        getContentPane().setLayout(layout);
        layout.setHorizontalGroup(
            layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addComponent(jPanel1, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
        );
        layout.setVerticalGroup(
            layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
            .addComponent(jPanel1, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
        );

        pack();
    }// </editor-fold>//GEN-END:initComponents

    private boolean hayElementoEnEspacio(Rectangle espacio, List<Rectangle> elementos) {
        for (Rectangle elemento : elementos) {
            if (espacio.intersects(elemento)) return true;
        }
        return false;
    }

    private Rectangle encontrarSubespacio(int x, int y) {
        int inicioX = 0;
        int inicioY = 0;
        int finX = anchoModulo;
        int finY = alturaModulo;

        // Encuentra los límites verticales
        int izquierda = 0;
        for (Point pv : divisoriosVerticales) {
            if (x >= pv.x && pv.x >= izquierda) {
                izquierda = pv.x;
            }
        }
        int derecha = anchoModulo;
        for (Point pv : divisoriosVerticales) {
            if (x <= pv.x && pv.x <= derecha) {
                derecha = pv.x;
            }
        }

        // Encuentra los límites horizontales
        int arriba = 0;
        for (Point ph : divisoriosHorizontales) {
            if (y >= ph.y && ph.y >= arriba) {
                arriba = ph.y;
            }
        }
        int abajo = alturaModulo;
        for (Point ph : divisoriosHorizontales) {
            if (y <= ph.y && ph.y <= abajo) {
                abajo = ph.y;
            }
        }

        // Verifica si el punto está dentro de los límites definidos por los divisores
        if (x >= izquierda && x <= derecha && y >= arriba && y <= abajo) {
            return new Rectangle(izquierda, arriba, derecha - izquierda, abajo - arriba);
        }
        return null; // Retorna null si no se encuentra un subespacio
    }

    

    private void colocarPuertas(Rectangle espacio, int cantidad) {
         if (cantidad == 1) {
            puertas.add(new Rectangle(espacio.x, espacio.y, espacio.width, espacio.height));
        } else if (cantidad == 2) {
            if (espacio.width > espacio.height) {
                int anchoPuerta = espacio.width / 2;
                puertas.add(new Rectangle(espacio.x, espacio.y, anchoPuerta, espacio.height));
                puertas.add(new Rectangle(espacio.x + anchoPuerta, espacio.y, anchoPuerta, espacio.height));
            } else {
                int altoPuerta = espacio.height / 2;
                puertas.add(new Rectangle(espacio.x, espacio.y, espacio.width, altoPuerta));
                puertas.add(new Rectangle(espacio.x, espacio.y + altoPuerta, espacio.width, altoPuerta));
            }
        }
    }

    private void colocarCajones(Rectangle espacio, int cantidad) {
        int altoCajon = espacio.height / cantidad;
        for (int i = 0; i < cantidad; i++) {
            cajones.add(new Rectangle(espacio.x, espacio.y + i * altoCajon, espacio.width, altoCajon));
        }
    }

    private void agregarDivisorio(boolean horizontal) {
        try {
            String input = JOptionPane.showInputDialog(this, "Ingrese la posición del divisorio:");
            if (input != null) { // Para evitar NullPointerException si el usuario cancela.
                int posicion = Integer.parseInt(input);
                if (horizontal) {
                    if (posicion > 0 && posicion < alturaModulo) {
                         divisoriosHorizontales.add(new Point(0, posicion));
                    } else {
                        JOptionPane.showMessageDialog(this, "La posición del divisorio horizontal debe estar entre 0 y " + alturaModulo + ".");
                    }
                } else {
                    if (posicion > 0 && posicion < anchoModulo) {
                         divisoriosVerticales.add(new Point(posicion, 0));
                    } else {
                        JOptionPane.showMessageDialog(this, "La posición del divisorio vertical debe estar entre 0 y " + anchoModulo + ".");
                    }
                }
            }
        } catch (NumberFormatException e) {
            JOptionPane.showMessageDialog(this, "Ingrese un número válido para la posición del divisorio.");
        }
    }
    private void dibujarModuloConElementos() {
    Graphics2D g2d = (Graphics2D) jPanel2.getGraphics();
        if (g2d == null) {
            return;
        }
        g2d.clearRect(0, 0, jPanel2.getWidth(), jPanel2.getHeight()); // Limpiar el jPanel2 antes de dibujar

    // Calcular escala
    escalaX = (float) jPanel2.getWidth() / 2600;
        escalaY = (float) jPanel2.getHeight() / 2600;

        int anchoDibujado = Math.round(anchoModulo * escalaX);
        int altoDibujado = Math.round(alturaModulo * escalaY);

        moduloX = (jPanel2.getWidth() - anchoDibujado) / 2;
        moduloY = (jPanel2.getHeight() - altoDibujado) / 2;

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

    public List<String> calcularMedidas() {
        List<String> medidas = new ArrayList<>();

        // Módulo base
        medidas.add("Cabezal = (" + anchoModulo + ", " + profundidadModulo + ")");
        medidas.add("Lateral = (" + (alturaModulo - 36) + ", " + profundidadModulo + ")");
        medidas.add("Lateral = (" + (alturaModulo - 36) + ", " + profundidadModulo + ")");

        // Divisorios horizontales
        for (Point ph : divisoriosHorizontales) {
             Rectangle subespacio = encontrarSubespacio(anchoModulo / 2, ph.y);
             if (subespacio != null)
             {
                 medidas.add("Divisorio Horizontal = (" + (subespacio.width - 36) + ", " + profundidadModulo + ")");
             }
             else
             {
                  medidas.add("Divisorio Horizontal = (" + (anchoModulo - 36) + ", " + profundidadModulo + ")");
             }
        }

        // Divisorios verticales
        for (Point pv : divisoriosVerticales) {
             Rectangle subespacio = encontrarSubespacio(pv.x, alturaModulo/2);
             if(subespacio != null){
                 medidas.add("Divisorio Vertical = (" + (subespacio.height - 36) + ", " + profundidadModulo + ")");
             }
             else{
                 medidas.add("Divisorio Vertical = (" + (alturaModulo - 36) + ", " + profundidadModulo + ")");
             }
        }

        // Puertas
        for (Rectangle puerta : puertas) {
             Rectangle subespacioContenedor = encontrarSubespacio(puerta.x + puerta.width / 2, puerta.y + puerta.height / 2);
             if (subespacioContenedor != null)
             {
                  medidas.add("Puerta = (" + (subespacioContenedor.width - 6) + ", " + (subespacioContenedor.height - 6) + ")");
             }
             else{
                  medidas.add("Puerta = (" + (anchoModulo - 6) + ", " + (alturaModulo - 6) + ")");
             }

        }

        // Cajones
        for (Rectangle cajon : cajones) {
            Rectangle subespacioContenedor = encontrarSubespacio(cajon.x + cajon.width / 2, cajon.y + cajon.height / 2);
            int cantidadCajonesEnSubespacio = contarCajonesEnSubespacio(subespacioContenedor);
             if (subespacioContenedor != null)
             {
                  medidas.add("Cajon Base = (" + (subespacioContenedor.width - 62) + ", " + (profundidadModulo - 40) + ")");
                  medidas.add("Cajon Frente = (" + (subespacioContenedor.width - 62) + ", " + ((subespacioContenedor.height / cantidadCajonesEnSubespacio) - 30) + ")");
                  medidas.add("Cajon Lateral = (" + ((subespacioContenedor.width - 62) - 36) + ", " + ((subespacioContenedor.height / cantidadCajonesEnSubespacio) - 30) + ")");
                  medidas.add("Cajon Tapa = (" + (subespacioContenedor.width - (subespacioContenedor.height/ cantidadCajonesEnSubespacio) - 5) + ")");
             }
             else{
                  medidas.add("Cajon Base = (" + (anchoModulo - 62) + ", " + (profundidadModulo - 40) + ")");
                  medidas.add("Cajon Frente = (" + (anchoModulo - 62) + ", " + ((alturaModulo / cantidadCajonesEnSubespacio) - 30) + ")");
                  medidas.add("Cajon Lateral = (" + ((anchoModulo - 62) - 36) + ", " + ((alturaModulo / cantidadCajonesEnSubespacio) - 30) + ")");
                  medidas.add("Cajon Tapa = (" + (anchoModulo - (alturaModulo / cantidadCajonesEnSubespacio) - 5) + ")");
             }
        }
        return medidas;
    }

    private int contarCajonesEnSubespacio(Rectangle subespacio) {
        int count = 0;
        for (Rectangle cajon : cajones) {
            if (subespacio.intersects(cajon)) {
                count++;
            }
        }
        return count;
    }

    private void mostrarErrorMedidas() {
        JOptionPane.showMessageDialog(this, "Ingrese medidas válidas para el módulo (Altura, Ancho, Profundidad).", "Error", JOptionPane.ERROR_MESSAGE);
    }

     private void actualizarDimensiones(int altura, int ancho, int profundidad) throws NumberFormatException {
        alturaModulo = Integer.parseInt(jTextField1.getText());
        anchoModulo = Integer.parseInt(jTextField2.getText());
        profundidadModulo = Integer.parseInt(jTextField3.getText());
    }

    private void dibujarModulo(int alto, int ancho, int profundidad) {
        try {
            actualizarDimensiones(alto,ancho,profundidad);
        } catch (NumberFormatException e) {
            JOptionPane.showMessageDialog(this, "Por favor ingrese valores numéricos válidos.");
            return;
        }

        if (alto <= 0 || alto > MAX_ALTO || ancho <= 0 || ancho > MAX_ALTO || profundidad <= 0 || profundidad > MAX_PROFUNDIDAD) {
            JOptionPane.showMessageDialog(this, "Las dimensiones superan los límites permitidos.\nAltura máxima: MAX_ALTO mm\nAncho máximo: MAX_ALTO mm");
            return;
        }

        Graphics2D g2d = (Graphics2D) jPanel2.getGraphics();
        if (g2d == null) {
            return;
        }
        g2d.clearRect(0, 0, jPanel2.getWidth(), jPanel2.getHeight());

        escalaX = (float) jPanel2.getWidth() / 2600;
        escalaY = (float) jPanel2.getHeight() / 2600;

        int anchoDibujado = Math.round(ancho * escalaX);
        int altoDibujado = Math.round(alto * escalaY);

        moduloX = (jPanel2.getWidth() - anchoDibujado) / 2;
        moduloY = (jPanel2.getHeight() - altoDibujado) / 2;

        g2d.setColor(Color.BLACK);
        g2d.setStroke(new BasicStroke(5));
        g2d.drawRect(moduloX, moduloY, anchoDibujado, altoDibujado);

        g2d.dispose();
    }
    private void jButton7ActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_jButton7ActionPerformed
        this.setVisible(false);
    }//GEN-LAST:event_jButton7ActionPerformed

    private void jButton8ActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_jButton8ActionPerformed
        this.dispose();
        lista.mostrar();
    }//GEN-LAST:event_jButton8ActionPerformed

    private void jButton9ActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_jButton9ActionPerformed
        actualizarDimensiones(alturaModulo, anchoModulo, profundidadModulo);
        dibujarModulo(alturaModulo,anchoModulo,profundidadModulo);
    }//GEN-LAST:event_jButton9ActionPerformed

    private void jButton1ActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_jButton1ActionPerformed

    }//GEN-LAST:event_jButton1ActionPerformed

    private void jButton2ActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_jButton2ActionPerformed

    }//GEN-LAST:event_jButton2ActionPerformed

    private void jButton5ActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_jButton5ActionPerformed

    }//GEN-LAST:event_jButton5ActionPerformed

    /**
     * @param args the command line arguments
     */
    public static void main(String args[]) {
        /* Set the Nimbus look and feel */
        //<editor-fold defaultstate="collapsed" desc=" Look and feel setting code (optional) ">
        /* If Nimbus (introduced in Java SE 6) is not available, stay with the default look and feel.
         * For details see http://download.oracle.com/javase/tutorial/uiswing/lookandfeel/plaf.html 
         */
        try {
            for (javax.swing.UIManager.LookAndFeelInfo info : javax.swing.UIManager.getInstalledLookAndFeels()) {
                if ("Nimbus".equals(info.getName())) {
                    javax.swing.UIManager.setLookAndFeel(info.getClassName());
                    break;
                }
            }
        } catch (ClassNotFoundException ex) {
            java.util.logging.Logger.getLogger(DisenarModuloView.class.getName()).log(java.util.logging.Level.SEVERE, null, ex);
        } catch (InstantiationException ex) {
            java.util.logging.Logger.getLogger(DisenarModuloView.class.getName()).log(java.util.logging.Level.SEVERE, null, ex);
        } catch (IllegalAccessException ex) {
            java.util.logging.Logger.getLogger(DisenarModuloView.class.getName()).log(java.util.logging.Level.SEVERE, null, ex);
        } catch (javax.swing.UnsupportedLookAndFeelException ex) {
            java.util.logging.Logger.getLogger(DisenarModuloView.class.getName()).log(java.util.logging.Level.SEVERE, null, ex);
        }
        //</editor-fold>
        //</editor-fold>

        /* Create and display the form */
        java.awt.EventQueue.invokeLater(new Runnable() {
            public void run() {
                new DisenarModuloView().setVisible(true);
            }
        });
    }

    // Variables declaration - do not modify//GEN-BEGIN:variables
    private javax.swing.JButton jButton1;
    private javax.swing.JButton jButton2;
    private javax.swing.JButton jButton3;
    private javax.swing.JButton jButton4;
    private javax.swing.JButton jButton5;
    private javax.swing.JButton jButton6;
    private javax.swing.JButton jButton7;
    private javax.swing.JButton jButton8;
    private javax.swing.JButton jButton9;
    private javax.swing.JCheckBox jCheckBox1;
    private javax.swing.JCheckBox jCheckBox2;
    private javax.swing.JCheckBox jCheckBox3;
    private javax.swing.JCheckBox jCheckBox4;
    private javax.swing.JLabel jLabel1;
    private javax.swing.JLabel jLabel10;
    private javax.swing.JLabel jLabel11;
    private javax.swing.JLabel jLabel12;
    private javax.swing.JLabel jLabel2;
    private javax.swing.JLabel jLabel3;
    private javax.swing.JLabel jLabel4;
    private javax.swing.JLabel jLabel5;
    private javax.swing.JLabel jLabel6;
    private javax.swing.JLabel jLabel7;
    private javax.swing.JLabel jLabel8;
    private javax.swing.JLabel jLabel9;
    private javax.swing.JPanel jPanel1;
    private javax.swing.JPanel jPanel2;
    private javax.swing.JTextField jTextField1;
    private javax.swing.JTextField jTextField2;
    private javax.swing.JTextField jTextField3;
    private javax.swing.JTextField jTextField4;
    // End of variables declaration//GEN-END:variables

    public JTextField getjTextField1() {
        return jTextField1;
    }

    public void setjTextField1(JTextField jTextField1) {
        this.jTextField1 = jTextField1;
    }

    public JTextField getjTextField2() {
        return jTextField2;
    }

    public void setjTextField2(JTextField jTextField2) {
        this.jTextField2 = jTextField2;
    }

    public JTextField getjTextField3() {
        return jTextField3;
    }

    public void setjTextField3(JTextField jTextField3) {
        this.jTextField3 = jTextField3;
    }

}
