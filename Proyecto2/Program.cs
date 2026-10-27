using System;
using System.IO;
using System.Globalization;
using RedElectricaMunicipal.Modelos;
using RedElectricaMunicipal.Estructuras;
using RedElectricaMunicipal.Servicios;

namespace RedElectricaMunicipal
{
    class Program
    {
        static void Main(string[] args)
        {
            GestorRedElectrica gestor = new GestorRedElectrica();

            // Carga automática inicial si existen los archivos en Datos/
            string rutaSubDefault = Path.Combine("Datos", "subestaciones.csv");
            string rutaLinDefault = Path.Combine("Datos", "lineas.csv");
            if (File.Exists(rutaSubDefault) || File.Exists(rutaLinDefault))
            {
                gestor.CargarDatosDesdeCSV(rutaSubDefault, rutaLinDefault);
            }

            bool salir = false;
            while (!salir)
            {
                Console.WriteLine("\n╔════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║        PLATAFORMA DE GESTIÓN DE RED ELÉCTRICA MUNICIPAL (C#)           ║");
                Console.WriteLine("╠════════════════════════════════════════════════════════════════════════╣");
                Console.WriteLine("║  1. Cargar datos desde archivos (.csv / .txt)                          ║");
                Console.WriteLine("║  2. Registrar nueva subestación manualmente (Tabla Hash + Trie + UF)   ║");
                Console.WriteLine("║  3. Aprobar y registrar línea eléctrica (Verifica zona con Union-Find) ║");
                Console.WriteLine("║  4. Buscar subestación por código único (Tabla Hash O(1))              ║");
                Console.WriteLine("║  5. Sugerir nombres de subestación mientras se escribe (Trie)          ║");
                Console.WriteLine("║  6. Verificar si 2 subestaciones están en la misma zona (Union-Find)   ║");
                Console.WriteLine("║  7. Recorridos del Grafo (BFS y DFS) desde una subestación             ║");
                Console.WriteLine("║  8. Calcular ruta de menor costo con Dijkstra (Grafo)                  ║");
                Console.WriteLine("║  9. ★ FLUJO INTEGRADO COMPLETO (Trie -> Hash -> Union-Find -> Dijkstra)║");
                Console.WriteLine("║ 10. Eliminar subestación o línea de distribución                       ║");
                Console.WriteLine("║ 11. Imprimir estructuras internas y generar Reporte Ejecutivo (.txt)   ║");
                Console.WriteLine("║  0. Salir                                                              ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════════════════╝");
                Console.Write("Seleccione una opción: ");

                string opcion = Console.ReadLine()?.Trim();
                switch (opcion)
                {
                    case "1":
                        Console.Write("Ruta de archivo de subestaciones (Enter para 'Datos/subestaciones.csv'): ");
                        string rSub = Console.ReadLine()?.Trim();
                        if (string.IsNullOrEmpty(rSub)) rSub = rutaSubDefault;

                        Console.Write("Ruta de archivo de líneas (Enter para 'Datos/lineas.csv'): ");
                        string rLin = Console.ReadLine()?.Trim();
                        if (string.IsNullOrEmpty(rLin)) rLin = rutaLinDefault;

                        gestor.CargarDatosDesdeCSV(rSub, rLin);
                        break;

                    case "2":
                        Console.Write("Código único: ");
                        string cod = Console.ReadLine();
                        Console.Write("Nombre de la subestación: ");
                        string nom = Console.ReadLine();
                        Console.Write("Capacidad en MW: ");
                        if (double.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out double cap))
                        {
                            bool nueva = gestor.RegistrarSubestacion(cod, nom, cap);
                            Console.WriteLine(nueva
                                ? " Subestación registrada en Tabla Hash, Trie, Grafo y Union-Find."
                                : " Subestación actualizada.");
                        }
                        else
                        {
                            Console.WriteLine("Capacidad inválida.");
                        }
                        break;

                    case "3":
                        Console.Write("Código de subestación origen: ");
                        string orig = Console.ReadLine();
                        Console.Write("Código de subestación destino: ");
                        string dest = Console.ReadLine();
                        Console.Write("Longitud en km: ");
                        double.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out double km);
                        Console.Write("Costo de mantenimiento (Q): ");
                        double.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out double costo);
                        Console.Write("¿Es unidireccional de principal a secundaria? (s/n): ");
                        bool uni = Console.ReadLine()?.Trim().ToLower() == "s";

                        if (gestor.RegistrarLinea(orig, dest, km, costo, uni, out string msgZona))
                        {
                            Console.WriteLine(msgZona);
                            Console.WriteLine("Línea eléctrica registrada exitosamente.");
                        }
                        else
                        {
                            Console.WriteLine(msgZona);
                        }
                        break;

