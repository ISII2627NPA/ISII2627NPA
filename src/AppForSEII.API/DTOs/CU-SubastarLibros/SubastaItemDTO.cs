using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs
{
    public class SubastaItemDTO
    {
        public SubastaItemDTO(int libroId, string titulo, string autor, string editorial, decimal precioPuja, string? descripcion = "")
        {
            LibroId = libroId;
            Titulo = titulo;
            Autor = autor;
            Editorial = editorial;
            PrecioPuja = precioPuja;
            Descripcion = descripcion;
        }

        public SubastaItemDTO()
        {
        }

        public int LibroId { get; set; }

        public string Titulo { get; set; }

        public string Autor { get; set; }

        public string Editorial { get; set; }

        [Required(ErrorMessage = "El precio de la puja es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio de puja debe ser mayor que 0")]
        public decimal PrecioPuja { get; set; }

        [StringLength(100, MinimumLength = 20, ErrorMessage = "La descripción adicional debe tener entre 20 y 100 caracteres")]
        public string? Descripcion { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is SubastaItemDTO dTO &&
                   LibroId == dTO.LibroId &&
                   Titulo == dTO.Titulo &&
                   Autor == dTO.Autor &&
                   Editorial == dTO.Editorial &&
                   PrecioPuja == dTO.PrecioPuja &&
                   Descripcion == dTO.Descripcion;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LibroId, Titulo, Autor, Editorial, PrecioPuja, Descripcion);
        }
    }
}