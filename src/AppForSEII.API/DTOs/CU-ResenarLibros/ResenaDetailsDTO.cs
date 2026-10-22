using System;
using System.Collections.Generic;
using AppForSEII.API.Models;

namespace AppForSEII.API.DTOs
{
    // Descomentar cuando exista ResenaForCreateDTO
    // public class ResenaDetailDTO : ResenaForCreateDTO
    public class ResenaDetailDTO
    {
       /* public ResenaDetailDTO(int id, DateTime fechaResena, string usuarioUserName, string nombre, string apellidos,
            string direccion, string telefono, string titulo, IList<ResenaItemDTO> resenaItems)
            : base(usuarioUserName,
                   nombre,
                   apellidos,
                   direccion,
                   telefono,
                   titulo,
                   resenaItems)
            
        {
            Id = id;
            FechaResena = fechaResena;
        }
*/
        public int Id { get; set; }

        public DateTime FechaResena { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is ResenaDetailDTO dTO &&
                   /* Descomentar cuando exista ResenaForCreateDTO
                   base.Equals(obj) &&
                   CompareDate(FechaResena, dTO.FechaResena) &&
                   */
                   Id == dTO.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Id, FechaResena);
        }
    }
}