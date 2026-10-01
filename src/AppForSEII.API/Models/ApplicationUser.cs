
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Identity; 

    namespace AppForSEII.API.Models
    {
        public class ApplicationUser : IdentityUser
        {
            public ApplicationUser()
            {
                Nombre = string.Empty;
                Apellidos = string.Empty;
                Direccion = string.Empty;
                Telefono = string.Empty;
                // Resenas = new List<Resena>(); // TODO: Descomentar cuando Resena esté integrada
            }

            public ApplicationUser(string nombre, string apellidos, string direccion, string telefono)
            {
                Nombre = nombre;
                Apellidos = apellidos;
                Direccion = direccion;
                Telefono = telefono;
            }

            [Required(ErrorMessage = "El nombre es obligatorio.")]
            public string Nombre { get; set; }

            [Required(ErrorMessage = "Los apellidos son obligatorios.")]
            public string Apellidos { get; set; }

            [Required(ErrorMessage = "La dirección es obligatoria.")]
            public string Direccion { get; set; }

            [Required(ErrorMessage = "El teléfono es obligatorio.")]
            public string Telefono { get; set; }

            // Relación 1:N con Resena (Caso de Uso 4)
            // TODO: Descomentar cuando la clase Resena esté en development
            // public IList<Resena> Resenas { get; set; }

            public override bool Equals(object? obj)
            {
                if (obj is ApplicationUser user)
                {
                    return Id == user.Id; 
                }
                return false;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Id);
            }
        }
    }
