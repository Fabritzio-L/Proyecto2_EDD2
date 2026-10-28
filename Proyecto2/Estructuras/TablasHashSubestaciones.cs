// ESTRUCTURA: Tabla Hash con Encadenamiento Separado 
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
            _capacidad = EsPrimo(capacidadInicial) ? capacidadInicial : ObtenerSiguientePrimo(capacidadInicial);
            _cubetas = new NodoHash[_capacidad];
            _cantidadElementos = 0;
        }

        // Función Hash Polinómica 
        private int FuncionHash(string claveNormalizada)
        {
            ulong hash = 5381;
            for (int i = 0; i < claveNormalizada.Length; i++)
            {
                hash = ((hash << 5) + hash) + claveNormalizada[i]; // hash * 33 + c
            }
            return (int)(hash % (ulong)_capacidad);
        }

        // Inserta o actualiza una subestación en la Tabla Hash.
        // Complejidad temporal: O(1) promedio | O(n) peor caso (si ocurre Rehash o todas colisionan).
        public bool Insertar(Subestacion subestacion)
        {
            if (subestacion == null || string.IsNullOrWhiteSpace(subestacion.Codigo))
                return false;

            string claveNorm = subestacion.Codigo.Trim().ToUpper();
            int indice = FuncionHash(claveNorm);
            NodoHash actual = _cubetas[indice];

            // 1. Verificar si la clave ya existe para actualizar el registro sin duplicar nodos
            while (actual != null)
            {
                if (actual.Clave == claveNorm)
                {
                    actual.Valor = subestacion;
                    return false; // Retorna false indicando que fue actualización y no inserción nueva
                }
                actual = actual.Siguiente;
            }

            // 2. Verificar factor de carga antes de insertar un nuevo nodo
            if ((double)(_cantidadElementos + 1) / _capacidad > FACTOR_CARGA_MAXIMO)
            {
                Rehash();
                indice = FuncionHash(claveNorm); // Recalcular índice con la nueva capacidad prima
            }

            // 3. Insertar al inicio de la lista enlazada (O(1))
            NodoHash nuevo = new NodoHash(claveNorm, subestacion);
            nuevo.Siguiente = _cubetas[indice];
            _cubetas[indice] = nuevo;
            _cantidadElementos++;
            return true;
        }

        // Localiza una subestación a partir de su código único.
        // Complejidad temporal: O(1) promedio | O(k) peor caso (k = elementos en el mismo espacio).
        public Subestacion Buscar(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return null;

            string claveNorm = codigo.Trim().ToUpper();
            int indice = FuncionHash(claveNorm);
            NodoHash actual = _cubetas[indice];

            while (actual != null)
            {
                if (actual.Clave == claveNorm)
                {
                    return actual.Valor;
                }
                actual = actual.Siguiente;
            }
            return null;
        }

        // Elimina una subestación de la Tabla Hash desenlazando su nodo en la cubeta correspondiente.
        // Complejidad temporal: O(1) promedio | O(k) peor caso.
        public bool Eliminar(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return false;

            string claveNorm = codigo.Trim().ToUpper();
            int indice = FuncionHash(claveNorm);
            NodoHash actual = _cubetas[indice];
            NodoHash anterior = null;

            while (actual != null)
            {
                if (actual.Clave == claveNorm)
                {
                    if (anterior == null)
                        _cubetas[indice] = actual.Siguiente;
                    else
                        anterior.Siguiente = actual.Siguiente;

                    actual.Siguiente = null;
                    _cantidadElementos--;
                    return true;
                }
                anterior = actual;
                actual = actual.Siguiente;
            }
            return false;
        }


        private void Rehash()
        {
            int nuevaCapacidad = ObtenerSiguientePrimo(_capacidad * 2);
            NodoHash[] viejasCubetas = _cubetas;

            _cubetas = new NodoHash[nuevaCapacidad];
            _capacidad = nuevaCapacidad;

            for (int i = 0; i < viejasCubetas.Length; i++)
            {
                NodoHash actual = viejasCubetas[i];
                while (actual != null)
                {
                    NodoHash siguienteTemp = actual.Siguiente;

                    // Reubicar el mismo nodo directamente en su nueva cubeta en O(1)
                    int nuevoIndice = FuncionHash(actual.Clave);
                    actual.Siguiente = _cubetas[nuevoIndice];
                    _cubetas[nuevoIndice] = actual;

                    actual = siguienteTemp;
                }
            }
        }

        // Devuelve un arreglo propio con todas las subestaciones registradas.
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

        public void ImprimirEstructuraHash()
        {
            double factorCarga = _capacidad > 0 ? (double)_cantidadElementos / _capacidad : 0;
            int cubetasOcupadas = 0;
            int totalColisiones = 0;

            Console.WriteLine($"\n--- ESTADO DE LA TABLA HASH (Capacidad Prima: {_capacidad}, Elementos: {_cantidadElementos}, Factor Carga: {factorCarga:F2}/{FACTOR_CARGA_MAXIMO}) ---");

            for (int i = 0; i < _capacidad; i++)
            {
                if (_cubetas[i] != null)
                {
                    cubetasOcupadas++;
                    Console.Write($"Cubeta [{i:D2}]: ");
                    NodoHash actual = _cubetas[i];
                    int nodosEnCubeta = 0;

                    while (actual != null)
                    {
                        nodosEnCubeta++;
                        Console.Write($"({actual.Valor.Codigo}: {actual.Valor.Nombre}) -> ");
                        actual = actual.Siguiente;
                    }
                    if (nodosEnCubeta > 1)
                    {
                        totalColisiones += (nodosEnCubeta - 1);
                    }
                    Console.WriteLine("NULL");
                }
            }

            Console.WriteLine($"[Métricas Hash] Cubetas usadas: {cubetasOcupadas}/{_capacidad} | Colisiones encadenadas: {totalColisiones}");
        }

        // Métodos auxiliares matemáticos propios para garantizar capacidades primas
        private static int ObtenerSiguientePrimo(int numeroMinimo)
        {
            int candidato = (numeroMinimo % 2 == 0) ? numeroMinimo + 1 : numeroMinimo;
            while (!EsPrimo(candidato))
            {
                candidato += 2;
            }
            return candidato;
        }

        private static bool EsPrimo(int n)
        {
            if (n <= 1) return false;
            if (n == 2) return true;
            if (n % 2 == 0) return false;

            for (int i = 3; i * i <= n; i += 2)
            {
                if (n % i == 0) return false;
            }
            return true;
        }
    }
}