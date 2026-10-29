// ESTRUCTURA: Grafo Dirigido/Mixto Ponderado con Lista de Adyacencia Propia y Min-Heap Propio
using System;
using System.Collections.Generic; // Se usa únicamente Queue<int> para BFS según Regla 7
using RedElectricaMunicipal.Modelos;

namespace RedElectricaMunicipal.Estructuras
{
    public class NodoArista
    {
        public int IndiceDestino;
        public string CodigoDestino;
        public double LongitudKm;
        public double CostoMantenimiento;
        public bool EsUnidireccional;
        public NodoArista Siguiente;

        public NodoArista(int idxDestino, string codDestino, double longitud, double costo, bool unidireccional)
        {
            IndiceDestino = idxDestino;
            CodigoDestino = codDestino;
            LongitudKm = longitud;
            CostoMantenimiento = costo;
            EsUnidireccional = unidireccional;
            Siguiente = null;
        }
    }

    // Cola de prioridad mínima (Min-Heap) implementada desde cero para el algoritmo de Dijkstra.
    public class MinHeapPropio
    {
        private int[] _vertices;
        private double[] _distancias;
        private int _tamano;

        public int Tamano => _tamano;

        public MinHeapPropio(int capacidad)
        {
            _vertices = new int[capacidad + 1];
            _distancias = new double[capacidad + 1];
            _tamano = 0;
        }

        public void Encolar(int vertice, double distancia)
        {
            if (_tamano + 1 >= _vertices.Length)
            {
                int nuevaCap = _vertices.Length * 2;
                int[] nVert = new int[nuevaCap];
                double[] nDist = new double[nuevaCap];
                for (int i = 0; i <= _tamano; i++)
                {
                    nVert[i] = _vertices[i];
                    nDist[i] = _distancias[i];
                }
                _vertices = nVert;
                _distancias = nDist;
            }

            _tamano++;
            int actual = _tamano;
            _vertices[actual] = vertice;
            _distancias[actual] = distancia;

            while (actual > 1 && _distancias[actual] < _distancias[actual / 2])
            {
                Intercambiar(actual, actual / 2);
                actual /= 2;
            }
        }

        public void Desencolar(out int vertice, out double distancia)
        {
            vertice = _vertices[1];
            distancia = _distancias[1];
            _vertices[1] = _vertices[_tamano];
            _distancias[1] = _distancias[_tamano];
            _tamano--;
            Hundir(1);
        }

        private void Hundir(int idx)
        {
            int menor = idx;
            int izq = 2 * idx;
            int der = 2 * idx + 1;

            if (izq <= _tamano && _distancias[izq] < _distancias[menor])
                menor = izq;
            if (der <= _tamano && _distancias[der] < _distancias[menor])
                menor = der;

            if (menor != idx)
            {
                Intercambiar(idx, menor);
                Hundir(menor);
            }
        }

        private void Intercambiar(int a, int b)
        {
            int tempV = _vertices[a];
            double tempD = _distancias[a];
            _vertices[a] = _vertices[b];
            _distancias[a] = _distancias[b];
            _vertices[b] = tempV;
            _distancias[b] = tempD;
        }
    }

    public class GrafoRedElectrica
    {
        private string[] _codigosVertices;
        private NodoArista[] _listasAdyacencia;
        private int _numVertices;
        private int _capacidad;

        public GrafoRedElectrica(int capacidadInicial = 32)
        {
            _capacidad = capacidadInicial;
            _codigosVertices = new string[_capacidad];
            _listasAdyacencia = new NodoArista[_capacidad];
            _numVertices = 0;
        }

        private void AsegurarCapacidad()
        {
            if (_numVertices < _capacidad) return;
            int nuevaCapacidad = _capacidad * 2;
            string[] nuevosCodigos = new string[nuevaCapacidad];
            NodoArista[] nuevasListas = new NodoArista[nuevaCapacidad];

            for (int i = 0; i < _numVertices; i++)
            {
                nuevosCodigos[i] = _codigosVertices[i];
                nuevasListas[i] = _listasAdyacencia[i];
            }

            _codigosVertices = nuevosCodigos;
            _listasAdyacencia = nuevasListas;
            _capacidad = nuevaCapacidad;
        }

        public int ObtenerIndiceVertice(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return -1;
            string norm = codigo.Trim().ToUpper();
            for (int i = 0; i < _numVertices; i++)
            {
                if (_codigosVertices[i] == norm) return i;
            }
            return -1;
        }

