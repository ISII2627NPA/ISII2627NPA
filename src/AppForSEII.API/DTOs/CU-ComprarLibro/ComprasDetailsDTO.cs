using System;
using System.Collections.Generic;
using AppForSEII.API.Models;

namespace AppForSEII.API.DTOs
{
    // TODO: Cuando CompraForCreateDTO se fusione, descomenta la herencia y borra la línea actual
    // public class CompraDetailDTO : CompraForCreateDTO
    public class CompraDetailDTO
    {
       /* public CompraDetailDTO(int id, DateTime fechaCompra, string usuarioUserName, string nombre, string apellidos,
            string direccion, string telefono, MetodoPago metodoPago, string? codigoDescuento, IList<CompraItemDTO> compraItems)
            TODO: Descomentar esta llamada a base() cuando heredes de CompraForCreateDTO
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
*/
        public int Id { get; set; }

        public DateTime FechaCompra { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is CompraDetailDTO dTO &&
                   /* TODO: Descomentar estas comprobaciones cuando CompraForCreateDTO exista
                   base.Equals(obj) &&
                   PrecioTotal == dTO.PrecioTotal &&
                   CompareDate(FechaCompra, dTO.FechaCompra) &&
                   */
                   Id == dTO.Id;
        }

        public override int GetHashCode()
        {
            // TODO: Cuando CompraForCreateDTO exista, cambiar a: return HashCode.Combine(base.GetHashCode(), Id, FechaCompra);
            return HashCode.Combine(Id, FechaCompra);
        }
    }
}