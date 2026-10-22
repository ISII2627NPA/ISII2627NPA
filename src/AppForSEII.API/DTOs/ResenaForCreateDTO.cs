using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AppForSEII.API.DTOs
{
    public class ResenaForCreateDTO
    {
        public ResenaForCreateDTO(string usuarioUserName, string nombre, string apellidos, string direccion, string telefono, string titulo) //IList<ResenaItemDTO> resenaItems
        {                                                                                                                                   // AÑADIR LUEGO
            UsuarioUserName = usuarioUserName ?? throw new ArgumentNullException(nameof(usuarioUserName));
            Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            Apellidos = apellidos ?? throw new ArgumentNullException(nameof(apellidos));
            Direccion = direccion ?? throw new ArgumentNullException(nameof(direccion));
            Telefono = telefono ?? throw new ArgumentNullException(nameof(telefono));
            Titulo = titulo ?? throw new ArgumentNullException(nameof(titulo));
            //ResenaItems = resenaItems ?? throw new ArgumentNullException(nameof(resenaItems));
        }

        public ResenaForCreateDTO()
        {
            //ResenaItems = new List<ResenaItemDTO>();
        }

        [EmailAddress]
        [Required]
        public string UsuarioUserName { get; set; } = "peter@uclm.es";

        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, establece tu nombre")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")]
        public string Nombre { get; set; } = "Peter";

        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, establece tus apellidos")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener al menos 2 caracteres")]
        public string Apellidos { get; set; } = "Jackson";

        [DataType(System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "La dirección debe tener al menos 10 caracteres")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, establece tu dirección")]
        public string Direccion { get; set; } = "Albacete";

        [Phone]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, establece tu teléfono")]
        public string Telefono { get; set; } = "600000000";

        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, establece el título de la reseña")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "El título debe tener entre 10 y 20 caracteres")]
        public string Titulo { get; set; } = "Reseña de prueba";

        //public IList<ResenaItemDTO> ResenaItems { get; set; } = new List<ResenaItemDTO>();

        public override bool Equals(object? obj)
        {
            return obj is ResenaForCreateDTO dTO &&
                   UsuarioUserName == dTO.UsuarioUserName &&
                   Nombre == dTO.Nombre &&
                   Apellidos == dTO.Apellidos &&
                   Direccion == dTO.Direccion &&
                   Telefono == dTO.Telefono &&
                   Titulo == dTO.Titulo;            // AÑADIR LUEGO 
                   //ResenaItems.SequenceEqual(dTO.ResenaItems
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UsuarioUserName, Direccion, Telefono, Titulo);
        }
    }
}