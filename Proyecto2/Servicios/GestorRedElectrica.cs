using System;
using System.IO;
using System.Globalization;
using RedElectricaMunicipal.Modelos;
using RedElectricaMunicipal.Estructuras;

namespace RedElectricaMunicipal.Servicios
{
    // Coordina de forma integrada la Tabla Hash, el Trie, el Union-Find y el Grafo,
    // además de gestionar la lectura de archivos CSV y generación de reportes.
    public class GestorRedElectrica
    {
        public TablaHashSubestaciones TablaHash { get; }
        public TrieSubestaciones TrieNombres { get; }
        public UnionFindZonas UnionFind { get; }
        public GrafoRedElectrica Grafo { get; }

        public GestorRedElectrica()
        {
            TablaHash = new TablaHashSubestaciones();
            TrieNombres = new TrieSubestaciones();
            UnionFind = new UnionFindZonas();
            Grafo = new GrafoRedElectrica();
        }

        // Registra una subestación sincronizando Tabla Hash, Trie, Grafo y Union-Find.
        public bool RegistrarSubestacion(string codigo, string nombre, double capacidadMW)
        {
            Subestacion sub = new Subestacion(codigo, nombre, capacidadMW);
            bool esNueva = TablaHash.Insertar(sub);
            TrieNombres.Insertar(sub.Nombre, sub.Codigo);
            Grafo.AgregarVertice(sub.Codigo);
            UnionFind.RegistrarSubestacion(sub.Codigo);
            return esNueva;
        }

        // Elimina una subestación de la Tabla Hash y del Trie.
        public bool EliminarSubestacion(string codigo)
        {
            Subestacion existente = TablaHash.Buscar(codigo);
            if (existente == null) return false;

            TrieNombres.Eliminar(existente.Nombre);
            return TablaHash.Eliminar(codigo);
        }

        // Verifica con Union-Find si dos subestaciones ya pertenecen a la misma zona eléctrica
        // y registra la línea en el Grafo y en el Union-Find.
        public bool RegistrarLinea(string origen, string destino, double longitudKm, double costo, bool unidireccional, out string mensajeZona)
        {
            mensajeZona = "";
            Subestacion subOrigen = TablaHash.Buscar(origen);
            Subestacion subDestino = TablaHash.Buscar(destino);

            if (subOrigen == null || subDestino == null)
            {
                mensajeZona = "Error: Ambas subestaciones deben existir en la Tabla Hash antes de conectar una línea.";
                return false;
            }

            bool mismaZona = UnionFind.PertenecenAMismaZona(subOrigen.Codigo, subDestino.Codigo);
            if (mismaZona)
            {
                mensajeZona = $"AVISO UNION-FIND: [{subOrigen.Codigo}] y [{subDestino.Codigo}] YA pertenecen a la misma zona eléctrica conectada (se creará una línea de respaldo/redundancia).";
            }
            else
            {
                mensajeZona = $"AVISO UNION-FIND: [{subOrigen.Codigo}] y [{subDestino.Codigo}] pertenecían a zonas eléctricas separadas. Ahora han quedado unificadas.";
            }

            LineaDistribucion nuevaLinea = new LineaDistribucion(subOrigen.Codigo, subDestino.Codigo, longitudKm, costo, unidireccional);
            Grafo.AgregarLinea(nuevaLinea);
            UnionFind.Union(subOrigen.Codigo, subDestino.Codigo);
            return true;
        }

