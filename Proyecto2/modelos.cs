public class Subestacion
{
    public string Codigo { get; set; }
    public string Nombre { get; set; }
    public double Capacidad { get; set; }
}

public class LineaDistribucion
{
    public string CodigoOrigen { get; set; }
    public string CodigoDestino { get; set; }
    public double Longitud { get; set; }
    public double CostoMantenimiento { get; set; }
}

public class NodoLista<T>
{
    public T Valor { get; set; }
    public NodoLista<T> Siguiente { get; set; }

    public NodoLista(T valor)
    {
        Valor = valor;
        Siguiente = null;
    }
}

public class ListaPersonalizada<T>
{
    public NodoLista<T> Cabeza { get; set; }
    
    public void Agregar(T valor)
    {
        // Para agregar al final o al inicio
    }

    // Metodos Buscar, Eliminar, etc
}