// ESTRUCTURA: Conjuntos Disjuntos (Union-Find) con Compresión de Caminos y Unión por Rango
using System;

namespace RedElectricaMunicipal.Estructuras
{
    public class UnionFindZonas
    {
        private string[] _codigos;
        private int[] _padre;
        private int[] _rango;
        private int[] _tamanoZona;
        private int _cantidad;
        private int _numZonas;
        private int _capacidad;

        public int CantidadSubestaciones => _cantidad;
        public int CantidadZonasIndependientes => _numZonas;

        public UnionFindZonas(int capacidadInicial = 32)
        {
            _capacidad = capacidadInicial > 0 ? capacidadInicial : 32;
            _codigos = new string[_capacidad];
            _padre = new int[_capacidad];
            _rango = new int[_capacidad];
            _tamanoZona = new int[_capacidad];
            _cantidad = 0;
            _numZonas = 0;
        }

        // Duplica la capacidad de los arreglos internos cuando se llena la estructura.
        private void AsegurarCapacidad()
        {
            if (_cantidad < _capacidad) return;

            int nuevaCapacidad = _capacidad * 2;
            string[] nuevosCodigos = new string[nuevaCapacidad];
            int[] nuevosPadres = new int[nuevaCapacidad];
            int[] nuevosRangos = new int[nuevaCapacidad];
            int[] nuevosTamanos = new int[nuevaCapacidad];

            for (int i = 0; i < _cantidad; i++)
            {
                nuevosCodigos[i] = _codigos[i];
                nuevosPadres[i] = _padre[i];
                nuevosRangos[i] = _rango[i];
                nuevosTamanos[i] = _tamanoZona[i];
            }

            _codigos = nuevosCodigos;
            _padre = nuevosPadres;
            _rango = nuevosRangos;
            _tamanoZona = nuevosTamanos;
            _capacidad = nuevaCapacidad;
        }

        // Busca el índice interno asociado al código de una subestación.
        public int ObtenerIndice(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return -1;
            string norm = codigo.Trim().ToUpper();

            for (int i = 0; i < _cantidad; i++)
            {
                if (_codigos[i] == norm) return i;
            }
            return -1;
        }

        // Registra una nueva subestación como su propio conjunto disjunto inicial.
        public int RegistrarSubestacion(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return -1;

            int existente = ObtenerIndice(codigo);
            if (existente != -1) return existente;

            AsegurarCapacidad();
            int idx = _cantidad++;
            _codigos[idx] = codigo.Trim().ToUpper();
            _padre[idx] = idx;      // Al inicio es su propio representante (raíz)
            _rango[idx] = 0;        // Altura inicial del árbol es 0
            _tamanoZona[idx] = 1;   // La zona inicia con 1 sola subestación
            _numZonas++;            // Aumenta el número de zonas aisladas

            return idx;
        }

        // Encuentra el representante de la zona eléctrica aplicando Compresión de Caminos 
        // Aplana el árbol conectando cada nodo visitado directamente a la raíz.
        public int Find(int i)
        {
            if (i < 0 || i >= _cantidad) return -1;

            if (_padre[i] != i)
            {
                _padre[i] = Find(_padre[i]); // Compresión de camino recursiva
            }
            return _padre[i];
        }

        // Devuelve el código de la subestación raíz que representa la zona de la subestación dada.
        public string ObtenerRaiz(string codigo)
        {
            int idx = ObtenerIndice(codigo);
            if (idx == -1) return null;
            int raiz = Find(idx);
            return _codigos[raiz];
        }

        // Une dos zonas eléctricas aplicando Unión por Rango 
        // Cuelga el árbol de menor rango bajo la raíz del árbol de mayor rango 
        public bool Union(string codigoA, string codigoB)
        {
            int idxA = ObtenerIndice(codigoA);
            int idxB = ObtenerIndice(codigoB);
            if (idxA == -1 || idxB == -1) return false;

            int raizA = Find(idxA);
            int raizB = Find(idxB);

            if (raizA == raizB) return false; // Ya pertenecen a la misma zona eléctrica

            if (_rango[raizA] < _rango[raizB])
            {
                _padre[raizA] = raizB;
                _tamanoZona[raizB] += _tamanoZona[raizA];
            }
            else if (_rango[raizA] > _rango[raizB])
            {
                _padre[raizB] = raizA;
                _tamanoZona[raizA] += _tamanoZona[raizB];
            }
            else
            {
                _padre[raizB] = raizA;
                _tamanoZona[raizA] += _tamanoZona[raizB];
                _rango[raizA]++;
            }

            _numZonas--; // Dos zonas separadas se fusionaron en una sola
            return true;
        }

        // Verifica si dos subestaciones pertenecen a la misma zona eléctrica conectada.
        public bool PertenecenAMismaZona(string codigoA, string codigoB)
        {
            int idxA = ObtenerIndice(codigoA);
            int idxB = ObtenerIndice(codigoB);
            if (idxA == -1 || idxB == -1) return false;

            return Find(idxA) == Find(idxB);
        }

        // Reconstruye los conjuntos disjuntos desde cero (útil tras eliminar líneas o subestaciones).
        public void ReiniciarZonas()
        {
            for (int i = 0; i < _cantidad; i++)
            {
                _padre[i] = i;
                _rango[i] = 0;
                _tamanoZona[i] = 1;
            }
            _numZonas = _cantidad;
        }

        // Imprime las zonas eléctricas conectadas agrupadas por su nodo raíz.
        public void ImprimirZonasElectricas()
        {
            Console.WriteLine($"\n--- ZONAS ELÉCTRICAS CONECTADAS (UNION-FIND | Subestaciones: {_cantidad}, Zonas Independientes: {_numZonas}) ---");
            if (_cantidad == 0)
            {
                Console.WriteLine("No hay subestaciones registradas.");
                return;
            }

            bool[] visitado = new bool[_cantidad];
            int numZona = 1;

            for (int i = 0; i < _cantidad; i++)
            {
                if (!visitado[i])
                {
                    int raiz = Find(i);
                    string estado = _tamanoZona[raiz] > 1 ? "Red Interconectada" : "Subestación Aislada";
                    Console.Write($"Zona #{numZona++} [Raíz: {_codigos[raiz]} | Nodos: {_tamanoZona[raiz]} | Rango: {_rango[raiz]} | {estado}]: ");

                    for (int j = 0; j < _cantidad; j++)
                    {
                        if (Find(j) == raiz)
                        {
                            visitado[j] = true;
                            Console.Write($"[{_codigos[j]}] ");
                        }
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}