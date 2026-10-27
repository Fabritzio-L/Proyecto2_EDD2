// ESTRUCTURA: Tabla Hash con Encadenamiento Separado (Separate Chaining) y Rehashing Dinámico

using System;
using RedElectricaMunicipal.Modelos;

namespace RedElectricaMunicipal.Estructuras
{
    public class NodoHash
    {
        public string Clave;
        public Subestacion Valor;
        public NodoHash Siguiente;

        public NodoHash(string clave, Subestacion valor)
        {
            Clave = clave;
            Valor = valor;
            Siguiente = null;
        }
    }

    public class TablaHashSubestaciones
    {
        private NodoHash[] _cubetas;
        private int _capacidad;
        private int _cantidadElementos;
        private const double FACTOR_CARGA_MAXIMO = 0.75;

        public int Cantidad => _cantidadElementos;

        public TablaHashSubestaciones(int capacidadInicial = 17)
        {
            _capacidad = capacidadInicial;
            _cubetas = new NodoHash[_capacidad];
            _cantidadElementos = 0;
        }

        // Función Hash Polinómica (algoritmo DJB2 adaptado) para distribuir claves uniformemente.
        private int FuncionHash(string clave)
        {
            string claveNormalizada = clave.Trim().ToUpper();
            ulong hash = 5381;
            for (int i = 0; i < claveNormalizada.Length; i++)
            {
                hash = ((hash << 5) + hash) + claveNormalizada[i];
            }
            return (int)(hash % (ulong)_capacidad);
        }
        public bool Insertar(Subestacion subestacion)
        {
            if (subestacion == null || string.IsNullOrWhiteSpace(subestacion.Codigo))
                return false;

            if ((double)(_cantidadElementos + 1) / _capacidad > FACTOR_CARGA_MAXIMO)
            {
                Rehash();
            }

            int indice = FuncionHash(subestacion.Codigo);
            NodoHash actual = _cubetas[indice];

            while (actual != null)
            {
                if (actual.Clave.Equals(subestacion.Codigo, StringComparison.OrdinalIgnoreCase))
                {
                    actual.Valor = subestacion;
                    return false;
                }
                actual = actual.Siguiente;
            }

            NodoHash nuevo = new NodoHash(subestacion.Codigo, subestacion);
            nuevo.Siguiente = _cubetas[indice];
            _cubetas[indice] = nuevo;
            _cantidadElementos++;
            return true;
        }
        public Subestacion Buscar(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return null;
            string clave = codigo.Trim().ToUpper();
            int indice = FuncionHash(clave);
            NodoHash actual = _cubetas[indice];

            while (actual != null)
            {
                if (actual.Clave.Equals(clave, StringComparison.OrdinalIgnoreCase))
                {
                    return actual.Valor;
                }
                actual = actual.Siguiente;
            }
            return null;
        }

        public bool Eliminar(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return false;
            string clave = codigo.Trim().ToUpper();
            int indice = FuncionHash(clave);
            NodoHash actual = _cubetas[indice];
            NodoHash anterior = null;

            while (actual != null)
            {
                if (actual.Clave.Equals(clave, StringComparison.OrdinalIgnoreCase))
                {
                    if (anterior == null)
                        _cubetas[indice] = actual.Siguiente;
                    else
                        anterior.Siguiente = actual.Siguiente;

                    _cantidadElementos--;
                    return true;
                }
                anterior = actual;
                actual = actual.Siguiente;
            }
            return false;
        }

        // Duplica la capacidad de la tabla y reubica los registros cuando se supera el factor de carga.
        // Complejidad: O(n).
        private void Rehash()
        {
            int nuevaCapacidad = _capacidad * 2 + 1;
            NodoHash[] viejasCubetas = _cubetas;

            _cubetas = new NodoHash[nuevaCapacidad];
            _capacidad = nuevaCapacidad;
            _cantidadElementos = 0;

            for (int i = 0; i < viejasCubetas.Length; i++)
            {
                NodoHash actual = viejasCubetas[i];
                while (actual != null)
                {
                    Insertar(actual.Valor);
                    actual = actual.Siguiente;
                }
            }
        }

        // Devuelve un arreglo propio con todas las subestaciones registradas. Complejidad: O(n + m).
        public Subestacion[] ObtenerTodas()
        {
            Subestacion[] resultado = new Subestacion[_cantidadElementos];
            int contador = 0;

            for (int i = 0; i < _capacidad; i++)
            {
                NodoHash actual = _cubetas[i];
                while (actual != null)
                {
                    if (contador < _cantidadElementos)
                    {
                        resultado[contador++] = actual.Valor;
                    }
                    actual = actual.Siguiente;
                }
            }
            return resultado;
        }

        // Recorre e imprime el estado interno de las cubetas y colisiones de la Tabla Hash.
        public void ImprimirEstructuraHash()
        {
            Console.WriteLine($"\n--- ESTADO DE LA TABLA HASH (Capacidad: {_capacidad}, Elementos: {_cantidadElementos}) ---");
            for (int i = 0; i < _capacidad; i++)
            {
                if (_cubetas[i] != null)
                {
                    Console.Write($"Cubeta [{i:D2}]: ");
                    NodoHash actual = _cubetas[i];
                    while (actual != null)
                    {
                        Console.Write($"({actual.Valor.Codigo}: {actual.Valor.Nombre}) -> ");
                        actual = actual.Siguiente;
                    }
                    Console.WriteLine("NULL");
                }
            }
        }
    }
}