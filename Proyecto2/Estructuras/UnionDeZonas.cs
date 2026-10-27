//ESTRUCTURA: Conjuntos Disjuntos (Union-Find) con Compresión de Caminos y Unión por Rango

using System;

namespace RedElectricaMunicipal.Estructuras
{
    public class UnionFindZonas
    {
        private string[] _codigos;
        private int[] _padre;
        private int[] _rango;
        private int _cantidad;
        private int _capacidad;

        public int CantidadSubestaciones => _cantidad;

        public UnionFindZonas(int capacidadInicial = 32)
        {
            _capacidad = capacidadInicial;
            _codigos = new string[_capacidad];
            _padre = new int[_capacidad];
            _rango = new int[_capacidad];
            _cantidad = 0;
        }

        private void AsegurarCapacidad()
        {
            if (_cantidad < _capacidad) return;
            int nuevaCapacidad = _capacidad * 2;
            string[] nuevosCodigos = new string[nuevaCapacidad];
            int[] nuevosPadres = new int[nuevaCapacidad];
            int[] nuevosRangos = new int[nuevaCapacidad];

            for (int i = 0; i < _cantidad; i++)
            {
                nuevosCodigos[i] = _codigos[i];
                nuevosPadres[i] = _padre[i];
                nuevosRangos[i] = _rango[i];
            }

            _codigos = nuevosCodigos;
            _padre = nuevosPadres;
            _rango = nuevosRangos;
            _capacidad = nuevaCapacidad;
        }

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

        /// Registra una nueva subestación como su propio conjunto disjunto inicial (MakeSet).
        public int RegistrarSubestacion(string codigo)
        {
            int existente = ObtenerIndice(codigo);
            if (existente != -1) return existente;

            AsegurarCapacidad();
            int idx = _cantidad++;
            _codigos[idx] = codigo.Trim().ToUpper();
            _padre[idx] = idx;
            _rango[idx] = 0;
            return idx;
        }

        // Encuentra el representante de la zona eléctrica aplicando Compresión de Caminos (Path Compression).
        // Complejidad amortizada: O(α(n)) ≈ O(1).
        public int Find(int i)
        {
            if (i < 0 || i >= _cantidad) return -1;
            if (_padre[i] != i)
            {
                _padre[i] = Find(_padre[i]); // Compresión de camino
            }
            return _padre[i];
        }

        // Une dos zonas eléctricas aplicando Unión por Rango (Union by Rank).
        // Complejidad amortizada: O(α(n)) ≈ O(1).
        public bool Union(string codigoA, string codigoB)
        {
            int idxA = ObtenerIndice(codigoA);
            int idxB = ObtenerIndice(codigoB);
            if (idxA == -1 || idxB == -1) return false;

            int raizA = Find(idxA);
            int raizB = Find(idxB);

            if (raizA == raizB) return false; // Ya estaban en la misma zona

            if (_rango[raizA] < _rango[raizB])
            {
                _padre[raizA] = raizB;
            }
            else if (_rango[raizA] > _rango[raizB])
            {
                _padre[raizB] = raizA;
            }
            else
            {
                _padre[raizB] = raizA;
                _rango[raizA]++;
            }
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
            }
        }

        // Imprime las zonas eléctricas conectadas (componentes conexas) agrupadas por su nodo raíz.
        public void ImprimirZonasElectricas()
        {
            Console.WriteLine("\n--- ZONAS ELÉCTRICAS CONECTADAS (UNION-FIND) ---");
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
                    Console.Write($"Zona #{numZona++} (Raíz Representante: {_codigos[raiz]}): ");
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