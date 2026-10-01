using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class CompraItem
    {
        public CompraItem()
        {
        }

        public CompraItem(int cantidad, int libroId, int compraId)
        {
            Cantidad = cantidad;
            LibroId = libroId;
            CompraId = compraId;
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
        public int Cantidad { get; set; }

        // Relación N:1 con Libro
        public int LibroId { get; set; }
        
        // TODO: Descomentar estas líneas cuando todas las ramas estén en development
        // [ForeignKey("LibroId")]
        // public virtual Libro Libro { get; set; } = null!;

        // Relación N:1 con Compra
        public int CompraId { get; set; }
        
        // TODO: Descomentar estas líneas cuando la clase Compra esté en development
        // [ForeignKey("CompraId")]
        // public virtual Compra Compra { get; set; } = null!;

        public override bool Equals(object? obj)
        {
            if (obj is CompraItem item)
            {
                return Id == item.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}