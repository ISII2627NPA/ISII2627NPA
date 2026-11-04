using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs
{
    // Clase DTO para mostrar los detalles de un libro que está disponible para comprar.
    public class LibroParaComprarDTO
    {
        // Este constructor es OBLIGATORIO. El "deserializador" de JSON 
        // de ASP.NET lo necesita para poder crear este objeto cuando recibe un JSON desde el cliente.
        public LibroParaComprarDTO()
        {
        }
       
       // Lo usamos nosotros manualmente en el código (por ejemplo en el SELECT del Controlador)
        // para coger los datos de la BD y construir este "paquete" DTO rápidamente en una sola línea.
        public LibroParaComprarDTO(int id, string titulo, string genero, string editorial, string autor, DateTime fechaLanzamiento, double precioCompra, int stock)
        {
            Id = id;
            Titulo = titulo;
            Genero = genero;
            Editorial = editorial;
            Autor = autor;
            FechaLanzamiento = fechaLanzamiento;
            PrecioCompra = precioCompra;
            Stock = stock;
        }

        // Tienen valores por defecto (ej: = "Título"). Esto evita que sean nulos (null) 
        // si al crear el objeto por defecto no se les asigna nada.
        public int Id { get; set; }

        public string Titulo { get; set; } = "Título";

        public string Genero { get; set; } = "Ficción";

        public string Editorial { get; set; } = "Editorial";


        // [Required]: Hace que este campo sea obligatorio. Si el cliente envía un JSON sin autor,
        // la API ni siquiera entra al controlador; devuelve directamente un HTTP 400 (Bad Request)
        // mostrando el mensaje que hemos puesto.
        [Required(ErrorMessage = "El autor es obligatorio")]
        public string Autor { get; set; } = "Autor";

        
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        public DateTime FechaLanzamiento { get; set; } = DateTime.Today;


        // [DataType] currency solo informa de que es moneda (ej: para poner el símbolo € en una web). 
        // El que de verdad valida y bloquea números negativos es el [Range]
        [Required]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        [Range(1, float.MaxValue, ErrorMessage = "El precio mínimo es 1")]
        public double PrecioCompra { get; set; } = 1;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "El stock debe ser mayor que 0")]
        public int Stock { get; set; } = 1;

        public override bool Equals(object? obj)
        {
            return obj is LibroParaComprarDTO dTO &&
                   Id == dTO.Id &&
                   Titulo == dTO.Titulo &&
                   Genero == dTO.Genero &&
                   Editorial == dTO.Editorial &&
                   Autor == dTO.Autor &&
                   FechaLanzamiento == dTO.FechaLanzamiento &&
                   PrecioCompra == dTO.PrecioCompra &&
                   Stock == dTO.Stock;
        }

        public override int GetHashCode()
        {
            // HashCode.Combine admite un máximo de 8 parámetros exactos, por lo que encaja perfecto
            return HashCode.Combine(Id, Titulo, Genero, Editorial, Autor, FechaLanzamiento, PrecioCompra, Stock);
        }
    }
}