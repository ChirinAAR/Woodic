/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package Controlador;

import Modelo.*;
import Vistas.CrearPedidoView;
import java.sql.*;
import javax.swing.JOptionPane;

public class CrearPedidoController {

    private final CrearPedidoView vista;

    public CrearPedidoController(CrearPedidoView vista) {
        this.vista = vista;
    }

    /**
     * Guarda el pedido y el cliente en la base de datos.
     *
     * @param pedido El pedido a guardar.
     * @return El id generado para el pedido, o -1 si hubo error.
     */
    public int guardar(Pedido pedido) {
        int idPedido = -1;
        Cliente cliente = pedido.getCliente();
        try (Connection conn = DriverManager.getConnection("jdbc:mysql://localhost/woodicbase", "root", "")) {
            // Guardar cliente (si no existe)
            String sqlCliente = "INSERT IGNORE INTO Cliente (NOMBRE, DIRECCION, CONTACTO) VALUES (?, ?, ?)";
            try (PreparedStatement psCliente = conn.prepareStatement(sqlCliente)) {
                psCliente.setString(1, cliente.getNombre());
                psCliente.setString(2, cliente.getDireccion());
                psCliente.setInt(3, cliente.getContacto());
                psCliente.executeUpdate();
            }

            // Guardar pedido
            String sqlPedido = "INSERT INTO Pedido (CANTIDADMODULOS, PRECIO, CLIENTE_CONTACTO) VALUES (?, ?, ?)";
            try (PreparedStatement psPedido = conn.prepareStatement(sqlPedido, Statement.RETURN_GENERATED_KEYS)) {
                psPedido.setInt(1, pedido.getCantidadModulos());
                psPedido.setInt(2, pedido.getPrecio());
                psPedido.setInt(3, cliente.getContacto());
                psPedido.executeUpdate();
                ResultSet rs = psPedido.getGeneratedKeys();
                if (rs.next()) {
                    idPedido = rs.getInt(1);
                }
                rs.close();
            }
        } catch (SQLException e) {
            JOptionPane.showMessageDialog(vista, "Error al guardar pedido o cliente: " + e.getMessage());
        }
        return idPedido;
    }

    /**
     * Carga las líneas disponibles según el compuesto seleccionado en la vista.
     */
    public void cargarLinea() {
        String compuesto = vista.jCheckBox1.isSelected() ? "Aglomerado" : "MDF";
        vista.getjComboBox1().removeAllItems();
        try (Connection c = DriverManager.getConnection("jdbc:mysql://localhost/woodicbase", "root", "");
             PreparedStatement ps = c.prepareStatement("SELECT DISTINCT LINEA FROM PLACA WHERE COMPUESTO = ?")) {
            ps.setString(1, compuesto);
            ResultSet rs = ps.executeQuery();
            while (rs.next()) {
                vista.getjComboBox1().addItem(rs.getString("LINEA"));
            }
        } catch (SQLException e) {
            JOptionPane.showMessageDialog(vista, "Error al cargar líneas: " + e.getMessage());
        }
    }

    /**
     * Carga los colores disponibles según la línea seleccionada en la vista.
     */
    public void cargarColor() {
        String lineaSeleccionada = (String) vista.getjComboBox1().getSelectedItem();
        vista.getjComboBox2().removeAllItems();
        if (lineaSeleccionada == null) return;
        try (Connection c = DriverManager.getConnection("jdbc:mysql://localhost/woodicbase", "root", "");
             PreparedStatement ps = c.prepareStatement("SELECT COLOR FROM PLACA WHERE LINEA = ?")) {
            ps.setString(1, lineaSeleccionada);
            ResultSet rs = ps.executeQuery();
            while (rs.next()) {
                vista.getjComboBox2().addItem(rs.getString("COLOR"));
            }
        } catch (SQLException e) {
            JOptionPane.showMessageDialog(vista, "Error al cargar colores: " + e.getMessage());
        }
    }
}