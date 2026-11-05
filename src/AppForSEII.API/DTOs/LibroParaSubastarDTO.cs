using System;

namespace AppForSEII.API.DTOs
{
    // Esta clase es un DTO (Data Transfer Object) diseñado exclusivamente para enviar 
    // la información básica de un libro cuando se lista en el catálogo de subastas.
    // Al usar este DTO, evitas exponer la entidad 'Libro' completa y filtras solo lo necesario.
    public class LibroParaSubastarDTO
    {
        // 1. Constructor Vacío:
        // Es obligatorio para el funcionamiento interno de ASP.NET Core y Entity Framework. 
        // Permite al sistema instanciar el objeto en blanco antes de inyectarle valores 
        // dinámicamente (por ejemplo, al deserializar o mapear datos).
        public LibroParaSubastarDTO()
        {
        }

        // 2. Constructor Parametrizado:
        // Permite inicializar el objeto asignando todos sus valores en una sola operación.
        // Es exactamente el que estás llamando en el '.Select(...)' de tu controlador 
        // para transformar la consulta de la base de datos directamente en este DTO.
        public LibroParaSubastarDTO(int id, string titulo, string autor, string editorial, string tipoLibro)
        {
            Id = id;
            Titulo = titulo;
            Autor = autor;
            Editorial = editorial;
            TipoLibro = tipoLibro;
        }
        // 3. Propiedades de Datos:
        // Representan la información que viajará al Frontend para pintar la tabla de selección.
    
        // El ID es vital para saber qué libro exacto seleccionó el usuario para pujar.  
        public int Id { get; set; }
        // A las cadenas de texto se les asigna un valor por defecto ("Título", "Autor", etc.).
        // Esto previene excepciones de referencia nula (NullReferenceException) si el objeto 
        // se crea con el constructor vacío y se intenta leer antes de asignarle valores reales.
        public string Titulo { get; set; } = "Título";
        
        public string Autor { get; set; } = "Autor";
        
        public string Editorial { get; set; } = "Editorial";
        
        public string TipoLibro { get; set; } = "Físico";

        // 4. Sobreescritura del método Equals:
        // Convierte la comparación de memoria en una comparación de valores. 
        // Esto es lo que permite que tus 'Assert.Equal()' en las pruebas unitarias funcionen 
        // al comparar la lista de libros esperados contra la devuelta por el controlador.
        public override bool Equals(object? obj)
        {
            return obj is LibroParaSubastarDTO dTO &&
                   Id == dTO.Id &&
                   Titulo == dTO.Titulo &&
                   Autor == dTO.Autor &&
                   Editorial == dTO.Editorial &&
                   TipoLibro == dTO.TipoLibro;
        }

        // 5. Sobreescritura de GetHashCode:
        // Es una regla estricta de C#: si sobreescribes Equals, debes sobreescribir GetHashCode.
        // Combina todos los campos en una fórmula matemática para generar un número (hash) único,
        // necesario si alguna vez decides meter estos DTOs dentro de un HashSet o usar .Distinct().
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Titulo, Autor, Editorial, TipoLibro);
        }
    }
}