using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AppForSEII.API.DTOs
{
    public class ResenaForCreateDTO
    {
        public ResenaForCreateDTO(string usuarioUserName, string nombre, string apellidos, string direccion, string telefono, string titulo, IList<ResenaItemDTO> resenaItems)
        {                                                                                                                                              
            // throw new ArgumentNullException evita que se instancie el DTO si algún dato obligatorio llega nulo desde el código.
            UsuarioUserName = usuarioUserName ?? throw new ArgumentNullException(nameof(usuarioUserName));
            Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            Apellidos = apellidos ?? throw new ArgumentNullException(nameof(apellidos));
            Direccion = direccion ?? throw new ArgumentNullException(nameof(direccion));
            Telefono = telefono ?? throw new ArgumentNullException(nameof(telefono));
            Titulo = titulo ?? throw new ArgumentNullException(nameof(titulo));
            ResenaItems = resenaItems ?? throw new ArgumentNullException(nameof(resenaItems));
        }

        // Constructor sin parámetros necesario para que el framework pueda instanciar el DTO automáticamente al recibir un JSON en el POST.
        public ResenaForCreateDTO()
        {
            ResenaItems = new List<ResenaItemDTO>();
        }

        // Valida que el string tenga formato de correo (@ y dominio).
        [EmailAddress]
        [Required]
        public string UsuarioUserName { get; set; } = "peter@uclm.es";

        // Estas Data Annotations validan el input del usuario antes de que llegue al controlador, devolviendo un error 400 automático si fallan.
        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, establece tu nombre")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")]
        public string Nombre { get; set; } = "Peter";

        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, establece tus apellidos")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener al menos 2 caracteres")]
        public string Apellidos { get; set; } = "Jackson";

        // MultilineText sugiere a las vistas (front-end) que rendericen un <textarea> en lugar de un <input> normal.
        [DataType(System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "La dirección debe tener al menos 10 caracteres")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, establece tu dirección")]
        public string Direccion { get; set; } = "Albacete";

        // Valida formatos de número telefónico.
        [Phone]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, establece tu teléfono")]
        public string Telefono { get; set; } = "600000000";

        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, establece el título de la reseña")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "El título debe tener entre 10 y 20 caracteres")]
        public string Titulo { get; set; } = "Reseña de prueba";

        // Lista de ítems anidados. Si un usuario reseña 3 libros a la vez, vendrán aquí dentro.
        public IList<ResenaItemDTO> ResenaItems { get; set; } = new List<ResenaItemDTO>();

        // Este Equals es el que llama la clase hija mediante "base.Equals(obj)".
        public override bool Equals(object? obj)
        {
            // Compara estrictamente todas las propiedades de texto una por una.
            return obj is ResenaForCreateDTO dTO &&
                   UsuarioUserName == dTO.UsuarioUserName &&
                   Nombre == dTO.Nombre &&
                   Apellidos == dTO.Apellidos &&
                   Direccion == dTO.Direccion &&
                   Telefono == dTO.Telefono &&
                   Titulo == dTO.Titulo &&        
                   // SequenceEqual exige que las listas tengan los mismos elementos en el mismo orden.
                   ResenaItems.SequenceEqual(dTO.ResenaItems);
        

        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UsuarioUserName, Nombre, Apellidos, Direccion, Telefono, Titulo, ResenaItems);
            
        }
    }
}