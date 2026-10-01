using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class Compra
    {
        public Compra()
        {
            ApplicationUserId = string.Empty;
        }

        public Compra(string applicationUserId, int metodoPagoId, DateTime fechaCompra, decimal precioTotal, string? codigoDescuento)
        {
            ApplicationUserId = applicationUserId;
            MetodoPagoId = metodoPagoId;
            FechaCompra = fechaCompra;
            PrecioTotal = precioTotal;
            CodigoDescuento = codigoDescuento;
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha de compra es obligatoria")]
        public DateTime FechaCompra { get; set; }

        [Required(ErrorMessage = "El precio total es obligatorio")]
        public decimal PrecioTotal { get; set; }

        
        [StringLength(10, MinimumLength = 5, ErrorMessage = "El código de descuento debe tener entre 5 y 10 caracteres")]
        public string? CodigoDescuento { get; set; }

        // Relación N:1 con ApplicationUser
        [Required(ErrorMessage = "El identificador del usuario es obligatorio")]
        public string ApplicationUserId { get; set; } 
        
        
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }

        // Relación N:1 con MetodoPago
        public int MetodoPagoId { get; set; }
        
        
        [ForeignKey("MetodoPagoId")]
        public MetodoPago MetodoPago { get; set; }

        // Relación 1:N con CompraItem
        public List<CompraItem> CompraItems { get; set; } = new List<CompraItem>();

        public override bool Equals(object? obj)
        {
            if (obj is Compra c)
            {
                return Id == c.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}