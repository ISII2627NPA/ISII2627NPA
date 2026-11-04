using System;

namespace AppForSEII.API.DTOs
{
    // Objeto de Transferencia de Datos (DTO) que representa un libro dentro de un pedido.
    // Su función es actuar como una "caja" ligera para enviar al cliente solo los datos exactos que necesita,
    // ocultando la entidad completa y pesada de la base de datos.
    public class CompraItemDTO
    {

        // Constructor parametrizado.
        // Obliga a que cualquier ítem de compra se cree con todos sus datos rellenos desde el primer momento.
        // Esto evita errores de variables nulas o datos a medias en el resto de la aplicación.
        public CompraItemDTO(int libroId, string titulo, string genero, string editorial, int cantidad, decimal precioCompra)
        {
            LibroId = libroId;
            Titulo = titulo;
            Genero = genero;
            Editorial = editorial;
            Cantidad = cantidad;
            PrecioCompra = precioCompra;
        }

        // Propiedades (get para leer el dato, set para modificarlo).
        // Representan las columnas de la "factura" de esta compra.
        public int LibroId { get; set; }
        public string Titulo { get; set; }
        public string Genero { get; set; }
        public string Editorial { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioCompra { get; set; }

        public override bool Equals(object? obj)
        {
            // Aquí le decimos que dos DTOs son iguales si sus números y textos son exactamente los mismos.
            return obj is CompraItemDTO dTO &&
                   LibroId == dTO.LibroId &&
                   Titulo == dTO.Titulo &&
                   Genero == dTO.Genero &&
                   Editorial == dTO.Editorial &&
                   Cantidad == dTO.Cantidad &&
                   PrecioCompra == dTO.PrecioCompra;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LibroId, Titulo, Genero, Editorial, Cantidad, PrecioCompra);
        }
    }
}