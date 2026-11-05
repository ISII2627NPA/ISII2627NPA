using System;
using System.Collections.Generic;

namespace AppForSEII.API.DTOs
{
    // Los dos puntos (:) indican herencia. ResenaDetailDTO es hijo de ResenaForCreateDTO.
    public class ResenaDetailDTO : ResenaForCreateDTO
    {
        // El constructor recibe todos los datos (los del padre y los suyos propios).
        public ResenaDetailDTO(int id, DateTime fechaResena, string usuarioUserName, string nombre, string apellidos,
            string direccion, string telefono, string titulo, IList<ResenaItemDTO> resenaItems)
            // ": base(...)" envía los datos compartidos al constructor de la clase padre (ResenaForCreateDTO) para que los asigne allí.
            : base(usuarioUserName, nombre, apellidos, direccion, telefono, titulo, resenaItems)
        {
            // Y aquí solo asigna las dos propiedades exclusivas que el padre no tiene.
            Id = id;
            FechaResena = fechaResena;
        }

        public int Id { get; set; }
        public DateTime FechaResena { get; set; }

        // Método auxiliar para evitar que los tests fallen si la base de datos devuelve una fecha con milisegundos de diferencia.
        protected bool CompareDate(DateTime date1, DateTime date2)
        {
        // Comprueba que la diferencia absoluta (.Duration) entre las dos fechas sea menor a 1 minuto.
        return date1.Subtract(date2).Duration() < new TimeSpan(0, 1, 0);
        }

        // Sobrescribe la comparación por defecto de C# para que Assert.Equal evalúe el contenido, no la memoria.
        public override bool Equals(object? obj)
        {
            // Si el objeto a comparar no es un ResenaDetailDTO, devuelve falso de inmediato.
            if (obj is not ResenaDetailDTO dTO) return false;

            // Comprobamos que la lista de libros de dentro también es exactamente igual
            // Valida 3 escenarios: ambas nulas, o ambas con datos y con el mismo contenido usando SequenceEqual.
            bool itemsEqual = (ResenaItems == null && dTO.ResenaItems == null) ||
                            (ResenaItems != null && dTO.ResenaItems != null && System.Linq.Enumerable.SequenceEqual(ResenaItems, dTO.ResenaItems));

            // base.Equals(obj) es clave: llama al Equals del padre para comparar Nombre, Apellidos, etc., sin tener que reescribirlos aquí.
            return base.Equals(obj) &&
                CompareDate(FechaResena, dTO.FechaResena) &&
                Id == dTO.Id &&
                itemsEqual;
                }

        // Genera un código hash único combinando el hash del padre y los campos propios. Necesario si usas diccionarios o HashSet.
        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Id, FechaResena);
        }
    }
}