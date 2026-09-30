using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class Resena
    {
        public Resena()
        {
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha de la reseña es obligatoria")]
        public DateTime FechaResena { get; set; }

        [Required(ErrorMessage = "El título de la reseña es obligatorio")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "El título de la reseña debe tener entre 10 y 20 caracteres")]
        public string Titulo { get; set; }

        // --- RELACIONES ---
        [Required(ErrorMessage = "El identificador del usuario es obligatorio")]
        public string UsuarioId { get; set; }
        
        
        // [ForeignKey("UsuarioId")]
        // public virtual ApplicationUser Usuario { get; set; } = null!;

        
        // public virtual ICollection<ResenaItem> ResenaItems { get; set; } = new List<ResenaItem>();

      
        public override bool Equals(object? obj)
        {
            if (obj is Resena resena)
            {
                return Id == resena.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}