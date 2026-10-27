//ESTRUCTURA: Árbol de Prefijos (Trie) para Autocompletado y Búsqueda Léxica
using System;

namespace RedElectricaMunicipal.Estructuras
{
    public class EntradaSugerencia
    {
        public string NombreSubestacion;
        public string CodigoSubestacion;

        public EntradaSugerencia(string nombre, string codigo)
        {
            NombreSubestacion = nombre;
            CodigoSubestacion = codigo;
        }
    }

    public class NodoHijoTrie
    {
        public char Caracter;
        public NodoTrie Hijo;
        public NodoHijoTrie Siguiente;

        public NodoHijoTrie(char c, NodoTrie hijo)
        {
            Caracter = c;
            Hijo = hijo;
            Siguiente = null;
        }
    }

    public class NodoTrie
    {
        public NodoHijoTrie Hijos;
        public bool EsFinDePalabra;
        public string NombreOriginal;
        public string CodigoAsociado;

        public NodoTrie()
        {
            Hijos = null;
            EsFinDePalabra = false;
            NombreOriginal = null;
            CodigoAsociado = null;
        }

        public NodoTrie ObtenerHijo(char c)
        {
            NodoHijoTrie actual = Hijos;
            while (actual != null)
            {
                if (actual.Caracter == c)
                    return actual.Hijo;
                actual = actual.Siguiente;
            }
            return null;
        }

        public NodoTrie AgregarHijo(char c)
        {
            NodoTrie existente = ObtenerHijo(c);
            if (existente != null) return existente;

            NodoTrie nuevoNodo = new NodoTrie();
            NodoHijoTrie nuevoEnlace = new NodoHijoTrie(c, nuevoNodo);
            nuevoEnlace.Siguiente = Hijos;
            Hijos = nuevoEnlace;
            return nuevoNodo;
        }
    }

    public class TrieSubestaciones
    {
        private readonly NodoTrie _raiz;

        public TrieSubestaciones()
        {
            _raiz = new NodoTrie();
        }

        /// Inserta el nombre de una subestación vinculado a su código único. Complejidad: O(L).
        public void Insertar(string nombreSubestacion, string codigoSubestacion)
        {
            if (string.IsNullOrWhiteSpace(nombreSubestacion) || string.IsNullOrWhiteSpace(codigoSubestacion))
                return;

            string textoNormalizado = nombreSubestacion.Trim().ToUpper();
            NodoTrie actual = _raiz;

            for (int i = 0; i < textoNormalizado.Length; i++)
            {
                char c = textoNormalizado[i];
                actual = actual.AgregarHijo(c);
            }

            actual.EsFinDePalabra = true;
            actual.NombreOriginal = nombreSubestacion.Trim();
            actual.CodigoAsociado = codigoSubestacion.Trim().ToUpper();
        }

        /// Elimina el nombre de una subestación del Trie. Complejidad: O(L).
        public bool Eliminar(string nombreSubestacion)
        {
            if (string.IsNullOrWhiteSpace(nombreSubestacion)) return false;
            string textoNormalizado = nombreSubestacion.Trim().ToUpper();
            NodoTrie actual = _raiz;

            for (int i = 0; i < textoNormalizado.Length; i++)
            {
                actual = actual.ObtenerHijo(textoNormalizado[i]);
                if (actual == null) return false;
            }

            if (!actual.EsFinDePalabra) return false;
            actual.EsFinDePalabra = false;
            actual.NombreOriginal = null;
            actual.CodigoAsociado = null;
            return true;
        }

        /// Devuelve hasta maxResultados sugerencias que comienzan con el prefijo indicado.
        /// Complejidad: O(P + N_subarbol).
        public EntradaSugerencia[] BuscarPorPrefijo(string prefijo, int maxResultados = 10)
        {
            if (prefijo == null) prefijo = "";
            string prefijoNorm = prefijo.Trim().ToUpper();
            NodoTrie actual = _raiz;

            for (int i = 0; i < prefijoNorm.Length; i++)
            {
                actual = actual.ObtenerHijo(prefijoNorm[i]);
                if (actual == null)
                    return new EntradaSugerencia[0];
            }

            EntradaSugerencia[] temporal = new EntradaSugerencia[maxResultados];
            int encontrados = 0;
            RecolectarSugerencias(actual, temporal, ref encontrados, maxResultados);

            EntradaSugerencia[] resultado = new EntradaSugerencia[encontrados];
            for (int i = 0; i < encontrados; i++)
            {
                resultado[i] = temporal[i];
            }
            return resultado;
        }

        private void RecolectarSugerencias(NodoTrie nodo, EntradaSugerencia[] acumulador, ref int contador, int limite)
        {
            if (nodo == null || contador >= limite) return;

            if (nodo.EsFinDePalabra)
            {
                acumulador[contador++] = new EntradaSugerencia(nodo.NombreOriginal, nodo.CodigoAsociado);
            }

            NodoHijoTrie hijoActual = nodo.Hijos;
            while (hijoActual != null && contador < limite)
            {
                RecolectarSugerencias(hijoActual.Hijo, acumulador, ref contador, limite);
                hijoActual = hijoActual.Siguiente;
            }
        }

        /// Imprime todas las subestaciones almacenadas en el Trie en recorrido en profundidad.
        public void ImprimirTodoElTrie()
        {
            Console.WriteLine("\n DIRECTORIO LÉXICO DE SUBESTACIONES EN EL TRIE");
            EntradaSugerencia[] todas = BuscarPorPrefijo("", 500);
            if (todas.Length == 0)
            {
                Console.WriteLine("El Trie está vacío.");
                return;
            }
            for (int i = 0; i < todas.Length; i++)
            {
                Console.WriteLine($"  {i + 1}. {todas[i].NombreSubestacion} --> Código: [{todas[i].CodigoSubestacion}]");
            }
        }
    }
}