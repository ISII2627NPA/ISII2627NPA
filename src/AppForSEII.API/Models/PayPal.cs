using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.Models
{
    public class PayPal : MetodoPago
    {
        [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
        public string NumeroTelefono { get; set; }

        public PayPal() 
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is PayPal p)
            {
                return Id == p.Id;
            }
            return false;
        }
    }
}