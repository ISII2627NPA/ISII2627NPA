using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    // FIX: Se elimina la duplicidad de código y se corrigen los atributos Display conflictivos
    public class Libro
    {
        public Libro()
        {
            Titulo = string.Empty;
        }

        public Libro(string titulo, decimal precio, int editorialId, int generoId)
        {
            Titulo = titulo;
            Precio = precio;
            EditorialId = editorialId;
            GeneroId = generoId;
        }

        [Key]
        public int Id { get; set; }

        [Display(Name = "Título del Libro")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "El título del libro es obligatorio")]
        public string Titulo { get; set; }

        [Display(Name = "Precio")]
        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, 10000.0, ErrorMessage = "El precio debe ser mayor que cero")]
        public decimal Precio { get; set; }

        // Relación N:1 con Editorial
        [Display(Name = "Editorial")]
        public int EditorialId { get; set; }
        
        [ForeignKey("EditorialId")]
        public virtual Editorial Editorial { get; set; } = null!;

        // Relación N:1 con Género
        [Display(Name = "Género")]
        public int GeneroId { get; set; }
        
        [ForeignKey("GeneroId")]
        public virtual Genero Genero { get; set; } = null!;

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