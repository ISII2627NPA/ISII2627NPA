using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models // ¡Ojo! Asegúrate de que este namespace coincida con el de tus otras clases
{
    public class Resena
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime FechaResena { get; set; }

        [Required]
        [StringLength(200)] 
        public string Titulo { get; set; }

       
        public string UsuarioId { get; set; } 
        [ForeignKey("UsuarioId")]
        public virtual ApplicationUser Usuario { get; set; }

       
        
        // public virtual ICollection<ResenaItem> ResenaItems { get; set; }

        public Resena()
        {
            // ResenaItems = new List<ResenaItem>();
        }
    }
}