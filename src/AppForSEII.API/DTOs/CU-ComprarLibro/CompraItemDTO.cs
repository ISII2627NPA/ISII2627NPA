using System;

namespace AppForSEII.API.DTOs
{
    public class CompraItemDTO
    {
        public CompraItemDTO(int libroId, string titulo, string genero, string editorial, int cantidad, decimal precioCompra)
        {
            LibroId = libroId;
            Titulo = titulo;
            Genero = genero;
            Editorial = editorial;
            Cantidad = cantidad;
            PrecioCompra = precioCompra;
        }

        public int LibroId { get; set; }
        public string Titulo { get; set; }
        public string Genero { get; set; }
        public string Editorial { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioCompra { get; set; }

        public override bool Equals(object? obj)
        {
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