        // Carga subestaciones y líneas desde archivos CSV validando su ubicación y existencia.
        public void CargarDatosDesdeCSV(string rutaSubestaciones, string rutaLineas)
        {
            Console.WriteLine("\n CARGA DE ARCHIVOS DE DATOS (.CSV)");

            if (!File.Exists(rutaSubestaciones))
            {
                Console.WriteLine($"[!] El archivo de subestaciones no existe en la ruta: {Path.GetFullPath(rutaSubestaciones)}");
            }
            else
            {
                string[] lineasSub = File.ReadAllLines(rutaSubestaciones);
                int contSub = 0;
                for (int i = 0; i < lineasSub.Length; i++)
                {
                    string fila = lineasSub[i].Trim();
                    if (string.IsNullOrEmpty(fila) || fila.StartsWith("#")) continue;

                    string[] partes = fila.Split(new char[] { ';', ',' });
                    if (partes.Length >= 3 && double.TryParse(partes[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double cap))
                    {
                        RegistrarSubestacion(partes[0], partes[1], cap);
                        contSub++;
                    }
                }
                Console.WriteLine($"Se cargaron {contSub} subestaciones desde '{rutaSubestaciones}'.");
            }

            if (!File.Exists(rutaLineas))
            {
                Console.WriteLine($"El archivo de líneas no existe en la ruta: {Path.GetFullPath(rutaLineas)}");
            }
            else
            {
                string[] lineasRed = File.ReadAllLines(rutaLineas);
                int contLin = 0;
                for (int i = 0; i < lineasRed.Length; i++)
                {
                    string fila = lineasRed[i].Trim();
                    if (string.IsNullOrEmpty(fila) || fila.StartsWith("#")) continue;

                    string[] partes = fila.Split(new char[] { ';', ',' });
                    if (partes.Length >= 5 &&
                        double.TryParse(partes[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double km) &&
                        double.TryParse(partes[3].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double costo) &&
                        bool.TryParse(partes[4].Trim(), out bool uni))
                    {
                        if (RegistrarLinea(partes[0], partes[1], km, costo, uni, out _))
                            contLin++;
                    }
                }
                Console.WriteLine($"Se cargaron {contLin} líneas de distribución desde '{rutaLineas}'.");
            }
        }

        // Asistente interactivo que sugiere nombres en tiempo real desde el Trie mientras el usuario escribe.
        public string SeleccionarSubestacionConAutocompletado(string etiqueta)
        {
            Console.WriteLine($"\nEscriba el nombre de la subestación ({etiqueta}).");
            Console.WriteLine("Use las sugerencias en vivo y presione [ENTER] para elegir la #1, o escriba el código directamente:");
            string buffer = "";

            while (true)
            {
                Console.Write($"\r> Búsqueda: {buffer}                              ");
                EntradaSugerencia[] sugerencias = TrieNombres.BuscarPorPrefijo(buffer, 5);

                Console.WriteLine("\n  Sugerencias del Trie:");
                for (int i = 0; i < 5; i++)
                {
                    if (i < sugerencias.Length)
                        Console.WriteLine($"    [{i + 1}] {sugerencias[i].NombreSubestacion} ({sugerencias[i].CodigoSubestacion})          ");
                    else
                        Console.WriteLine("                                                                      ");
                }

                Console.SetCursorPosition(12 + buffer.Length, Console.CursorTop - 6);
                ConsoleKeyInfo tecla = Console.ReadKey(true);

                if (tecla.Key == ConsoleKey.Enter)
                {
                    Console.SetCursorPosition(0, Console.CursorTop + 6);
                    if (sugerencias.Length > 0)
                    {
                        Console.WriteLine($"Seleccionado: {sugerencias[0].NombreSubestacion} [{sugerencias[0].CodigoSubestacion}]");
                        return sugerencias[0].CodigoSubestacion;
                    }
                    // Si escribió directamente un código válido (ej. SUB-01)
                    Subestacion directa = TablaHash.Buscar(buffer);
                    if (directa != null) return directa.Codigo;

                    Console.WriteLine("No se encontró coincidencia.");
                    return null;
                }
                else if (tecla.Key == ConsoleKey.Backspace)
                {
                    if (buffer.Length > 0)
                        buffer = buffer.Substring(0, buffer.Length - 1);
                }
                else if (tecla.Key >= ConsoleKey.D1 && tecla.Key <= ConsoleKey.D5)
                {
                    int idx = (int)tecla.Key - (int)ConsoleKey.D1;
                    if (idx < sugerencias.Length)
                    {
                        Console.SetCursorPosition(0, Console.CursorTop + 6);
                        Console.WriteLine($"Seleccionado #{idx + 1}: {sugerencias[idx].NombreSubestacion} [{sugerencias[idx].CodigoSubestacion}]");
                        return sugerencias[idx].CodigoSubestacion;
                    }
                }
                else if (!char.IsControl(tecla.KeyChar))
                {
                    buffer += tecla.KeyChar;
                }
            }
        }

        // Ejecuta el Flujo Integrado Obligatorio (Requisito Técnico #2):
        // 1. Trie sugiere nombre/código -> 2. Tabla Hash recupera fichas completas ->
        // 3. Union-Find valida conectividad de zona -> 4. Grafo calcula Dijkstra de menor costo.
        public void EjecutarFlujoIntegradoRestablecimiento()
        {
            Console.WriteLine("   FLUJO INTEGRADO: RESTABLECIMIENTO DE SUMINISTRO (4 ESTRUCTURAS)    ");

            string codOrigen = SeleccionarSubestacionConAutocompletado("ORIGEN");
            if (codOrigen == null) return;

            string codDestino = SeleccionarSubestacionConAutocompletado("DESTINO");
            if (codDestino == null) return;

            Console.WriteLine("\n[PASO 2 - TABLA HASH] Localizando registros técnicos en O(1)...");
            Subestacion subOrigen = TablaHash.Buscar(codOrigen);
            Subestacion subDestino = TablaHash.Buscar(codDestino);
            Console.WriteLine($"  -> Origen : {subOrigen}");
            Console.WriteLine($"  -> Destino: {subDestino}");

            Console.WriteLine("\n[PASO 3 - UNION-FIND] Verificando conectividad de zona eléctrica...");
            if (!UnionFind.PertenecenAMismaZona(codOrigen, codDestino))
            {
                Console.WriteLine($"  -> [ALERTA] Las subestaciones [{codOrigen}] y [{codDestino}] pertenecen a ZONAS ELÉCTRICAS AISLADAS.");
                Console.WriteLine("  -> No es posible restablecer el suministro sin antes construir una línea de interconexión (no se ejecuta Dijkstra).");
                return;
            }
            Console.WriteLine("  -> Ambas subestaciones pertenecen a la misma zona eléctrica conectada.");

            Console.WriteLine("\n[PASO 4 - GRAFO (DIJKSTRA)] Calculando ruta óptima de menor costo de mantenimiento...");
            bool exito = Grafo.CalcularRutaMinimaDijkstra(
                codOrigen,
                codDestino,
                out string[] ruta,
                out double costoTotal,
                out double kmTotal);

            if (!exito)
            {
                Console.WriteLine("  -> Aunque están en la misma zona eléctrica, el sentido unidireccional de las líneas impide el flujo en esta dirección.");
                return;
            }

            Console.WriteLine("\n--- RUTA ÓPTIMA PARA RESTABLECER EL SUMINISTRO ---");
            for (int i = 0; i < ruta.Length; i++)
            {
                Subestacion info = TablaHash.Buscar(ruta[i]);
                string flecha = (i < ruta.Length - 1) ? " ==Line==> " : "";
                Console.Write($"[{info.Codigo}: {info.Nombre}]{flecha}");
            }
            Console.WriteLine($"\n  * Longitud acumulada        : {kmTotal:F2} km");
            Console.WriteLine($"  * Costo total mantenimiento : Q{costoTotal:F2}");
        }

        // Genera un reporte ejecutivo en archivo de texto para apoyo a la toma de decisiones (Competencia CE9).
        public void GenerarReporteEstrategico(string rutaReporte)
        {
            Subestacion[] todas = TablaHash.ObtenerTodas();
            double capacidadTotal = 0;
            for (int i = 0; i < todas.Length; i++) capacidadTotal += todas[i].CapacidadMW;

            using (StreamWriter sw = new StreamWriter(rutaReporte))
            {
                sw.WriteLine("================================================================");
                sw.WriteLine(" REPORTE EJECUTIVO DE LA RED ELÉCTRICA MUNICIPAL");
                sw.WriteLine($" Fecha de generación: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sw.WriteLine("================================================================");
                sw.WriteLine($"Total de Subestaciones Registradas : {todas.Length}");
                sw.WriteLine($"Capacidad Instalada Total          : {capacidadTotal:F2} MW");
                sw.WriteLine("----------------------------------------------------------------");
                sw.WriteLine("DETALLE DE SUBESTACIONES:");
                for (int i = 0; i < todas.Length; i++)
                {
                    sw.WriteLine($" - {todas[i]}");
                }
                sw.WriteLine("================================================================");
            }
            Console.WriteLine($"Reporte estratégico generado exitosamente en: {Path.GetFullPath(rutaReporte)}");
        }
    }
}