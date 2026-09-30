using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class ResenaItem
    {
        public ResenaItem()
        {
        }

        [Key]
        public int Id { get; set; }

        // Según el Flujo Básico (Paso 5): la descripción es opcional, entre 20 y 100 caracteres.
        [StringLength(100, MinimumLength = 20, ErrorMessage = "La descripción debe tener entre 20 y 100 caracteres")]
        public string? Descripcion { get; set; }

        // Según el Flujo Básico (Paso 5): la calificación es obligatoria, valor entre 1 y 5.
        [Required(ErrorMessage = "La calificación es obligatoria")]
        [Range(1, 5, ErrorMessage = "La calificación debe ser un valor entre 1 y 5")]
        public int Calificacion { get; set; }

        // Relación N:1 con Libro
        public int LibroId { get; set; }
        
        // TODO: Descomentar estas líneas cuando la clase Libro esté en development
        // [ForeignKey("LibroId")]
        // public virtual Libro Libro { get; set; } = null!;

        // Relación N:1 con Resena
        public int ResenaId { get; set; }
        
        // TODO: Descomentar estas líneas cuando la clase Resena esté en development
        // [ForeignKey("ResenaId")]
        // public virtual Resena Resena { get; set; } = null!;

        public override bool Equals(object? obj)
        {
            if (obj is ResenaItem item)
            {
                return Id == item.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
