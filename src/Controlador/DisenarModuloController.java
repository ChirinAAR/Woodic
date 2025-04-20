
package Controlador;
import Vistas.DisenarModuloView;
/**
 *
 * @author Alumno
 */
public class DisenarModuloController {
    DisenarModuloView vista = new DisenarModuloView();
    public void mostrar()
    {
        vista.setVisible(true);
    }
    public void ocultar()
    {
        vista.setVisible(false);
    }
}
