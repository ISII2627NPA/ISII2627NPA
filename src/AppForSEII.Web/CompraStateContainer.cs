using AppForSEII.Web.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppForSEII.Web
{
    public class CompraStateContainer
    {
        public CompraForCreateDTO Compra { get; private set; } = new CompraForCreateDTO()
        {
            CompraItems = new List<CompraItemDTO>()
        };

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();

        public void AddLibroToCompra(LibroParaComprarDTO libro, int cantidadSolicitada)
        {
            if (!Compra.CompraItems.Any(ri => ri.LibroId == libro.Id))
                Compra.CompraItems.Add(new CompraItemDTO()
                {
                    LibroId = libro.Id,
                    Titulo = libro.Titulo,
                    Genero = libro.Genero,
                    Editorial = libro.Editorial,
                    Cantidad = cantidadSolicitada,
                    PrecioCompra = libro.PrecioCompra,
                }
            );
            
            // Llamamos al cálculo exactamente igual que en Movies
            ComputeTotalPrice(); 
        }

        private void ComputeTotalPrice()
        {
            // Multiplicamos la cantidad por el precio (en Movies multiplicaban por los días)
            Compra.PrecioTotal = Compra.CompraItems.Sum(ci => ci.PrecioCompra * ci.Cantidad);
        }

        public void RemoveCompraItemToCompra(CompraItemDTO item)
        {
            Compra.CompraItems.Remove(item);
            
            // Llamamos al cálculo exactamente igual que en Movies
            ComputeTotalPrice();
        }

        public void ClearCompraCart()
        {
            Compra.CompraItems.Clear();
            
            // Forzamos el precio a 0 exactamente igual que en Movies
            Compra.PrecioTotal = 0; 
        }

        public void CompraProcessed()
        {
            Compra = new CompraForCreateDTO()
            {
                CompraItems = new List<CompraItemDTO>()
            };
        }
    }
}