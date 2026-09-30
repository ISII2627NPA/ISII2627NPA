using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.Models
{
    public class Visa : MetodoPago
    {
        [Required(ErrorMessage = "El número de tarjeta es obligatorio.")]
        public string NumeroTarjeta { get; set; }

        [Required(ErrorMessage = "La fecha de caducidad es obligatoria.")]
        public DateTime FechaCaducidad { get; set; }

        public Visa()
        {
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            
            Visa otraVisa = (Visa)obj;
            
            return this.Id == otraVisa.Id; 
        }
    }
}