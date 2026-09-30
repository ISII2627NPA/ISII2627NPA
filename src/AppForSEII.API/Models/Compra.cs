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
        }

        public Compra(DateTime fechaCompra, decimal precioTotal, string? codigoDescuento)
        {
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

        // Restricción del flujo: Opcional (string?) pero entre 5 y 10 caracteres
        [StringLength(10, MinimumLength = 5, ErrorMessage = "El código de descuento debe tener entre 5 y 10 caracteres")]
        public string? CodigoDescuento { get; set; }

        

        // Relación N:1 con ApplicationUser
        // TODO: Descomentar cuando ApplicationUser se integre en development
        // public string UsuarioId { get; set; } 
        // [ForeignKey("UsuarioId")]
        // public ApplicationUser Usuario { get; set; }

        // Relación N:1 con MetodoPago
        public int MetodoPagoId { get; set; }
        [ForeignKey("MetodoPagoId")]
        public MetodoPago MetodoPago { get; set; }

        // Relación 1:N con CompraItem
        // TODO: Descomentar cuando CompraItem se integre en development
        // public List<CompraItem> CompraItems { get; set; } = new List<CompraItem>();

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