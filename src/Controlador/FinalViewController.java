/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package Controlador;
import Vistas.*;

public class FinalViewController {
    FinalView vista = new FinalView();
    public void mostrar()
    {
        vista.setVisible(true);
    }
    public void ocultar(){
        vista.setVisible(false);
    }
}
