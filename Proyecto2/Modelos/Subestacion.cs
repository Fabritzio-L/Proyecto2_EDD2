using System;

namespace RedElectricaMunicipal.Modelos
{
    /// Representa una subestación eléctrica municipal con código único, nombre y capacidad.
    public class Subestacion
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public double CapacidadMW { get; set; }

        public Subestacion(string codigo, string nombre, double capacidadMW)
        {
            Codigo = codigo.Trim().ToUpper();
            Nombre = nombre.Trim();
            CapacidadMW = capacidadMW;
        }

        public override string ToString()
        {
            return $"[{Codigo}] {Nombre} | Capacidad: {CapacidadMW:F2} MW";
        }
    }
}