using System;

namespace AppForSEII.API.DTOs
{
    public class ResenaItemDTO
    {
        // El "= """ en descripcion lo hace un parámetro opcional. Si no se le pasa, asume cadena vacía.
        public ResenaItemDTO(int libroId, string titulo, string editorial, string tipoLibro, int calificacion, string? descripcion = "")
        {
            LibroId = libroId;
            Titulo = titulo;
            Editorial = editorial;
            TipoLibro = tipoLibro;
            Calificacion = calificacion;
            Descripcion = descripcion;
        }

        public int LibroId { get; set; }
        public string Titulo { get; set; }
        public string Editorial { get; set; }
        public string TipoLibro { get; set; }
        public int Calificacion { get; set; }
        // El interrogante (string?) indica que este campo permite explícitamente nulos.
        public string? Descripcion { get; set; }

        // Este Equals es crucial. Cuando las clases padre usan "SequenceEqual" sobre la lista de ResenaItems, 
        // SequenceEqual llama internamente a este método para decidir si el libro A es igual al libro B.
        public override bool Equals(object? obj)
        {
            return obj is ResenaItemDTO dTO &&
                   LibroId == dTO.LibroId &&
                   Titulo == dTO.Titulo &&
                   Editorial == dTO.Editorial &&
                   TipoLibro == dTO.TipoLibro &&
                   Calificacion == dTO.Calificacion &&
                   Descripcion == dTO.Descripcion;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LibroId, Titulo, Editorial, TipoLibro, Calificacion, Descripcion);
        }
    }
}