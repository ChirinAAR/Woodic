using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Woodic.Modelo
{
    public class Placa
    {
        private int id_placa;
        private string linea;
        private string color;
        private string compuesto;
        private int precio;
        private bool beta;
        private int ancho;
        private int largo;
        public Placa() {
        }
        public int getLargo() {
            return largo;
        }
        public void setLargo(int largo) {
            this.largo = largo;
        }
        public int getId_placa(){
            return id_placa;
        }
        public void setId_placa(int id_placa){
            this.id_placa = id_placa;
        }

        public string getLinea(){
            return linea;
        }
        public void setLinea(string linea){
            this.linea = linea;
        }
        public string getColor(){
            return color;
        }
        public void setColor(string color){
            this.color = color;
        }
        public string getCompuesto(){
            return compuesto;
        }
        public void setCompuesto(string compuesto){
            this.compuesto = compuesto;
        }
        public int getPrecio(){
            return precio;
        }
        public void setPrecio(int precio){
            this.precio = precio;
        }
        public bool isBeta(){
            return beta;
        }
        public void setBeta(bool beta){
            this.beta = beta;
        }
        public int getAncho(){
            return ancho;
        }
        public void setAncho(int ancho){
            this.ancho = ancho;
        }
    }
}
