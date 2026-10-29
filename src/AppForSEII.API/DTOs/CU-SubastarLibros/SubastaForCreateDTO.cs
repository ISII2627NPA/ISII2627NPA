using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AppForSEII.API.DTOs
{
    public class SubastaForCreateDTO
    {
        
        public SubastaForCreateDTO(string usuarioUserName, string nombre, string apellidos, string telefono, int metodoPago, IList<SubastaItemDTO> subastaItems)
        {
            UsuarioUserName = usuarioUserName ?? throw new ArgumentNullException(nameof(usuarioUserName));
            Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            Apellidos = apellidos ?? throw new ArgumentNullException(nameof(apellidos));
            Telefono = telefono ?? throw new ArgumentNullException(nameof(telefono));
            MetodoPago = metodoPago;
            
            
            SubastaItems = subastaItems ?? throw new ArgumentNullException(nameof(subastaItems));
        }

        public SubastaForCreateDTO()
        {
            
            SubastaItems = new List<SubastaItemDTO>();
        }

        [EmailAddress]
        [Required]
        public string UsuarioUserName { get; set; } = "peter.jackson@uclm.es";

        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, establece tu nombre")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")]
        public string Nombre { get; set; } = "Peter";

        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, establece tus apellidos")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener al menos 2 caracteres")]
        public string Apellidos { get; set; } = "Jackson";

        [Phone]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, establece tu teléfono")]
        public string Telefono { get; set; } = "666555444";

        [Required]
        public int MetodoPago { get; set; }

        
        public IList<SubastaItemDTO> SubastaItems { get; set; } = new List<SubastaItemDTO>();

        public decimal PrecioTotal
        {
            get
            {
                
                return SubastaItems.Sum(si => si.PrecioPuja);
                
            }
        }

        protected bool CompareDate(DateTime date1, DateTime date2)
        {
            return (date1.Subtract(date2) < new TimeSpan(0, 1, 0));
        }

        public override bool Equals(object? obj)
        {
            return obj is SubastaForCreateDTO dTO &&
                   UsuarioUserName == dTO.UsuarioUserName &&
                   Nombre == dTO.Nombre &&
                   Apellidos == dTO.Apellidos &&
                   Telefono == dTO.Telefono &&
                   MetodoPago == dTO.MetodoPago &&
                   
                   SubastaItems.SequenceEqual(dTO.SubastaItems) &&
                   PrecioTotal == dTO.PrecioTotal;
        }

        public override int GetHashCode()
        {
    
            return HashCode.Combine(UsuarioUserName, Nombre, Apellidos, Telefono, MetodoPago);
        }
    }
}