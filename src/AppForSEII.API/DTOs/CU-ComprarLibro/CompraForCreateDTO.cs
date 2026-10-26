using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AppForSEII.API.Models; 

namespace AppForSEII.API.DTOs
{
    public class CompraForCreateDTO
    {
       
        public CompraForCreateDTO(string usuarioUserName, string nombre, string apellidos, string direccion, string telefono, MetodoPago metodoPago, string? codigoDescuento , IList<CompraItemDTO> compraItems)
        {
            UsuarioUserName = usuarioUserName ?? throw new ArgumentNullException(nameof(usuarioUserName));
            Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            Apellidos = apellidos ?? throw new ArgumentNullException(nameof(apellidos));
            Direccion = direccion ?? throw new ArgumentNullException(nameof(direccion));
            Telefono = telefono ?? throw new ArgumentNullException(nameof(telefono));
            MetodoPago = metodoPago;
            CodigoDescuento = codigoDescuento;
            
           
             CompraItems = compraItems ?? throw new ArgumentNullException(nameof(compraItems));
        }

        public CompraForCreateDTO()
        {
           
            CompraItems = new List<CompraItemDTO>();
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

        [DataType(System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "La dirección debe tener al menos 10 caracteres")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, establece tu dirección para el envío")]
        public string Direccion { get; set; } = "Albacete";

        [Phone]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, establece tu teléfono")]
        public string Telefono { get; set; } = "666555444";

        [Required]
        public MetodoPago MetodoPago { get; set; } 

        [StringLength(10, MinimumLength = 5, ErrorMessage = "El código de descuento debe tener entre 5 y 10 caracteres")]
        public string? CodigoDescuento { get; set; }

        
         public IList<CompraItemDTO> CompraItems { get; set; } = new List<CompraItemDTO>();

        public decimal PrecioTotal
        {
            get
            {
                
                return CompraItems.Sum(ci => ci.PrecioCompra * ci.Cantidad);
                
            }
        }

        protected bool CompareDate(DateTime date1, DateTime date2)
        {
            return (date1.Subtract(date2) < new TimeSpan(0, 1, 0));
        }

        public override bool Equals(object? obj)
        {
            return obj is CompraForCreateDTO dTO &&
                   UsuarioUserName == dTO.UsuarioUserName &&
                   Nombre == dTO.Nombre &&
                   Apellidos == dTO.Apellidos &&
                   Direccion == dTO.Direccion &&
                   Telefono == dTO.Telefono &&
                   MetodoPago == dTO.MetodoPago &&
                   CodigoDescuento == dTO.CodigoDescuento &&
                   
                   
                    CompraItems.SequenceEqual(dTO.CompraItems) &&
                   
                   PrecioTotal == dTO.PrecioTotal;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), UsuarioUserName, Direccion, Telefono);
        }
    }
}