        public int AgregarVertice(string codigo)
        {
            int existente = ObtenerIndiceVertice(codigo);
            if (existente != -1) return existente;

            AsegurarCapacidad();
            int idx = _numVertices++;
            _codigosVertices[idx] = codigo.Trim().ToUpper();
            _listasAdyacencia[idx] = null;
            return idx;
        }

        public void AgregarLinea(LineaDistribucion linea)
        {
            int u = AgregarVertice(linea.CodigoOrigen);
            int v = AgregarVertice(linea.CodigoDestino);

            AgregarAristaDirigida(u, v, linea.CodigoDestino, linea.LongitudKm, linea.CostoMantenimiento, linea.EsUnidireccional);
            if (!linea.EsUnidireccional)
            {
                AgregarAristaDirigida(v, u, linea.CodigoOrigen, linea.LongitudKm, linea.CostoMantenimiento, false);
            }
        }

        private void AgregarAristaDirigida(int u, int v, string codDestino, double longitud, double costo, bool unidireccional)
        {
            NodoArista actual = _listasAdyacencia[u];
            while (actual != null)
            {
                if (actual.IndiceDestino == v)
                {
                    actual.LongitudKm = longitud;
                    actual.CostoMantenimiento = costo;
                    actual.EsUnidireccional = unidireccional;
                    return;
                }
                actual = actual.Siguiente;
            }

            NodoArista nueva = new NodoArista(v, codDestino, longitud, costo, unidireccional);
            nueva.Siguiente = _listasAdyacencia[u];
            _listasAdyacencia[u] = nueva;
        }

        public bool EliminarLinea(string codigoOrigen, string codigoDestino)
        {
            int u = ObtenerIndiceVertice(codigoOrigen);
            int v = ObtenerIndiceVertice(codigoDestino);
            if (u == -1 || v == -1) return false;

            bool eliminadoIda = EliminarAristaSimple(u, v);
            EliminarAristaSimple(v, u);
            return eliminadoIda;
        }

        private bool EliminarAristaSimple(int u, int v)
        {
            NodoArista actual = _listasAdyacencia[u];
            NodoArista anterior = null;

            while (actual != null)
            {
                if (actual.IndiceDestino == v)
                {
                    if (anterior == null)
                        _listasAdyacencia[u] = actual.Siguiente;
                    else
                        anterior.Siguiente = actual.Siguiente;
                    return true;
                }
                anterior = actual;
                actual = actual.Siguiente;
            }
            return false;
        }

        public void DesconectarVertice(string codigo)
        {
            int u = ObtenerIndiceVertice(codigo);
            if (u == -1) return;

            _listasAdyacencia[u] = null;

            for (int i = 0; i < _numVertices; i++)
            {
                if (i != u) EliminarAristaSimple(i, u);
            }
        }

        // Recorrido en Anchura (BFS) utilizando Queue<int> como única estructura auxiliar permitida.
        // Complejidad: O(V + E).
        public string[] RecorridoBFS(string codigoInicio)
        {
            int inicio = ObtenerIndiceVertice(codigoInicio);
            if (inicio == -1) return new string[0];

            bool[] visitado = new bool[_numVertices];
            string[] ordenVisita = new string[_numVertices];
            int contador = 0;

            Queue<int> cola = new Queue<int>();
            visitado[inicio] = true;
            cola.Enqueue(inicio);

            while (cola.Count > 0)
            {
                int u = cola.Dequeue();
                ordenVisita[contador++] = _codigosVertices[u];

                NodoArista arista = _listasAdyacencia[u];
                while (arista != null)
                {
                    int v = arista.IndiceDestino;
                    if (!visitado[v])
                    {
                        visitado[v] = true;
                        cola.Enqueue(v);
                    }
                    arista = arista.Siguiente;
                }
            }

            string[] resultado = new string[contador];
            for (int i = 0; i < contador; i++) resultado[i] = ordenVisita[i];
            return resultado;
        }

        // Recorrido en Profundidad (DFS) implementado de forma recursiva propia.
        // Complejidad: O(V + E).
        public string[] RecorridoDFS(string codigoInicio)
        {
            int inicio = ObtenerIndiceVertice(codigoInicio);
            if (inicio == -1) return new string[0];

            bool[] visitado = new bool[_numVertices];
            string[] ordenVisita = new string[_numVertices];
            int contador = 0;

            DFSRecursivo(inicio, visitado, ordenVisita, ref contador);

            string[] resultado = new string[contador];
            for (int i = 0; i < contador; i++) resultado[i] = ordenVisita[i];
            return resultado;
        }

