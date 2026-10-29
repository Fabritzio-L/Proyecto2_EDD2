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
            if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nombre))
                return false;

            // 1. Verificar si ya existe una subestación con ese código en la Tabla Hash
            Subestacion existente = TablaHash.Buscar(codigo);
            if (existente != null)
            {
                // Eliminar el nombre antiguo del Trie para que no quede duplicado
                TrieNombres.Eliminar(existente.Nombre);
            }

            // 2. Crear y registrar la subestación actualizada en todas las estructuras
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
            Grafo.DesconectarVertice(existente.Codigo);
            Grafo.SincronizarUnionFind(UnionFind);
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
                mensajeZona = $"AVISO UNION-FIND: [{subOrigen.Codigo}] y [{subDestino.Codigo}] ya pertenecen a la misma zona eléctrica conectada (se creará una línea de respaldo.";
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
                Console.WriteLine($"El archivo de subestaciones no existe en la ruta: {Path.GetFullPath(rutaSubestaciones)}");
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
                Console.WriteLine($"Se cargaron {contSub} subestaciones desde {rutaSubestaciones}.");
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
                Console.WriteLine($"Se cargaron {contLin} líneas de distribución desde {rutaLineas}.");
            }
        }

        // Asistente interactivo que sugiere nombres en tiempo real desde el Trie mientras el usuario escribe.
        public string SeleccionarSubestacionConAutocompletado(string etiqueta)
        {
            Console.WriteLine($"\nEscriba el nombre de la subestación ({etiqueta}).");
            Console.WriteLine("Use las sugerencias en vivo (1-5) o escriba el código:");

            string buffer = "";
            const int ANCHO_LIMPIEZA = 76;

            while (true)
            {
                EntradaSugerencia[] sugerencias = TrieNombres.BuscarPorPrefijo(buffer, 5);

                // 1. Posicionar al inicio de la línea actual y dibujar la barra de búsqueda
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.WriteLine($"> Búsqueda: {buffer}".PadRight(ANCHO_LIMPIEZA));

                // 2. Dibujar encabezado de sugerencias 
                Console.WriteLine("  Sugerencias del Trie:".PadRight(ANCHO_LIMPIEZA));

                // 3. Dibujar exactamente las 5 filas de sugerencias 
                for (int i = 0; i < 5; i++)
                {
                    if (i < sugerencias.Length)
                    {
                        string textoSug = $"    [{i + 1}] {sugerencias[i].NombreSubestacion} ({sugerencias[i].CodigoSubestacion})";
                        Console.WriteLine(textoSug.PadRight(ANCHO_LIMPIEZA));
                    }
                    else if (i == 0 && sugerencias.Length == 0)
                    {
                        Console.WriteLine("(Sin coincidencias de prefijo en el Trie)".PadRight(ANCHO_LIMPIEZA));
                    }
                    else
                    {
                        Console.WriteLine("".PadRight(ANCHO_LIMPIEZA));
                    }
                }

                int filaBusqueda = Console.CursorTop - 7;
                Console.SetCursorPosition(12 + buffer.Length, filaBusqueda);

                ConsoleKeyInfo tecla = Console.ReadKey(true);

                if (tecla.Key == ConsoleKey.Escape)
                {
                    Console.SetCursorPosition(0, filaBusqueda + 7);
                    Console.WriteLine("Búsqueda cancelada.");
                    return null;
                }
                else if (tecla.Key == ConsoleKey.Enter)
                {
                    Console.SetCursorPosition(0, filaBusqueda + 7);
                    if (sugerencias.Length > 0)
                    {
                        Console.WriteLine($"Seleccionado: {sugerencias[0].NombreSubestacion} [{sugerencias[0].CodigoSubestacion}]");
                        return sugerencias[0].CodigoSubestacion;
                    }

                    // Si escribió directamente un código válido
                    Subestacion directa = TablaHash.Buscar(buffer);
                    if (directa != null)
                    {
                        Console.WriteLine($"Seleccionado por código: {directa.Nombre} [{directa.Codigo}]");
                        return directa.Codigo;
                    }

                    Console.WriteLine("No se encontró coincidencia.");
                    return null;
                }
                else if (tecla.Key == ConsoleKey.Backspace)
                {
                    if (buffer.Length > 0)
                        buffer = buffer.Substring(0, buffer.Length - 1);
                }
                else if (tecla.Key >= ConsoleKey.D1 && tecla.Key <= ConsoleKey.D5 && ((int)tecla.Key - (int)ConsoleKey.D1) < sugerencias.Length)
                {
                    int idx = (int)tecla.Key - (int)ConsoleKey.D1;
                    Console.SetCursorPosition(0, filaBusqueda + 7);
                    Console.WriteLine($"Seleccionado #{idx + 1}: {sugerencias[idx].NombreSubestacion} [{sugerencias[idx].CodigoSubestacion}]");
                    return sugerencias[idx].CodigoSubestacion;
                }
                else if (!char.IsControl(tecla.KeyChar))
                {
                    buffer += tecla.KeyChar;
                }
            }
        }

        public void EjecutarFlujoIntegradoRestablecimiento()
        {
            Console.WriteLine("FLUJO INTEGRADO: RESTABLECIMIENTO DE SUMINISTRO");

            string codOrigen = SeleccionarSubestacionConAutocompletado("ORIGEN");
            if (codOrigen == null) return;

            string codDestino = SeleccionarSubestacionConAutocompletado("DESTINO");
            if (codDestino == null) return;

            Console.WriteLine("\n[PASO 2 - TABLA HASH] Localizando registros técnicos en...");
            Subestacion subOrigen = TablaHash.Buscar(codOrigen);
            Subestacion subDestino = TablaHash.Buscar(codDestino);
            Console.WriteLine($"  -> Origen : {subOrigen}");
            Console.WriteLine($"  -> Destino: {subDestino}");

            Console.WriteLine("\n[PASO 3 - UNION-FIND] Verificando conectividad de zona eléctrica...");
            if (!UnionFind.PertenecenAMismaZona(codOrigen, codDestino))
            {
                Console.WriteLine($"Las subestaciones [{codOrigen}] y [{codDestino}] pertenecen a ZONAS ELÉCTRICAS AISLADAS.");
                Console.WriteLine("No es posible restablecer el suministro sin antes construir una línea de interconexión");
                return;
            }
            Console.WriteLine("Ambas subestaciones pertenecen a la misma zona eléctrica conectada.");

            Console.WriteLine("\n[PASO 4 - GRAFO (DIJKSTRA)] Calculando ruta óptima de menor costo de mantenimiento...");
            bool exito = Grafo.CalcularRutaMinimaDijkstra(
                codOrigen,
                codDestino,
                out string[] ruta,
                out double costoTotal,
                out double kmTotal);

            if (!exito)
            {
                Console.WriteLine("Aunque están en la misma zona eléctrica, el sentido unidireccional de las líneas impide el flujo en esta dirección.");
                return;
            }

            Console.WriteLine("\n--- RUTA ÓPTIMA PARA RESTABLECER EL SUMINISTRO ---");
            for (int i = 0; i < ruta.Length; i++)
            {
                Subestacion info = TablaHash.Buscar(ruta[i]);
                string flecha = (i < ruta.Length - 1) ? " ---> " : "";
                Console.Write($"[{info.Codigo}: {info.Nombre}]{flecha}");
            }
            Console.WriteLine($"\n  * Longitud acumulada        : {kmTotal:F2} km");
            Console.WriteLine($"  * Costo total mantenimiento : Q{costoTotal:F2}");
        }

        // Genera un reporte ejecutivo en archivo de texto 
        public void GenerarReporteEstrategico(string rutaReporte)
        {
            Subestacion[] todas = TablaHash.ObtenerTodas();
            double capacidadTotal = 0;
            for (int i = 0; i < todas.Length; i++) capacidadTotal += todas[i].CapacidadMW;

            using (StreamWriter sw = new StreamWriter(rutaReporte))
            {
                sw.WriteLine("----------------------------------------------------------------");
                sw.WriteLine(" REPORTE EJECUTIVO DE LA RED ELÉCTRICA MUNICIPAL");
                sw.WriteLine($" Fecha de generación: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sw.WriteLine($"Total de Subestaciones Registradas : {todas.Length}");
                sw.WriteLine($"Capacidad Instalada Total          : {capacidadTotal:F2} MW");
                sw.WriteLine("----------------------------------------------------------------");
                sw.WriteLine("DETALLE DE SUBESTACIONES:");
                for (int i = 0; i < todas.Length; i++)
                {
                    sw.WriteLine($" - {todas[i]}");
                }
                sw.WriteLine("----------------------------------------------------------------");

            }
            Console.WriteLine($"Reporte estratégico generado exitosamente en: {Path.GetFullPath(rutaReporte)}");
        }
    }
}