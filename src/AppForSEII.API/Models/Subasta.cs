using System;
using System.ComponentModel.DataAnnotations;

namespace ISII2627NPA.Models
{
    public class Subasta
    {

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha de la subasta es obligatoria.")]
        public DateTime FechaSubasta { get; set; }

        [Required(ErrorMessage = "El precio de la subasta es obligatorio.")]
        public decimal PrecioSubasta { get; set; }

        public Subasta()
        {
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            
            Subasta otraSubasta = (Subasta)obj;
            return this.Id == otraSubasta.Id;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}