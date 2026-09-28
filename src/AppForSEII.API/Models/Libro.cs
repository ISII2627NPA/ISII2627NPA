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

        
        [Required(AllowEmptyStrings = false, ErrorMessage = "El título del libro es obligatorio")]
        public string Titulo { get; set; }

       
        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, 10000.0, ErrorMessage = "El precio debe ser mayor que cero")]
        public decimal Precio { get; set; }

        // Relación N:1 con Editorial
        public int EditorialId { get; set; }
        
        //TODO: Descomentar estas lineas cuando la clase Editorial se intregre en development
       // [ForeignKey("EditorialId")]
        //public virtual Editorial Editorial { get; set; } = null!;

        
      //Relaccion N:1 con Genero
        public int GeneroId { get; set; }
        
        //TODO: Descomentar estas lineas cuando la clase Genero se intregre en development
       // [ForeignKey("GeneroId")]
       // public virtual Genero Genero { get; set; } = null!;

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