using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.Models
{
    public class Editorial
    {
        public Editorial()
        {
            Nombre = string.Empty; // Resuelve el aviso de propiedad no nula
            // Libros = new List<Libro>(); // Desactivado temporalmente
        }

        public Editorial(string nombre) 
        {
            Nombre = nombre;
        }

        [Key]
        public int Id { get; set; }

        
        [Required(AllowEmptyStrings = false, ErrorMessage = "El nombre de la editorial es obligatorio")]
        public string Nombre { get; set; }

        // public virtual IList<Libro> Libros { get; set; } // Desactivado temporalmente

        public override bool Equals(object? obj)
        {
            if (obj is Editorial editorial)
            {
                return Id == editorial.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}