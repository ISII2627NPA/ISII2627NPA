using System;
using System.Collections.Generic;
using AppForSEII.API.Models;

namespace AppForSEII.API.DTOs
{
    
    // Objeto de Transferencia de Datos (DTO) de "Detalle".
    // A diferencia del de creación, este se usa para devolver los datos de una compra YA EXISTENTE en la base de datos.
    // Hereda (:) de CompraForCreateDTO para reutilizar todas sus propiedades (nombre, dirección, items, etc.) sin reescribirlas.
    public class CompraDetailDTO : CompraForCreateDTO
   
    {

        // Constructor parametrizado.
        //  El ": base(...)". Esto llama al constructor de la clase padre (CompraForCreateDTO) 
        // para que inicialice los datos compartidos. Luego, el bloque {} inicializa los datos exclusivos de este DTO.
        public CompraDetailDTO(int id, DateTime fechaCompra,decimal precioTotal, string usuarioUserName, string nombre, string apellidos,
            string direccion, string telefono, int metodoPagoId, string? codigoDescuento, IList<CompraItemDTO> compraItems)
           
            : base(usuarioUserName,
                   nombre,
                   apellidos,
                   direccion,
                   telefono,
                   metodoPagoId,
                   codigoDescuento,
                   compraItems)
            
        {
            Id = id;
            FechaCompra = fechaCompra;
            PrecioTotal = precioTotal;
        }

        // Propiedades exclusivas del detalle de la compra (lo que ya existe en base de datos)
        public int Id { get; set; }

        public DateTime FechaCompra { get; set; }

    // Sobrescritura del método Equals para pruebas unitarias.
        // 1. base.Equals(obj): Primero comprueba que todos los datos de la clase padre sean idénticos por valor.
        // 2. Comprueba que las propiedades exclusivas de esta clase (Id, FechaCompra y PrecioTotal) también coincidan.
        // 3. Usa CompareDate() porque las fechas en C# pueden tener milisegundos invisibles que harían fallar el test.
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