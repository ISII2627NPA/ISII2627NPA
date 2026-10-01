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
            Titulo = string.Empty;
            ApplicationUserId = string.Empty;
        }

        public Resena(string applicationUserId, DateTime fechaResena, string titulo)
        {
            ApplicationUserId = applicationUserId;
            FechaResena = fechaResena;
            Titulo = titulo;
        }


        [Key]
        public int Id { get; set; } 
        
        [Required(ErrorMessage = "El identificador del usuario es obligatorio")]
        public string ApplicationUserId { get; set; }

        [Required(ErrorMessage = "La fecha de la reseña es obligatoria")]
        public DateTime FechaResena { get; set; }

        [Required(ErrorMessage = "El título de la reseña es obligatorio")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "El título de la reseña debe tener entre 10 y 20 caracteres")]
        public string Titulo { get; set; }

        // --- RELACIONES ---
        [Required(ErrorMessage = "El identificador del usuario es obligatorio")]
        public string UsuarioId { get; set; }
        
        
        // public ApplicationUser Usuario { get; set; } // Descomentar luego

        // public IList<ResenaItem> ResenaItems { get; set; } // Descomentar luego

      
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