using System;
using System.Collections.Generic;
using AppForSEII.API.Models;

namespace AppForSEII.API.DTOs
{
    

    public class SubastaDetailDTO:SubastaForCreateDTO
    {
        
        public SubastaDetailDTO(int id, string usuarioUserName, string nombre, string apellidos, string telefono, 
            DateTime fechaSubasta, decimal precioSubasta, MetodoPago metodoPago, 
            IList<SubastaItemDTO> subastaItems)
            : base(usuarioUserName,
                   nombre,
                   apellidos,
                   telefono,
                   metodoPago,
                   subastaItems)
        {
            Id = id;
            FechaSubasta = fechaSubasta;
            PrecioSubasta = precioSubasta;
        }
        

        public int Id { get; set; }

        public DateTime FechaSubasta { get; set; }

        public decimal PrecioSubasta { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is SubastaDetailDTO dTO &&
                   
                   base.Equals(obj) &&
                   CompareDate(FechaSubasta, dTO.FechaSubasta) &&
                   
                   PrecioSubasta == dTO.PrecioSubasta &&
                   Id == dTO.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Id, FechaSubasta);
            
        }
    }
    
}