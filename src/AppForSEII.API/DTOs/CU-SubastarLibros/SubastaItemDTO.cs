using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs
{
    // Esta clase es un DTO (Data Transfer Object) que representa la información de un libro específico 
    // (y su respectiva puja) dentro de la lista de items de una subasta.
    public class SubastaItemDTO
    {
        // 1. Constructor Parametrizado: 
        // Se utiliza para instanciar el objeto pasándole todos sus valores de golpe (muy útil en los tests o al mapear desde la BD).
        // Nota: El parámetro 'descripcion' tiene un valor por defecto (""), lo que lo hace opcional al llamar a este constructor.
        public SubastaItemDTO(int libroId, string titulo, string autor, string editorial, decimal precioPuja, string? descripcion = "")
        {
            LibroId = libroId;
            Titulo = titulo;
            Autor = autor;
            Editorial = editorial;
            PrecioPuja = precioPuja;
            Descripcion = descripcion;
        }
        // 2. Constructor Vacío:
        // Obligatorio para el framework de ASP.NET Core. Cuando el controlador recibe un JSON desde el cliente (Frontend),
        // el sistema utiliza este constructor para crear un objeto vacío y luego va rellenando sus propiedades una a una.

        public SubastaItemDTO()
        {
        }
        // 3. Propiedades Básicas:
        // Datos identificativos y descriptivos del libro. Se devuelven al frontend para que pueda mostrarlos 
        // sin tener que hacer peticiones extra a la base de datos.

        public int LibroId { get; set; }

        public string Titulo { get; set; }

        public string Autor { get; set; }

        public string Editorial { get; set; }
        // 4. Propiedades con Validaciones (Data Annotations):
        // Estas etiquetas aplican reglas de negocio automáticas. Si el JSON enviado incumple alguna, 
        // el ModelState del controlador será inválido y devolverá un BadRequest automáticamente.
        [Required(ErrorMessage = "El precio de la puja es obligatorio")]
        // Asegura que el precio de la puja no pueda ser 0 ni negativo (va de 0.01 hasta el valor máximo posible de un double).
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio de puja debe ser mayor que 0")]
        public decimal PrecioPuja { get; set; }
        // El '?' indica que este string puede ser nulo (la descripción es opcional según el caso de uso).
        // Sin embargo, si el usuario decide rellenarla, se fuerza a que tenga un tamaño estricto de entre 20 y 100 caracteres.
        [StringLength(100, MinimumLength = 20, ErrorMessage = "La descripción adicional debe tener entre 20 y 100 caracteres")]
        public string? Descripcion { get; set; }
        // 5. Sobreescritura de Equals:
        // Define cómo se comparan dos objetos de tipo SubastaItemDTO. 
        // En lugar de comparar si son el mismo espacio en memoria, compara que absolutamente todos sus valores internos sean idénticos.
        // Esto es clave para que métodos como 'SequenceEqual' (usado en la clase padre) funcionen correctamente en los tests.
        public override bool Equals(object? obj)
        {
            return obj is SubastaItemDTO dTO &&
                   LibroId == dTO.LibroId &&
                   Titulo == dTO.Titulo &&
                   Autor == dTO.Autor &&
                   Editorial == dTO.Editorial &&
                   PrecioPuja == dTO.PrecioPuja &&
                   Descripcion == dTO.Descripcion;
        }
        // 6. Sobreescritura de GetHashCode:
        // Acompaña obligatoriamente al 'Equals'. Crea un código numérico único basado en la combinación de todos los campos del item.
        // Es utilizado por el sistema para identificar y ordenar este objeto eficientemente en colecciones y listas.
        public override int GetHashCode()
        {
            return HashCode.Combine(LibroId, Titulo, Autor, Editorial, PrecioPuja, Descripcion);
        }
    }
}