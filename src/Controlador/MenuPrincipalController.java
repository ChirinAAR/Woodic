
package Controlador;
import Vistas.*;

public class MenuPrincipalController {
    MenuPrincipalView menu = new MenuPrincipalView();
    public void mostrar()
    {
        menu.setVisible(true);
    }
    public void ocultar()
    {
        menu.setVisible(false);
    }
}
