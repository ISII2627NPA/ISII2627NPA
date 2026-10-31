using System;
using System.Collections.Generic;

namespace AppForSEII.API.DTOs
{
    public class ResenaDetailDTO : ResenaForCreateDTO
    {
        public ResenaDetailDTO(int id, DateTime fechaResena, string usuarioUserName, string nombre, string apellidos,
            string direccion, string telefono, string titulo, IList<ResenaItemDTO> resenaItems)
            : base(usuarioUserName, nombre, apellidos, direccion, telefono, titulo, resenaItems)
        {
            Id = id;
            FechaResena = fechaResena;
        }

        public int Id { get; set; }
        public DateTime FechaResena { get; set; }

        protected bool CompareDate(DateTime date1, DateTime date2)
        {
        return date1.Subtract(date2).Duration() < new TimeSpan(0, 1, 0);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not ResenaDetailDTO dTO) return false;

            // Comprobamos que la lista de libros de dentro también es exactamente igual
            bool itemsEqual = (ResenaItems == null && dTO.ResenaItems == null) ||
                            (ResenaItems != null && dTO.ResenaItems != null && System.Linq.Enumerable.SequenceEqual(ResenaItems, dTO.ResenaItems));

            return base.Equals(obj) &&
                CompareDate(FechaResena, dTO.FechaResena) &&
                Id == dTO.Id &&
                itemsEqual;
                }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Id, FechaResena);
        }
    }
}