                    case "4":
                        Console.Write("Ingrese el código único de la subestación: ");
                        string codBuscar = Console.ReadLine();
                        Subestacion encontrada = gestor.TablaHash.Buscar(codBuscar);
                        if (encontrada != null)
                            Console.WriteLine($"[ENCONTRADA] {encontrada}");
                        else
                            Console.WriteLine("No existe ninguna subestación con ese código.");
                        break;

                    case "5":
                        gestor.SeleccionarSubestacionConAutocompletado("Búsqueda Interactiva en Trie");
                        break;

                    case "6":
                        Console.Write("Código Subestación A: ");
                        string cA = Console.ReadLine();
                        Console.Write("Código Subestación B: ");
                        string cB = Console.ReadLine();
                        bool conectadas = gestor.UnionFind.PertenecenAMismaZona(cA, cB);
                        Console.WriteLine(conectadas
                            ? $"[CONECTADAS] {cA.ToUpper()} y {cB.ToUpper()} pertenecen a la MISMA zona eléctrica."
                            : $"[AISLADAS] {cA.ToUpper()} y {cB.ToUpper()} están en zonas eléctricas DIFERENTES.");
                        break;

                    case "7":
                        Console.Write("Código de subestación inicial para BFS y DFS: ");
                        string codIni = Console.ReadLine();
                        string[] bfs = gestor.Grafo.RecorridoBFS(codIni);
                        string[] dfs = gestor.Grafo.RecorridoDFS(codIni);

                        if (bfs.Length == 0)
                        {
                            Console.WriteLine("Subestación no encontrada en el grafo.");
                        }
                        else
                        {
                            Console.WriteLine("Recorrido BFS (Anchura)   : " + string.Join(" -> ", bfs));
                            Console.WriteLine("Recorrido DFS (Profundidad): " + string.Join(" -> ", dfs));
                        }
                        break;

                    case "8":
                        Console.Write("Código Subestación Origen: ");
                        string dOrig = Console.ReadLine();
                        Console.Write("Código Subestación Destino: ");
                        string dDest = Console.ReadLine();
                        if (gestor.Grafo.CalcularRutaMinimaDijkstra(dOrig, dDest, out string[] rutaD, out double cTotal, out double kTotal))
                        {
                            Console.WriteLine("Ruta de menor costo: " + string.Join(" -> ", rutaD));
                            Console.WriteLine($"Costo Mantenimiento Total: Q{cTotal:F2} | Distancia: {kTotal:F2} km");
                        }
                        else
                        {
                            Console.WriteLine("No existe ruta dirigida disponible entre ambos puntos.");
                        }
                        break;

                    case "9":
                        gestor.EjecutarFlujoIntegradoRestablecimiento();
                        break;

                    case "10":
                        Console.Write("¿Desea eliminar (1) Subestación o (2) Línea?: ");
                        string tipoElim = Console.ReadLine()?.Trim();
                        if (tipoElim == "1")
                        {
                            Console.Write("Código de subestación a eliminar: ");
                            string codEl = Console.ReadLine();
                            Console.WriteLine(gestor.EliminarSubestacion(codEl)
                                ? "[OK] Subestación eliminada de Tabla Hash y Trie."
                                : "[!] No se encontró la subestación.");
                        }
                        else if (tipoElim == "2")
                        {
                            Console.Write("Código origen de la línea: ");
                            string lO = Console.ReadLine();
                            Console.Write("Código destino de la línea: ");
                            string lD = Console.ReadLine();
                            if (gestor.Grafo.EliminarLinea(lO, lD))
                            {
                                gestor.Grafo.SincronizarUnionFind(gestor.UnionFind);
                                Console.WriteLine("[OK] Línea eliminada y zonas Union-Find actualizadas.");
                            }
                            else
                            {
                                Console.WriteLine("[!] No se encontró la línea indicada.");
                            }
                        }
                        break;

                    case "11":
                        gestor.TablaHash.ImprimirEstructuraHash();
                        gestor.TrieNombres.ImprimirTodoElTrie();
                        gestor.UnionFind.ImprimirZonasElectricas();
                        gestor.Grafo.ImprimirTopologiaGrafo();
                        gestor.GenerarReporteEstrategico("ReporteRedElectrica.txt");
                        break;

                    case "0":
                        salir = true;
                        break;

                    default:
                        Console.WriteLine("Opción no válida.");
                        break;
                }
            }
        }
    }
}