using System;
using System.IO;
using System.Text;
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
            Console.OutputEncoding = Encoding.UTF8;
            GestorRedElectrica gestor = new GestorRedElectrica();

            // Carga automática inicial si existen los archivos por defecto
            string rutaSubDefault = Path.Combine("Datos", "Subestaciones.csv");
            string rutaLinDefault = Path.Combine("Datos", "Lineas.csv");
            if (File.Exists(rutaSubDefault) || File.Exists(rutaLinDefault))
            {
                gestor.CargarDatosDesdeCSV(rutaSubDefault, rutaLinDefault);
            }

            bool salir = false;
            while (!salir)
            {
                Console.WriteLine("\n--------------------------------------------------------------------------");
                Console.WriteLine("PLATAFORMA DE GESTIÓN DE RED ELÉCTRICA MUNICIPAL");
                Console.WriteLine("1. Cargar datos desde archivos (.csv / .txt)");
                Console.WriteLine("2. Registrar Subestación o Aprobar Línea Eléctrica (Hash + Trie + UF)");
                Console.WriteLine("3. Buscar Subestación con Autocompletado o Código (Trie -> Tabla Hash)");
                Console.WriteLine("4. Verificar conectividad de Zona Eléctrica (Union-Find)");
                Console.WriteLine("5. Recorridos de la Red Eléctrica (BFS y DFS en Grafo)");
                Console.WriteLine("6. Restablecer Suministro / Ruta de Menor Costo (Flujo Integrado + Dijkstra)");
                Console.WriteLine("7. Eliminar Subestación o Línea de Distribución");
                Console.WriteLine("8. Imprimir Estructuras Internas y Generar Reporte (.txt)");
                Console.WriteLine("0. Salir");
                Console.WriteLine("\n--------------------------------------------------------------------------");
                Console.Write("Seleccione una opción: ");
                string opcion = Console.ReadLine()?.Trim();

                switch (opcion)
                {
                    case "1":
                        Console.Write("Ruta de subestaciones (Enter para 'Datos/Subestaciones.csv'): ");
                        string rSub = Console.ReadLine()?.Trim();
                        if (string.IsNullOrEmpty(rSub)) rSub = rutaSubDefault;

                        Console.Write("Ruta de líneas (Enter para 'Datos/Lineas.csv'): ");
                        string rLin = Console.ReadLine()?.Trim();
                        if (string.IsNullOrEmpty(rLin)) rLin = rutaLinDefault;

                        gestor.CargarDatosDesdeCSV(rSub, rLin);
                        break;

                    case "2":
                        Console.Write("¿Desea registrar (1) Subestación o (2) Línea Eléctrica?: ");
                        string tipoReg = Console.ReadLine()?.Trim();
                        if (tipoReg == "1")
                        {
                            Console.Write("Código único: ");
                            string cod = Console.ReadLine();
                            Console.Write("Nombre de la subestación: ");
                            string nom = Console.ReadLine();
                            Console.Write("Capacidad en MW: ");
                            if (double.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out double cap))
                            {
                                bool nueva = gestor.RegistrarSubestacion(cod, nom, cap);
                                Console.WriteLine(nueva
                                    ? "Subestación registrada en Tabla Hash, Trie, Grafo y Union-Find."
                                    : "Subestación actualizada.");
                            }
                            else
                            {
                                Console.WriteLine("Capacidad inválida.");
                            }
                        }
                        else if (tipoReg == "2")
                        {
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
                        }
                        break;

                    case "3":
                        string codSeleccionado = gestor.SeleccionarSubestacionConAutocompletado("Búsqueda Trie + Hash");
                        if (codSeleccionado != null)
                        {
                            Subestacion encontrada = gestor.TablaHash.Buscar(codSeleccionado);
                            if (encontrada != null)
                                Console.WriteLine($"[FICHA TABLA HASH O(1)] {encontrada}");
                        }
                        break;

                    case "4":
                        Console.Write("Código Subestación A: ");
                        string cA = Console.ReadLine();
                        Console.Write("Código Subestación B: ");
                        string cB = Console.ReadLine();
                        bool conectadas = gestor.UnionFind.PertenecenAMismaZona(cA, cB);
                        Console.WriteLine(conectadas
                            ? $"{cA?.ToUpper()} y {cB?.ToUpper()} pertenecen a la misma zona eléctrica."
                            : $"{cA?.ToUpper()} y {cB?.ToUpper()} están en zonas eléctricas diferentes.");
                        break;

                    case "5":
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
                            Console.WriteLine("Recorrido BFS (Anchura)    : " + string.Join(" -> ", bfs));
                            Console.WriteLine("Recorrido DFS (Profundidad): " + string.Join(" -> ", dfs));
                        }
                        break;

                    case "6":
                        gestor.EjecutarFlujoIntegradoRestablecimiento();
                        break;

                    case "7":
                        Console.Write("¿Desea eliminar (1) Subestación o (2) Línea?: ");
                        string tipoElim = Console.ReadLine()?.Trim();
                        if (tipoElim == "1")
                        {
                            Console.Write("Código de subestación a eliminar: ");
                            string codEl = Console.ReadLine();
                            Console.WriteLine(gestor.EliminarSubestacion(codEl)
                                ? "Subestación eliminada del sistema."
                                : "No se encontró la subestación.");
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
                                Console.WriteLine("Línea eliminada y zonas Union-Find actualizadas.");
                            }
                            else
                            {
                                Console.WriteLine("No se encontró la línea indicada.");
                            }
                        }
                        break;

                    case "8":
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