using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.Models
{
    public class Genero
    {
        public Genero()
        {
            Nombre = string.Empty;
            // Libros = new List<Libro>(); // Desactivado temporalmente
        }

        public Genero(string nombre) 
        {
            Nombre = nombre;
        }

        [Key]
        public int Id { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Nombre del Género")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "El nombre del género es obligatorio")]
        public string Nombre { get; set; }

        // public virtual IList<Libro> Libros { get; set; } // Desactivado temporalmente

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