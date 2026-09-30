using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class Libro
    {
        public Libro()
        {
            Titulo = string.Empty;
            Autor = string.Empty;
        }

        public Libro(string titulo, string autor, DateTime fechaLanzamiento, decimal precioCompra, int stock, int editorialId, int generoId)
        {
            Titulo = titulo;
            Autor = autor;
            FechaLanzamiento = fechaLanzamiento;
            PrecioCompra = precioCompra;
            Stock = stock;
            EditorialId = editorialId;
            GeneroId = generoId;
        }

        [Key]
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El título del libro es obligatorio")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "El autor es obligatorio")]
        public string Autor { get; set; }

        [Required(ErrorMessage = "La fecha de lanzamiento es obligatoria")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        public DateTime FechaLanzamiento { get; set; }

        [Required(ErrorMessage = "El precio de compra es obligatorio")]
        [Range(0.01, 10000.0, ErrorMessage = "El precio debe ser mayor que cero")]
        public decimal PrecioCompra { get; set; }

        [Required(ErrorMessage = "El stock es obligatorio")]
        public int Stock { get; set; }

        // Relación N:1 con Editorial
        public int EditorialId { get; set; }
        
        //TODO: Descomentar estas lineas cuando la clase Editorial se intregre en development
        // [ForeignKey("EditorialId")]
        // public Editorial Editorial { get; set; }

        // Relación N:1 con Genero
        public int GeneroId { get; set; }
        
        //TODO: Descomentar estas lineas cuando la clase Genero se intregre en development
        // [ForeignKey("GeneroId")]
        // public Genero Genero { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is Libro libro)
            {
                return Id == libro.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}