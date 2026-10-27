using System;

namespace RedElectricaMunicipal.Modelos
{
    /// <summary>
    /// Representa una línea de transmisión o distribución entre dos subestaciones.
    /// </summary>
    public class LineaDistribucion
    {
        public string CodigoOrigen { get; set; }
        public string CodigoDestino { get; set; }
        public double LongitudKm { get; set; }
        public double CostoMantenimiento { get; set; }
        public bool EsUnidireccional { get; set; }

        public LineaDistribucion(string origen, string destino, double longitudKm, double costo, bool esUnidireccional)
        {
            CodigoOrigen = origen.Trim().ToUpper();
            CodigoDestino = destino.Trim().ToUpper();
            LongitudKm = longitudKm;
            CostoMantenimiento = costo;
            EsUnidireccional = esUnidireccional;
        }

        public override string ToString()
        {
            string tipo = EsUnidireccional ? "--> (Unidireccional)" : "<-> (Bidireccional)";
            return $"{CodigoOrigen} {tipo} {CodigoDestino} | Longitud: {LongitudKm:F2} km | Costo Mant.: Q{CostoMantenimiento:F2}";
        }
    }
}