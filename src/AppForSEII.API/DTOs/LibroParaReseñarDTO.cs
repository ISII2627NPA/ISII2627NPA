using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs
{
    public class LibroParaReseñarDTO
    {
        public LibroParaReseñarDTO()
        {
        }

        public LibroParaReseñarDTO(int id, string titulo, string tipoLibro, string genero, string editorial, string autor, decimal calificacionMedia, DateTime fechaLanzamiento)
        {
            Id = id;
            Titulo = titulo;
            TipoLibro = tipoLibro;
            Genero = genero;
            Editorial = editorial;
            Autor = autor;
            CalificacionMedia = calificacionMedia;
            FechaLanzamiento = fechaLanzamiento;
        }

        public int Id { get; set; }

        public string Titulo { get; set; } = "Título";

        public string TipoLibro { get; set; } = "Tapa dura";

        public string Genero { get; set; } = "Ficción";

        public string Editorial { get; set; } = "Editorial";

        [Required(ErrorMessage = "El autor es obligatorio")]
        public string Autor { get; set; } = "Autor";

        [Required(ErrorMessage = "La calificación media es obligatoria")]
        [Range(0, 5, ErrorMessage = "La calificación media debe estar entre 0 y 5")]
        public decimal CalificacionMedia { get; set; } = 0;

        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        public DateTime FechaLanzamiento { get; set; } = DateTime.Today;

        public override bool Equals(object? obj)
        {
            return obj is LibroParaReseñarDTO dTO &&
                   Id == dTO.Id &&
                   Titulo == dTO.Titulo &&
                   TipoLibro == dTO.TipoLibro &&
                   Genero == dTO.Genero &&
                   Editorial == dTO.Editorial &&
                   Autor == dTO.Autor &&
                   CalificacionMedia == dTO.CalificacionMedia &&
                   FechaLanzamiento == dTO.FechaLanzamiento;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Titulo, TipoLibro, Genero, Editorial, Autor, CalificacionMedia, FechaLanzamiento);
        }
    }
}