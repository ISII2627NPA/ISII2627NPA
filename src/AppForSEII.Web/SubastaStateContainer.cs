using AppForSEII.Web.API; // Ajusta a tu namespace real

namespace AppForSEII.Web 
{
    public class SubastaStateContainer 
    {
        // Almacena el DTO principal de la subasta en curso
        public SubastaForCreateDTO Subasta { get; private set; } = new SubastaForCreateDTO() 
        {
            SubastaItems = new List<SubastaItemDTO>()
        };

        // Flujo Básico 3: Añadir libro a la lista
        public void AddLibroToSubasta(LibroParaSubastarDTO libro) 
        {
            if (!Subasta.SubastaItems.Any(item => item.LibroId == libro.Id)) 
            {
                Subasta.SubastaItems.Add(new SubastaItemDTO() 
                {
                    LibroId = libro.Id,
                    Titulo = libro.Titulo,
                    Autor = libro.Autor,
                    Editorial = libro.Editorial
                });
            }
        }

        // Flujo Alternativo 3: Eliminar libros que ya no le interesen
        public void RemoveLibroFromSubasta(SubastaItemDTO item) 
        {
            Subasta.SubastaItems.Remove(item);
        }

        public void ClearSubasta() 
        {
            Subasta.SubastaItems.Clear();
        }
    }
}