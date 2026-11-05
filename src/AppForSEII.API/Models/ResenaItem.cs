using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    // [PrimaryKey] define una Clave Primaria Compuesta (no hay un "Id" único, la clave es la suma del Libro y la Reseña).
    [PrimaryKey(nameof(LibroId), nameof(ResenaId))]
    public class ResenaItem
    {
        public ResenaItem()
        {
        }

        public ResenaItem(string? descripcion, int calificacion, int libroId, int resenaId)
        {
            Descripcion = descripcion;
            Calificacion = calificacion;
            LibroId = libroId;
            ResenaId = resenaId;
        }
        
        [StringLength(100, MinimumLength = 20, ErrorMessage = "La descripción debe tener entre 20 y 100 caracteres")]
        public string? Descripcion { get; set; }
       
        // [Range] restringe los valores numéricos válidos en base de datos.
        [Required(ErrorMessage = "La calificación es obligatoria")]
        [Range(1, 5, ErrorMessage = "La calificación debe ser un valor entre 1 y 5")]
        public int Calificacion { get; set; }

        // --- RELACIONES ---
        // Almacena el ID del Libro valorado.
        public int LibroId { get; set; }
        
        // [ForeignKey] indica a EF Core explícitamente a qué tabla pertenece el ID anterior.
        [ForeignKey("LibroId")]
        public Libro Libro { get; set; }

        public int ResenaId { get; set; }
        
        [ForeignKey("ResenaId")]
        public Resena Resena { get; set; }   

        // Al usar Clave Primaria Compuesta, Equals exige que ambos IDs coincidan.
        public override bool Equals(object? obj)
        {
            if (obj is ResenaItem item)
            {
                return LibroId == item.LibroId && ResenaId == item.ResenaId;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LibroId, ResenaId);
        }
    }
}