/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package Controlador;
import Vistas.*;
import Modelo.*;
import java.sql.*;
import java.time.LocalDateTime;
import javax.swing.JOptionPane;


public class CrearPedidoController {
    private CrearPedidoView vista;
    private Placa modeloPlaca;

    public CrearPedidoController() {
    }
    
    public CrearPedidoController(CrearPedidoView vista, Placa modeloPlaca) {
    this.vista = vista;
    this.modeloPlaca = modeloPlaca;
}

    public void mostrar()
    {
        vista.setVisible(true);
    }
    public void ocultar()
    {
        vista.setVisible(false);
    }
    
    public void guardar(Placa placa) {
    String nombre = vista.getjTextField1().getText();
    int contacto = Integer.parseInt(vista.getjTextField2().getText());
    String direccion = vista.getjTextField3().getText();
    String compuesto = placa.getCompuesto();  // ya se setea al seleccionar checkbox
    String linea = (String) vista.getjComboBox1().getSelectedItem();
    String color = (String) vista.getjComboBox2().getSelectedItem();
    int cantidad = Integer.parseInt(vista.getjTextField4().getText());

    try {
        cantidad = Integer.parseInt(vista.getjTextField4().getText());
    } catch (NumberFormatException e) {
        JOptionPane.showMessageDialog(null, "Cantidad inválida. Debe ser un número.");
        return;
    }

    // Intentar guardar en la base de datos
    try (Connection conn = DriverManager.getConnection("jdbc:mysql://localhost/woodicbase", "root", "");
         PreparedStatement ps = conn.prepareStatement(
             "INSERT INTO CLIENTE (CONTACTO, NOMBRE, DIRECCION) VALUES (?, ?, ?)")) {

        ps.setInt(1, contacto);
        ps.setString(2, nombre);
        ps.setString(3, direccion);


    } catch (SQLException e) {
        e.printStackTrace();
        JOptionPane.showMessageDialog(null, "Error al guardar datos del cliente:\n" + e.getMessage());
    }

}

    public void cargarLinea() {
    String compuesto = modeloPlaca.getCompuesto();
    vista.getjComboBox1().removeAllItems();

    try (Connection c = DriverManager.getConnection("jdbc:mysql://localhost/woodicbase", "root", "");
         PreparedStatement ps = c.prepareStatement("SELECT LINEA FROM PLACA WHERE COMPUESTO = ?")) {
        
        ps.setString(1, compuesto);
        ResultSet rs = ps.executeQuery();

        while (rs.next()) {
            vista.getjComboBox1().addItem(rs.getString("LINEA"));
        }

    } catch (Exception e) {
        e.printStackTrace();
        JOptionPane.showMessageDialog(null, "Error al cargar linea: " + e.getMessage());
    }
}

    
    public void cargarColor() {
    String lineaSeleccionada = (String) vista.getjComboBox1().getSelectedItem();
    vista.getjComboBox2().removeAllItems();

    try (Connection c = DriverManager.getConnection("jdbc:mysql://localhost/woodicbase", "root", "");
         PreparedStatement ps = c.prepareStatement("SELECT COLOR FROM PLACA WHERE LINEA = ?")) {

        ps.setString(1, lineaSeleccionada);
        ResultSet rs = ps.executeQuery();

        while (rs.next()) {
            vista.getjComboBox2().addItem(rs.getString("COLOR"));
        }

    } catch (Exception e) {
        e.printStackTrace();
        JOptionPane.showMessageDialog(null, "Error al cargar colores: " + e.getMessage());
    }
}
    
}