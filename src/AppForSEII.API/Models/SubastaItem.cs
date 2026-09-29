using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.Models
{
    public class SubastaItem
    {
    
        [Required(ErrorMessage = "El precio de la puja es obligatorio.")]
        public decimal PrecioPuja { get; set; }

        public string Descripcion { get; set; }

        
        public int LibroId { get; set; }
        //public Libro Libro { get; set; }    Desactivada temporalmente

        public int SubastaId { get; set; }
        //public Subasta Subasta { get; set; }    Desactivada temporalmente

       
        public SubastaItem()
        {
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            
            SubastaItem otroItem = (SubastaItem)obj;
            return this.LibroId == otroItem.LibroId && this.SubastaId == otroItem.SubastaId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LibroId, SubastaId);
        }
    }
}