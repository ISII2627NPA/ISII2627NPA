using AppForSEII.Web.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppForSEII.Web 
{
    public class ResenaStateContainer 
    {
        
        public ResenaForCreateDTO Resena { get; private set; } = new ResenaForCreateDTO() 
        {
            ResenaItems = new List<ResenaItemDTO>()
        };

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();


        public void AddLibroToResena(LibroParaReseñarDTO libro) 
        {
            
            if (!Resena.ResenaItems.Any(ri => ri.LibroId == libro.Id)) 
            {
                
                Resena.ResenaItems.Add(new ResenaItemDTO() 
                {
                    LibroId = libro.Id,
                    Titulo = libro.Titulo,
                    Editorial = libro.Editorial,
                    TipoLibro = libro.TipoLibro,
                    
                    Calificacion = 1 
                });
                NotifyStateChanged();
            }
        }

        public void RemoveResenaItem(ResenaItemDTO item) 
        {
            Resena.ResenaItems.Remove(item);
            NotifyStateChanged();
        }


        public void ClearResenaCart() 
        {
            Resena.ResenaItems.Clear();
            NotifyStateChanged();
        }

        
        public void ResenaProcessed() 
        {
            Resena = new ResenaForCreateDTO() 
            {
                ResenaItems = new List<ResenaItemDTO>()
            };
            NotifyStateChanged();
        }
    }
}