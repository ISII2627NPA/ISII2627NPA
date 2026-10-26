using System;
using System.Collections.Generic;
using AppForSEII.API.Models;

namespace AppForSEII.API.DTOs
{
    
    public class CompraDetailDTO : CompraForCreateDTO
   
    {
        public CompraDetailDTO(int id, DateTime fechaCompra, string usuarioUserName, string nombre, string apellidos,
            string direccion, string telefono, MetodoPago metodoPago, string? codigoDescuento, IList<CompraItemDTO> compraItems)
           
            : base(usuarioUserName,
                   nombre,
                   apellidos,
                   direccion,
                   telefono,
                   metodoPago,
                   codigoDescuento,
                   compraItems)
            
        {
            Id = id;
            FechaCompra = fechaCompra;
        }

        public int Id { get; set; }

        public DateTime FechaCompra { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is CompraDetailDTO dTO &&
                  
                   base.Equals(obj) &&
                   PrecioTotal == dTO.PrecioTotal &&
                   CompareDate(FechaCompra, dTO.FechaCompra) &&
                   
                   Id == dTO.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Id, FechaCompra);
           
        }
    }
}