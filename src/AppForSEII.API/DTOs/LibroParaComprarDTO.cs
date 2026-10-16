using System;
using System.ComponentModel.DataAnnotations;

namespace ISII2627NPA.Shared.LibroDTOs 
{
    public class LibroParaComprarDTO
    {
        public LibroParaComprarDTO()
        {
        }

        
        public LibroParaComprarDTO(int id, string titulo, string genero, string editorial, DateTime fechaLanzamiento, double precioCompra)
        {
            Id = id;
            Titulo = titulo;
            Genero = genero;
            Editorial = editorial;
            FechaLanzamiento = fechaLanzamiento;
            PrecioCompra = precioCompra;
        }

        public int Id { get; set; }

        [StringLength(50, ErrorMessage = "El título debe tener una longitud máxima de 50 caracteres")]
        public string Titulo { get; set; } = "Título";

        [StringLength(50, ErrorMessage = "El género debe tener una longitud máxima de 50 caracteres", MinimumLength = 4)]
        public string Genero { get; set; } = "Ficción";

        [StringLength(50, ErrorMessage = "La editorial debe tener una longitud máxima de 50 caracteres")]
        public string Editorial { get; set; } = "Editorial";

        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        public DateTime FechaLanzamiento { get; set; } = DateTime.Today;

        [Required]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        [Range(1, float.MaxValue, ErrorMessage = "El precio mínimo es 1")]
        public double PrecioCompra { get; set; } = 1;

        public override bool Equals(object? obj)
        {
            return obj is LibroParaComprarDTO dTO &&
                   Id == dTO.Id &&
                   Titulo == dTO.Titulo &&
                   Genero == dTO.Genero &&
                   Editorial == dTO.Editorial &&
                   FechaLanzamiento == dTO.FechaLanzamiento &&
                   PrecioCompra == dTO.PrecioCompra;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Titulo, Genero, Editorial, FechaLanzamiento, PrecioCompra);
        }
    }
}