using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class Resena
    {
        // Constructor vacío obligatorio para que Entity Framework (EF Core) pueda instanciar la clase al leer de la BD.
        public Resena()
        {
            Titulo = string.Empty;
            ApplicationUserId = string.Empty;
        }

        public Resena(string applicationUserId, DateTime fechaResena, string titulo)
        {
            ApplicationUserId = applicationUserId;
            FechaResena = fechaResena;
            Titulo = titulo;
        }

        // [Key] define esta propiedad como la Clave Primaria de la tabla.
        [Key]
        public int Id { get; set; } 
        
        // [Required] impide valores nulos en la base de datos y lanza el ErrorMessage si llega vacío desde la API.
        [Required(ErrorMessage = "El identificador del usuario es obligatorio")]
        public string ApplicationUserId { get; set; }

        [Required(ErrorMessage = "La fecha de la reseña es obligatoria")]
        public DateTime FechaResena { get; set; }

        // [StringLength] limita el tamaño de la columna en SQL (VARCHAR) y valida las longitudes mínima y máxima.
        [Required(ErrorMessage = "El título de la reseña es obligatorio")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "El título de la reseña debe tener entre 10 y 20 caracteres")]
        public string Titulo { get; set; }

        // --- RELACIONES ---
        [Required(ErrorMessage = "El identificador del usuario es obligatorio")]
        public string UsuarioId { get; set; }
        
        
        // Propiedad de Navegación (N:1): Permite cargar el objeto ApplicationUser completo (ej. usando .Include(r => r.Usuario)).
        public ApplicationUser Usuario { get; set; } 


        // Propiedad de Navegación (1:N): Una reseña engloba una lista de ítems (los libros valorados).
        public IList<ResenaItem> ResenaItems { get; set; } 


        // En las Entidades, la igualdad se define de forma estricta por identidad (Primary Key).
        public override bool Equals(object? obj)
        {
            if (obj is Resena resena)
            {
                return Id == resena.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}