using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.Models
{
    public abstract class MetodoPago
    {
        public MetodoPago()
        {
        }

        [Key]
        public int Id { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is MetodoPago m)
            {
                return Id == m.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}