        private void DFSRecursivo(int u, bool[] visitado, string[] ordenVisita, ref int contador)
        {
            visitado[u] = true;
            ordenVisita[contador++] = _codigosVertices[u];

            NodoArista arista = _listasAdyacencia[u];
            while (arista != null)
            {
                int v = arista.IndiceDestino;
                if (!visitado[v])
                {
                    DFSRecursivo(v, visitado, ordenVisita, ref contador);
                }
                arista = arista.Siguiente;
            }
        }

        // Algoritmo de Dijkstra con Min-Heap propio para calcular la ruta de menor costo
        // de mantenimiento (o menor longitud) entre dos subestaciones.
        // Complejidad: O((V + E) log V).
        public bool CalcularRutaMinimaDijkstra(
            string codigoOrigen,
            string codigoDestino,
            out string[] rutaCodigos,
            out double costoTotalMantenimiento,
            out double longitudTotalKm)
        {
            rutaCodigos = new string[0];
            costoTotalMantenimiento = 0;
            longitudTotalKm = 0;

            int origen = ObtenerIndiceVertice(codigoOrigen);
            int destino = ObtenerIndiceVertice(codigoDestino);
            if (origen == -1 || destino == -1) return false;

            double[] distCosto = new double[_numVertices];
            double[] distKm = new double[_numVertices];
            int[] previo = new int[_numVertices];
            bool[] procesado = new bool[_numVertices];

            for (int i = 0; i < _numVertices; i++)
            {
                distCosto[i] = double.MaxValue;
                distKm[i] = 0;
                previo[i] = -1;
                procesado[i] = false;
            }

            distCosto[origen] = 0;
            MinHeapPropio heap = new MinHeapPropio(_numVertices * 4 + 4);
            heap.Encolar(origen, 0);

            while (heap.Tamano > 0)
            {
                heap.Desencolar(out int u, out double dActual);
                if (procesado[u]) continue;
                procesado[u] = true;

                if (u == destino) break;

                NodoArista arista = _listasAdyacencia[u];
                while (arista != null)
                {
                    int v = arista.IndiceDestino;
                    double pesoCosto = arista.CostoMantenimiento;

                    if (!procesado[v] && distCosto[u] != double.MaxValue && distCosto[u] + pesoCosto < distCosto[v])
                    {
                        distCosto[v] = distCosto[u] + pesoCosto;
                        distKm[v] = distKm[u] + arista.LongitudKm;
                        previo[v] = u;
                        heap.Encolar(v, distCosto[v]);
                    }
                    arista = arista.Siguiente;
                }
            }

            if (distCosto[destino] == double.MaxValue)
                return false;

            // Reconstruir camino desde destino hacia origen
            int[] caminoIndices = new int[_numVertices];
            int pasos = 0;
            int actual = destino;
            while (actual != -1)
            {
                caminoIndices[pasos++] = actual;
                actual = previo[actual];
            }

            rutaCodigos = new string[pasos];
            for (int i = 0; i < pasos; i++)
            {
                rutaCodigos[i] = _codigosVertices[caminoIndices[pasos - 1 - i]];
            }

            costoTotalMantenimiento = distCosto[destino];
            longitudTotalKm = distKm[destino];
            return true;
        }

        // Reconstruye el Union-Find recorriendo todas las aristas activas del grafo.
        public void SincronizarUnionFind(UnionFindZonas uf)
        {
            uf.ReiniciarZonas();
            for (int i = 0; i < _numVertices; i++)
            {
                uf.RegistrarSubestacion(_codigosVertices[i]);
            }
            for (int u = 0; u < _numVertices; u++)
            {
                NodoArista arista = _listasAdyacencia[u];
                while (arista != null)
                {
                    uf.Union(_codigosVertices[u], arista.CodigoDestino);
                    arista = arista.Siguiente;
                }
            }
        }

        public void ImprimirTopologiaGrafo()
        {
            Console.WriteLine("\n TOPOLOGÍA DEL GRAFO DE LA RED ELÉCTRICA");
            if (_numVertices == 0)
            {
                Console.WriteLine("El grafo no tiene subestaciones registradas.");
                return;
            }

            for (int i = 0; i < _numVertices; i++)
            {
                Console.Write($"Subestación [{_codigosVertices[i]}]: ");
                NodoArista arista = _listasAdyacencia[i];
                if (arista == null)
                {
                    Console.WriteLine("(Sin líneas salientes)");
                    continue;
                }
                while (arista != null)
                {
                    string modo = arista.EsUnidireccional ? "Uni" : "Bi";
                    Console.Write($"--> {arista.CodigoDestino} ({arista.LongitudKm}km, Q{arista.CostoMantenimiento}, {modo}) ");
                    arista = arista.Siguiente;
                }
                Console.WriteLine();
            }
        }
    }
}