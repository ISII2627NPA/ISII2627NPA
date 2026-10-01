using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.Models
{
    public class Subasta
    {
        public Subasta()
        {
            ApplicationUserId = string.Empty;
            // SubastaItems = new List<SubastaItem>(); // Descomentar luego
        }

        public Subasta(DateTime fechaSubasta, decimal precioSubasta, string applicationUserId, int metodoPagoId)
        {
            FechaSubasta = fechaSubasta;
            PrecioSubasta = precioSubasta;
            ApplicationUserId = applicationUserId;
            MetodoPagoId = metodoPagoId;
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha de la subasta es obligatoria.")]
        public DateTime FechaSubasta { get; set; }

        [Required(ErrorMessage = "El precio de la subasta es obligatorio.")]
        public decimal PrecioSubasta { get; set; }

        // public IList<SubastaItem> SubastaItems { get; set; } // Descomentar luego

        [Required(ErrorMessage = "El identificador del usuario es obligatorio.")]
        public string ApplicationUserId { get; set; }
        // public ApplicationUser Usuario { get; set; } // Descomentar luego

        [Required(ErrorMessage = "El método de pago es obligatorio.")]
        public int MetodoPagoId { get; set; }
        // public MetodoPago MetodoPago { get; set; } // Descomentar luego

        public override bool Equals(object? obj)
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