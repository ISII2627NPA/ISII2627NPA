using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs
{
    public class LibroParaComprarDTO
    {
        public LibroParaComprarDTO()
        {
        }

        public LibroParaComprarDTO(int id, string titulo, string genero, string editorial, string autor, DateTime fechaLanzamiento, double precioCompra, int stock)
        {
            Id = id;
            Titulo = titulo;
            Genero = genero;
            Editorial = editorial;
            Autor = autor;
            FechaLanzamiento = fechaLanzamiento;
            PrecioCompra = precioCompra;
            Stock = stock;
        }

        public int Id { get; set; }

        public string Titulo { get; set; } = "Título";

        public string Genero { get; set; } = "Ficción";

        public string Editorial { get; set; } = "Editorial";

        [Required(ErrorMessage = "El autor es obligatorio")]
        public string Autor { get; set; } = "Autor";

        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        public DateTime FechaLanzamiento { get; set; } = DateTime.Today;

        [Required]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        [Range(1, float.MaxValue, ErrorMessage = "El precio mínimo es 1")]
        public double PrecioCompra { get; set; } = 1;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "El stock debe ser mayor que 0")]
        public int Stock { get; set; } = 1;

        public override bool Equals(object? obj)
        {
            return obj is LibroParaComprarDTO dTO &&
                   Id == dTO.Id &&
                   Titulo == dTO.Titulo &&
                   Genero == dTO.Genero &&
                   Editorial == dTO.Editorial &&
                   Autor == dTO.Autor &&
                   FechaLanzamiento == dTO.FechaLanzamiento &&
                   PrecioCompra == dTO.PrecioCompra &&
                   Stock == dTO.Stock;
        }

        public override int GetHashCode()
        {
            // HashCode.Combine admite un máximo de 8 parámetros exactos, por lo que encaja perfecto
            return HashCode.Combine(Id, Titulo, Genero, Editorial, Autor, FechaLanzamiento, PrecioCompra, Stock);
        }
    }
}