using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.Models
{
    public class GooglePay : MetodoPago
    {
        public GooglePay()
        {
            Email = string.Empty;
        }

        public GooglePay(string email)
        {
            Email = email;
        }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        public string Email { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is GooglePay gp)
            {
                return Id == gp.Id; // El Id lo hereda de MetodoPago
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}