using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Models
{
    [PrimaryKey(nameof(LibroId), nameof(SubastaId))]
    public class SubastaItem
    {
    
        [Required(ErrorMessage = "El precio de la puja es obligatorio.")]
        public decimal PrecioPuja { get; set; }

        
        
        [StringLength(100, MinimumLength = 20, ErrorMessage = "La descripción debe tener entre 20 y 100 caracteres")]
        public string? Descripcion { get; set; }

        
        public int LibroId { get; set; }
        //public Libro Libro { get; set; }    Desactivada temporalmente

        public int SubastaId { get; set; }
        //public Subasta Subasta { get; set; }    Desactivada temporalmente

       
        public SubastaItem()
    {
        Descripcion = string.Empty;
    }

    public SubastaItem(decimal precioPuja, string? descripcion, int libroId, int subastaId)
    {
        PrecioPuja = precioPuja;
        Descripcion = descripcion;
        LibroId = libroId;
        SubastaId = subastaId;
    }

        public override bool Equals(object? obj)
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