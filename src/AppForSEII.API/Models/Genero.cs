using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.Models
{
    public class Genero
    {
        public Genero()
        {
            Nombre = string.Empty;
            //Libros = new List<Libro>(); 
        }

        [Key]
        public int Id { get; set; }
        
        [Required(AllowEmptyStrings = false, ErrorMessage = "El nombre del género es obligatorio")]
        public string Nombre { get; set; }

        // Relación 1:N con Libro 
     //   public List<Libro> Libros { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is Genero genero)
            {
                return Id == genero.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}