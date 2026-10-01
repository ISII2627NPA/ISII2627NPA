using System;
using System.Collections.Generic;
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
            TipoLibro = string.Empty;
        }

        public Libro(string titulo, string autor, DateTime fechaLanzamiento, decimal precioCompra, int stock, int editorialId, int generoId, string tipoLibro,decimal calificacionMedia)
        {
            Titulo = titulo;
            Autor = autor;
            FechaLanzamiento = fechaLanzamiento;
            PrecioCompra = precioCompra;
            Stock = stock;
            EditorialId = editorialId;
            GeneroId = generoId;
            TipoLibro = tipoLibro;
            CalificacionMedia = calificacionMedia;
        }

        [Key]
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El título del libro es obligatorio")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "El autor es obligatorio")]
        public string Autor { get; set; }


        [Required(ErrorMessage = "La calificación media es obligatoria")]
        [Range(0, 5, ErrorMessage = "La calificación media debe estar entre 0 y 5")]
        public decimal CalificacionMedia { get; set; }


        [Required(ErrorMessage = "El tipo de libro es obligatorio")]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "El tipo de libro debe tener entre 10 y 50 caracteres")]
        public string TipoLibro { get; set; }


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
        
    
        [ForeignKey("EditorialId")]
        public Editorial Editorial { get; set; }

        // Relación N:1 con Genero
        public int GeneroId { get; set; }
        
        
        [ForeignKey("GeneroId")]
        public Genero Genero { get; set; }

        // Relación 1:N con SubastaItem (Caso de Uso 3)
        public IList<SubastaItem> SubastaItems { get; set; }

        // Relación 1:N con ResenaItem (Caso de Uso 4)
       public IList<ResenaItem> ResenaItems { get; set; }

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