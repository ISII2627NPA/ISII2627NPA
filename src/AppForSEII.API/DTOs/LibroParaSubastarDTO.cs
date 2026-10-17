using System;

namespace AppForSEII.API.DTOs
{
    public class LibroParaSubastarDTO
    {
        
        public LibroParaSubastarDTO()
        {
        }

        
        public LibroParaSubastarDTO(int id, string titulo, string autor, string editorial, string tipoLibro)
        {
            Id = id;
            Titulo = titulo;
            Autor = autor;
            Editorial = editorial;
            TipoLibro = tipoLibro;
        }

        
        public int Id { get; set; }
        
        public string Titulo { get; set; } = "Título";
        
        public string Autor { get; set; } = "Autor";
        
        public string Editorial { get; set; } = "Editorial";
        
        public string TipoLibro { get; set; } = "Físico";

        
        public override bool Equals(object? obj)
        {
            return obj is LibroParaSubastarDTO dTO &&
                   Id == dTO.Id &&
                   Titulo == dTO.Titulo &&
                   Autor == dTO.Autor &&
                   Editorial == dTO.Editorial &&
                   TipoLibro == dTO.TipoLibro;
        }

        
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Titulo, Autor, Editorial, TipoLibro);
        }
    }
}