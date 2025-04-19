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
    public CrearPedidoController(CrearPedidoView vista, Placa modeloPlaca) {
    this.vista = vista;
    this.modeloPlaca = modeloPlaca;
}

    public void guardar() {
        try{
            Connection c = DriverManager.getConnection("jdbc:mysql://localhost/woodic", "root", "");
            System.out.println("Conectado a la base de datos");

            String nombre = vista.getjTextField1().getText();
            String direccion = vista.getjTextField3().getText();
            int telefono = Integer.parseInt(vista.getjTextField2().getText());

            String linea = vista.getjComboBox1().getSelectedItem().toString();
            String color = vista.getjComboBox2().getSelectedItem().toString();
            int cantidadMod = Integer.parseInt(vista.getjTextField4().getText());
            int precio = 0; // temporal

            PreparedStatement psCliente = c.prepareStatement("INSERT IGNORE INTO Cliente (nombreApellido, direccion, contacto) VALUES (?, ?, ?)");
            psCliente.setString(1, nombre);
            psCliente.setString(2, direccion);
            psCliente.setInt(3, telefono);
            psCliente.executeUpdate();


            PreparedStatement psBuscarPlaca = c.prepareStatement("SELECT id_placa FROM Placa WHERE color = ?");
            psBuscarPlaca.setString(1, color);
            ResultSet rsPlaca = psBuscarPlaca.executeQuery();

            int id_placa = 0;
            if (rsPlaca.next()) {
                id_placa = rsPlaca.getInt("id_placa");
            } else {
                JOptionPane.showMessageDialog(null, "No se encontró una placa con ese color.");
                return;
            }

            LocalDateTime fecha = LocalDateTime.now();
            PreparedStatement psPedido = c.prepareStatement("INSERT INTO Pedido (id_cliente, color, precio, cantidadmod, fecha) VALUES (?, ?, ?, ?, ?)");
            psPedido.setInt(1, telefono);
            psPedido.setInt(2, id_placa);
            psPedido.setInt(3, precio);
            psPedido.setInt(4, cantidadMod);
            psPedido.setTimestamp(5, Timestamp.valueOf(fecha));
            psPedido.executeUpdate();

            JOptionPane.showMessageDialog(null, "Pedido guardado con éxito");

        } catch (Exception e) {
            e.printStackTrace();
            JOptionPane.showMessageDialog(null, "Error al guardar el pedido: " + e.getMessage());
        }
    }

    public void cargarLinea() {
    String opcion = modeloPlaca.getCompuesto();
    vista.getjComboBox1().removeAllItems();
    
    try {
        Connection c = DriverManager.getConnection("jdbc:mysql://localhost/woodic", "root", "");
        PreparedStatement ps = c.prepareStatement("SELECT linea FROM Placa WHERE compuesto = ?");
        ps.setString(1, opcion);
        ResultSet rs = ps.executeQuery();

        while (rs.next()) {
            vista.getjComboBox1().addItem(rs.getString("linea"));
        }

        rs.close();
        ps.close();
        c.close();
        
    } catch (Exception e) {
        e.printStackTrace();
        JOptionPane.showMessageDialog(null, "Error al cargar linea: " + e.getMessage());
    }
}
    
    public void cargarColor() {
    String opcion = vista.getjComboBox1().getSelectedItem().toString();
    vista.getjComboBox2().removeAllItems();
    
    try {
        Connection c = DriverManager.getConnection("jdbc:mysql://localhost/woodic", "root", "");
        PreparedStatement ps = c.prepareStatement("SELECT color FROM Placa WHERE linea = ?");
        ps.setString(1, opcion);
        ResultSet rs = ps.executeQuery();

        while (rs.next()) {
            vista.getjComboBox2().addItem(rs.getString("color"));
        }

        rs.close();
        ps.close();
        c.close();
        
    } catch (Exception e) {
        e.printStackTrace();
        JOptionPane.showMessageDialog(null, "Error al cargar colores: " + e.getMessage());
    }
}
    
}