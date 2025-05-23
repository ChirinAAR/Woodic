/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package Controlador;
import Vistas.*;
import Modelo.*;
import java.sql.*;
import java.util.*;

public class FinalViewController {
    public List<Map<String, List<double[]>>> obtenerListaDespiece(int idPedido) {
        List<Map<String, List<double[]>>> listaDespiece = new ArrayList<>();
        // Lista de nombres de componentes esperados
        String[] nombresComponentes = {
            "Zocalo", "Cabezal", "Laterales", "Div Horizontal", "Div Vertical",
            "Banq Horizontal", "Banq Vertical", "Base Cajon", "Frente Cajon",
            "Lateral Cajon", "Tapa Cajon", "Puertas"
        };

        try (Connection conn = DriverManager.getConnection("jdbc:mysql://localhost/woodicbase", "root", "")) {
            String sqlModulos = "SELECT idMODULO FROM Modulo WHERE PEDIDO_idPEDIDO = ? ORDER BY idMODULO";
            PreparedStatement psMod = conn.prepareStatement(sqlModulos);
            psMod.setInt(1, idPedido);
            ResultSet rsMod = psMod.executeQuery();

            List<Integer> modulosIds = new ArrayList<>();
            while (rsMod.next()) {
                modulosIds.add(rsMod.getInt("idMODULO"));
            }
            rsMod.close();
            psMod.close();

            if (modulosIds.isEmpty()) {
                return listaDespiece;
            }

            for (Integer idModulo : modulosIds) {
                Map<String, List<double[]>> despieceModulo = new HashMap<>();
                // Inicializa todas las claves con listas vacías
                for (String nombre : nombresComponentes) {
                    despieceModulo.put(nombre, new ArrayList<>());
                }
                String sqlComp = "SELECT NOMBRECOMPONENTE, ANCHOCOMP, LARGOCOMP, CANTIDADCOMP FROM Componente WHERE MODULO_idMODULO = ?";
                try (PreparedStatement psComp = conn.prepareStatement(sqlComp)) {
                    psComp.setInt(1, idModulo);
                    ResultSet rsComp = psComp.executeQuery();
                    while (rsComp.next()) {
                        String nombre = rsComp.getString("NOMBRECOMPONENTE");
                        int ancho = rsComp.getInt("ANCHOCOMP");
                        int largo = rsComp.getInt("LARGOCOMP");
                        int cantidad = rsComp.getInt("CANTIDADCOMP");
                        double[] datos = new double[] { ancho, largo, cantidad };
                        // Solo agrega si el nombre está en la lista esperada
                        if (despieceModulo.containsKey(nombre)) {
                            despieceModulo.get(nombre).add(datos);
                        }
                    }
                    rsComp.close();
                }
                listaDespiece.add(despieceModulo);
            }
        } catch (SQLException e) {
            e.printStackTrace();
        }
        return listaDespiece;
    }
}
